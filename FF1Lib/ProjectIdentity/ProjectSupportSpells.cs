using System;
using System.Collections.Generic;
using System.Linq;
using FF1Lib.Helpers;

namespace FF1Lib.ProjectIdentity
{
	/// <summary>
	/// Suggested path:
	/// FF1Lib/ProjectIdentity/ProjectSupportSpells.cs
	///
	/// Purpose:
	/// Defines project-owned clean support / item-cast spell pools.
	///
	/// Important:
	/// This resolves by current generated spell name, not fixed enum slot.
	///
	/// Current pool note:
	/// - LOCK is intentionally removed for now.
	/// - LOK2 remains as the broader contact-shaping / setup permission.
	/// - CURE is replaced by CUR2 to make item-cast healing less trivial and
	///   more worth carrying as an active weapon-side option.
	/// </summary>
	public static class ProjectSupportSpells
	{
		public static IReadOnlyList<string> SupportCleanSpellNames { get; } = new List<string>
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

			"LOK2",

			"FAST",
		};

		/// <summary>
		/// Main ItemMagic-facing method.
		/// Resolves the intended clean support spell names against the current generated spellbook.
		/// </summary>
		public static List<MagicSpell> GetSupportCleanMagicSpells(
			IReadOnlyList<MagicSpell> allSpells,
			MT19337 rng,
			bool duplicateForAssignment = true,
			bool shuffle = true)
		{
			var selected = ProjectSpellResolver.ResolveMagicSpellsByName(
				allSpells,
				SupportCleanSpellNames);

			if (duplicateForAssignment)
			{
				selected = selected.Concat(selected).ToList();
			}

			if (shuffle)
			{
				selected.Shuffle(rng);
			}

			return selected;
		}

		/// <summary>
		/// Weaponizer-facing method.
		/// Resolves clean support spell names to current generated spell slots.
		/// </summary>
		public static List<Spell> GetSupportCleanSpellIds(
			SpellHelper spellHelper,
			bool duplicateForAssignment = false)
		{
			var selected = ProjectSpellResolver.ResolveSpellIdsByName(
				spellHelper,
				SupportCleanSpellNames);

			if (duplicateForAssignment)
			{
				selected.AddRange(selected.ToList());
			}

			return selected;
		}

		/// <summary>
		/// Legacy enum-slot helper retained only for compile compatibility.
		/// Prefer GetSupportCleanSpellIds(SpellHelper) for generated / shuffled spellbooks.
		/// </summary>
		public static List<Spell> GetSupportCleanSpellIds(bool duplicateForAssignment = false)
		{
			var selected = new List<Spell>
	{
		Spell.CUR2,
		Spell.HEAL,
		Spell.SOFT,
		Spell.LAMP,
		Spell.AMUT,
		Spell.MUTE,
		Spell.SLEP,
		Spell.DARK,
		Spell.HOLD,
		Spell.CONF,
		Spell.LOK2,
	};

			// existing duplicateForAssignment logic here, if any
			return selected;
		}
	}
}
