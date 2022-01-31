﻿using Microsoft.Extensions.DependencyInjection;
using Movies.Contracts;

namespace Movies.GrainClients
{
	public static class GrainClientsServiceCollectionExtensions
	{
		public static void AddAppClients(this IServiceCollection services) => services
				.AddSingleton<ISampleGrainClient, SampleGrainClient>()
				.AddTransient<IMovieClient, MovieClient>();
	}
}