using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MonitorApp
{
    public class MessageGenerator
    {
        public string Generate(string template, List<Dictionary<string, object>> data)
        {
            var wrappedTemplate = $"<root>{template}</root>";
            var doc = XDocument.Parse(wrappedTemplate);
            var root = doc.Root;
            if (root == null) return string.Empty;

            var groupElement = root.Element("group");

            var resultBuilder = new StringBuilder();
            resultBuilder.AppendLine("<html><body>");

            if (groupElement != null)
            {
                resultBuilder.Append(GenerateGroupedMessage(root, groupElement, data));
            }
            else
            {
                resultBuilder.Append(GenerateSimpleMessage(root, data));
            }

            resultBuilder.AppendLine("</body></html>");
            return resultBuilder.ToString();
        }

        private string GenerateSimpleMessage(XElement root, List<Dictionary<string, object>> data)
        {
            var sb = new StringBuilder();
            var header = GetInnerXml(root.Element("head"));
            var bodyTemplate = GetInnerXml(root.Element("body"));
            var footer = GetInnerXml(root.Element("footer"));

            if (!string.IsNullOrEmpty(header))
            {
                sb.Append($"<div class=\"head\">{header}</div>");
            }

            var bodyContent = new StringBuilder();
            bool hasPlaceholders = Regex.IsMatch(bodyTemplate, @"\{[^{}]+\}");

            if (hasPlaceholders && data.Any())
            {
                foreach (var row in data)
                {
                    bodyContent.Append(ReplacePlaceholders(bodyTemplate, row));
                }
            }
            else
            {
                bodyContent.Append(bodyTemplate);
            }

            if (bodyContent.Length > 0)
            {
                sb.Append($"<div class=\"body\">{bodyContent.ToString()}</div>");
            }

            if (!string.IsNullOrEmpty(footer))
            {
                sb.Append($"<div class=\"footer\">{footer}</div>");
            }
            return sb.ToString();
        }

        private string GenerateGroupedMessage(XElement root, XElement groupElement, List<Dictionary<string, object>> data)
        {
            var sb = new StringBuilder();
            var groupingColumn = groupElement.Attribute("by")?.Value;
            if (string.IsNullOrEmpty(groupingColumn))
            {
                return "Template Error: 'by' attribute is missing in <group> tag.";
            }

            var globalHeader = GetInnerXml(root.Element("head"));
            var globalFooter = GetInnerXml(root.Element("footer"));
            var groupHeaderTemplate = GetInnerXml(groupElement.Element("groupHeader"));
            var groupBodyTemplate = GetInnerXml(groupElement.Element("body"));
            var groupFooterTemplate = GetInnerXml(groupElement.Element("groupFooter"));

            if (!string.IsNullOrEmpty(globalHeader))
            {
                sb.Append($"<div class=\"head\">{globalHeader}</div>");
            }

            var groupedData = data.GroupBy(row => row.ContainsKey(groupingColumn) ? row[groupingColumn]?.ToString() : "Unknown");

            foreach (var group in groupedData)
            {
                var groupData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) {{ groupingColumn, group.Key ?? "Unknown" }};

                if (!string.IsNullOrEmpty(groupHeaderTemplate))
                {
                    sb.Append($"<div class=\"group-header\">{ReplacePlaceholders(groupHeaderTemplate, groupData)}</div>");
                }

                var bodyContent = new StringBuilder();
                foreach (var row in group)
                {
                    bodyContent.Append(ReplacePlaceholders(groupBodyTemplate, row));
                }
                if (bodyContent.Length > 0)
                {
                    sb.Append($"<div class=\"body\">{bodyContent.ToString()}</div>");
                }

                if (!string.IsNullOrEmpty(groupFooterTemplate))
                {
                    sb.Append($"<div class=\"group-footer\">{ReplacePlaceholders(groupFooterTemplate, groupData)}</div>");
                }
            }

            if (!string.IsNullOrEmpty(globalFooter))
            {
                sb.Append($"<div class=\"footer\">{globalFooter}</div>");
            }
            return sb.ToString();
        }

        private string ReplacePlaceholders(string template, Dictionary<string, object> data)
        {
            if (string.IsNullOrEmpty(template)) return "";
            
            return Regex.Replace(template, @"\{([^}]+)\}", match =>
            {
                string key = match.Groups[1].Value;
                if (data.TryGetValue(key, out object value))
                {
                    return value?.ToString() ?? "";
                }
                return match.Value; 
            }, RegexOptions.IgnoreCase);
        }

        private string GetInnerXml(XElement? element)
        {
            if (element == null) return string.Empty;
            var reader = element.CreateReader();
            reader.MoveToContent();
            return reader.ReadInnerXml();
        }
    }
}
