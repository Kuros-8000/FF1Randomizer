namespace FF1R.Curated
{
	public sealed class CuratedSeedPlan
	{
		public int MaxAttempts { get; set; } = 200;
		public bool RequireClassDiversity { get; set; } = true;
		public bool RejectFighterCentricSeeds { get; set; } = true;
		public bool RequireReasonableEarlyProgression { get; set; } = true;
		public int MinimumScore { get; set; } = 60;
	}
}
