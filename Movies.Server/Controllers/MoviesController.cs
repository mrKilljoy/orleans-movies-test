using Microsoft.AspNetCore.Mvc;
using Movies.Contracts;
using Movies.Server.Infrastructure;
using Movies.Server.Models;
using Orleans.Providers;
using System;
using System.Collections.Generic;
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

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(long id)
		{
			var grain = await _movieClient.Get(id);
			return grain is null ? NotFound() : Json(grain);
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var results = await _movieClient.GetAll();
			
			return Ok(results);
		}

		[HttpGet("search")]
		public async Task<IActionResult> GetByName([FromQuery]FilterOptions options)
		{
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

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] MovieModel movie)
		{
			await _movieClient.Set(movie);
			return Ok();
		}

		[HttpPost("batch")]
		public async Task<IActionResult> Batch([FromBody] MovieModelSet data)
		{
			foreach (var item in data.Movies)
			{
				await _movieClient.Set(item);
			}
			
			return Ok();
		}

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
