using Microsoft.AspNetCore.Mvc;
using Moq;
using Movies.Contracts;
using Movies.Server.Controllers;
using Movies.Server.Infrastructure;
using Movies.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Movies.Tests
{
    public class MoviesControllerTest
    {
        [Fact]
        public async Task GetAllRecordsSuccessfully()
        {
			var client = new Mock<IMovieClient>();
			client
				.Setup(c => c.GetAll())
				.Returns(() => Task.FromResult(GetTestData()));

			var controller = new MoviesController(client.Object);
			var response = await controller.GetAll() as OkObjectResult;

			Assert.NotNull(response);
			Assert.IsType<List<MovieModel>>(response.Value);
			Assert.Equal(GetTestData().Count, ((List<MovieModel>)response.Value).Count);
        }

		[Theory]
		[InlineData(1)]
		public async Task GetSingleRecordSuccessfully(int recordId)
		{
			var client = new Mock<IMovieClient>();
			client
				.Setup(c => c.Get(recordId))
				.Returns(()=> Task.FromResult(GetTestData().First()));

			var controller = new MoviesController(client.Object);
			var response = await controller.Get(recordId) as OkObjectResult;

			Assert.NotNull(response);
			Assert.IsType<MovieModel>(response.Value);
			Assert.Equal(GetTestData().First().Id, ((MovieModel)response.Value).Id);
		}

		[Theory]
		[InlineData(999)]
		public async Task RecordNotFound(int recordId)
		{
			var client = new Mock<IMovieClient>();
			client
				.Setup(c => c.Get(recordId))
				.Returns(() => Task.FromResult<MovieModel>(null));

			var controller = new MoviesController(client.Object);
			var response = await controller.Get(recordId);

			Assert.IsType<NotFoundResult>(response);
		}

		[Theory]
		[InlineData("Movie #1")]
		public async Task RecordFoundByNameSuccessfully(string recordName)
		{
			var client = new Mock<IMovieClient>();
			client
				.Setup(c => c.GetAll())
				.Returns(() => Task.FromResult(GetTestData()));

			var controller = new MoviesController(client.Object);
			var response = await controller.GetByName(new FilterOptions
			{
				Limit = 5,
				Name = recordName
			}) as OkObjectResult;

			Assert.NotNull(response);
			Assert.IsType<List<MovieModel>>(response.Value);
			Assert.Contains(((List<MovieModel>)response.Value), m => m.Name == recordName);
		}

		[Theory]
		[InlineData("peppa the pig")]
		public async Task RecordNotFoundByName(string recordName)
		{
			var client = new Mock<IMovieClient>();
			client
				.Setup(c => c.GetAll())
				.Returns(() => Task.FromResult(new List<MovieModel>()));

			var controller = new MoviesController(client.Object);
			var response = await controller.GetByName(new FilterOptions
			{
				Limit = 5,
				Name = recordName
			}) as OkObjectResult;

			Assert.NotNull(response);
			Assert.IsType<List<MovieModel>>(response.Value);
			Assert.Equal(0, ((List<MovieModel>)response.Value).Count(m => m.Name == recordName));
		}

		[Theory]
		[InlineData("g3")]
		public async Task RecordsFoundByGenreSuccessfully(string genreName)
		{
			var client = new Mock<IMovieClient>();
			client
				.Setup(c => c.GetAll())
				.Returns(() => Task.FromResult(GetTestData()));

			var controller = new MoviesController(client.Object);
			var response = await controller.GetByGenre(new FilterOptions
			{
				Limit = 5,
				Name = genreName
			}) as OkObjectResult;

			Assert.NotNull(response);
			Assert.IsType<List<MovieModel>>(response.Value);
			Assert.True(((List<MovieModel>)response.Value).Count(m => m.Genres.Contains(genreName)) > 0);
		}

		[Theory]
		[InlineData("noir")]
		public async Task NoRecordsFoundByGenre(string genreName)
		{
			var client = new Mock<IMovieClient>();
			client
				.Setup(c => c.GetAll())
				.Returns(() => Task.FromResult(new List<MovieModel>()));

			var controller = new MoviesController(client.Object);
			var response = await controller.GetByGenre(new FilterOptions
			{
				Limit = 5,
				Name = genreName
			}) as OkObjectResult;

			Assert.NotNull(response);
			Assert.IsType<List<MovieModel>>(response.Value);
			Assert.Equal(0, ((List<MovieModel>)response.Value).Count(m => m.Genres.Contains(genreName)));
		}

		[Theory]
		[InlineData(1)]
		public async Task RecordDeletedSuccessfully(int recordId)
		{
			var client = new Mock<IMovieClient>();
			client
				.Setup(c => c.Delete(recordId))
				.Returns(() => Task.FromResult(true));

			var controller = new MoviesController(client.Object);
			var response = await controller.Delete(recordId);

			Assert.IsType<OkResult>(response);
		}

		[Theory]
		[InlineData(999)]
		public async Task RecordWasNotDeleted(int recordId)
		{
			var client = new Mock<IMovieClient>();
			client
				.Setup(c => c.Delete(recordId))
				.Returns(() => Task.FromResult(false));

			var controller = new MoviesController(client.Object);
			var response = await controller.Delete(recordId);

			Assert.IsType<StatusCodeResult>(response);
			Assert.Equal(500, ((StatusCodeResult)response).StatusCode);
		}

		[Fact]
		public async Task RecordsOrderedProperly()
		{
			var client = new Mock<IMovieClient>();
			client
				.Setup(c => c.GetAll())
				.Returns(() => Task.FromResult(GetTestData()));

			var controller = new MoviesController(client.Object);
			var response = await controller.GetTopRated() as OkObjectResult;
			var results = response?.Value as List<MovieModel>;

			Assert.NotNull(response);
			Assert.NotNull(results);
			Assert.True(results.First().Rate > results.Last().Rate);
		}

		[Fact]
		public async Task RecordCreatedSuccessfully()
		{
			var testObject = new MovieModel();
			var client = new Mock<IMovieClient>();
			client
				.Setup(c => c.Set(testObject))
				.Returns(() => Task.CompletedTask);

			var controller = new MoviesController(client.Object);
			var response = await controller.Post(testObject) as OkResult;

			Assert.NotNull(response);
		}

		[Fact]
		public async Task MultipleRecordImportedSuccessfully()
		{
			var testObject = new MovieModel();
			
			var client = new Mock<IMovieClient>();
			client
				.Setup(c => c.Set(It.IsAny<MovieModel>()))
				.Returns(() => Task.CompletedTask);

			var controller = new MoviesController(client.Object);
			var response = await controller.Batch(new MovieModelSet()
			{
				Movies = GetTestData()
			}) as OkResult;

			Assert.NotNull(response);
		}

		/// <summary>
		/// Returns a set of test records.
		/// </summary>
		private List<MovieModel> GetTestData() =>
			new List<MovieModel>()
			{
				new MovieModel
				{
					Id = 1,
					Name = "Movie #1",
					Description = "Description",
					Genres = new[] { "g1", "g2", "g3" },
					Rate = 3
				},
				new MovieModel
				{
					Id = 2,
					Name = "Movie #2",
					Description = "Description #2",
					Genres = new[] { "g3" },
					Rate = 6
				},
				new MovieModel
				{
					Id = 3,
					Name = "Movie #3",
					Description = "Description #3",
					Genres = new[] { "g4" },
					Rate = 10
				}
			};
	}
}
