using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Movies.Core;
using Movies.Server.Infrastructure;
using Orleans;
using Orleans.Hosting;
using Serilog;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Movies.Grains;
using Movies.Server.Silo;
using Movies.Contracts;
using Movies.GrainClients;

namespace Movies.Server
{
	public class Program
	{
		public static Task Main(string[] args)
		{
			var hostBuilder = new HostBuilder();

			IAppInfo appInfo = null;
			hostBuilder
				.ConfigureHostConfiguration(cfg =>
				{
					cfg.SetBasePath(Directory.GetCurrentDirectory())
						.AddEnvironmentVariables("ASPNETCORE_")
						.AddCommandLine(args);
				})
				.ConfigureServices((ctx, services) =>
				{
					appInfo = new AppInfo(ctx.Configuration);
					Console.Title = $"{appInfo.Name} - {appInfo.Environment}";

					services.AddSingleton(appInfo);
					services.Configure<ApiHostedServiceOptions>(options =>
					{
						options.Port = GetAvailablePort(6600, 6699);
					});

					services.AddTransient<IMovieClient, MovieClient>();
					services.Configure<ConsoleLifetimeOptions>(options =>
					{
						options.SuppressStatusMessages = true;
					});
				})
				.ConfigureAppConfiguration((ctx, cfg) =>
				{
					var shortEnvName = AppInfo.MapEnvironmentName(ctx.HostingEnvironment.EnvironmentName);
					cfg.AddJsonFile("appsettings.json")
						.AddJsonFile($"appsettings.{shortEnvName}.json", optional: true)
						.AddJsonFile("app-info.json")
						.AddEnvironmentVariables()
						.AddCommandLine(args);

					appInfo = new AppInfo(cfg.Build());

					if (!appInfo.IsDockerized) return;

					cfg.Sources.Clear();

					cfg.AddJsonFile("appsettings.json")
						.AddJsonFile($"appsettings.{shortEnvName}.json", optional: true)
						.AddJsonFile("app-info.json")
						.AddEnvironmentVariables()
						.AddCommandLine(args);
				})
				.UseSerilog((ctx, loggerConfig) =>
				{
					loggerConfig.Enrich.FromLogContext()
						.ReadFrom.Configuration(ctx.Configuration)
						.Enrich.WithMachineName()
						.Enrich.WithDemystifiedStackTraces()
						.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message:lj}{NewLine}{Exception}");

					loggerConfig.WithAppInfo(appInfo);
				})
				.UseOrleans((ctx, builder) =>
				{
					//	possible to use .UseInMemoryPersistenceConfiguration instead
					//	if grain persistence is not required
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
							ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TestOrleans;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;",
							UseJson = true
						})

						.ConfigureApplicationParts(parts => parts
							.AddApplicationPart(typeof(MovieGrain).Assembly).WithReferences()
						)
						.AddIncomingGrainCallFilter<LoggingIncomingCallFilter>()						
						.ConfigureLogging(lg => lg.AddSerilog())

						//	initial preloading of all grains
						//	(does not make sense with InMemory persistence model)
						.AddStartupTask<InitialLaunchTask>()    
					;

				})
				.ConfigureServices((ctx, services) =>
				{
					services.AddHostedService<ApiHostedService>();
				})
				;

			return hostBuilder.RunConsoleAsync();
		}

		private static int GetAvailablePort(int start, int end)
		{
			for (var port = start; port < end; ++port)
			{
				var listener = TcpListener.Create(port);
				listener.ExclusiveAddressUse = true;
				try
				{
					listener.Start();
					return port;
				}
				catch (SocketException)
				{
				}
				finally
				{
					listener.Stop();
				}
			}

			throw new InvalidOperationException();
		}
	}
}