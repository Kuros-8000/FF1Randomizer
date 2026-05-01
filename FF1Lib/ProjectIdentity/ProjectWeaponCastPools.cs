using System;
using System.Collections.Generic;
using System.Linq;
using FF1Lib.Helpers;

namespace FF1Lib.ProjectIdentity
{
	/// <summary>
	/// Suggested path:
	/// FF1Lib/ProjectIdentity/ProjectWeaponCastPools.cs
	///
	/// Suggested namespace:
	/// FF1Lib.ProjectIdentity
	///
	/// Purpose:
	/// Defines project-owned weapon-side active item-cast pools.
	///
	/// Helper family:
	/// generation-facing source helper
	///
	/// What this does:
	/// - Centralizes weapon-side candidate spell lists that can later replace local
	///   inline lists inside Weaponizer.
	/// - Preserves "legacy-equivalent" shapes so wiring can be staged safely.
	/// - Adds project-intent pools for cleaner active hand-side cast identity.
	///
	/// What this does NOT do:
	/// - It does not mutate weapons directly.
	/// - It does not assign SpellIndex.
	/// - It does not write to ROM data.
	/// - It does not change ItemMagic, ArmorCrafter, or Weaponizer until wired.
	///
	/// Important implementation note:
	/// SpellHelper.FindSpells(...) returns tuple records:
	/// (Spell Id, MagicSpell Info)
	/// rather than plain MagicSpell values.
	/// </summary>
	public static class ProjectWeaponCastPools
	{
		/// <summary>
		/// Default generic weapon pool for project-owned active hand-side casts.
		/// </summary>
		public static List<Spell> ForGenericWeapon(SpellHelper spellHelper)
		{
			RequireSpellHelper(spellHelper);
			return ProjectSupportSpells.GetSupportCleanSpellIds(spellHelper);
		}

		/// <summary>
		/// Legacy enum-slot fallback retained for compile compatibility only.
		/// Prefer ForGenericWeapon(SpellHelper).
		/// </summary>
		public static List<Spell> ForGenericWeapon()
		{
			return ProjectSupportSpells.GetSupportCleanSpellIds();
		}

		/// <summary>
		/// Defense sword pool, project v1.
		/// </summary>
		public static List<Spell> ForDefenseSword(SpellHelper spellHelper)
		{
			RequireSpellHelper(spellHelper);

			return DistinctSpellIds(
				spellHelper.FindSpells(SpellRoutine.Ruse, SpellTargeting.Any)
					.Concat(spellHelper.FindSpells(SpellRoutine.ArmorUp, SpellTargeting.AllCharacters))
					.Concat(spellHelper.FindSpells(SpellRoutine.DefElement, SpellTargeting.Any)
						.Where(spell => spell.Info.status == SpellStatus.Any)));
		}

		/// <summary>
		/// Thor Hammer pool, project v1.
		/// </summary>
		public static List<Spell> ForThorHammer(SpellHelper spellHelper)
		{
			RequireSpellHelper(spellHelper);

			return DistinctSpellIds(
				spellHelper.FindSpells(SpellRoutine.Damage, SpellTargeting.AllEnemies)
					.Where(spell => spell.Info.elem != (byte)SpellElement.None));
		}

		/// <summary>
		/// Legacy-equivalent generic weapon pool.
		///
		/// This mirrors the current Weaponizer "otherSpells" shape as closely as possible.
		/// Use this if the first wiring pass should centralize code without changing behavior.
		/// </summary>
		public static List<Spell> ForGenericWeaponLegacyEquivalent(SpellHelper spellHelper)
		{
			RequireSpellHelper(spellHelper);

			return DistinctSpellIds(
				spellHelper.FindSpells(SpellRoutine.Damage, SpellTargeting.AllEnemies)
					.Where(spell => spell.Info.elem != (byte)SpellElement.None)
					.Concat(spellHelper.FindSpells(SpellRoutine.DamageUndead, SpellTargeting.AllEnemies))
					.Concat(spellHelper.FindSpells(SpellRoutine.Heal, SpellTargeting.AllCharacters))
					.Concat(spellHelper.FindSpells(SpellRoutine.ArmorUp, SpellTargeting.AllCharacters))
					.Concat(spellHelper.FindSpells(SpellRoutine.Lock, SpellTargeting.OneEnemy))
					.Concat(spellHelper.FindSpells(SpellRoutine.Lock, SpellTargeting.AllEnemies))
					.Concat(spellHelper.FindSpells(SpellRoutine.Ruse, SpellTargeting.Any)));
		}

		/// <summary>
		/// Elemental / prey-facing pressure pool.
		/// </summary>
		public static List<Spell> ForElementalPressureWeapon(SpellHelper spellHelper)
		{
			RequireSpellHelper(spellHelper);

			return DistinctSpellIds(
				spellHelper.FindSpells(SpellRoutine.Damage, SpellTargeting.AllEnemies)
					.Where(spell => spell.Info.elem != (byte)SpellElement.None)
					.Concat(spellHelper.FindSpells(SpellRoutine.DamageUndead, SpellTargeting.AllEnemies)));
		}

		/// <summary>
		/// Control/contact-shaping pool.
		/// </summary>
		public static List<Spell> ForControlWeapon(SpellHelper spellHelper)
		{
			RequireSpellHelper(spellHelper);

			return DistinctSpellIds(
				spellHelper.FindSpells(SpellRoutine.Lock, SpellTargeting.OneEnemy)
					.Concat(spellHelper.FindSpells(SpellRoutine.Lock, SpellTargeting.AllEnemies))
					.Concat(spellHelper.FindSpells(SpellRoutine.InflictStatus, SpellTargeting.OneEnemy))
					.Concat(spellHelper.FindSpells(SpellRoutine.InflictStatus, SpellTargeting.AllEnemies))
					.Concat(spellHelper.FindSpells(SpellRoutine.Slow, SpellTargeting.OneEnemy))
					.Concat(spellHelper.FindSpells(SpellRoutine.Slow, SpellTargeting.AllEnemies)));
		}

		/// <summary>
		/// Care / recovery weapon pool.
		/// </summary>
		public static List<Spell> ForCareWeapon(SpellHelper spellHelper)
		{
			RequireSpellHelper(spellHelper);

			return DistinctSpellIds(
				spellHelper.FindSpells(SpellRoutine.Heal, SpellTargeting.OneCharacter)
					.Concat(spellHelper.FindSpells(SpellRoutine.Heal, SpellTargeting.AllCharacters))
					.Concat(spellHelper.FindSpells(SpellRoutine.CureAilment, SpellTargeting.OneCharacter)));
		}

		/// <summary>
		/// Dispatch helper for future wiring / testing.
		/// </summary>
		public static List<Spell> ForKind(SpellHelper spellHelper, ProjectWeaponCastPoolKind kind)
		{
			return kind switch
			{
				ProjectWeaponCastPoolKind.GenericProjectSupport => ForGenericWeapon(),
				ProjectWeaponCastPoolKind.DefenseSword => ForDefenseSword(spellHelper),
				ProjectWeaponCastPoolKind.ThorHammer => ForThorHammer(spellHelper),
				ProjectWeaponCastPoolKind.GenericLegacyEquivalent => ForGenericWeaponLegacyEquivalent(spellHelper),
				ProjectWeaponCastPoolKind.ElementalPressure => ForElementalPressureWeapon(spellHelper),
				ProjectWeaponCastPoolKind.Control => ForControlWeapon(spellHelper),
				ProjectWeaponCastPoolKind.Care => ForCareWeapon(spellHelper),
				_ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown project weapon cast-pool kind."),
			};
		}

		private static void RequireSpellHelper(SpellHelper spellHelper)
		{
			if (spellHelper == null)
			{
				throw new ArgumentNullException(nameof(spellHelper));
			}
		}

		private static List<Spell> DistinctSpellIds(IEnumerable<(Spell Id, MagicSpell Info)> spells)
		{
			if (spells == null)
			{
				return new List<Spell>();
			}

			return spells
				.Select(spell => spell.Id)
				.Distinct()
				.ToList();
		}
	}

	public enum ProjectWeaponCastPoolKind
	{
		GenericProjectSupport = 0,
		DefenseSword = 1,
		ThorHammer = 2,
		GenericLegacyEquivalent = 3,
		ElementalPressure = 4,
		Control = 5,
		Care = 6,
	}
}
