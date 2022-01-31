using Movies.Contracts;
using Orleans;
using System.Threading.Tasks;

namespace Movies.Grains
{
	public class MovieGrain : Grain<MovieState>, IMovieGrain
	{
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
	}
}
