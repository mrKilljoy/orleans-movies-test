using System.Collections.Generic;
using System.Threading.Tasks;

namespace Movies.Contracts
{
	public interface IMovieClient
	{
		Task<MovieModel> Get(long movieId);

		Task<List<MovieModel>> GetAll();

		Task Set(MovieModel movie);

		Task<bool> Delete(long movieId);
	}
}
