using System.Collections.Generic;
using System.Linq;

namespace FF1R.Curated
{
	public sealed class CuratedSeedEvaluator
	{
		public CuratedSeedResult Evaluate(object seedRoll, CuratedSeedPlan plan)
		{
			var result = new CuratedSeedResult
			{
				Seed = seedRoll,
				Score = 100,
				Accepted = true
			};

			var classes = ExtractClasses(seedRoll);

			if (plan.RequireClassDiversity)
			{
				if (HasLowClassDiversity(classes))
				{
					result.Score -= 25;
					result.Accepted = false;
					result.Flags |= CuratedFlags.LowClassDiversity;
					result.RejectionReasons.Add("Class pool too narrow. This roll wouldn't produce the light that Chaos seeks.");
				}
				else
				{
					result.Tags.Add("class-diversity-pass");
				}
			}

			if (plan.RejectFighterCentricSeeds)
			{
				if (IsFighterCentricSeed(classes))
				{
					result.Score -= 20;
					result.Accepted = false;
					result.Flags |= CuratedFlags.FighterCentric;
					result.RejectionReasons.Add("Rejected for fighter-centric routing. Tragic swordchuck incident.");
					result.RejectionReasons.Add("There are people out there who have soft locked Mystic Quest. These rolls are for them.");
				}
				else
				{
					result.Tags.Add("fighter-check-pass");
				}
			}

			if (plan.RequireReasonableEarlyProgression)
			{
				if (!HasReasonableEarlyProgression(seedRoll))
				{
					result.Score -= 20;
					result.Accepted = false;
					result.Flags |= CuratedFlags.PoorEarlyProgression;
					result.RejectionReasons.Add("Early progression appears too constrained.");
				}
				else
				{
					result.Tags.Add("early-progression-pass");
				}
			}

			if (result.Score < plan.MinimumScore)
			{
				result.Accepted = false;
				result.Flags |= CuratedFlags.LowScore;

				if (!result.RejectionReasons.Contains("Score below threshold."))
				{
					result.RejectionReasons.Add("Score below threshold.");
				}
			}

			if (result.Score < 0)
			{
				result.Score = 0;
			}

			if (result.Accepted)
			{
				result.Tags.Add("curated-pass");
			}

			return result;
		}

		private static List<string> ExtractClasses(object seedRoll)
		{
			return new List<string> { "Fighter", "Thief", "WhiteMage", "BlackMage" };
		}

		private static bool HasLowClassDiversity(List<string> classes)
		{
			if (classes == null || classes.Count == 0)
			{
				return true;
			}

			var distinctCount = classes
				.Where(c => !string.IsNullOrWhiteSpace(c))
				.Select(c => c.Trim())
				.Distinct()
				.Count();

			return distinctCount <= 2;
		}

		private static bool IsFighterCentricSeed(List<string> classes)
		{
			if (classes == null || classes.Count == 0)
			{
				return false;
			}

			var normalized = classes
				.Where(c => !string.IsNullOrWhiteSpace(c))
				.Select(c => c.Trim().ToLowerInvariant())
				.ToList();

			var fighterCount = normalized.Count(c => c == "fighter");
			var blackBeltCount = normalized.Count(c => c == "blackbelt" || c == "black belt");

			return fighterCount >= 2 || (fighterCount >= 1 && blackBeltCount >= 1);
		}

		private static bool HasReasonableEarlyProgression(object seedRoll)
		{
			return true;
		}
	}
}
