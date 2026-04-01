# MonitorApp

A console application designed to monitor data sources (MSSQL or Elasticsearch) by executing defined queries and sending notifications via Email, SMS, or Microsoft Teams.
- An example of a configuration file can be found in `mock.json`
---

## Getting Started
### First Run
Upon its first execution, if the main configuration file (`_Config/config.json`) is not found, MonitorApp will automatically create a template `config.json` populated with example connections and queries. The application will then exit. User then needs to set up his configuration.

### Running the Application
*   **Standard Run:** To execute all defined queries in `config.json`:
	* you can also build and run the `.exe`
```bash
dotnet run
```
*   **Test Specific Query:** To run only a single, named query for testing purposes:
```bash
dotnet run /test:"YourQueryName"
```
- Replace `"YourQueryName"` with the `name` of a query defined in your `config.json`.
---

## Configuration (`_Config/config.json`)

All application behavior is driven by the `_Config/config.json` file. This file defines your database connections, the queries to be executed, and the notification channels to be used.

The basic structure of `config.json` is:
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
This section defines your connections to various data sources.

#### MSSQL Connection (`"type": "sql"`)
```json
{
  "type": "sql",
  "name": "MyDatabaseConnection",
  "connectionString": "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
}
```
*   `"type"`: Must be `"sql"`.
*   `"name"`: A unique identifier for this connection. Used by queries to reference this connection.
*   `"connectionString"`: (Required) The standard connection string for your MSSQL database.

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
*   `"type"`: Must be `"elastic"`.
*   `"name"`: A unique identifier for this connection.
*   `"uri"`: (Required) The URI of your Elasticsearch instance.
*   `"username"`: (Optional) Username for authentication.
*   `"password"`: (Optional) Password for authentication.
---

### Queries
This section defines the queries MonitorApp will execute. Each query can specify multiple notifications.

#### Query Definition (`"type": "sql"` or `"type": "elastic"`)
These queries are defined directly within `config.json` (or an `inFile` reference).

```json
{
  "type": "sql", // or "elastic"
  "name": "MonitorCriticalErrors",
  "connection": "MyDatabaseConnection", // Name of a defined connection
  "queryText": "SELECT TOP 10 ErrorMessage, Timestamp FROM ErrorLogs WHERE Severity = 'Critical'",
  // "queryLang": "sql", // Optional for Elastic. If omitted, and not SQL, treated as JSON DSL.
  "notifications": [
    // List of notification definitions
  ]
}
```
*   `"type"`: Can be `"sql"` or `"elastic"`, depending on the connection type.
*   `"name"`: A unique name for this query. Used for `dotnet run /test:"QueryName"`.
*   `"connection"`: (Required) The `name` of a connection defined in the `Connections` section.
*   `"queryText"`: (Required) The actual query string. For SQL connections, this is a SQL statement. For Elasticsearch, this can be an Elasticsearch SQL query or a JSON DSL query.
*   `"queryLang"`: (Optional, for Elasticsearch only) Specify `"sql"` if `queryText` contains Elasticsearch SQL. If `queryText` is JSON DSL, omit this property.
*   `"notifications"`: (Required) An array of notification definitions. Each object in this array configures a specific notification type to be sent when this query returns results.

#### Query from File (`"type": "inFile"`)
This allows you to load query definitions from a separate JSON file, useful for organizing complex configurations.

```json
{
  "type": "inFile",
  "name": "LoadQueriesFromFile",
  "path": "AdditionalQueries.json" // Path relative to the _Config directory
}
```
*   `"type"`: Must be `"inFile"`.
*   `"name"`: A unique name for this `inFile` entry.
*   `"path"`: (Required) The filename of the JSON file containing additional queries. This file should be placed in the `_Config` directory. The content of this file should be an array of query definitions, similar to the `Queries` array in the main `config.json`.
    A working example can be found at `mock_sql_selects.json`.

    **Example `AdditionalQueries.json` content:**
```json
    [
      {
        "type": "sql",
        "name": "Sample SQL Query",
        "connection": "MySqlConnection",
        "queryText": "SELECT * FROM users",
        "queryLang": "sql",
        "notifications": [
          {
            "type": "email",
            "name": "SampleEmailNotification",
            "smtpServer": "smtp.example.com",
            "smtpPort": "587",
            "username": "user@example.com",
            "password": "password",
            "fromEmail": "from@example.com",
            "toEmail": "to@example.com",
            "subject": "Inactive Users Alert",
            "useSsl": "true",
            "notificationText": "User found: {username}"
          }
        ]
      },
	   //...
    ]
```
---

### Notifications (within Queries)
Each query have an array of notification objects.

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
  "format": "xml", // or "plaintext"
  "notificationText": "<head><h1>Alert!</h1></head><body><p>Query <b>{QueryName}</b> returned results:</p><group by="Severity"><header><h3>Severity: {Severity}</h3></header><item><p>- {ErrorMessage} at {Timestamp}</p></item><footer><hr/></footer></group></body><footer><small>Generated by MonitorApp</small></footer>"
}
```
*   `"type"`: Must be `"email"`.
*   `"name"`: A unique name for this notification.
*   `"smtpServer"`, `"smtpPort"`, `"fromEmail"`, `"toEmail"`, `"subject"`, `"useSsl"`: (Required) Standard SMTP configuration details.
*   `"username"`, `"password"`: (Optional) Credentials for SMTP authentication. Only required if your SMTP server requires authentication.
*   `"plaintext"`: Treats `notificationText` as a literal string. Placeholders will be replaced with data from each row of the query result. The entire `notificationText` will be repeated for every row returned by the query.
    **Example:** If `notificationText` is `"Error: {ErrorMessage}"` and two errors are returned, the email body will be `Error: Message1\nError: Message2`.
*   `"notificationText"`: (Required) The content of the email. This can be plain text, an XML-like structure for rich HTML, or raw Markdown.

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
*   `"type"`: Must be `"sms"`.
*   `"name"`: A unique name for this notification.
*   `"apiUrl"`, `"accountSid"`, `"authToken"`, `"fromNumber"`, `"toNumber"`: (Required) Configuration for your SMS provider's API.
*   `"notificationText"`: (Required) The content of the SMS message. Placeholders will be replaced.

#### Teams Notification (`"type": "teams"`)
```json
{
  "type": "teams",
  "name": "TeamChannelAlert",
  "webhookUrl": "https://outlook.office.com/webhook/...",
  "format": "xml", // or "markdown", or "plaintext"
  "notificationText": "<head># **Alert: {QueryName}**</head><group by="Severity"><header>## Severity: {Severity}</header><item>- **Alert ID**: {AlertID} | Message: {Message}</item><footer>---</footer></group><footer>End of Alerts.</footer>"
}
```
*   `"type"`: Must be `"teams"`.
*   `"name"`: A unique name for this notification.
*   `"webhookUrl"`: (Required) The Microsoft Teams incoming webhook URL. 
*   `"format"`: (Optional) Specifies the format of `notificationText`.
    *   `"xml"`: Treats `notificationText` as an XML-like structure for Markdown generation and grouping.
    *   `"markdown"`: Treats `notificationText` as a raw Markdown string. Placeholders will be replaced with data from each row of the query result.
    *   `"plaintext"`: Treats `notificationText` as a literal string. Placeholders will be replaced with data from each row of the query result. The entire `notificationText` will be repeated for every row returned by the query.
        **Example:** If `notificationText` is `"Alert: {AlertID}"` and two alerts are returned, the Teams message body will be `Alert: 123\nAlert: 456`.
*   `"notificationText"`: (Required) The content for the Teams message. This can be plain text, a raw Markdown string, or an XML-like structure combining Markdown with grouping.
---

## Advanced Notification Text Formatting
MonitorApp supports powerful templating for `notificationText` to generate dynamic and structured messages.

### Placeholders
You can embed placeholders directly into your `notificationText` using curly braces `{ColumnName}`. These will be automatically replaced with values from your query results.

**Example:** If your query returns a column named `ErrorMessage`, you can use `{ErrorMessage}` in your `notificationText`.
```
"notificationText": "Error in system: {ErrorMessage} at {Timestamp}"
```

### XML-like Formatting (for `Email` and `Teams` with `"format": "xml"`)
For rich, structured messages, you can use an XML-like syntax within your `notificationText`. The system will automatically parse these tags to create a formatted output (HTML for Email, Markdown for Teams).

**Supported Tags:**
*   `<head>...</head>`: Content for the message header. Rendered once.
*   `<body>...</body>`: Content for the main body. If it contains placeholders, it will repeat for each row in the query result. If it does not contain placeholders, it will render once as static content.
*   `<footer>...</footer>`: Content for the message footer. Rendered once.
*   `<group by="ColumnName">...</group>`: This tag allows you to group query results by a specified column and define templates for the group header, individual items, and group footer.

#### Grouping (`<group by="ColumnName">`)
When `format` is `"xml"` and you use the `<group>` tag, MonitorApp will group your query results by the value of the column specified in the `by` attribute.

**Sub-tags within `<group>`:**
*   `<header>...</header>`: Content for the header of each group. Rendered once per unique group. Placeholders here will be filled using the data from the *first item* in that group.
*   `<item>...</item>`: Content for each individual item within a group. This template will repeat for every row belonging to that group.
*   `<footer>...</footer>`: Content for the footer of each group. Rendered once per unique group. Placeholders here will also be filled using data from the *first item* in that group.

**Example (Email HTML with grouping):**
```xml
<head><h1>Server Issues Report</h1></head>
<group by="ServerName">
  <header><h2>Server: {ServerName}</h2><table border="1"><thead><tr><th>Error</th><th>Time</th></tr></thead><tbody></header>
  <item><tr><td>{ErrorMessage}</td><td>{Timestamp}</td></tr></item>
  <footer></tbody></table></footer>
</group>
<footer><hr/><p>Report Generated.</p></footer>
```

**Example (Teams Markdown with grouping):**

```xml
<head># Critical Alerts Summary</head>
<group by="Severity">
  <header>## Severity: {Severity}</header>
  <item>- **Alert ID**: {AlertID} | Message: {Message}</item>
  <footer>---</footer>
</group>
<footer>End of Alerts.</footer>
```

---

## Developer Notes
The application uses C# and .NET 8.0.
Configuration is deserialized using `System.Text.Json` with `JsonPolymorphic` attributes for handling different DTO types.
Connection services implement `IConnectionService`, and notification services implement `INotificationService`.
Message generation is handled by the `MessageBuilder` and `IMessageRenderer` pattern.
