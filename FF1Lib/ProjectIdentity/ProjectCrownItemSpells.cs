using System;
using System.Collections.Generic;
using System.Linq;
using FF1Lib.Helpers;

namespace FF1Lib.ProjectIdentity
{
	/// <summary>
	/// Suggested path:
	/// FF1Lib/ProjectIdentity/ProjectCrownItemSpells.cs
	///
	/// Purpose:
	/// Defines the current project-owned crown-item spell payload.
	///
	/// Crown pool:
	/// NUKE, LIFE, FADE, CUR4, FAST
	///
	/// Important:
	/// This version can resolve through SpellHelper, matching the current generated
	/// spellbook identity used by Weaponizer-style code.
	///
	/// Why:
	/// Fixed Spell enum values are slots. In shuffled/crafted spellbooks, slot identity
	/// can drift. Power Rod / crown item should choose the current generated spell
	/// named NUKE / LIFE / FADE / CUR4 / FAST, not whichever behavior happens to live
	/// in an old vanilla slot.
	/// </summary>
	public static class ProjectCrownItemSpells
	{
		public static IReadOnlyList<string> CrownSpellNames { get; } = new List<string>
		{
			"NUKE",
			"LIFE",
			"FADE",
			"CUR4",
			"FAST",
		};

		/// <summary>
		/// Preferred Power Rod / crown-item picker.
		///
		/// Returns both:
		/// - Id: the current Spell enum slot where the named spell lives
		/// - Info: the MagicSpell data for WriteItemSpellData
		/// </summary>
		public static (Spell Id, MagicSpell Info) PickPowerRodCrownSpell(
			SpellHelper spellHelper,
			GuaranteedDefenseItem guaranteedDefenseItem,
			MT19337 rng)
		{
			if (spellHelper == null)
			{
				throw new ArgumentNullException(nameof(spellHelper));
			}

			if (guaranteedDefenseItem == GuaranteedDefenseItem.None)
			{
				throw new ArgumentException("Cannot pick a crown spell when GuaranteedDefenseItem is None.", nameof(guaranteedDefenseItem));
			}

			var crownSpells = GetCrownSpellRecords(spellHelper);

			if (crownSpells.Count == 0)
			{
				throw new InvalidOperationException("No crown spells were available in the current generated spellbook.");
			}

			if (rng == null)
			{
				return PickFallbackByLegacySetting(crownSpells, guaranteedDefenseItem);
			}

			return crownSpells[Rng.Between(rng, 0, crownSpells.Count - 1)];
		}

		/// <summary>
		/// Gets current generated crown spell records by displayed/current spell name.
		/// </summary>
		public static List<(Spell Id, MagicSpell Info)> GetCrownSpellRecords(SpellHelper spellHelper)
		{
			if (spellHelper == null)
			{
				throw new ArgumentNullException(nameof(spellHelper));
			}

			var allSpells = spellHelper.GetAllSpells().ToList();
			var resolved = new List<(Spell Id, MagicSpell Info)>();

			foreach (var spellName in CrownSpellNames)
			{
				var normalized = ProjectSpellResolver.NormalizeSpellName(spellName);

				var match = allSpells.FirstOrDefault(spell =>
					spell.Info != null &&
					ProjectSpellResolver.NormalizeSpellName(spell.Info.Name) == normalized);

				if (match.Info != null)
				{
					resolved.Add(match);
				}
			}

			return resolved
				.GroupBy(spell => spell.Id)
				.Select(group => group.First())
				.ToList();
		}

		/// <summary>
		/// Older MagicSpell-list picker retained for compile compatibility.
		/// Prefer PickPowerRodCrownSpell(SpellHelper, ...).
		/// </summary>
		public static MagicSpell PickPowerRodCrownSpell(
			IReadOnlyList<MagicSpell> allSpells,
			GuaranteedDefenseItem guaranteedDefenseItem,
			MT19337 rng)
		{
			if (allSpells == null)
			{
				throw new ArgumentNullException(nameof(allSpells));
			}

			if (guaranteedDefenseItem == GuaranteedDefenseItem.None)
			{
				throw new ArgumentException("Cannot pick a crown spell when GuaranteedDefenseItem is None.", nameof(guaranteedDefenseItem));
			}

			var crownSpells = GetCrownMagicSpells(allSpells);

			if (crownSpells.Count == 0)
			{
				throw new InvalidOperationException("No crown spells were available in the current spell table.");
			}

			if (rng == null)
			{
				return PickFallbackByLegacySetting(crownSpells, guaranteedDefenseItem);
			}

			return crownSpells[Rng.Between(rng, 0, crownSpells.Count - 1)];
		}

		/// <summary>
		/// Older MagicSpell-list resolver retained for compile compatibility.
		/// Prefer GetCrownSpellRecords(SpellHelper).
		/// </summary>
		public static List<MagicSpell> GetCrownMagicSpells(IReadOnlyList<MagicSpell> allSpells)
		{
			return ProjectSpellResolver.ResolveMagicSpellsByName(
				allSpells,
				CrownSpellNames);
		}

		private static (Spell Id, MagicSpell Info) PickFallbackByLegacySetting(
			IReadOnlyList<(Spell Id, MagicSpell Info)> crownSpells,
			GuaranteedDefenseItem guaranteedDefenseItem)
		{
			var index = guaranteedDefenseItem switch
			{
				GuaranteedDefenseItem.INV => 0,    // NUKE
				GuaranteedDefenseItem.INV2 => 1,   // LIFE
				GuaranteedDefenseItem.RUSE => 2,   // FADE
				GuaranteedDefenseItem.Any => 3,    // CUR4
				GuaranteedDefenseItem.Random => 4, // FAST
				_ => 0,
			};

			index = Math.Max(0, Math.Min(index, crownSpells.Count - 1));
			return crownSpells[index];
		}

		private static MagicSpell PickFallbackByLegacySetting(
			IReadOnlyList<MagicSpell> crownSpells,
			GuaranteedDefenseItem guaranteedDefenseItem)
		{
			var index = guaranteedDefenseItem switch
			{
				GuaranteedDefenseItem.INV => 0,    // NUKE
				GuaranteedDefenseItem.INV2 => 1,   // LIFE
				GuaranteedDefenseItem.RUSE => 2,   // FADE
				GuaranteedDefenseItem.Any => 3,    // CUR4
				GuaranteedDefenseItem.Random => 4, // FAST
				_ => 0,
			};

			index = Math.Max(0, Math.Min(index, crownSpells.Count - 1));
			return crownSpells[index];
		}
	}
}
