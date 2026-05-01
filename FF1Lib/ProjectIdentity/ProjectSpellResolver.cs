using System;
using System.Collections.Generic;
using System.Linq;
using FF1Lib.Helpers;

namespace FF1Lib.ProjectIdentity
{
	/// <summary>
	/// Suggested path:
	/// FF1Lib/ProjectIdentity/ProjectSpellResolver.cs
	///
	/// Purpose:
	/// Resolves project-owned spell pools against the current generated spellbook.
	///
	/// Why this exists:
	/// In shuffled / crafted spellbooks, fixed Spell enum values refer to slots.
	/// They do not reliably mean "the current spell named FAST" or "the current
	/// spell behaving like LIFE." This helper resolves against current spell data.
	/// </summary>
	public static class ProjectSpellResolver
	{
		public static List<MagicSpell> ResolveMagicSpellsByName(
			IReadOnlyList<MagicSpell> allSpells,
			IEnumerable<string> spellNames)
		{
			if (allSpells == null)
			{
				throw new ArgumentNullException(nameof(allSpells));
			}

			if (spellNames == null)
			{
				throw new ArgumentNullException(nameof(spellNames));
			}

			var resolved = new List<MagicSpell>();

			foreach (var name in spellNames)
			{
				var spell = FindMagicSpellByName(allSpells, name);
				if (spell != null)
				{
					resolved.Add(spell);
				}
			}

			return DistinctMagicSpells(resolved);
		}

		public static List<Spell> ResolveSpellIdsByName(
			SpellHelper spellHelper,
			IEnumerable<string> spellNames)
		{
			if (spellHelper == null)
			{
				throw new ArgumentNullException(nameof(spellHelper));
			}

			if (spellNames == null)
			{
				throw new ArgumentNullException(nameof(spellNames));
			}

			var allSpells = spellHelper.GetAllSpells().ToList();
			var resolved = new List<Spell>();

			foreach (var name in spellNames)
			{
				var spell = FindSpellByName(allSpells, name);
				if (spell.HasValue)
				{
					resolved.Add(spell.Value);
				}
			}

			return resolved.Distinct().ToList();
		}

		public static MagicSpell FindMagicSpellByName(
			IReadOnlyList<MagicSpell> allSpells,
			string spellName)
		{
			if (allSpells == null || string.IsNullOrWhiteSpace(spellName))
			{
				return null;
			}

			var normalized = NormalizeSpellName(spellName);

			return allSpells.FirstOrDefault(spell =>
				NormalizeSpellName(spell.Name) == normalized);
		}

		public static Spell? FindSpellByName(
			IEnumerable<(Spell Id, MagicSpell Info)> allSpells,
			string spellName)
		{
			if (allSpells == null || string.IsNullOrWhiteSpace(spellName))
			{
				return null;
			}

			var normalized = NormalizeSpellName(spellName);

			var match = allSpells.FirstOrDefault(spell =>
				NormalizeSpellName(spell.Info.Name) == normalized);

			if (match.Info == null)
			{
				return null;
			}

			return match.Id;
		}

		public static string NormalizeSpellName(string spellName)
		{
			return (spellName ?? string.Empty)
				.Trim()
				.Replace(" ", string.Empty)
				.ToUpperInvariant();
		}

		private static List<MagicSpell> DistinctMagicSpells(IEnumerable<MagicSpell> spells)
		{
			var seen = new HashSet<byte>();
			var result = new List<MagicSpell>();

			foreach (var spell in spells)
			{
				if (spell == null)
				{
					continue;
				}

				if (seen.Add(spell.Index))
				{
					result.Add(spell);
				}
			}

			return result;
		}
	}
}
