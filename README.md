# opisense-sample-dotnet-console
A simple Opisense API Client in a .Net C# Console

# Setup
1. Contact [Opisense support team](mailto:support@opinum.com) for the setup of the application and the user you would like to use

2. Create a file named appSettings.config next to the App.config file with the following structure:
```xml
  <?xml version="1.0" encoding="utf-8"?>
  <appSettings>
    <add key="OpisenseApi" value="https://api.opinum.com/"/>
    <add key="OpisenseIdentity" value="https://identity.opinum.com/"/>
    <add key="OpisensePush" value="https://push.opinum.com/api/"/>
    <add key="OpisenseUsername" value="[YOUR USERNAME]"/>
    <add key="OpisensePassword" value="[PASSWORD OF THE USER]"/>
    <add key="OpisenseClientId" value="[CLIENT ID]"/>
    <add key="OpisenseClientSecret" value="[CLIENT SECRET]"/>
    <add key="DefaultTimezone" value="Romance Standard Time"/>
  </appSettings>
```
