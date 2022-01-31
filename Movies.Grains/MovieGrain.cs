using Movies.Contracts;
using Orleans;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movies.Grains
{
	public class MovieGrain : Grain<MovieState>, IMovieGrain
	{
		private readonly IGrainFactory _grainFactory;

		public MovieGrain(IGrainFactory grainFactory)
		{
			_grainFactory = grainFactory;
		}

		public async Task<MovieModel> Get()
		{
			await ReadStateAsync();
			return State.Value;
		}

		public async Task Set(MovieModel movie)
		{
			State = new MovieState { Value = movie };
			await WriteStateAsync();
		}

		public Task Drop() => ClearStateAsync();

		public async Task AddGenre(string genre)
		{
			await ReadStateAsync();

			var movieInstance = State;
			var genreGrain = _grainFactory.GetGrain<IGenreGrain>(genre);
			var genreInstance = await genreGrain.Get();
			
			if (movieInstance.Value is not null && genreInstance is not null && genreInstance.Name != null)
			{
				//await genreGrain.LinkMovie(movieInstance.Value.Id);
				movieInstance.Value.Genres = new HashSet<string>(movieInstance.Value.Genres) { genre }.ToArray();

				await WriteStateAsync();
			}
		}

		public async Task RemoveGenre(string genre)
		{
			await ReadStateAsync();

			var movieInstance = State;
			var genreGrain = _grainFactory.GetGrain<IGenreGrain>(genre);
			var genreInstance = await genreGrain.Get();

			if (movieInstance.Value is not null && genreInstance is not null && genreInstance.Name != null)
			{
				//await genreGrain.UnlinkMovie(movieInstance.Value.Id);
				movieInstance.Value.Genres = movieInstance.Value.Genres.Where(g => g != genre).ToArray();

				await WriteStateAsync();
			}
		}
	}
}
