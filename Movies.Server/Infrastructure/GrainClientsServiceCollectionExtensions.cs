using Microsoft.Extensions.DependencyInjection;
using Movies.Contracts;
using Movies.GrainClients;

namespace Movies.Server.Infrastructure
{
	public static class GrainClientsServiceCollectionExtensions
	{
		public static void AddAppClients(this IServiceCollection services) => 
			services.AddTransient<IMovieClient, MovieClient>();
	}
}