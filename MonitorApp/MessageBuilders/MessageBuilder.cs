using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using MonitorApp.ConnectionServices;

namespace MonitorApp.MessageBuilders;

/// <summary>
/// Parses notification templates and populates them with data from a QueryResult.
/// </summary>
public class MessageBuilder
{
    private const string PlainTextFormat = "plaintext";
    private const string XmlFormat = "xml";
    private const string MarkdownFormat = "markdown";

    /// <summary>
    /// Builds a structured message from a template and query result.
    /// </summary>
    public Message Build(QueryResult queryResult, string template, string? format)
    {
        string effectiveFormat = format ?? PlainTextFormat;

        if (effectiveFormat == PlainTextFormat)
        {
            return new Message { Body = template };
        }

        if (queryResult.Data.Count == 0)
        {
            return new Message();
        }

        int globalCount = queryResult.Data.Count;
        template = template.Replace("{global.count}", globalCount.ToString());

        string headerTemplate = string.Empty;
        string bodyContent = string.Empty;
        string footerTemplate = string.Empty;

        if (effectiveFormat == XmlFormat)
        {
            try
            {
                XDocument xmlDoc = XDocument.Parse($"<root>{template}</root>");
                XElement? root = xmlDoc.Root;

                headerTemplate = root.Element("head") != null ? GetInnerXml(root.Element("head")) : string.Empty;
                footerTemplate = root.Element("footer") != null ? GetInnerXml(root.Element("footer")) : string.Empty;

                XElement? groupElement = root.Element("group");
                XElement? bodyElement = root.Element("body");
                XAttribute? groupByAttribute = groupElement?.Attribute("by");

                if (groupElement != null && groupByAttribute != null && !string.IsNullOrEmpty(groupByAttribute.Value))
                {
                    bodyContent = BuildGroupedBody(queryResult, groupElement, groupByAttribute.Value);
                }
                else
                {
                    XElement? bodyTemplateElement = bodyElement ?? groupElement;
                    string bodyTemplate = bodyTemplateElement != null ? GetInnerXml(bodyTemplateElement) : string.Empty;

                    if (bodyTemplate.Contains("{") && bodyTemplate.Contains("}"))
                    {
                        bodyContent = BuildSimpleBody(queryResult.Data, bodyTemplate);
                    }
                    else
                    {
                        bodyContent = bodyTemplate;
                    }
                }
            }
            catch
            {
                // Fallback for failed XML parse or if format is Markdown but treated as XML
                bodyContent = BuildSimpleBody(queryResult.Data, template);
            }
        }
        else // MarkdownFormat
        {
            if (template.Contains("{") && template.Contains("}"))
            {
                bodyContent = BuildSimpleBody(queryResult.Data, template);
            }
            else
            {
                bodyContent = template;
            }
        }

        Dictionary<string, object> firstRow = queryResult.Data[0];
        string finalHeader = ReplacePlaceholders(headerTemplate, firstRow);
        string finalFooter = ReplacePlaceholders(footerTemplate, firstRow);

        return new Message
        {
            Header = finalHeader,
            Body = bodyContent.Trim(),
            Footer = finalFooter
        };
    }

    // Extracts the inner XML content of an XElement.
    private string GetInnerXml(XElement element)
    {
        using (System.Xml.XmlReader reader = element.CreateReader())
        {
            reader.MoveToContent();
            return reader.ReadInnerXml();
        }
    }

    // Builds the message body by repeating a template for each data row.
    private string BuildSimpleBody(List<Dictionary<string, object>> dataRows, string bodyTemplate)
    {
        StringBuilder bodyBuilder = new();
        foreach (Dictionary<string, object> row in dataRows)
        {
            bodyBuilder.AppendLine(ReplacePlaceholders(bodyTemplate, row));
        }
        return bodyBuilder.ToString();
    }

    // Builds the message body by grouping data rows.
    private string BuildGroupedBody(QueryResult queryResult, XElement groupElement, string groupingKey)
    {
        string groupHeaderTemplate = groupElement.Element("header") != null
            ? GetInnerXml(groupElement.Element("header"))
            : string.Empty;
        string groupItemTemplate = groupElement.Element("item") != null
            ? GetInnerXml(groupElement.Element("item"))
            : string.Empty;
        string groupFooterTemplate = groupElement.Element("footer") != null
            ? GetInnerXml(groupElement.Element("footer"))
            : string.Empty;

        System.Linq.IGrouping<string, Dictionary<string, object>>[] groupedData = queryResult.Data
            .GroupBy(row =>
            {
                string? key = row.Keys.FirstOrDefault(k => string.Equals(k, groupingKey, StringComparison.OrdinalIgnoreCase));
                return key == null ? null : row[key]?.ToString();
            }, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        // Inject group.count directly into the data rows for each group
        foreach (System.Linq.IGrouping<string, Dictionary<string, object>> group in groupedData)
        {
            int groupCount = group.Count();
            foreach (Dictionary<string, object> row in group)
            {
                // Add the count to each row in the group.
                row["group.count"] = groupCount;
            }
        }

        StringBuilder bodyBuilder = new();
        foreach (System.Linq.IGrouping<string, Dictionary<string, object>> group in groupedData)
        {
            if (!group.Any()) continue;
            Dictionary<string, object> firstRowOfGroup = group.First();

            bodyBuilder.AppendLine(ReplacePlaceholders(groupHeaderTemplate, firstRowOfGroup));
            foreach (Dictionary<string, object> row in group)
            {
                bodyBuilder.AppendLine(ReplacePlaceholders(groupItemTemplate, row));
            }
            bodyBuilder.AppendLine(ReplacePlaceholders(groupFooterTemplate, firstRowOfGroup));
        }
        return bodyBuilder.ToString();
    }

    // Replaces placeholders like {ColumnName} with values from a data row.
    private string ReplacePlaceholders(string text, Dictionary<string, object> row)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return Regex.Replace(text, @"\{(.+?)\}", match =>
        {
            string columnName = match.Groups[1].Value;
            if (row.TryGetValue(columnName, out object? value))
            {
                return value?.ToString() ?? string.Empty;
            }
            return match.Value;
        });
    }
}