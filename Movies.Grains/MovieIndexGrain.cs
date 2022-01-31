using Movies.Contracts;
using Orleans;
using System.Linq;
using System.Threading.Tasks;

namespace Movies.Grains
{
	public class MovieIndexGrain : Grain<MovieIndexState>, IMovieIndexGrain
	{
		private readonly IGrainFactory _grainFactory;

		public MovieIndexGrain(IGrainFactory grainFactory)
		{
			_grainFactory = grainFactory;
		}

		public override async Task OnActivateAsync()
		{
			await base.OnActivateAsync();
			if (State.Value is null)
			{
				State = new MovieIndexState()
				{
					Value = new MovieIndexModel()
				};

				await WriteStateAsync();
			}
		}

		public Task<MovieIndexModel> Get() => Task.FromResult(State.Value);

		public Task<MovieModel[]> GetAll() =>
			Task.WhenAll(State.Value.Index
				.Select(i => _grainFactory.GetGrain<IMovieGrain>(i.Key)
				.Get()));

		public Task Set(MovieIndexModel movieIndex)
		{
			State.Value = movieIndex;
			return WriteStateAsync();
		}
	}
}
