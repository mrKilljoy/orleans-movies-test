using Movies.Contracts;
using Orleans.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Server.Silo
{
	public class InitialLaunchTask : IStartupTask
	{
		private readonly IMovieClient _client;

		public InitialLaunchTask(IMovieClient client)
		{
			_client = client;
		}

		public Task Execute(CancellationToken cancellationToken) => _client.GetAll();
	}
}
