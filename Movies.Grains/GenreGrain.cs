using Movies.Contracts;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Grains
{
	public class GenreGrain : Grain<GenreModel>, IGenreGrain
	{
		private HashSet<long> _relatedMovies;
		private readonly IMovieClient _movieClient;

		public GenreGrain(IMovieClient movieClient)
		{
			_movieClient = movieClient;
		}

		public Task<GenreModel> Get() => Task.FromResult(State);

		public async Task Set(GenreModel genre)
		{
			State = genre;
			await WriteStateAsync();
		}

		public async Task LinkMovie(long movieId)
		{
			var movie = await _movieClient.Get(movieId);
			if (movie is not null)
			{
				_relatedMovies.Add(movieId);
			}
		}

		public Task UnlinkMovie(long movieId) => throw new NotImplementedException();

		public override async Task OnActivateAsync()
		{
			await base.OnActivateAsync();

			_relatedMovies ??= new HashSet<long>();
		}
	}
}
