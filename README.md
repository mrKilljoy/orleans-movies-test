# orleans-movies-test

This repository contains my test project featuring technologies like ASP.NET Core (Web API), Orleans and GraphQL.

### Setup
1) Decide which grain storage provider you want to use. By default, "in-memory" storage model is used.
However, if you need to create a persistent storage where your data may be stored between runs, you should change storage provider to ADO.NET (see example below):

```
.UseOrleans((ctx, builder) =>
{
    builder
    .UseAdoNetPersistenceConfiguration(new AppSiloBuilderContext
	{
		AppInfo = appInfo,
		HostBuilderContext = ctx,
		SiloOptions = new AppSiloOptions
		{
			SiloPort = GetAvailablePort(11111, 12000),
			GatewayPort = 30001
		}
	}, new AdoNetPersistenceProviderOptions
	{
		Invariant = "System.Data.SqlClient",
		ConnectionString = "*", // put correct connection string here
		UseJson = true
	})
```
2) In case ADO.NET storage provider was selected, you will need to execute `StorageInitialization.sql` script (can be found in "tools" folder).
3) As a final step, you may want to run `PopulateStorage.sh` script in order to add some sample records in the system.

### Debugging
At the moment, two options are available:
1) Standard debugging (running Orleans silo as a part of the web service)
2) WSL2 (providing that you have OS Windows 10+ and Hyper-V installed)

### How to use
There are two interfaces available for use:
1) `http://localhost:6600/swagger` - API front page based on SwaggerUI.
2) `http://localhost:6600/ui/playground` - interactive in-browser IDE GraphiQL allowing to test GraphQL queries.