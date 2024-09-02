## Blazor Identity

The Visual Studio project template with Authentication type: `Individual Accounts` selected will default to using the SQL Server Entity Framework, however this project was created using the .NET CLI which defaults to using the SQLite version.\
`dotnet new blazor --output BlazorIdentity --auth Individual --no-restore`

To build the Identity database uncomment the package reference and use the `dotnet-ef` global tool:

```powershell
cd src\BlazorIdentity\Data
dotnet ef database update --project ..\BlazorIdentity.csproj --connection "Data Source=Data\app.db"
dotnet sln add src\BlazorIdentity\BlazorIdentity.csproj
```
Once the database has been built the package reference can be re-commented out. It is only required to apply the migration.

See also:\
[What’s new with identity in .NET 8](https://devblogs.microsoft.com/dotnet/whats-new-with-identity-in-dotnet-8/#the-blazor-identity-ui)\
[Secure ASP.NET Core server-side Blazor apps](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/server)\
[Entity Framework Core tools reference - .NET Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
