using System.ComponentModel.DataAnnotations;

namespace Movies.Server.Infrastructure
{
	public class FilterOptions
	{
		[Range(0, int.MaxValue)]
		public int Limit { get; set; } = 20;

		[Range(0, int.MaxValue)]
		public int Offset { get; set; }

		public string Name { get; set; }

		public bool SortAscending { get; set; } = true;
	}
}
