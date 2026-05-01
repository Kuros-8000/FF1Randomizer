using System.Collections.Generic;
using System;

namespace FF1R.Curated
{
	public sealed class CuratedSeedGenerator
	{
		private readonly CuratedSeedEvaluator _evaluator = new();

		public CuratedSeedResult Generate(CuratedSeedPlan plan)
		{
			CuratedSeedResult bestRoll = null;

			for (int attempt = 0; attempt < plan.MaxAttempts; attempt++)
			{
				var seedRoll = GenerateSeedRoll();
				var rollResult = _evaluator.Evaluate(seedRoll, plan);

				rollResult.Tags.Add($"attempt-{attempt + 1}");

				if (rollResult.Accepted)
				{
					rollResult.Tags.Add("accepted-roll");
					return rollResult;
				}

				if (bestRoll == null || rollResult.Score > bestRoll.Score)
				{
					bestRoll = rollResult;
				}
			}

			if (bestRoll != null)
			{
				bestRoll.Tags.Add("fallback-roll");
				bestRoll.RejectionReasons.Add("No acceptable rolls found. Returning highest-scoring fallback roll.");
				return bestRoll;
			}

			return new CuratedSeedResult
			{
				Accepted = false,
				Score = 0
			};
		}

		public List<CuratedSeedResult> GenerateBatch(CuratedSeedPlan plan, int batchSize)
		{
			var results = new List<CuratedSeedResult>();

			if (batchSize < 1)
			{
				batchSize = 1;
			}

			if (batchSize > 20)
			{
				batchSize = 20;
			}

			for (int attempt = 0; attempt < batchSize; attempt++)
			{
				var seedRoll = GenerateSeedRoll();
				var rollResult = _evaluator.Evaluate(seedRoll, plan);
				rollResult.Tags.Add($"attempt-{attempt + 1}");
				results.Add(rollResult);
			}

			return results;
		}

		public static string BuildRollSummary(CuratedSeedResult result)
		{
			var lines = new List<string>
			{
				$"Accepted: {result.Accepted}",
				$"Score: {result.Score}",
				$"Flags: {result.Flags}"
			};

			if (result.Tags.Count > 0)
			{
				lines.Add("Tags:");
				lines.AddRange(result.Tags);
			}

			if (result.RejectionReasons.Count > 0)
			{
				lines.Add("Rejection Reasons:");
				lines.AddRange(result.RejectionReasons);
			}

			return string.Join(Environment.NewLine, lines);
		}

		private static object GenerateSeedRoll()
		{
			return new object();
		}
	}
}
