# EnviroMonitor

## Dependencies
### nuget
- `Microsoft.EntityFrameworkCore` - For database context

### Setting up the database
Ensure the following dependencies are installed:
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.Extensions.Configuration.Json

Ensure Docker and Azure Data Studio are both installed.

Run the following from the command line:
```bash
docker pull mcr.microsoft.com/mssql/server:2022-latest
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=[Password]" -p 1433:1433 --name sql1 --hostname sql1 -d mcr.microsoft.com/mssql/server:2022-latest
```

To connect to the database through Azure Studio, create a new connection with the following details:
- Server: localhost
- Authentication Type: SQL Login
- Username: sa
- Password: [Password]
- Trust Server Certificate: True

Or use the following connection string:
```
Server=localhost;Database=EnviroMonitorDB;User Id=sa;Password=[Password];TrustServerCertificate=True;
```

## Configurations
### `appsettings.json`
`appsettings.json` will contain the production configuration. Currently within this repo it is a template. Ensure you do not push sensitive data to the repo in this file.
`appsettings.development.json` is a gitignored file for development. Create a copy of `appsettings.json`, rename it and replace the variables inside to match your environment (database password, server IP, etc.)
- Run your IDE in DEBUG mode to use this configuration.
- Run your IDE in RELEASE mode to use the `appsettings.json` configuration
  - Ensure you have the correct database password in the `appsettings.json` file before running in RELEASE mode.
  - Do not push this file to the repo if you make any changes.