using System;
using System.Collections.Generic;

namespace Movies.Contracts
{
	public class MovieIndexModel
	{
		public Guid Id { get; set; }

		public Dictionary<long, string> Index { get; set; } = new Dictionary<long, string>();
	}
}
