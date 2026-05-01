using System.Collections.Generic;
using System.Linq;
using FF1Lib.Helpers;

namespace FF1Lib
{
	/// <summary>
	/// Project-owned Power Rod payload policy.
	///
	/// Current bridge intent:
	/// Power Rod guarantees one seal/fight-shaping permission:
	/// - FAST
	/// - LOK2
	///
	/// No healing fallback and no broad crown lottery for now.
	/// This keeps Power Rod as a mercy/seal stabilizer rather than a win-more item.
	/// </summary>
	internal static class ProjectCrownSpellPayloads
	{
		internal static bool TryGetPowerRodSealSpell(
			IList<MagicSpell> allSpells,
			SpellHelper spellHelper,
			MT19337 rng,
			out MagicSpell selectedSpell)
		{
			selectedSpell = default(MagicSpell);

			var candidateIndexes = new List<int>();

			// FAST: any generated spell currently using the FAST routine.
			candidateIndexes.AddRange(
				spellHelper
					.FindSpells(SpellRoutine.Fast, SpellTargeting.Any)
					.Select(spell => (int)spell.Id - 0xB0));

			// LOK2: the all-enemies lock routine. Avoid one-enemy LOCK.
			candidateIndexes.AddRange(
				spellHelper
					.FindSpells(SpellRoutine.Lock, SpellTargeting.AllEnemies)
					.Select(spell => (int)spell.Id - 0xB0));

			candidateIndexes = candidateIndexes
				.Where(index => index >= 0 && index < allSpells.Count)
				.Distinct()
				.ToList();

			if (candidateIndexes.Count == 0)
			{
				return false;
			}

			var selectedIndex = candidateIndexes[Rng.Between(rng, 0, candidateIndexes.Count - 1)];
			selectedSpell = allSpells[selectedIndex];
			return true;
		}
	}
}
