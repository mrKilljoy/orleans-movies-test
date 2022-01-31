using Microsoft.AspNetCore.Mvc;
using Movies.Contracts;
using Movies.Server.Infrastructure;
using Movies.Server.Models;
using Orleans.Providers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Movies.Server.Controllers
{
	[Route("api/[controller]")]
	[StorageProvider(ProviderName = Constants.GrainStorageName)]
	[ApiController]
	public class MoviesController : Controller
	{
		private readonly IMovieClient _movieClient;

		public MoviesController(IMovieClient movieClient)
		{
			_movieClient = movieClient;
		}

		/// <summary>
		/// Return a movie record from the storage.
		/// </summary>
		/// <param name="id">Movie ID.</param>
		/// <returns>Information about the movie.</returns>
		/// <response code="200">Request completed sucessfully.</response>
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(long id)
		{
			var grain = await _movieClient.Get(id);
			return grain is null ? NotFound() : Json(grain);
		}

		/// <summary>
		/// Return a set containing all movies in the storage.
		/// </summary>
		/// <returns>A set of all movies.</returns>
		/// <response code="200">Request completed sucessfully.</response>
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var results = await _movieClient.GetAll();
			
			return Ok(results);
		}

		/// <summary>
		/// Search movies by their name.
		/// </summary>
		/// <returns>A set of movies matching the search query.</returns>
		/// <response code="200">Request completed sucessfully.</response>
		[HttpGet("search")]
		[ProducesResponseType(typeof(MovieModel), 200)]
		public async Task<IActionResult> GetByName([FromQuery]FilterOptions options)
		{
			if (string.IsNullOrEmpty(options.Name))
			{
				return Ok(Array.Empty<MovieModel>());
			}

			var originalSet = await _movieClient.GetAll();
			var querySet = originalSet.AsQueryable();

			if (!string.IsNullOrEmpty(options.Name))
			{
				querySet = querySet.Where(m =>
					!string.IsNullOrEmpty(m.Name)
					&& m.Name.Contains(options.Name,
						StringComparison.InvariantCultureIgnoreCase));
			}

			querySet = options.SortAscending ?
				querySet.OrderBy(m => m.Name) :
				querySet.OrderByDescending(m => m.Name);

			if (options.Offset > 0)
			{
				querySet = querySet.Skip(options.Offset);
			}
			if (options.Limit > 0)
			{
				querySet = querySet.Take(options.Limit);
			}
			
			return Ok(querySet.ToList());
		}

		/// <summary>
		/// Search movies by their genre
		/// </summary>
		/// <returns>A set of movies matching selected genre.</returns>
		/// <response code="200">Request completed sucessfully.</response>
		[HttpGet("genre")]
		public async Task<IActionResult> GetByGenre([FromQuery] FilterOptions options)
		{
			if (string.IsNullOrEmpty(options.Name))
			{
				return Ok(Array.Empty<MovieModel>());
			}

			var originalSet = await _movieClient.GetAll();
			var querySet = originalSet.AsQueryable();

			if (!string.IsNullOrEmpty(options.Name))
			{
				querySet = querySet.Where(m => m.Genres != null
					&& m.Genres.Contains(
						options.Name,
						StringComparer.InvariantCultureIgnoreCase));
			}

			querySet = options.SortAscending ?
				querySet.OrderBy(m => m.Name) :
				querySet.OrderByDescending(m => m.Name);

			if (options.Offset > 0)
			{
				querySet = querySet.Skip(options.Offset);
			}
			if (options.Limit > 0)
			{
				querySet = querySet.Take(options.Limit);
			}

			return Ok(querySet.ToList());
		}

		/// <summary>
		/// Return top N movies from the storage.
		/// </summary>
		/// <param name="limit">Number of movies to retrieve.</param>
		/// <returns>A set of top N movies.</returns>
		/// <response code="200">Request completed sucessfully.</response>
		[HttpGet("top")]
		public async Task<IActionResult> GetTopRated([FromQuery]int limit = 5)
		{
			if (limit <= 0)
			{
				return Ok(Array.Empty<MovieModel>());
			}

			var originalSet = await _movieClient.GetAll();
			
			return Ok(originalSet.OrderByDescending(m => m.Rate).Take(limit).ToList());
		}

		/// <summary>
		/// Create or update a movie in the storage.
		/// </summary>
		/// <response code="200">Request completed sucessfully.</response>
		[HttpPost]
		public async Task<IActionResult> Post([FromBody] MovieModel movie)
		{
			await _movieClient.Set(movie);
			return Ok();
		}

		/// <summary>
		/// Import a set of movies into the storage.
		/// </summary>
		/// <response code="200">Request completed sucessfully.</response>
		[HttpPost("batch")]
		public async Task<IActionResult> Batch([FromBody] MovieModelSet data)
		{
			foreach (var item in data.Movies)
			{
				await _movieClient.Set(item);
			}
			
			return Ok();
		}

		/// <summary>
		/// Delete information about a movie.
		/// </summary>
		/// <param name="id">Movie ID.</param>
		/// <response code="200">Request completed sucessfully.</response>
		/// <response code="500">Request failed (no record was present in the storage / other).</response>
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(long id)
		{
			var result = await _movieClient.Delete(id);
			return result ?
				Ok() :
				StatusCode((int)System.Net.HttpStatusCode.InternalServerError);
		}
	}
}
