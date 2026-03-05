# Monitor app
Konzolová aplikace bez UI. Aplikace dělá dotazy na databáze - MSSQL nebo ElasticSearch a zasílá notifikace na Email, SMS a Teams.

**Note:** You can find an example of a full configuration file in `mock.json`.

**First Run:** If the configuration file `_Config/config.json` does not exist, the application will create a template file for you and then exit. You will need to fill in the details in this new file before running the application again.

# Vstup
Uživatel si vše nastaví v `config.json`, ten je ve formátu:
```json
{
	"Connections": [ ],
	"Notifications": [ ],
	"Queries": [ ]
}    
```
- `"Connections"` je pole databází na které budou prováděný dotazy.
- `"Notifications"` je pole různých notifikací.
- `"Queries"` je pole dotazů.

## Connections
V této sekci se definují připojení k databázím.

### MSSQL
Pro připojení k MSSQL databázi se používá `type: "sql"`.
```json
{
  "type": "sql",
  "name": "SQLConnection",
  "connectionString": "Server=YOURSERVER;Database=YOURDB;User Id=USERID;Password=YOURPASSWORD;TrustServerCertificate=True;"  
}
```
- `"connectionString"` je povinný string ve standardním formátu.
- `"name"` je uživatelem vybraný klíč. **NESMÍ** existovat 2 a více připojení se stejným jménem.

### ElasticSearch
Pro připojení k Elasticsearch se používá `type: "elastic"`.
```json
    {
      "type": "elastic",
      "name": "ElasticSearchConnection",
      "uri": "elasticsearch_link",
      "username": "your_username",
      "password": "your_password",
      "defaultIndex": "logs"
    }
```
- `"uri"`: adresa Elasticsearch serveru.
- `"username"`: (volitelné) uživatelské jméno pro připojení.
- `"password"`: (volitelné) heslo pro připojení.
- `"defaultIndex"`: výchozí index, který se má použít.
- `"name"` je uživatelem vybraný klíč. **NESMÍ** existovat 2 a více připojení se stejným jménem.

## Notifications
V této sekci se definují notifikační kanály.

### Email
Pro zasílání emailových notifikací se používá `type: "email"`.
```json
    {
      "type": "email",
      "name": "EmailNotification",
      "smtpServer": "smtp.example.com",
      "smtpPort": "123",
      "username": "your_email@example.com",
      "password": "your_password",
      "fromEmail": "your_email@example.com",
      "toEmail": "recipient@example.com",
      "subject": "MonitorApp Notification",
      "useSsl": "true"
    }
```
- `"name"`: unikátní název notifikace.
- `"smtpServer"`: adresa SMTP serveru.
- `"smtpPort"`: port SMTP serveru.
- `"username"`: (volitelné) uživatelské jméno pro SMTP server.
- `"password"`: (volitelné) heslo pro SMTP server.
- `"fromEmail"`: emailová adresa odesílatele.
- `"toEmail"`: emailová adresa příjemce.
- `"subject"`: předmět emailu.
- `"useSsl"`: `true` pokud se má použít SSL, jinak `false`.

### SMS
Pro zasílání SMS notifikací se používá `type: "sms"`.
```json
    {
      "type": "sms",
      "name": "SMSNotification",
      "apiUrl": "https://api.sms_provider.com/sendsms",
      "accountSid": "your_account_sid",
      "authToken": "your_auth_token",
      "fromNumber": "+1234567890",
      "toNumber": "+0987654321"
    }
```
- `"name"`: unikátní název notifikace.
- `"apiUrl"`: URL API pro odesílání SMS.
- `"accountSid"`: SID účtu vašeho SMS providera.
- `"authToken"`: autentizační token.
- `"fromNumber"`: telefonní číslo odesílatele.
- `"toNumber"`: telefonní číslo příjemce.

### Teams
Pro zasílání notifikací do Microsoft Teams se používá `type: "teams"`.
```json
    {
      "type": "teams",
      "name": "TeamsNotification",
      "webhookUrl": "https://your_webhook_url"
    }
```
- `"name"`: unikátní název notifikace.
- `"webhookUrl"`: URL webhooku pro kanál v Teams.

## Queries
V této sekci se definují samotné dotazy.

Existují dva typy dotazů: `query` pro přímé dotazy a `inFile` pro dotazy definované v samostatném souboru.

### Přímý dotaz
```json
    {
      "type": "query",
      "name": "High CPU Usage",
      "connection": "SQLConnectionName",
      "queryText": "SELECT * FROM Processes WHERE CPU > 90",
      "queryLang": "sql",
      "notificationText": "High CPU usage detected!",
      "notifications": ["EmailNotification", "TeamsNotification"]
    }
```
- `"type"`: `query`
- `"name"`: unikátní název dotazu.
- `"connection"`: název existujícího připojení z sekce `Connections`.
- `"queryText"`: text dotazu (např. SQL dotaz nebo Elasticsearch query).
- `"queryLang"`: (volitelné) jazyk dotazu, např. `sql`.
	- pokud nechcete dělat SQL dotaz, ale dotaz ve formátu JSON celý řádek odeberte
- `"notificationText"`: text, který bude odeslán v notifikaci.
- `"notifications"`: pole názvů notifikací, které se mají použít.
	- z sekce `Notifications`

### Dotaz ze souboru (`inFile`)
Tento typ umožňuje načíst dotazy z externího souboru.
```json
    {
      "type": "inFile",
      "name": "FileQueries",
      "path": "SQLSelects.json"
    }
```
- `"type"`: `inFile`
- `"name"`: unikátní název.
- `"path"`: cesta k souboru obsahujícímu dotazy.
	- cesta už automaticky bere složku `_Config` - stačí název json souboru
