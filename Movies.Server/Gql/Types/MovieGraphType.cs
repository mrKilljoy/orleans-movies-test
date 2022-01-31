using GraphQL.Types;
using Movies.Contracts;

namespace Movies.Server.Gql.Types
{
	public class MovieGraphType :  ObjectGraphType<MovieModel>
	{
		public MovieGraphType()
		{
			Name = "Movie";
			Description = "A movie data graphtype.";

			Field(x => x.Id, nullable: true).Description("Unique key.");
			Field(x => x.Name, nullable: true).Description("Name.");
			Field(x => x.Genres, nullable: true).Description("Genres.");
			Field(x => x.Description, nullable: true).Description("Description.");
			Field(x => x.Rate, nullable: true).Description("Rate.");
			Field(x => x.Length, nullable: true).Description("Length.");
		}
	}
}
