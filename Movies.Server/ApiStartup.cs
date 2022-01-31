using GraphiQl;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Movies.Core;
using Movies.GrainClients;
using Movies.Server.Gql.App;
using Movies.Server.Infrastructure;
using System;
using System.IO;
using System.Reflection;

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

			//	available at <default URL>/swagger
			services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "Orleans Movies API, v.1",
					Version = "v1",
					Description = "Test API description"
				});

				var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
			});
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
				app.UseSwagger();
				app.UseSwaggerUI();
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