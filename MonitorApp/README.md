# MonitorApp

## Interactive UI

When launched without command-line arguments, MonitorApp starts in an interactive UI mode. This provides a text-based menu for easier management and execution of monitoring tasks.

Use **↑** and **↓** arrow keys to navigate the menu, and **Enter** to select an option. Press **Esc** to exit the application.

### Menu Options

-   **Show Connections**: Displays a list of all data source connections (`SQL` and `Elasticsearch`) defined in `config.json`.
-   **Show Queries**: Shows a detailed view of all configured queries. For each query, it lists its target connection, query text, and the configured notifications (SMS, Email, Teams).
-   **Edit config.json**: Opens the `_Config/config.json` file in your system's default text editor.
-   **Test Query**: Prompts you to enter the name of a single query to execute it immediately. This is useful for debugging a specific query.
-   **Run All Queries**: Triggers an execution of all queries defined in the configuration.
-   **Exit**: Closes the application.

___

MonitorApp is a .NET console application for monitoring data sources like MSSQL and Elasticsearch. It executes user-defined queries and uses templated notifications through various channels, including Email, Microsoft Teams, and SMS, **when query results are found**.

An example of a complete configuration file can be found in `MockFiles/`.
___
## Getting Started

### First Run
On its first run, MonitorApp looks for a configuration file at `_Config/config.json` (relative to its execution directory). If this file doesn't exist, the application will notify you, create it and exit.

### Running the Application

-   **Standard Run:** To execute all queries defined in `config.json`:
```bash
dotnet run
```
Alternatively, build the project and run the executable.

-   **Test a Specific Query:** To run a single named query for testing or debugging:
```bash
dotnet run /test:"YourQueryName"
```
Replace `"YourQueryName"` with the exact `name` of a query from your configuration.

___
## Configuration (`_Config/config.json`)

All application behavior is driven by the `_Config/config.json` file. This file is the single source for database connections, queries, and notification settings.

The basic structure is a JSON object with two main keys:
```json
{
  "Connections": [
    // Define your database connections here
  ],
  "Queries": [
    // Define your queries here
  ]
}
```

### Connections
This array defines the connection details for your data sources.

#### MSSQL Connection (`"type": "sql"`)
```json
{
  "type": "sql",
  "name": "MyDatabaseConnection",
  "connectionString": "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
}
```
-   `"type"`: **(Required)** Must be `"sql"`.
-   `"name"`: **(Required)** A unique name to identify this connection. This is referenced by your queries.
-   `"connectionString"`: **(Required)** The standard connection string for your MSSQL database.

#### Elasticsearch Connection (`"type": "elastic"`)
```json
{
  "type": "elastic",
  "name": "MyElasticConnection",
  "uri": "http://localhost:9200",
  "username": "elastic",
  "password": "changeme"
}
```
-   `"type"`: **(Required)** Must be `"elastic"`.
-   `"name"`: **(Required)** A unique name for this connection.
-   `"uri"`: **(Required)** The URI of your Elasticsearch instance.
-   `"username"`: (Optional) Username for basic authentication.
-   `"password"`: (Optional) Password for basic authentication.

___
### Queries
This array defines the monitoring queries MonitorApp will execute.

#### Query Definition (`"type": "sql"` or `"type": "elastic"`)
These queries are defined directly within `config.json` or an `inFile` reference.

```json
{
  "type": "sql", // or "elastic"
  "name": "MonitorCriticalErrors",
  "connection": "MyDatabaseConnection", // Name of a defined connection
  "queryText": "SELECT - FROM ErrorLogs WHERE Severity = 'Critical'",
  "notifications": [
    // Array of notification definitions
  ]
}
```
-   `"type"`: **(Required)** Must be `"sql"` or `"elastic"`.
-   `"name"`: **(Required)** A unique name for the query. Essential for testing via the `/test` argument.
-   `"connection"`: **(Required)** The `name` of a connection defined in the `Connections` section.
-   `"queryText"`: **(Required)** The query to execute. For SQL, this is a standard SQL statement. For Elasticsearch, this can be an Elasticsearch SQL query or a JSON DSL query.
-   `"notifications"`: **(Required)** An array of one or more notification objects to be triggered if the query returns results.
##### Elasticsearch only
-   `"queryLang"`: (Optional) Specify `"sql"` if `queryText` contains an Elasticsearch SQL query. If omitted, `queryText` is treated as JSON DSL.
-   `"index"`: (Required for JSON DSL, optional for ES-SQL) The default index to run the query against. If `queryLang` is not `"sql"`, this field is mandatory.

#### Query from File (`"type": "inFile"`)
For better organization, you can load queries from an external JSON file.

```json
{
  "type": "inFile",
  "name": "LoadQueriesFromExternalFile",
  "path": "AdditionalQueries.json" // Path relative to the _Config directory
}
```
-   `"type"`: **(Required)** Must be `"inFile"`.
-   `"name"`: **(Required)** A unique name for this `inFile` entry.
-   `"path"`: **(Required)** The filename of the JSON file containing more queries. 
	- This file **must be located in the `_Config` directory**.
	- Its content should be a JSON array of query objects. 
		- An example is provided in `MockInFile.json`.

___
### Notifications
Defined within a query's `"notifications"` array, these objects specify how and where to send alerts. All notification types share a common structure for defining the message content, contained within the `notificationBody` object.

---
#### Notification Body Structure
The `notificationBody` object is a flexible container for your message template.

```json
"notificationBody": {
  "head": "Optional header",
  "body": "Required body",
  "foot": "Optional footer",
}
```
- For simple messages, you only need to provide the `body` field.
- You can group your query - for more below
#### Email Notification (`"type": "email"`)
```json
{
  "type": "email",
  "name": "AdminEmailNotification",
  "smtpServer": "smtp.example.com",
  "smtpPort": "587",
  "username": "user@example.com",
  "password": "your_password",
  "fromEmail": "monitor@example.com",
  "toEmail": "admin@example.com",
  "subject": "MonitorApp Alert: Found {global.count} issues",
  "useSsl": "true",
  "notificationBody": {
    "body": "<p>Found an issue: {ErrorMessage} on server {ServerName}.<\p>"
  }
}
```
- The `subject` can also contain placeholders, which will be populated from the first row of data.
- For body use html elements
#### SMS Notification (`"type": "sms"`)
```json
{
  "type": "sms",
  "name": "OnCallSmsAlert",
  "apiUrl": "https://api.sms_provider.com/send",
  "accountSid": "YOUR_SID",
  "authToken": "YOUR_TOKEN",
  "fromNumber": "+1234567890",
  "toNumber": "+1987654321",
  "notificationBody": {
    "body": "MonitorApp Alert for {QueryName}: {ErrorMessage} at {Timestamp}"
  }
}
```
-   For SMS only use "body" as it doesn't support html nor markdown formatting

#### Teams Notification (`"type": "teams"`)
```json
{
  "type": "teams",
  "name": "TeamChannelAlert",
  "webhookUrl": "https://outlook.office.com/webhook/...",
  "notificationBody": {
    "head": "# Critical System Alert!",
    "body": "- **Service**: {service.name}\\n- **Error**: {error.message}"
  }
}
```
- Content is treated as Markdown. 
- The message builder automatically handles newlines between sections, but you can include your own (`\n`) for finer control.
- Headers are limited to `# ` and `## `

___
## Advanced Notification Formatting

MonitorApp’s templating allows you to create dynamic, data-driven messages using the `notificationBody` object.

### Placeholders
Any column name from your query result can be used as a placeholder by wrapping it in curly braces: `{ColumnName}`.

**Example:**
If your query returns rows with an `ErrorMessage` column, you can use:
```json
"notificationBody": {
  "body": "Error found: {ErrorMessage}"
}
```

### Advanced Placeholders
Special placeholders provide metadata about the query results.

- **Global placeholders** - can be used in `head`, `body`, `foot`, `groupHead`, `groupFoot` and in `Subject` if email.
	- `{global.count}`: **total number of rows** returned by the query.
	- `{global.time}`: **current time**. (*HH:mm:ss*)
	- `{global.date}`: **current date**. (*yyyy-MM-dd*)
	- `{global.datetime}`: **current date and time**. (*yyyy-MM-dd HH:mm:ss*)
	
- **Group placeholders** - Only available when using `groupBy`. Can be used in `groupHead`, `body` and `groupFoot`
	- `{group.count}`: It returns the **number of items within the current group**.

**Example (using `global.count`):**
```json
"notificationBody": {
  "head": "<h1>Critical Errors Report</h1><p>Total errors found: {global.count}</p>",
  "body": "<p>Error: {ErrorMessage}</p>"
}
```

### Grouping Query Results
The `groupBy` feature is the key to creating structured reports. It organizes query results into logical sections based on the values in a specified column.

```json
{
  "notificationBody": {
    "groupBy": "Region", //Column
    
    "head": "# Message haed",
    "groupHead": "## Region: {{Region}}",
    "body": " - Store ID: {{StoreID}} (Sales: {{Total}})",
    "groupFoot": "---",
    "foot": "Global report finished."
  }
}
```

When using `groupBy`, the template sections are rendered in this order:
1.  `head` (once)
2.  For each group:
    1.  `groupHead`
    2.  `body` (for each item in the group)
    3.  `groupFoot`
3.  `foot` (once)

**Example: Email (HTML) with Grouping**
This template groups alerts by `ServerName` and formats the output as an HTML table.

```json
"notificationBody": {
  "head": "<h1>Server Issues Report ({global.count} total)</h1>",
  "groupBy": "ServerName",
  "groupHead": "<h2>Server: {ServerName} ({group.count} issues)</h2><table border='1'><thead><tr><th>Error</th><th>Time</th></tr></thead><tbody>",
  "body": "<tr><td>{ErrorMessage}</td><td>{Timestamp}</td></tr>",
  "groupFoot": "</tbody></table>",
  "foot": "<hr/><p>Report Generated by MonitorApp.</p>"
}
```

**Example: Teams (Markdown) with Grouping**
This template groups alerts by `Severity`, using Markdown formatting for a Teams message.

```json
"notificationBody": {
  "head": "# Critical Alerts Summary ({global.count} total)",
  "groupBy": "Severity",
  "groupHead": "## Severity: {Severity} ({group.count} alerts)",
  "body": "- **Alert ID**: {AlertID} | Message: {Message}",
  "groupFoot": "---",
  "foot": "End of Alerts."
}
```
___
## Developer Notes
-   **Technology**: Built with C# and .NET 8.0.
-   **Configuration Parsing**: Uses `System.Text.Json` with `JsonPolymorphic` attributes to handle different DTO types for connections, queries, and notifications.
-   **Extensibility**: Designed with interfaces like `IConnectionService` and `INotificationService` to allow for future expansion to other data sources and notification channels.
-   **Message Generation**: The `MessageBuilder` class, along with the `IMessageRenderer` pattern (`HtmlMessageRenderer`, `MarkdownMessageRenderer`), handles the transformation of data and templates into final message content.
