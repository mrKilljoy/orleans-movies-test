using Orleans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Movies.Contracts
{
	public interface IMovieIndexGrain : IGrainWithStringKey
	{
		Task<MovieIndexModel> Get();

		Task<MovieModel[]> GetAll();

		Task Set(MovieIndexModel movieIndex);
	}
}
