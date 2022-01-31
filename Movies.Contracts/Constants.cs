using System;

namespace Movies.Contracts
{
	public static class Constants
	{
		public const string GrainStorageName = "MoviesStorage";

		public readonly static string MovieIndexGrainId = Guid.Empty.ToString();
	}
}
