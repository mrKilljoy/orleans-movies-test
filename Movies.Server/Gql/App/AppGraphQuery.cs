using GraphQL.Types;
using Movies.Contracts;
using Movies.Server.Gql.Types;
using System;
using System.Linq;

namespace Movies.Server.Gql.App
{
	public class AppGraphQuery : ObjectGraphType
	{
		public AppGraphQuery(IMovieClient client)
		{
			Name = "AppQueries";

			//	Get a movie by its ID
			Field<MovieGraphType>("movieById",
				arguments: new QueryArguments(new QueryArgument<LongGraphType>
				{
					Name = "id"
				}),
				resolve: ctx => client.Get(ctx.Arguments["id"] as long? ?? default));

			//	Get the list of all movies
			Field<ListGraphType<MovieGraphType>>("allMovies", resolve: ctx => client.GetAll());

			//	Get a set of movies by a name fragment
			Field<ListGraphType<MovieGraphType>>("filterByName", arguments: new QueryArguments(new QueryArgument<StringGraphType>
			{
				Name = "contains"
			}),
			resolve: (ctx) =>
			{
				var name = ctx.Arguments["contains"]?.ToString() ?? string.Empty;

				return client.GetAll().ContinueWith(tc => tc.Result.Where(m => !string.IsNullOrEmpty(m.Name) 
					&& m.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase)));
			});

			//	Get a set of movies by selected genre
			Field<ListGraphType<MovieGraphType>>("filterByGenre", arguments: new QueryArguments(new QueryArgument<StringGraphType>
			{
				Name = "genre"
			}),
			resolve: (ctx) =>
			{
				var genre = ctx.Arguments["genre"]?.ToString() ?? string.Empty;

				return client.GetAll().ContinueWith(tc => tc.Result.Where(m => m.Genres != null && m.Genres.Contains(genre)));
			});

			//	Get a set of N top rated movies
			Field<ListGraphType<MovieGraphType>>("topRatedMovies", arguments: new QueryArguments(new QueryArgument<IntGraphType>
			{
				Name = "number",
				DefaultValue = 5
			}),
			resolve: (ctx) =>
			{
				int.TryParse(ctx.Arguments["number"]?.ToString() ?? string.Empty, out var limit);
				return client.GetAll().ContinueWith(tc => tc.Result.OrderByDescending(m => m.Rate).Take(limit));
			});
		}
	}
}
