using System;
using System.Collections.Generic;
using System.Linq;

namespace FF1Lib
{
	public enum ProjectTreasureNextLever
	{
		Unknown = 0,
		NoAction,
		PoolEdit,
		WeightEdit,
		WindowEdit,
		DuplicateRule,
		SpoilerRegression,
		CodeTrace,
		BridgeRegression
	}

	public sealed class ProjectTreasureMatrixEntry
	{
		public string ItemName { get; set; }
		public string SourceLabel { get; set; }

		public ProjectTreasureInput TreasureInput { get; set; }
		public ProjectTreasureWindowInput WindowInput { get; set; }

		public ProjectTreasureClassification Classification { get; set; }
		public ProjectTreasureWindowDecision WindowDecision { get; set; }

		public ProjectTreasureMatrixEntry()
		{
			ItemName = "";
			SourceLabel = "";
			TreasureInput = new ProjectTreasureInput();
			WindowInput = new ProjectTreasureWindowInput();
			Classification = null;
			WindowDecision = null;
		}
	}

	public sealed class ProjectTreasureBandSummary
	{
		public ProjectTreasureWindowBand Band { get; set; }

		public int TotalCount { get; set; }
		public int RegressionCount { get; set; }
		public int PreferredCount { get; set; }
		public int AllowedCount { get; set; }
		public int DiscouragedCount { get; set; }
		public int BlockedCount { get; set; }

		public Dictionary<ProjectTreasureRole, int> RoleCounts { get; private set; }
		public Dictionary<ProjectFutureStability, int> StabilityCounts { get; private set; }
		public Dictionary<ProjectDuplicateTolerance, int> DuplicateToleranceCounts { get; private set; }
		public Dictionary<ProjectImplementationImplication, int> ImplementationCounts { get; private set; }

		public List<string> Warnings { get; private set; }

		public ProjectTreasureBandSummary(ProjectTreasureWindowBand band)
		{
			Band = band;
			TotalCount = 0;
			RegressionCount = 0;
			PreferredCount = 0;
			AllowedCount = 0;
			DiscouragedCount = 0;
			BlockedCount = 0;

			RoleCounts = new Dictionary<ProjectTreasureRole, int>();
			StabilityCounts = new Dictionary<ProjectFutureStability, int>();
			DuplicateToleranceCounts = new Dictionary<ProjectDuplicateTolerance, int>();
			ImplementationCounts = new Dictionary<ProjectImplementationImplication, int>();
			Warnings = new List<string>();
		}

		public double RoleRatio(ProjectTreasureRole role)
		{
			if (TotalCount <= 0)
			{
				return 0.0d;
			}

			return Count(RoleCounts, role) / (double)TotalCount;
		}

		public int CountRole(ProjectTreasureRole role)
		{
			return Count(RoleCounts, role);
		}

		private static int Count<T>(Dictionary<T, int> dict, T key)
		{
			int value;
			return dict.TryGetValue(key, out value) ? value : 0;
		}
	}

	public sealed class ProjectTreasureMatrixReport
	{
		public int TotalEntries { get; set; }
		public int RegressionCount { get; set; }
		public int CodeTraceCount { get; set; }
		public int SpoilerRegressionCount { get; set; }

		public Dictionary<ProjectTreasureRole, int> RoleCounts { get; private set; }
		public Dictionary<ProjectTreasureWindowBand, ProjectTreasureBandSummary> BandSummaries { get; private set; }
		public Dictionary<string, int> CanonicalItemCounts { get; private set; }

		public List<ProjectTreasureMatrixEntry> Entries { get; private set; }
		public List<ProjectTreasureMatrixEntry> RegressionEntries { get; private set; }
		public List<ProjectTreasureMatrixEntry> DuplicatePressureEntries { get; private set; }
		public List<ProjectTreasureMatrixEntry> TraceFirstEntries { get; private set; }

		public List<string> Diagnoses { get; private set; }
		public List<string> Warnings { get; private set; }

		public ProjectTreasureNextLever RecommendedNextLever { get; set; }
		public string RecommendedNextStep { get; set; }

		public ProjectTreasureMatrixReport()
		{
			TotalEntries = 0;
			RegressionCount = 0;
			CodeTraceCount = 0;
			SpoilerRegressionCount = 0;

			RoleCounts = new Dictionary<ProjectTreasureRole, int>();
			BandSummaries = new Dictionary<ProjectTreasureWindowBand, ProjectTreasureBandSummary>();
			CanonicalItemCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

			Entries = new List<ProjectTreasureMatrixEntry>();
			RegressionEntries = new List<ProjectTreasureMatrixEntry>();
			DuplicatePressureEntries = new List<ProjectTreasureMatrixEntry>();
			TraceFirstEntries = new List<ProjectTreasureMatrixEntry>();

			Diagnoses = new List<string>();
			Warnings = new List<string>();

			RecommendedNextLever = ProjectTreasureNextLever.Unknown;
			RecommendedNextStep = "";
		}
	}

	public static class ProjectTreasureMatrixReporter
	{
		/*
			Design goal:
			This reporter turns spoiler/run samples into a compact matrix diagnosis.

			It is intentionally non-mutating.
			It does not choose items, alter pools, or rewrite DD generation.
			It consumes:
			- ProjectTreasureClassifier
			- ProjectTreasureWindowPolicy

			and produces:
			- role mix counts
			- band summaries
			- duplicate pressure warnings
			- current bridge regression flags
			- a suggested next lever

			Use this to avoid re-explaining every old item every time.
		*/

		public static ProjectTreasureMatrixReport BuildReport(IEnumerable<ProjectTreasureMatrixEntry> rawEntries)
		{
			if (rawEntries == null)
			{
				throw new ArgumentNullException(nameof(rawEntries));
			}

			var entries = rawEntries.ToList();
			PopulateClassifications(entries);
			PopulateDuplicateCounts(entries);

			var report = new ProjectTreasureMatrixReport();

			foreach (var entry in entries)
			{
				AddEntry(report, entry);
			}

			AddBandDiagnoses(report);
			AddGlobalDiagnoses(report);
			ChooseNextLever(report);

			return report;
		}

		public static ProjectTreasureMatrixEntry CreateEntry(
			string itemName,
			int windowIndex,
			int totalWindows,
			bool isWeapon,
			bool isArmor,
			bool hasActiveCast,
			string castName = "",
			string resistText = "",
			string sourceLabel = "")
		{
			return new ProjectTreasureMatrixEntry
			{
				ItemName = itemName ?? "",
				SourceLabel = sourceLabel ?? "",
				TreasureInput = new ProjectTreasureInput
				{
					ItemName = itemName ?? "",
					CastName = castName ?? "",
					ResistText = resistText ?? "",
					IsWeapon = isWeapon,
					IsArmor = isArmor,
					HasActiveCast = hasActiveCast
				},
				WindowInput = new ProjectTreasureWindowInput
				{
					WindowIndex = windowIndex,
					TotalWindows = totalWindows,
					AdvisoryOnly = true
				}
			};
		}

		public static string BuildCompactTextReport(ProjectTreasureMatrixReport report)
		{
			if (report == null)
			{
				throw new ArgumentNullException(nameof(report));
			}

			var lines = new List<string>();

			lines.Add("Project Treasure Matrix Report");
			lines.Add("--------------------------------");
			lines.Add("Total entries: " + report.TotalEntries);
			lines.Add("Regression flags: " + report.RegressionCount);
			lines.Add("Spoiler regression checks: " + report.SpoilerRegressionCount);
			lines.Add("Code trace needed: " + report.CodeTraceCount);
			lines.Add("");

			lines.Add("Role mix:");
			foreach (var role in EnumValues<ProjectTreasureRole>().Where(r => r != ProjectTreasureRole.Unknown))
			{
				lines.Add("- " + role + ": " + Count(report.RoleCounts, role));
			}
			lines.Add("");

			lines.Add("Band summaries:");
			foreach (var pair in report.BandSummaries.OrderBy(p => (int)p.Key))
			{
				var b = pair.Value;
				lines.Add("- " + b.Band + ": " + b.TotalCount + " entries"
					+ " | survival " + b.CountRole(ProjectTreasureRole.SurvivalCurrency)
					+ " | traction " + b.CountRole(ProjectTreasureRole.TractionLoot)
					+ " | tactical " + b.CountRole(ProjectTreasureRole.TacticalCarry)
					+ " | identity " + b.CountRole(ProjectTreasureRole.IdentityLoot)
					+ " | apex " + b.CountRole(ProjectTreasureRole.ApexLoot)
					+ " | altered " + b.CountRole(ProjectTreasureRole.AlteredEchoLoot)
					+ " | regressions " + b.RegressionCount);
			}
			lines.Add("");

			if (report.Warnings.Count > 0)
			{
				lines.Add("Warnings:");
				foreach (var warning in report.Warnings.Distinct())
				{
					lines.Add("- " + warning);
				}
				lines.Add("");
			}

			if (report.Diagnoses.Count > 0)
			{
				lines.Add("Diagnoses:");
				foreach (var diagnosis in report.Diagnoses.Distinct())
				{
					lines.Add("- " + diagnosis);
				}
				lines.Add("");
			}

			lines.Add("Recommended next lever: " + report.RecommendedNextLever);
			lines.Add("Recommended next step: " + report.RecommendedNextStep);

			return string.Join(Environment.NewLine, lines);
		}

		private static void PopulateClassifications(List<ProjectTreasureMatrixEntry> entries)
		{
			foreach (var entry in entries)
			{
				if (entry.TreasureInput == null)
				{
					entry.TreasureInput = new ProjectTreasureInput
					{
						ItemName = entry.ItemName ?? ""
					};
				}

				if (string.IsNullOrWhiteSpace(entry.TreasureInput.ItemName))
				{
					entry.TreasureInput.ItemName = entry.ItemName ?? "";
				}

				if (entry.WindowInput == null)
				{
					entry.WindowInput = new ProjectTreasureWindowInput
					{
						AdvisoryOnly = true
					};
				}

				if (entry.Classification == null)
				{
					entry.Classification = ProjectTreasureClassifier.Classify(entry.TreasureInput);
				}

				if (entry.WindowDecision == null)
				{
					entry.WindowDecision = ProjectTreasureWindowPolicy.Evaluate(entry.Classification, entry.WindowInput);
				}
			}
		}

		private static void PopulateDuplicateCounts(List<ProjectTreasureMatrixEntry> entries)
		{
			var exactCountsByCanonical = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			var roleCountsByBand = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

			foreach (var entry in entries)
			{
				var canonical = Canonicalize(entry.ItemName);
				var band = ProjectTreasureWindowPolicy.ResolveBand(entry.WindowInput);
				var role = entry.Classification != null ? entry.Classification.Role : ProjectTreasureRole.Unknown;
				var roleKey = band + "::" + role;

				int exactSeen;
				exactCountsByCanonical.TryGetValue(canonical, out exactSeen);
				entry.WindowInput.ExactDuplicateCount = exactSeen;
				exactCountsByCanonical[canonical] = exactSeen + 1;

				int roleSeen;
				roleCountsByBand.TryGetValue(roleKey, out roleSeen);
				entry.WindowInput.LocalRoleDuplicateCount = roleSeen;
				roleCountsByBand[roleKey] = roleSeen + 1;

				// Re-evaluate after duplicate fields have been populated.
				entry.WindowDecision = ProjectTreasureWindowPolicy.Evaluate(entry.Classification, entry.WindowInput);
			}
		}

		private static void AddEntry(ProjectTreasureMatrixReport report, ProjectTreasureMatrixEntry entry)
		{
			report.Entries.Add(entry);
			report.TotalEntries++;

			var classification = entry.Classification ?? new ProjectTreasureClassification();
			var decision = entry.WindowDecision ?? new ProjectTreasureWindowDecision();

			Increment(report.RoleCounts, classification.Role);
			Increment(report.CanonicalItemCounts, Canonicalize(entry.ItemName));

			var bandSummary = GetOrCreateBandSummary(report, decision.Band);
			AddToBandSummary(bandSummary, classification, decision);

			if (classification.IsCurrentBridgeRegression || decision.Fit == ProjectTreasureFit.Regression)
			{
				report.RegressionCount++;
				report.RegressionEntries.Add(entry);
				report.Warnings.Add("Current bridge regression detected: " + DisplayName(entry));
			}

			if (decision.NeedsCodeTraceBeforeMutation)
			{
				report.CodeTraceCount++;
				report.TraceFirstEntries.Add(entry);
			}

			if (decision.NeedsSpoilerRegressionCheck)
			{
				report.SpoilerRegressionCount++;
			}

			if (IsDuplicatePressureEntry(entry))
			{
				report.DuplicatePressureEntries.Add(entry);
			}
		}

		private static void AddToBandSummary(
			ProjectTreasureBandSummary summary,
			ProjectTreasureClassification classification,
			ProjectTreasureWindowDecision decision)
		{
			summary.TotalCount++;

			Increment(summary.RoleCounts, classification.Role);
			Increment(summary.StabilityCounts, classification.FutureStability);
			Increment(summary.DuplicateToleranceCounts, classification.DuplicateTolerance);
			Increment(summary.ImplementationCounts, classification.ImplementationImplication);

			switch (decision.Fit)
			{
				case ProjectTreasureFit.Preferred:
					summary.PreferredCount++;
					break;
				case ProjectTreasureFit.Allowed:
					summary.AllowedCount++;
					break;
				case ProjectTreasureFit.Discouraged:
					summary.DiscouragedCount++;
					break;
				case ProjectTreasureFit.Blocked:
					summary.BlockedCount++;
					break;
				case ProjectTreasureFit.Regression:
					summary.RegressionCount++;
					break;
			}

			if (!string.IsNullOrWhiteSpace(decision.Note))
			{
				summary.Warnings.Add(decision.Note);
			}
		}

		private static void AddBandDiagnoses(ProjectTreasureMatrixReport report)
		{
			foreach (var pair in report.BandSummaries.OrderBy(p => (int)p.Key))
			{
				var band = pair.Key;
				var summary = pair.Value;
				var target = ProjectTreasureWindowPolicy.GetTargetMix(band);

				if (summary.TotalCount <= 0)
				{
					continue;
				}

				var survivalRatio = summary.RoleRatio(ProjectTreasureRole.SurvivalCurrency);
				var tractionRatio = summary.RoleRatio(ProjectTreasureRole.TractionLoot);
				var tacticalRatio = summary.RoleRatio(ProjectTreasureRole.TacticalCarry);
				var identityRatio = summary.RoleRatio(ProjectTreasureRole.IdentityLoot);
				var apexRatio = summary.RoleRatio(ProjectTreasureRole.ApexLoot);

				if (survivalRatio > target.SurvivalCurrency + 0.15d)
				{
					report.Diagnoses.Add(band + ": survival currency is crowding the window; likely relief-before-revelation pressure.");
				}

				if ((band == ProjectTreasureWindowBand.EarlyRelief || band == ProjectTreasureWindowBand.EarlyTraction)
					&& tractionRatio < target.TractionLoot - 0.12d)
				{
					report.Diagnoses.Add(band + ": traction appears underfed; check early contact, resist, and anti-cruelty tools.");
				}

				if ((band == ProjectTreasureWindowBand.MidRevelation || band == ProjectTreasureWindowBand.LateCommitment)
					&& identityRatio < target.IdentityLoot - 0.10d)
				{
					report.Diagnoses.Add(band + ": identity loot appears underfed; revelation may be arriving too late or too weakly.");
				}

				if ((band == ProjectTreasureWindowBand.EarlyRelief || band == ProjectTreasureWindowBand.EarlyTraction)
					&& apexRatio > target.ApexLoot + 0.06d)
				{
					report.Diagnoses.Add(band + ": apex appears too early; inherited certainty risk.");
				}

				if (summary.DiscouragedCount + summary.BlockedCount > Math.Max(2, summary.TotalCount / 4))
				{
					report.Diagnoses.Add(band + ": many entries are discouraged/blocked by current policy; window composition may need tuning.");
				}
			}
		}

		private static void AddGlobalDiagnoses(ProjectTreasureMatrixReport report)
		{
			if (report.RegressionCount > 0)
			{
				report.Diagnoses.Add("Current bridge regressions are present. Fix regression before balance-tuning treasure.");
			}

			if (report.DuplicatePressureEntries.Count > Math.Max(2, report.TotalEntries / 6))
			{
				report.Diagnoses.Add("Duplicate pressure is high enough to investigate role-aware duplicate rules.");
			}

			var apexCount = Count(report.RoleCounts, ProjectTreasureRole.ApexLoot);
			if (apexCount > Math.Max(2, report.TotalEntries / 8))
			{
				report.Diagnoses.Add("Apex density is high; check whether late crowns are appearing as routine certainty.");
			}

			var identityCount = Count(report.RoleCounts, ProjectTreasureRole.IdentityLoot);
			var tacticalCount = Count(report.RoleCounts, ProjectTreasureRole.TacticalCarry);
			if (identityCount + tacticalCount < Math.Max(2, report.TotalEntries / 5))
			{
				report.Diagnoses.Add("Tactical/identity density is low; treasure may not be supporting inventory expression.");
			}
		}

		private static void ChooseNextLever(ProjectTreasureMatrixReport report)
		{
			if (report.RegressionCount > 0)
			{
				report.RecommendedNextLever = ProjectTreasureNextLever.BridgeRegression;
				report.RecommendedNextStep = "Fix current bridge regressions before interpreting treasure balance.";
				return;
			}

			if (report.CodeTraceCount > Math.Max(1, report.TotalEntries / 10))
			{
				report.RecommendedNextLever = ProjectTreasureNextLever.CodeTrace;
				report.RecommendedNextStep = "Too many entries require trace-first handling; narrow the weird item/source list before tuning.";
				return;
			}

			if (report.DuplicatePressureEntries.Count > Math.Max(2, report.TotalEntries / 6))
			{
				report.RecommendedNextLever = ProjectTreasureNextLever.DuplicateRule;
				report.RecommendedNextStep = "Add role-aware duplicate controls or local-band duplicate dampening.";
				return;
			}

			var windowProblem = report.Diagnoses.Any(d =>
				d.IndexOf("identity loot appears underfed", StringComparison.OrdinalIgnoreCase) >= 0
				|| d.IndexOf("traction appears underfed", StringComparison.OrdinalIgnoreCase) >= 0
				|| d.IndexOf("apex appears too early", StringComparison.OrdinalIgnoreCase) >= 0);

			if (windowProblem)
			{
				report.RecommendedNextLever = ProjectTreasureNextLever.WindowEdit;
				report.RecommendedNextStep = "Adjust treasure role mix by window/depth before changing individual item identities.";
				return;
			}

			var reliefProblem = report.Diagnoses.Any(d =>
				d.IndexOf("survival currency is crowding", StringComparison.OrdinalIgnoreCase) >= 0);

			if (reliefProblem)
			{
				report.RecommendedNextLever = ProjectTreasureNextLever.WeightEdit;
				report.RecommendedNextStep = "Downweight repeated relief/survival currency in affected windows.";
				return;
			}

			if (report.SpoilerRegressionCount > 0)
			{
				report.RecommendedNextLever = ProjectTreasureNextLever.SpoilerRegression;
				report.RecommendedNextStep = "Run spoiler regression checks for current bridge items/sources.";
				return;
			}

			report.RecommendedNextLever = ProjectTreasureNextLever.NoAction;
			report.RecommendedNextStep = "No obvious matrix-level issue detected. Use a short probe run if lived validation is needed.";
		}

		private static bool IsDuplicatePressureEntry(ProjectTreasureMatrixEntry entry)
		{
			if (entry == null || entry.WindowInput == null || entry.Classification == null)
			{
				return false;
			}

			var pressure = Math.Max(entry.WindowInput.LocalRoleDuplicateCount, entry.WindowInput.ExactDuplicateCount);

			switch (entry.Classification.DuplicateTolerance)
			{
				case ProjectDuplicateTolerance.VeryLow:
					return pressure >= 1;
				case ProjectDuplicateTolerance.Low:
					return pressure >= 2;
				case ProjectDuplicateTolerance.Medium:
					return pressure >= 3;
				case ProjectDuplicateTolerance.High:
					return pressure >= 4;
				default:
					return false;
			}
		}

		private static ProjectTreasureBandSummary GetOrCreateBandSummary(
			ProjectTreasureMatrixReport report,
			ProjectTreasureWindowBand band)
		{
			ProjectTreasureBandSummary summary;
			if (!report.BandSummaries.TryGetValue(band, out summary))
			{
				summary = new ProjectTreasureBandSummary(band);
				report.BandSummaries[band] = summary;
			}

			return summary;
		}

		private static string DisplayName(ProjectTreasureMatrixEntry entry)
		{
			if (entry == null)
			{
				return "(null)";
			}

			if (!string.IsNullOrWhiteSpace(entry.ItemName))
			{
				return entry.ItemName;
			}

			if (entry.TreasureInput != null && !string.IsNullOrWhiteSpace(entry.TreasureInput.ItemName))
			{
				return entry.TreasureInput.ItemName;
			}

			return "(unnamed treasure entry)";
		}

		private static string Canonicalize(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return "";
			}

			var chars = value
				.Trim()
				.ToUpperInvariant()
				.Where(c => char.IsLetterOrDigit(c) || c == '!' || c == '?')
				.ToArray();

			return new string(chars);
		}

		private static void Increment<T>(Dictionary<T, int> dict, T key)
		{
			int value;
			dict.TryGetValue(key, out value);
			dict[key] = value + 1;
		}

		private static int Count<T>(Dictionary<T, int> dict, T key)
		{
			int value;
			return dict.TryGetValue(key, out value) ? value : 0;
		}

		private static IEnumerable<T> EnumValues<T>()
		{
			return Enum.GetValues(typeof(T)).Cast<T>();
		}
	}
}
