using GraphiQl;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Movies.Core;
using Movies.GrainClients;
using Movies.Server.Gql.App;
using Movies.Server.Infrastructure;

namespace Movies.Server
{
	public class ApiStartup
	{
		private readonly IConfiguration _configuration;
		private readonly IAppInfo _appInfo;

		public ApiStartup(
			IConfiguration configuration,
			IAppInfo appInfo)
		{
			_configuration = configuration;
			_appInfo = appInfo;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddCustomAuthentication();
			services.ConfigureCors();

			services.Configure<KestrelServerOptions>(options =>
			{
				options.AllowSynchronousIO = true;
			});

			services.AddAppClients();
			services.AddAppGraphQL();
			services.AddControllers()
				.AddNewtonsoftJson();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(
			IApplicationBuilder app,
			IWebHostEnvironment env)
		{
			app.UseCors("TempCorsPolicy");
			app.UseGraphQL<ISchema>();
			app.UseGraphQLPlayground();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseGraphiQl();
			}

			app.UseRouting();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}