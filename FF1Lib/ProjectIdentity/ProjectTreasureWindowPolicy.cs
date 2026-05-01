using System;
using System.Collections.Generic;
using System.Linq;

namespace FF1Lib
{
	public enum ProjectTreasureWindowBand
	{
		Unknown = 0,
		EarlyRelief,
		EarlyTraction,
		MidRevelation,
		LateCommitment,
		EndgameCrowning
	}

	public enum ProjectTreasureFit
	{
		Unknown = 0,
		Preferred,
		Allowed,
		Discouraged,
		Blocked,
		Regression
	}

	public sealed class ProjectTreasureWindowInput
	{
		/// <summary>
		/// Zero-based depth/window index when available.
		/// For Deep Dungeon this can be floor index, window index, or another normalized progression index.
		/// </summary>
		public int WindowIndex { get; set; }

		/// <summary>
		/// Total windows in this run/model. Used to normalize depth.
		/// If unavailable, leave as 0 and WindowBandOverride can be supplied.
		/// </summary>
		public int TotalWindows { get; set; }

		public ProjectTreasureWindowBand WindowBandOverride { get; set; }

		/// <summary>
		/// How many items with the same canonical role/source have already appeared in the current local band.
		/// This is role-aware duplicate pressure, not necessarily exact item-name duplication.
		/// </summary>
		public int LocalRoleDuplicateCount { get; set; }

		/// <summary>
		/// How many exact copies of this item/source have already appeared in the current run sample.
		/// </summary>
		public int ExactDuplicateCount { get; set; }

		/// <summary>
		/// Set when this is a spoiler/matrix evaluation rather than a live mutation decision.
		/// Keeps callers honest that this helper is advisory.
		/// </summary>
		public bool AdvisoryOnly { get; set; }

		public ProjectTreasureWindowInput()
		{
			WindowBandOverride = ProjectTreasureWindowBand.Unknown;
			AdvisoryOnly = true;
		}
	}

	public sealed class ProjectTreasureWindowDecision
	{
		public ProjectTreasureWindowBand Band { get; set; }
		public ProjectTreasureFit Fit { get; set; }

		/// <summary>
		/// Advisory scalar for later weighting experiments.
		/// This helper does not apply the multiplier by itself.
		/// </summary>
		public double SuggestedWeightMultiplier { get; set; }

		public bool ShouldCountTowardDuplicatePressure { get; set; }
		public bool NeedsSpoilerRegressionCheck { get; set; }
		public bool NeedsCodeTraceBeforeMutation { get; set; }

		public string Diagnosis { get; set; }
		public string Note { get; set; }

		public ProjectTreasureWindowDecision()
		{
			Band = ProjectTreasureWindowBand.Unknown;
			Fit = ProjectTreasureFit.Unknown;
			SuggestedWeightMultiplier = 1.0d;
			ShouldCountTowardDuplicatePressure = true;
			NeedsSpoilerRegressionCheck = false;
			NeedsCodeTraceBeforeMutation = false;
			Diagnosis = "";
			Note = "";
		}
	}

	public sealed class ProjectTreasureRoleMixTarget
	{
		public double SurvivalCurrency { get; set; }
		public double TractionLoot { get; set; }
		public double TacticalCarry { get; set; }
		public double IdentityLoot { get; set; }
		public double ApexLoot { get; set; }
		public double AlteredEchoLoot { get; set; }

		public ProjectTreasureRoleMixTarget(
			double survivalCurrency,
			double tractionLoot,
			double tacticalCarry,
			double identityLoot,
			double apexLoot,
			double alteredEchoLoot)
		{
			SurvivalCurrency = survivalCurrency;
			TractionLoot = tractionLoot;
			TacticalCarry = tacticalCarry;
			IdentityLoot = identityLoot;
			ApexLoot = apexLoot;
			AlteredEchoLoot = alteredEchoLoot;
		}

		public double GetTarget(ProjectTreasureRole role)
		{
			switch (role)
			{
				case ProjectTreasureRole.SurvivalCurrency:
					return SurvivalCurrency;
				case ProjectTreasureRole.TractionLoot:
					return TractionLoot;
				case ProjectTreasureRole.TacticalCarry:
					return TacticalCarry;
				case ProjectTreasureRole.IdentityLoot:
					return IdentityLoot;
				case ProjectTreasureRole.ApexLoot:
					return ApexLoot;
				case ProjectTreasureRole.AlteredEchoLoot:
					return AlteredEchoLoot;
				default:
					return 0.0d;
			}
		}
	}

	public static class ProjectTreasureWindowPolicy
	{
		/*
			Design goal:
			This helper is deliberately advisory. It translates the current Forge treasure model into
			window/depth judgments without mutating treasure placement.

			Intended current sequence:
			1. ProjectTreasureClassifier.Classify(...) tells us what kind of item/source this is.
			2. ProjectTreasureWindowPolicy.Evaluate(...) tells us whether that kind of item fits this depth/window.
			3. Later integration code may use Fit / SuggestedWeightMultiplier / duplicate flags to shape DD generation.

			Current bridge assumptions preserved:
			- Armor active casts are regression.
			- Power Gauntlets are worn Death + Poison resist package.
			- Power Rod is FAST / LOK2 seal-mercy carrier.
			- Generic clean support weapons exclude CURE / LOCK / FAST.
		*/

		public static ProjectTreasureWindowDecision Evaluate(
			ProjectTreasureClassification classification,
			ProjectTreasureWindowInput window)
		{
			if (classification == null)
			{
				throw new ArgumentNullException(nameof(classification));
			}

			if (window == null)
			{
				throw new ArgumentNullException(nameof(window));
			}

			var band = ResolveBand(window);
			var decision = new ProjectTreasureWindowDecision
			{
				Band = band
			};

			if (classification.IsCurrentBridgeRegression)
			{
				decision.Fit = ProjectTreasureFit.Regression;
				decision.SuggestedWeightMultiplier = 0.0d;
				decision.NeedsSpoilerRegressionCheck = true;
				decision.NeedsCodeTraceBeforeMutation = true;
				decision.Diagnosis = "current bridge regression";
				decision.Note = "This item/source violates the current bridge rules. Do not tune around it as intended treasure behavior.";
				return decision;
			}

			ApplyBaseBandFit(classification, band, decision);
			ApplyStabilityPressure(classification, decision);
			ApplyDuplicatePressure(classification, window, decision);
			ApplyImplementationHints(classification, decision);

			return decision;
		}

		public static ProjectTreasureRoleMixTarget GetTargetMix(ProjectTreasureWindowBand band)
		{
			switch (band)
			{
				case ProjectTreasureWindowBand.EarlyRelief:
					return new ProjectTreasureRoleMixTarget(
						survivalCurrency: 0.45d,
						tractionLoot: 0.35d,
						tacticalCarry: 0.12d,
						identityLoot: 0.06d,
						apexLoot: 0.01d,
						alteredEchoLoot: 0.01d);

				case ProjectTreasureWindowBand.EarlyTraction:
					return new ProjectTreasureRoleMixTarget(
						survivalCurrency: 0.30d,
						tractionLoot: 0.40d,
						tacticalCarry: 0.17d,
						identityLoot: 0.10d,
						apexLoot: 0.02d,
						alteredEchoLoot: 0.01d);

				case ProjectTreasureWindowBand.MidRevelation:
					return new ProjectTreasureRoleMixTarget(
						survivalCurrency: 0.18d,
						tractionLoot: 0.27d,
						tacticalCarry: 0.25d,
						identityLoot: 0.23d,
						apexLoot: 0.05d,
						alteredEchoLoot: 0.02d);

				case ProjectTreasureWindowBand.LateCommitment:
					return new ProjectTreasureRoleMixTarget(
						survivalCurrency: 0.12d,
						tractionLoot: 0.18d,
						tacticalCarry: 0.26d,
						identityLoot: 0.27d,
						apexLoot: 0.13d,
						alteredEchoLoot: 0.04d);

				case ProjectTreasureWindowBand.EndgameCrowning:
					return new ProjectTreasureRoleMixTarget(
						survivalCurrency: 0.10d,
						tractionLoot: 0.12d,
						tacticalCarry: 0.22d,
						identityLoot: 0.24d,
						apexLoot: 0.25d,
						alteredEchoLoot: 0.07d);

				default:
					return new ProjectTreasureRoleMixTarget(
						survivalCurrency: 0.20d,
						tractionLoot: 0.25d,
						tacticalCarry: 0.25d,
						identityLoot: 0.20d,
						apexLoot: 0.07d,
						alteredEchoLoot: 0.03d);
			}
		}

		public static ProjectTreasureWindowBand ResolveBand(ProjectTreasureWindowInput input)
		{
			if (input.WindowBandOverride != ProjectTreasureWindowBand.Unknown)
			{
				return input.WindowBandOverride;
			}

			if (input.TotalWindows <= 0)
			{
				return ProjectTreasureWindowBand.Unknown;
			}

			var normalized = Math.Max(0.0d, Math.Min(1.0d, (double)input.WindowIndex / Math.Max(1, input.TotalWindows - 1)));

			if (normalized < 0.18d)
			{
				return ProjectTreasureWindowBand.EarlyRelief;
			}

			if (normalized < 0.35d)
			{
				return ProjectTreasureWindowBand.EarlyTraction;
			}

			if (normalized < 0.62d)
			{
				return ProjectTreasureWindowBand.MidRevelation;
			}

			if (normalized < 0.84d)
			{
				return ProjectTreasureWindowBand.LateCommitment;
			}

			return ProjectTreasureWindowBand.EndgameCrowning;
		}

		public static string GetBandPurpose(ProjectTreasureWindowBand band)
		{
			switch (band)
			{
				case ProjectTreasureWindowBand.EarlyRelief:
					return "Keep the run alive without letting relief become the whole language.";
				case ProjectTreasureWindowBand.EarlyTraction:
					return "Give the party contact, stabilization, and first believable options.";
				case ProjectTreasureWindowBand.MidRevelation:
					return "Start revealing who this run can become.";
				case ProjectTreasureWindowBand.LateCommitment:
					return "Let identity and tactical carry decisions become sharper commitments.";
				case ProjectTreasureWindowBand.EndgameCrowning:
					return "Crown established lanes without letting apex loot erase them.";
				default:
					return "Unknown band; use conservative classification only.";
			}
		}

		private static void ApplyBaseBandFit(
			ProjectTreasureClassification classification,
			ProjectTreasureWindowBand band,
			ProjectTreasureWindowDecision decision)
		{
			switch (band)
			{
				case ProjectTreasureWindowBand.EarlyRelief:
					ApplyEarlyReliefFit(classification, decision);
					break;

				case ProjectTreasureWindowBand.EarlyTraction:
					ApplyEarlyTractionFit(classification, decision);
					break;

				case ProjectTreasureWindowBand.MidRevelation:
					ApplyMidRevelationFit(classification, decision);
					break;

				case ProjectTreasureWindowBand.LateCommitment:
					ApplyLateCommitmentFit(classification, decision);
					break;

				case ProjectTreasureWindowBand.EndgameCrowning:
					ApplyEndgameCrowningFit(classification, decision);
					break;

				default:
					decision.Fit = ProjectTreasureFit.Allowed;
					decision.SuggestedWeightMultiplier = 1.0d;
					decision.Diagnosis = "unknown band; conservative allow";
					decision.Note = AppendNote(decision.Note, "No band information supplied; avoid making mutation decisions from this alone.");
					break;
			}
		}

		private static void ApplyEarlyReliefFit(ProjectTreasureClassification c, ProjectTreasureWindowDecision d)
		{
			switch (c.Role)
			{
				case ProjectTreasureRole.SurvivalCurrency:
					SetFit(d, ProjectTreasureFit.Allowed, 1.10d, "early relief");
					break;

				case ProjectTreasureRole.TractionLoot:
					SetFit(d, ProjectTreasureFit.Preferred, 1.30d, "early traction");
					break;

				case ProjectTreasureRole.TacticalCarry:
					SetFit(d, ProjectTreasureFit.Allowed, 0.85d, "early tactical permission");
					break;

				case ProjectTreasureRole.IdentityLoot:
					SetFit(d, ProjectTreasureFit.Allowed, 0.55d, "early identity hint");
					break;

				case ProjectTreasureRole.ApexLoot:
					SetFit(d, ProjectTreasureFit.Discouraged, 0.10d, "too early for apex certainty");
					break;

				case ProjectTreasureRole.AlteredEchoLoot:
					SetFit(d, ProjectTreasureFit.Discouraged, 0.15d, "too early for altered echo unless intentionally seeded");
					break;

				default:
					SetFit(d, ProjectTreasureFit.Allowed, 0.75d, "unknown early item");
					break;
			}
		}

		private static void ApplyEarlyTractionFit(ProjectTreasureClassification c, ProjectTreasureWindowDecision d)
		{
			switch (c.Role)
			{
				case ProjectTreasureRole.SurvivalCurrency:
					SetFit(d, ProjectTreasureFit.Allowed, 0.90d, "relief still useful but should start yielding space");
					break;

				case ProjectTreasureRole.TractionLoot:
					SetFit(d, ProjectTreasureFit.Preferred, 1.35d, "traction target");
					break;

				case ProjectTreasureRole.TacticalCarry:
					SetFit(d, ProjectTreasureFit.Preferred, 1.10d, "early tactical carry");
					break;

				case ProjectTreasureRole.IdentityLoot:
					SetFit(d, ProjectTreasureFit.Allowed, 0.85d, "first lane reveal");
					break;

				case ProjectTreasureRole.ApexLoot:
					SetFit(d, ProjectTreasureFit.Discouraged, 0.18d, "apex still too early");
					break;

				case ProjectTreasureRole.AlteredEchoLoot:
					SetFit(d, ProjectTreasureFit.Discouraged, 0.25d, "altered echo still too early unless intentionally rare");
					break;

				default:
					SetFit(d, ProjectTreasureFit.Allowed, 0.85d, "unknown early-traction item");
					break;
			}
		}

		private static void ApplyMidRevelationFit(ProjectTreasureClassification c, ProjectTreasureWindowDecision d)
		{
			switch (c.Role)
			{
				case ProjectTreasureRole.SurvivalCurrency:
					SetFit(d, ProjectTreasureFit.Discouraged, 0.65d, "relief should not dominate revelation window");
					break;

				case ProjectTreasureRole.TractionLoot:
					SetFit(d, ProjectTreasureFit.Allowed, 0.95d, "traction still useful");
					break;

				case ProjectTreasureRole.TacticalCarry:
					SetFit(d, ProjectTreasureFit.Preferred, 1.25d, "tactical carry target");
					break;

				case ProjectTreasureRole.IdentityLoot:
					SetFit(d, ProjectTreasureFit.Preferred, 1.35d, "identity reveal target");
					break;

				case ProjectTreasureRole.ApexLoot:
					SetFit(d, ProjectTreasureFit.Allowed, 0.55d, "careful early apex signal");
					break;

				case ProjectTreasureRole.AlteredEchoLoot:
					SetFit(d, ProjectTreasureFit.Allowed, 0.45d, "rare altered echo candidate");
					break;

				default:
					SetFit(d, ProjectTreasureFit.Allowed, 0.90d, "unknown mid item");
					break;
			}
		}

		private static void ApplyLateCommitmentFit(ProjectTreasureClassification c, ProjectTreasureWindowDecision d)
		{
			switch (c.Role)
			{
				case ProjectTreasureRole.SurvivalCurrency:
					SetFit(d, ProjectTreasureFit.Discouraged, 0.55d, "late relief should be supplemental");
					break;

				case ProjectTreasureRole.TractionLoot:
					SetFit(d, ProjectTreasureFit.Allowed, 0.80d, "late traction as backup or second-body support");
					break;

				case ProjectTreasureRole.TacticalCarry:
					SetFit(d, ProjectTreasureFit.Preferred, 1.25d, "late tactical commitment");
					break;

				case ProjectTreasureRole.IdentityLoot:
					SetFit(d, ProjectTreasureFit.Preferred, 1.30d, "late identity commitment");
					break;

				case ProjectTreasureRole.ApexLoot:
					SetFit(d, ProjectTreasureFit.Allowed, 0.95d, "apex begins to fit");
					break;

				case ProjectTreasureRole.AlteredEchoLoot:
					SetFit(d, ProjectTreasureFit.Allowed, 0.75d, "altered echo can appear as rare commitment spice");
					break;

				default:
					SetFit(d, ProjectTreasureFit.Allowed, 0.90d, "unknown late item");
					break;
			}
		}

		private static void ApplyEndgameCrowningFit(ProjectTreasureClassification c, ProjectTreasureWindowDecision d)
		{
			switch (c.Role)
			{
				case ProjectTreasureRole.SurvivalCurrency:
					SetFit(d, ProjectTreasureFit.Discouraged, 0.50d, "endgame relief should not crowd crowns");
					break;

				case ProjectTreasureRole.TractionLoot:
					SetFit(d, ProjectTreasureFit.Allowed, 0.70d, "endgame backup support");
					break;

				case ProjectTreasureRole.TacticalCarry:
					SetFit(d, ProjectTreasureFit.Allowed, 1.00d, "endgame tactical answer");
					break;

				case ProjectTreasureRole.IdentityLoot:
					SetFit(d, ProjectTreasureFit.Preferred, 1.15d, "endgame identity completion");
					break;

				case ProjectTreasureRole.ApexLoot:
					SetFit(d, ProjectTreasureFit.Preferred, 1.25d, "endgame crowning");
					break;

				case ProjectTreasureRole.AlteredEchoLoot:
					SetFit(d, ProjectTreasureFit.Allowed, 0.90d, "rare endgame altered echo");
					break;

				default:
					SetFit(d, ProjectTreasureFit.Allowed, 0.90d, "unknown endgame item");
					break;
			}
		}

		private static void ApplyStabilityPressure(ProjectTreasureClassification c, ProjectTreasureWindowDecision d)
		{
			switch (c.FutureStability)
			{
				case ProjectFutureStability.OldBehaviorDoNotUse:
					d.Fit = ProjectTreasureFit.Blocked;
					d.SuggestedWeightMultiplier = 0.0d;
					d.NeedsCodeTraceBeforeMutation = true;
					d.Diagnosis = "old behavior; do not plan around";
					d.Note = AppendNote(d.Note, "Old behavior tag means this should not be treated as current intended treasure.");
					break;

				case ProjectFutureStability.TraceFirst:
					d.NeedsCodeTraceBeforeMutation = true;
					d.SuggestedWeightMultiplier *= 0.70d;
					d.Note = AppendNote(d.Note, "Trace before mutating generator behavior around this source.");
					break;

				case ProjectFutureStability.Volatile:
					d.SuggestedWeightMultiplier *= 0.80d;
					d.Note = AppendNote(d.Note, "Future-volatile source: useful now, but avoid building long-term treasure identity around it.");
					break;

				case ProjectFutureStability.LikelyLimited:
				case ProjectFutureStability.LikelyRelocated:
				case ProjectFutureStability.LikelyConsolidated:
					d.SuggestedWeightMultiplier *= 0.90d;
					d.Note = AppendNote(d.Note, "Future-limited/consolidated source: record current value but avoid over-weighting.");
					break;
			}
		}

		private static void ApplyDuplicatePressure(
			ProjectTreasureClassification c,
			ProjectTreasureWindowInput input,
			ProjectTreasureWindowDecision d)
		{
			var duplicatePressure = Math.Max(input.LocalRoleDuplicateCount, input.ExactDuplicateCount);

			if (duplicatePressure <= 0)
			{
				return;
			}

			switch (c.DuplicateTolerance)
			{
				case ProjectDuplicateTolerance.High:
					if (duplicatePressure >= 4)
					{
						d.SuggestedWeightMultiplier *= 0.80d;
						d.Note = AppendNote(d.Note, "High-tolerance duplicate is acceptable, but count is starting to crowd the window.");
					}
					break;

				case ProjectDuplicateTolerance.Medium:
					if (duplicatePressure >= 3)
					{
						d.SuggestedWeightMultiplier *= 0.65d;
						d.Fit = DowngradeFit(d.Fit);
						d.Note = AppendNote(d.Note, "Medium-tolerance duplicate pressure: likely relief/utility repetition.");
					}
					break;

				case ProjectDuplicateTolerance.Low:
					if (duplicatePressure >= 2)
					{
						d.SuggestedWeightMultiplier *= 0.45d;
						d.Fit = DowngradeFit(d.Fit);
						d.Note = AppendNote(d.Note, "Low-tolerance duplicate pressure: only healthy if it supports a different wearer or matchup.");
					}
					break;

				case ProjectDuplicateTolerance.VeryLow:
					if (duplicatePressure >= 1)
					{
						d.SuggestedWeightMultiplier *= 0.20d;
						d.Fit = ProjectTreasureFit.Discouraged;
						d.Note = AppendNote(d.Note, "Very-low duplicate tolerance: repeated apex/special item likely collapses scarcity.");
					}
					break;
			}
		}

		private static void ApplyImplementationHints(
			ProjectTreasureClassification c,
			ProjectTreasureWindowDecision d)
		{
			if (c.ImplementationImplication == ProjectImplementationImplication.TraceFirst)
			{
				d.NeedsCodeTraceBeforeMutation = true;
			}

			if (c.ImplementationImplication == ProjectImplementationImplication.SpoilerRegressionOnly)
			{
				d.NeedsSpoilerRegressionCheck = true;
				d.Note = AppendNote(d.Note, "Use spoiler regression checks before deeper code work.");
			}

			if (c.ImplementationImplication == ProjectImplementationImplication.Regression)
			{
				d.Fit = ProjectTreasureFit.Regression;
				d.SuggestedWeightMultiplier = 0.0d;
				d.NeedsSpoilerRegressionCheck = true;
				d.NeedsCodeTraceBeforeMutation = true;
			}
		}

		private static void SetFit(ProjectTreasureWindowDecision d, ProjectTreasureFit fit, double multiplier, string diagnosis)
		{
			d.Fit = fit;
			d.SuggestedWeightMultiplier = multiplier;
			d.Diagnosis = diagnosis;
		}

		private static ProjectTreasureFit DowngradeFit(ProjectTreasureFit fit)
		{
			switch (fit)
			{
				case ProjectTreasureFit.Preferred:
					return ProjectTreasureFit.Allowed;
				case ProjectTreasureFit.Allowed:
					return ProjectTreasureFit.Discouraged;
				case ProjectTreasureFit.Discouraged:
					return ProjectTreasureFit.Blocked;
				default:
					return fit;
			}
		}

		private static string AppendNote(string existing, string addition)
		{
			if (string.IsNullOrWhiteSpace(addition))
			{
				return existing ?? "";
			}

			if (string.IsNullOrWhiteSpace(existing))
			{
				return addition;
			}

			return existing + " " + addition;
		}
	}
}
