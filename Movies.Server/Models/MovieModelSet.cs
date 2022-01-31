using Movies.Contracts;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Movies.Server.Models
{
	public class MovieModelSet
	{
		[Required]
		public List<MovieModel> Movies { get; set; }
	}
}
