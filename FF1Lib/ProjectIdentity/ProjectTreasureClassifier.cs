using System;
using System.Collections.Generic;
using System.Linq;

namespace FF1Lib
{
	public enum ProjectTreasureRole
	{
		Unknown = 0,
		SurvivalCurrency,
		TractionLoot,
		TacticalCarry,
		IdentityLoot,
		ApexLoot,
		AlteredEchoLoot
	}

	public enum ProjectLiveInventoryRead
	{
		Unknown = 0,
		EquipNow,
		CarriedLive,
		BackupUseful,
		DeadBudget
	}

	public enum ProjectFutureStability
	{
		Unknown = 0,
		Stable,
		CurrentBridgeStable,
		LikelyLimited,
		LikelyRelocated,
		LikelyConsolidated,
		Volatile,
		TraceFirst,
		OldBehaviorDoNotUse
	}

	public enum ProjectDuplicateTolerance
	{
		Unknown = 0,
		High,
		Medium,
		Low,
		VeryLow
	}

	public enum ProjectImplementationImplication
	{
		Unknown = 0,
		LeaveAlone,
		PoolEdit,
		WeightEdit,
		WindowEdit,
		DuplicateRule,
		SpoilerRegressionOnly,
		TraceFirst,
		Regression
	}

	public sealed class ProjectTreasureInput
	{
		public string ItemName { get; set; }
		public string CastName { get; set; }
		public string ResistText { get; set; }

		public bool IsWeapon { get; set; }
		public bool IsArmor { get; set; }
		public bool HasActiveCast { get; set; }

		// Optional observed-state fields, usually filled from Witness/run logs rather than spoiler data.
		public bool Found { get; set; }
		public bool Carried { get; set; }
		public bool Equipped { get; set; }
		public bool Used { get; set; }

		public ProjectTreasureInput()
		{
			ItemName = "";
			CastName = "";
			ResistText = "";
		}
	}

	public sealed class ProjectTreasureClassification
	{
		public ProjectTreasureRole Role { get; set; }
		public ProjectLiveInventoryRead LiveInventoryRead { get; set; }
		public ProjectFutureStability FutureStability { get; set; }
		public ProjectDuplicateTolerance DuplicateTolerance { get; set; }
		public ProjectImplementationImplication ImplementationImplication { get; set; }

		public bool IsCurrentBridgeRegression { get; set; }
		public string Note { get; set; }

		public ProjectTreasureClassification()
		{
			Role = ProjectTreasureRole.Unknown;
			LiveInventoryRead = ProjectLiveInventoryRead.Unknown;
			FutureStability = ProjectFutureStability.Unknown;
			DuplicateTolerance = ProjectDuplicateTolerance.Unknown;
			ImplementationImplication = ProjectImplementationImplication.Unknown;
			IsCurrentBridgeRegression = false;
			Note = "";
		}
	}

	public static class ProjectTreasureClassifier
	{
		private static readonly HashSet<string> CleanSupportWeaponCasts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"CUR2",
			"HEAL",
			"SOFT",
			"LAMP",
			"AMUT",
			"MUTE",
			"SLEP",
			"DARK",
			"HOLD",
			"CONF",
			"LOK2"
		};

		private static readonly HashSet<string> RemovedGenericSupportCasts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"CURE",
			"LOCK",
			"FAST"
		};

		private static readonly HashSet<string> VolatileBuffFamilyCasts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"FOG",
			"FOG2",
			"RUSE",
			"INV",
			"INV2",
			"INVS",
			"FAST",
			"SABR",
			"TMPR"
		};

		private static readonly HashSet<string> BroadAoeDamageCasts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"LIT2",
			"LIT3",
			"FIR2",
			"FIR3",
			"ICE2",
			"ICE3",
			"BANE",
			"NUKE",
			"FADE",
			"HRM2",
			"HRM3"
		};

		public static ProjectTreasureClassification Classify(ProjectTreasureInput input)
		{
			if (input == null)
			{
				throw new ArgumentNullException(nameof(input));
			}

			var item = Normalize(input.ItemName);
			var cast = Normalize(input.CastName);
			var resist = Normalize(input.ResistText);

			var result = new ProjectTreasureClassification();

			ApplyWitnessLiveState(input, result);
			ApplyCurrentBridgeRegressionChecks(item, cast, resist, input, result);

			// Current bridge distorted items first, so older generic cast logic cannot override them.
			if (IsPowerRod(item))
			{
				result.Role = ProjectTreasureRole.TacticalCarry;
				result.FutureStability = ProjectFutureStability.CurrentBridgeStable;
				result.DuplicateTolerance = ProjectDuplicateTolerance.VeryLow;
				result.ImplementationImplication = result.IsCurrentBridgeRegression
					? ProjectImplementationImplication.Regression
					: ProjectImplementationImplication.SpoilerRegressionOnly;
				result.Note = "Power Rod current bridge: FAST / LOK2 seal-mercy carrier. Do not use older RUSE / INV / INV2 defensive-carrier model for current planning.";
				return result;
			}

			if (IsPowerGauntlets(item))
			{
				result.Role = ProjectTreasureRole.TractionLoot;
				result.FutureStability = ProjectFutureStability.CurrentBridgeStable;
				result.DuplicateTolerance = ProjectDuplicateTolerance.Low;
				result.ImplementationImplication = result.IsCurrentBridgeRegression
					? ProjectImplementationImplication.Regression
					: ProjectImplementationImplication.SpoilerRegressionOnly;
				result.Note = "Power Gauntlets current bridge: worn Death + Poison resist package, no active cast.";
				return result;
			}

			ApplyKnownIconicItemClassification(item, cast, input, result);

			if (result.Role == ProjectTreasureRole.Unknown)
			{
				ApplyCastFamilyClassification(cast, input, result);
			}

			if (result.Role == ProjectTreasureRole.Unknown)
			{
				ApplyFallbackClassification(input, result);
			}

			return result;
		}

		public static bool IsCleanSupportWeaponCast(string castName)
		{
			return CleanSupportWeaponCasts.Contains(Normalize(castName));
		}

		public static bool IsRemovedGenericSupportCast(string castName)
		{
			return RemovedGenericSupportCasts.Contains(Normalize(castName));
		}

		public static bool IsVolatileBuffFamilyCast(string castName)
		{
			return VolatileBuffFamilyCasts.Contains(Normalize(castName));
		}

		public static bool IsBroadAoeDamageCast(string castName)
		{
			return BroadAoeDamageCasts.Contains(Normalize(castName));
		}

		private static void ApplyWitnessLiveState(ProjectTreasureInput input, ProjectTreasureClassification result)
		{
			if (input.Equipped)
			{
				result.LiveInventoryRead = ProjectLiveInventoryRead.EquipNow;
			}
			else if (input.Carried || input.Used)
			{
				result.LiveInventoryRead = ProjectLiveInventoryRead.CarriedLive;
			}
			else if (input.Found)
			{
				result.LiveInventoryRead = ProjectLiveInventoryRead.BackupUseful;
			}
			else
			{
				result.LiveInventoryRead = ProjectLiveInventoryRead.Unknown;
			}
		}

		private static void ApplyCurrentBridgeRegressionChecks(
			string item,
			string cast,
			string resist,
			ProjectTreasureInput input,
			ProjectTreasureClassification result)
		{
			// Current bridge rule: armor should not provide active item casts.
			if (input.IsArmor && input.HasActiveCast)
			{
				result.IsCurrentBridgeRegression = true;
				result.ImplementationImplication = ProjectImplementationImplication.Regression;
				result.Note = AppendNote(result.Note, "Regression: armor active cast detected while armor active casts should be disabled.");
			}

			// Current bridge rule: generic clean support weapon casts exclude CURE / LOCK / FAST.
			if (input.IsWeapon && input.HasActiveCast && RemovedGenericSupportCasts.Contains(cast) && !IsPowerRod(item))
			{
				result.IsCurrentBridgeRegression = true;
				result.ImplementationImplication = ProjectImplementationImplication.Regression;
				result.Note = AppendNote(result.Note, "Regression or special-lane leak: generic weapon cast is CURE / LOCK / FAST.");
			}

			// Current bridge rule: Power Rod should be FAST / LOK2 only.
			if (IsPowerRod(item) && input.HasActiveCast && !(cast == "FAST" || cast == "LOK2"))
			{
				result.IsCurrentBridgeRegression = true;
				result.ImplementationImplication = ProjectImplementationImplication.Regression;
				result.Note = AppendNote(result.Note, "Regression: Power Rod payload is not FAST or LOK2.");
			}

			// Current bridge rule: Power Gauntlets should not actively cast and should carry Death + Poison resist package.
			if (IsPowerGauntlets(item))
			{
				if (input.HasActiveCast)
				{
					result.IsCurrentBridgeRegression = true;
					result.ImplementationImplication = ProjectImplementationImplication.Regression;
					result.Note = AppendNote(result.Note, "Regression: Power Gauntlets has active casting.");
				}

				if (!(resist.Contains("DEATH") && resist.Contains("POISON")))
				{
					result.IsCurrentBridgeRegression = true;
					result.ImplementationImplication = ProjectImplementationImplication.Regression;
					result.Note = AppendNote(result.Note, "Regression: Power Gauntlets missing Death + Poison resist package.");
				}
			}
		}

		private static void ApplyKnownIconicItemClassification(
			string item,
			string cast,
			ProjectTreasureInput input,
			ProjectTreasureClassification result)
		{
			if (ContainsAny(item, "RIBBON", "RIBBO"))
			{
				SetIfUnset(result, ProjectTreasureRole.ApexLoot, ProjectFutureStability.Stable, ProjectDuplicateTolerance.VeryLow, ProjectImplementationImplication.WindowEdit,
					"Ribbon: apex inherited-certainty protection. Keep scarce and late enough that it crowns rather than erases pressure.");
				return;
			}

			if (ContainsAny(item, "MASAMUNE", "MASMU"))
			{
				SetIfUnset(result, ProjectTreasureRole.ApexLoot, ProjectFutureStability.Stable, ProjectDuplicateTolerance.VeryLow, ProjectImplementationImplication.WindowEdit,
					"Masamune: universal offensive apex. Highest convergence risk because it patches damage gaps broadly.");
				return;
			}

			if (ContainsAny(item, "XCALB", "XCALBER", "XCAL"))
			{
				SetIfUnset(result, ProjectTreasureRole.ApexLoot, ProjectFutureStability.Stable, ProjectDuplicateTolerance.VeryLow, ProjectImplementationImplication.WindowEdit,
					"Xcalber: directed apex. Healthier than Masamune because it crowns a specific body.");
				return;
			}

			if (ContainsAny(item, "KATANA"))
			{
				SetIfUnset(result, ProjectTreasureRole.IdentityLoot, ProjectFutureStability.Stable, ProjectDuplicateTolerance.Low, ProjectImplementationImplication.LeaveAlone,
					"Katana: role-specific attacker identity.");
				return;
			}

			if (ContainsAny(item, "CATCLAW", "CATCL"))
			{
				SetIfUnset(result, ProjectTreasureRole.IdentityLoot, ProjectFutureStability.Stable, ProjectDuplicateTolerance.Low, ProjectImplementationImplication.LeaveAlone,
					"CatClaw: sharp identity weapon / closer-defining item.");
				return;
			}

			if (ContainsAny(item, "WHITESHIRT", "WHITE", "WHIT"))
			{
				SetIfUnset(result, ProjectTreasureRole.IdentityLoot, ProjectFutureStability.LikelyLimited, ProjectDuplicateTolerance.Low, ProjectImplementationImplication.WeightEdit,
					"WhiteShirt: current survival-grammar identity carry, but buff-family infrastructure is future-volatile.");
				return;
			}

			if (ContainsAny(item, "BLACKSHIRT", "BLACK", "BLAC"))
			{
				SetIfUnset(result, ProjectTreasureRole.IdentityLoot, ProjectFutureStability.Volatile, ProjectDuplicateTolerance.Low, ProjectImplementationImplication.WeightEdit,
					"BlackShirt: current offensive/tactical identity carry; broad reusable AOE damage remains future-volatile.");
				return;
			}

			if (ContainsAny(item, "DEFENSE", "DEFEN"))
			{
				SetIfUnset(result, ProjectTreasureRole.TacticalCarry, ProjectFutureStability.LikelyLimited, ProjectDuplicateTolerance.Low, ProjectImplementationImplication.WeightEdit,
					"Defense: tactical carry that can become identity weapon. RUSE-style inherited certainty remains future-limited.");
				return;
			}

			if (ContainsAny(item, "ZEUS"))
			{
				SetIfUnset(result, ProjectTreasureRole.TacticalCarry, ProjectFutureStability.Volatile, ProjectDuplicateTolerance.Medium, ProjectImplementationImplication.WeightEdit,
					"Zeus Gauntlet: tactical carry / matchup damage. Watch broad reusable AOE value.");
				return;
			}

			if (ContainsAny(item, "HEAL"))
			{
				SetIfUnset(result, ProjectTreasureRole.TractionLoot, ProjectFutureStability.LikelyConsolidated, ProjectDuplicateTolerance.Medium, ProjectImplementationImplication.DuplicateRule,
					"Heal gear: anti-cruelty traction and sustain. Useful, but repeated sustain can become relief-over-revelation.");
				return;
			}

			if (ContainsAny(item, "AEGIS", "AEGI", "PRORING", "PRO RING", "PROCA", "PROCAPE"))
			{
				SetIfUnset(result, ProjectTreasureRole.TractionLoot, ProjectFutureStability.Stable, ProjectDuplicateTolerance.Low, ProjectImplementationImplication.LeaveAlone,
					"Protection item: traction / anti-cruelty protection. Good if narrow; risky if it becomes broad inherited safety.");
				return;
			}

			if (ContainsAny(item, "OPAL", "DRAGON"))
			{
				SetIfUnset(result, ProjectTreasureRole.ApexLoot, ProjectFutureStability.Stable, ProjectDuplicateTolerance.VeryLow, ProjectImplementationImplication.WindowEdit,
					"Heavy defensive apex. Keep scarce; avoid early inherited-certainty shell.");
				return;
			}
		}

		private static void ApplyCastFamilyClassification(
			string cast,
			ProjectTreasureInput input,
			ProjectTreasureClassification result)
		{
			if (string.IsNullOrEmpty(cast))
			{
				return;
			}

			if (CleanSupportWeaponCasts.Contains(cast))
			{
				SetIfUnset(result, ProjectTreasureRole.TacticalCarry, ProjectFutureStability.CurrentBridgeStable, ProjectDuplicateTolerance.Medium, ProjectImplementationImplication.LeaveAlone,
					"Clean support weapon cast: bridge-stable tactical carry candidate.");
				return;
			}

			if (VolatileBuffFamilyCasts.Contains(cast))
			{
				SetIfUnset(result, ProjectTreasureRole.IdentityLoot, ProjectFutureStability.Volatile, ProjectDuplicateTolerance.Low, ProjectImplementationImplication.WeightEdit,
					"Volatile buff-family source. Record current power honestly but do not canonize as long-term infrastructure.");
				return;
			}

			if (BroadAoeDamageCasts.Contains(cast))
			{
				SetIfUnset(result, ProjectTreasureRole.TacticalCarry, ProjectFutureStability.Volatile, ProjectDuplicateTolerance.Medium, ProjectImplementationImplication.WeightEdit,
					"Broad AOE damage cast. Useful tactical carry, but future-volatile if reusable AOE item damage gets cleaned up.");
				return;
			}
		}

		private static void ApplyFallbackClassification(ProjectTreasureInput input, ProjectTreasureClassification result)
		{
			if (input.HasActiveCast)
			{
				SetIfUnset(result, ProjectTreasureRole.TacticalCarry, ProjectFutureStability.Unknown, ProjectDuplicateTolerance.Medium, ProjectImplementationImplication.TraceFirst,
					"Unclassified cast item. Treat as tactical carry candidate until spoiler/use data proves otherwise.");
				return;
			}

			if (input.IsWeapon || input.IsArmor)
			{
				SetIfUnset(result, ProjectTreasureRole.TractionLoot, ProjectFutureStability.Unknown, ProjectDuplicateTolerance.Medium, ProjectImplementationImplication.WeightEdit,
					"Unclassified equipment. Start as traction loot until live-use or stat shape proves identity/apex role.");
				return;
			}

			SetIfUnset(result, ProjectTreasureRole.SurvivalCurrency, ProjectFutureStability.Stable, ProjectDuplicateTolerance.High, ProjectImplementationImplication.PoolEdit,
				"Fallback non-equipment treasure. Treat as survival currency unless proven otherwise.");
		}

		private static void SetIfUnset(
			ProjectTreasureClassification result,
			ProjectTreasureRole role,
			ProjectFutureStability stability,
			ProjectDuplicateTolerance duplicateTolerance,
			ProjectImplementationImplication implication,
			string note)
		{
			if (result.Role == ProjectTreasureRole.Unknown)
			{
				result.Role = role;
			}

			if (result.FutureStability == ProjectFutureStability.Unknown)
			{
				result.FutureStability = stability;
			}

			if (result.DuplicateTolerance == ProjectDuplicateTolerance.Unknown)
			{
				result.DuplicateTolerance = duplicateTolerance;
			}

			if (result.ImplementationImplication == ProjectImplementationImplication.Unknown)
			{
				result.ImplementationImplication = implication;
			}

			result.Note = AppendNote(result.Note, note);
		}

		private static bool IsPowerRod(string item)
		{
			return ContainsAny(item, "POWERROD", "POWER ROD", "POWERR", "PWRROD");
		}

		private static bool IsPowerGauntlets(string item)
		{
			return ContainsAny(item, "POWERGAUNTLETS", "POWER GAUNTLETS", "POWERG", "PWRGAUNT", "PWRG");
		}

		private static bool ContainsAny(string value, params string[] tokens)
		{
			if (string.IsNullOrEmpty(value))
			{
				return false;
			}

			return tokens.Any(t => value.IndexOf(Normalize(t), StringComparison.OrdinalIgnoreCase) >= 0);
		}

		private static string Normalize(string value)
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
