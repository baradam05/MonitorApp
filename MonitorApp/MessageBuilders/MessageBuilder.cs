using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MonitorApp.ConnectionServices;
using MonitorApp.JsonParsing.DTO.Notifications;

namespace MonitorApp.MessageBuilders
{
    /// <summary>
    /// Builds notification messages by replacing placeholders in templates with query data.
    /// Handles simple, grouped, and global placeholders.
    /// </summary>
    public class MessageBuilder
    {
        /// <summary>
        /// Main method to construct the final notification string.
        /// </summary>
        public string Build(QueryResult queryResult, NotificationDto notificationDto)
        {
            NotificationBodyDto template = notificationDto.notificationBody;
            bool isTeams = notificationDto is TeamsNotificationsDto;
            Dictionary<string, string> globals = new()
            {
                ["global.time"] = DateTime.Now.ToString("HH:mm:ss"),
                ["global.date"] = DateTime.Now.ToString("yyyy-MM-dd"),
                ["global.datetime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                ["global.count"] = queryResult.Data.Count.ToString()
            };

            Dictionary<string, object>? firstRow = queryResult.Data.FirstOrDefault();

            // Replace placeholders in the subject if it exists, using data from the first row.
            if (notificationDto is EmailNotificationDto emailNotificationDto)
            {
                emailNotificationDto.subject = ReplacePlaceholders(emailNotificationDto.subject, firstRow, globals);
            }

            StringBuilder bodyBuilder = new();
            
            // Build the head, body, and foot sections.
            if (!string.IsNullOrEmpty(template.head))
            {
                string headContent = ReplacePlaceholders(template.head, firstRow, globals);
                AppendWithTeamsFormatting(bodyBuilder, headContent, isTeams);
            }

            // Build body (already handles Teams \n formatting internally per row/group)
            string bodyContent = BuildBody(queryResult, template, globals, isTeams);
            bodyBuilder.Append(bodyContent);

            if (!string.IsNullOrEmpty(template.foot))
            {
                string footContent = ReplacePlaceholders(template.foot, firstRow, globals);
                AppendWithTeamsFormatting(bodyBuilder, footContent, isTeams);
            }

            return bodyBuilder.ToString();
        }

        // Constructs the main body of the message, delegating to grouping logic if needed.
        private string BuildBody(QueryResult queryResult, NotificationBodyDto template, IReadOnlyDictionary<string, string> globals, bool isTeams)
        {
            // If 'groupBy' key is specified -> use the grouping logic.
            if (!string.IsNullOrEmpty(template.groupBy))
            {
                return BuildGroupedBody(queryResult, template, globals, isTeams);
            }

            // Does body contains any data-related placeholders
            ICollection<string>? columnKeys = queryResult.Data.FirstOrDefault()?.Keys;
            bool hasDataPlaceholders = BodyHasDataPlaceholders(template.body, columnKeys);
            
            StringBuilder bodyBuilder = new();

            // If it has data placeholders, iterate through each row to build the body.
            if (hasDataPlaceholders)
            {
                foreach (Dictionary<string, object> row in queryResult.Data)
                {
                    string body = ReplacePlaceholders(template.body, row, globals);
                    AppendWithTeamsFormatting(bodyBuilder, body, isTeams);
                }
            }
            else
            {
                // Otherwise, just process the body once against the first row.
                string firstRow = ReplacePlaceholders(template.body, queryResult.Data.FirstOrDefault(), globals);
                AppendWithTeamsFormatting(bodyBuilder, firstRow, isTeams);
            }
            
            return bodyBuilder.ToString();
        }

        // Constructs a message body by grouping results based on a specified key.
        private string BuildGroupedBody(QueryResult queryResult, NotificationBodyDto template, IReadOnlyDictionary<string, string> globals, bool isTeams)
        {
            StringBuilder groupedBodyBuilder = new();
            IEnumerable<IGrouping<string?, Dictionary<string, object>>> groupedData = queryResult.Data
                .GroupBy(row => row.TryGetValue(template.groupBy, out object? key) ? key?.ToString() : null);

            foreach (IGrouping<string?, Dictionary<string, object>> group in groupedData)
            {
                if (group.Key == null) continue;
                
                // Group-specific placeholders
                Dictionary<string, string> groupGlobals = new()
                {
                    ["group.count"] = group.Count().ToString()
                };
                Dictionary<string, object> firstRowOfGroup = group.First();

                // Build group header
                if (!string.IsNullOrEmpty(template.groupHead))
                {
                    // Fixed: Using template.groupHead and firstRowOfGroup instead of body and global first row
                    string headContent = ReplacePlaceholders(template.groupHead, firstRowOfGroup, globals, groupGlobals);
                    AppendWithTeamsFormatting(groupedBodyBuilder, headContent, isTeams);
                }
                
                // Build each item in the group
                foreach (Dictionary<string, object> row in group)
                {
                    string bodyContent = ReplacePlaceholders(template.body, row, globals, groupGlobals);
                    AppendWithTeamsFormatting(groupedBodyBuilder, bodyContent, isTeams);
                }
                
                // Build group footer
                if (!string.IsNullOrEmpty(template.groupFoot))
                {
                    string footContent = ReplacePlaceholders(template.groupFoot, firstRowOfGroup, globals, groupGlobals);
                    AppendWithTeamsFormatting(groupedBodyBuilder, footContent, isTeams);
                }
            }
            return groupedBodyBuilder.ToString();
        }

        /// Replaces all placeholders in a given string with data from global, group, or row-level dictionaries.
        private string ReplacePlaceholders(string text, IReadOnlyDictionary<string, object>? row, IReadOnlyDictionary<string, string> globals, IReadOnlyDictionary<string, string>? groupGlobals = null)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Find all instances of {placeholder}.
            return Regex.Replace(text, @"\{(.+?)\}", match =>
            {
                string key = match.Groups[1].Value;

                //global placeholders
                if (globals.TryGetValue(key, out string? globalValue))
                {
                    return globalValue;
                }
                
                //group placeholders
                if (groupGlobals != null && groupGlobals.TryGetValue(key, out string? groupGlobalValue))
                {
                    return groupGlobalValue;
                }
                
                //row placeholders
                if (row != null && row.TryGetValue(key, out object? value))
                {
                    return value?.ToString() ?? string.Empty;
                }

                //unmatched
                return match.Value;
            });
        }
        
        // Checks if a template string contains any placeholders that correspond to actual data columns, ignoring global and group placeholders
        private bool BodyHasDataPlaceholders(string bodyTemplate, ICollection<string>? columnKeys)
        {
            if (string.IsNullOrEmpty(bodyTemplate) || columnKeys == null || columnKeys.Count == 0) return false;

            MatchCollection matches = Regex.Matches(bodyTemplate, @"\{(.+?)\}");
            return (from Match match in matches
                let key = match.Groups[1].Value
                where !key.StartsWith("global.") && !key.StartsWith("group.")
                select key).Any(columnKeys.Contains);
        }

        /// <summary>
        /// Helper method to append content to a StringBuilder.
        /// If it's a Teams notification, ensures the content ends with a newline.
        /// </summary>
        private void AppendWithTeamsFormatting(StringBuilder sb, string content, bool isTeams)
        {
            if (string.IsNullOrEmpty(content)) return;

            sb.Append(content);

            if (isTeams && !content.EndsWith("\n"))
            {
                sb.Append('\n');
            }
        }
    }
}