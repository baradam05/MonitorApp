# MonitorApp

MonitorApp is a .NET console application for monitoring data sources like MSSQL and Elasticsearch. It executes user-defined queries and uses templated notifications through various channels, including Email, Microsoft Teams, and SMS, **when query results are found**.

An example of a complete configuration file can be found in `MockFiles/`.
___
## Getting Started

### First Run
On its first run, MonitorApp looks for a configuration file at `_Config/config.json` (relative to its execution directory). If this file doesn't exist, the application will notify you, create it and exit.

### Running the Application

*   **Standard Run:** To execute all queries defined in `config.json`:
```bash
dotnet run
```
Alternatively, build the project and run the executable.

*   **Test a Specific Query:** To run a single named query for testing or debugging:
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
*   `"type"`: **(Required)** Must be `"sql"`.
*   `"name"`: **(Required)** A unique name to identify this connection. This is referenced by your queries.
*   `"connectionString"`: **(Required)** The standard connection string for your MSSQL database.

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
*   `"type"`: **(Required)** Must be `"elastic"`.
*   `"name"`: **(Required)** A unique name for this connection.
*   `"uri"`: **(Required)** The URI of your Elasticsearch instance.
*   `"username"`: (Optional) Username for basic authentication.
*   `"password"`: (Optional) Password for basic authentication.

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
  "queryText": "SELECT * FROM ErrorLogs WHERE Severity = 'Critical'",
  "notifications": [
    // Array of notification definitions
  ]
}
```
*   `"type"`: **(Required)** Must be `"sql"` or `"elastic"`.
*   `"name"`: **(Required)** A unique name for the query. Essential for testing via the `/test` argument.
*   `"connection"`: **(Required)** The `name` of a connection defined in the `Connections` section.
*   `"queryText"`: **(Required)** The query to execute. For SQL, this is a standard SQL statement. For Elasticsearch, this can be an Elasticsearch SQL query or a JSON DSL query.
*   `"notifications"`: **(Required)** An array of one or more notification objects to be triggered if the query returns results.
##### Elasticsearch only
*   `"queryLang"`: (Optional) Specify `"sql"` if `queryText` contains an Elasticsearch SQL query. If omitted, `queryText` is treated as JSON DSL.
*   `"index"`: (Required for JSON DSL, optional for ES-SQL) The default index to run the query against. If `queryLang` is not `"sql"`, this field is mandatory.

#### Query from File (`"type": "inFile"`)
For better organization, you can load queries from an external JSON file.

```json
{
  "type": "inFile",
  "name": "LoadQueriesFromExternalFile",
  "path": "AdditionalQueries.json" // Path relative to the _Config directory
}
```
*   `"type"`: **(Required)** Must be `"inFile"`.
*   `"name"`: **(Required)** A unique name for this `inFile` entry.
*   `"path"`: **(Required)** The filename of the JSON file containing more queries. 
	* This file **must be located in the `_Config` directory**.
	* Its content should be a JSON array of query objects. 
		* An example is provided in `MockInFile.json`.

___
### Notifications
Defined within a query's `"notifications"` array, these objects specify how and where to send alerts.

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
  "subject": "MonitorApp Alert: {QueryName} results",
  "useSsl": "true",
  "format": "xml",
  "notificationText": "..."
}
```
*   `"format"`: (Optional) Defines the format of `notificationText`.
    *   `"plaintext"`: (Default) Text is sent as-is.
    *   `"xml"`: Enables rich HTML generation using an XML-like structure. Allows placeholders
*   `"notificationText"`: **(Required)** The email body. Can be plaintext or an XML structure for HTML. See advanced formatting below.

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
  "notificationText": "MonitorApp Alert for {QueryName}: {ErrorMessage} at {Timestamp}"
}
```
* This notification type only supports plaintext messages. 
* Also allows placeholder replacement.

#### Teams Notification (`"type": "teams"`)
```json
{
  "type": "teams",
  "name": "TeamChannelAlert",
  "webhookUrl": "https://outlook.office.com/webhook/...",
  "format": "markdown",
  "notificationText": "..."
}
```
*   `"webhookUrl"`: **(Required)** The Microsoft Teams incoming webhook URL.
*   `"format"`: (Optional) Specifies how to interpret `notificationText`.
    *   `"plaintext"`: Text is sent as-is.
    *   `"markdown"`: Treats the text as a raw Markdown string. Use `\n` for line breaks.
    *   `"xml"`: Enables Markdown generation using an XML-like structure, supporting grouping and complex layouts.
*   `"notificationText"`: **(Required)** The message content. Can be plaintext, raw Markdown, or an XML structure for advanced Markdown. See advanced formatting below.

___
## Advanced Notification Formatting

MonitorApp’s templating allows you to create dynamic, data-driven messages.

### Placeholders
Any column name from your query result can be used as a placeholder by wrapping it in curly braces: `{ColumnName}`.

**Example:**
If your query returns rows with an `ErrorMessage` column, you can use:
`"notificationText": "Error found: {ErrorMessage}"`

### Advanced Placeholders
Special placeholders provide metadata about the query results.

*   `{global.count}`: Returns the **total number of rows** returned by the query. This can be used anywhere in the template.
*   `{group.count}`: Only available within a `<group>` block (see XML formatting). It returns the **number of items within the current group**.

**Example (using `global.count`):**
```xml
<head>
  <h1>Critical Errors Report</h1>
  <p>Total errors found: {global.count}</p>
</head>
```

### XML-like Formatting (`"format": "xml"`)
This formatting option is available for both **Email (HTML)** and **Teams (Markdown)**. It allows for structured layouts with headers, footers, and result grouping.

The engine build's it into a working HTML or  Adaptive card's format.

**Core Tags:**
*   `<head>...</head>`: Renders once at the beginning of the message.
*   `<body>...</body>`: Defines the template for each result row if it contains placeholders. If not, it renders once as static content.
*   `<footer>...</footer>`: Renders once at the end of the message.
*   `<group by="ColumnName">...</group>`: The key feature for structured reports. It groups query results by a specified column.

#### Grouping with `<group>`
The `<group>` tag organizes results into logical sections. It must contain `<header>`, `<item>`, and `<footer>` tags.

*   `<header>...</header>`: Template for the beginning of each new group. You can use placeholders here, including `{ColumnName}` (for the value being grouped by) and `{group.count}`.
*   `<item>...</item>`: Template for each individual row within a group.
*   `<footer>...</footer>`: Template for the end of each group.

**Example: Email (HTML) with Grouping**
This template groups alerts by `ServerName` and puts them in a table.

```xml
<head><h1>Server Issues Report ({global.count} total)</h1></head>
<group by="ServerName">
  <header>
    <h2>Server: {ServerName} ({group.count} issues)</h2>
    <table border="1">
      <thead><tr><th>Error</th><th>Time</th></tr></thead>
      <tbody>
  </header>
  <item>
    <tr><td>{ErrorMessage}</td><td>{Timestamp}</td></tr>
  </item>
  <footer>
      </tbody>
    </table>
  </footer>
</group>
<footer><hr/><p>Report Generated by MonitorApp.</p></footer>
```

**Example: Teams (Markdown) with Grouping**
This template groups alerts by `Severity`, using Markdown formatting.

```xml
<head># Critical Alerts Summary ({global.count} total)</head>
<group by="Severity">
  <header>## Severity: {Severity} ({group.count} alerts)</header>
  <item>- **Alert ID**: {AlertID} | Message: {Message}</item>
  <footer>---</footer>
</group>
<footer>End of Alerts.</footer>
```
___
## Developer Notes
*   **Technology**: Built with C# and .NET 8.0.
*   **Configuration Parsing**: Uses `System.Text.Json` with `JsonPolymorphic` attributes to handle different DTO types for connections, queries, and notifications.
*   **Extensibility**: Designed with interfaces like `IConnectionService` and `INotificationService` to allow for future expansion to other data sources and notification channels.
*   **Message Generation**: The `MessageBuilder` class, along with the `IMessageRenderer` pattern (`HtmlMessageRenderer`, `MarkdownMessageRenderer`), handles the transformation of data and templates into final message content.
