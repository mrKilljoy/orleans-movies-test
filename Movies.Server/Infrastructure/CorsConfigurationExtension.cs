using Microsoft.Extensions.DependencyInjection;

namespace Movies.Server.Infrastructure
{
	public static class CorsConfigurationExtension
	{
		public static void ConfigureCors(this IServiceCollection services) =>
			services.AddCors(o => o.AddPolicy("TempCorsPolicy", builder =>
			{
				builder.WithOrigins("http://localhost:4200")
				.AllowAnyMethod()
				.AllowAnyHeader()
				.AllowCredentials();
			}));
	}
}
