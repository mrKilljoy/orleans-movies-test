using Movies.Contracts;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movies.GrainClients
{
	public class MovieClient : IMovieClient
	{
		private readonly IGrainFactory _grainFactory;

		public MovieClient(IGrainFactory grainFactory)
		{
			_grainFactory = grainFactory;
		}

		public async Task<bool> Delete(long movieId)
		{
			var movieGrain = _grainFactory.GetGrain<IMovieGrain>(movieId);
			var indexGrain = _grainFactory.GetGrain<IMovieIndexGrain>(Constants.MovieIndexGrainId);
			var movieInst = await movieGrain.Get();
			var indexInst = await indexGrain.Get();

			if (movieInst is not null && indexInst is not null)
			{
				await movieGrain.Drop();
				if (indexInst.Index.ContainsKey(movieId))
				{
					indexInst.Index.Remove(movieId);
					await indexGrain.Set(indexInst);
				}

				return true;
			}

			return false;
		}

		public Task<MovieModel> Get(long movieId)
		{
			var grain = _grainFactory.GetGrain<IMovieGrain>(movieId);
			return grain.Get();
		}

		public async Task<List<MovieModel>> GetAll()
		{
			var indexGrain = _grainFactory.GetGrain<IMovieIndexGrain>(Constants.MovieIndexGrainId);
			var movies = await indexGrain.GetAll();
			return movies.ToList();
		}

		public async Task Set(MovieModel movie)
		{
			if (movie is null)
			{
				throw new ArgumentNullException(nameof(movie));
			}

			var movieGrain = _grainFactory.GetGrain<IMovieGrain>(movie.Id);
			var indexGrain = _grainFactory.GetGrain<IMovieIndexGrain>(Constants.MovieIndexGrainId);

			var indexInst = await indexGrain.Get();
			await movieGrain.Set(movie);

			if (!indexInst.Index.ContainsKey(movie.Id))
			{
				indexInst.Index.Add(movie.Id, movie.Name);
				await indexGrain.Set(indexInst);
			}
		}
	}
}
