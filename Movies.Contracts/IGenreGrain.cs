using Orleans;
using System.Threading.Tasks;

namespace Movies.Contracts
{
	public interface IGenreGrain : IGrainWithStringKey
	{
		Task<GenreModel> Get();

		Task Set(GenreModel genre);

		Task LinkMovie(long movieId);

		Task UnlinkMovie(long movieId);
	}
}
