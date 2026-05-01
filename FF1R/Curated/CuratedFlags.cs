using System;

namespace FF1R.Curated
{
	[Flags]
	public enum CuratedFlags
	{
		None = 0,
		FighterCentric = 1 << 0,
		LowClassDiversity = 1 << 1,
		PoorEarlyProgression = 1 << 2,
		LowScore = 1 << 3
	}
}
