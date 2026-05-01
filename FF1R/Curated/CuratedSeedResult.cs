using System.Collections.Generic;

namespace FF1R.Curated
{
	public sealed class CuratedSeedResult
	{
		public bool Accepted { get; set; }
		public int Score { get; set; }
		public object Seed { get; set; }

		public CuratedFlags Flags { get; set; } = CuratedFlags.None;
		public List<string> Tags { get; } = new();
		public List<string> RejectionReasons { get; } = new();
	}
}
