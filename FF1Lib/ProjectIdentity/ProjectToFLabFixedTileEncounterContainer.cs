using System;
using System.Collections.Generic;

namespace FF1Lib.ProjectIdentity
{
	/// <summary>
	/// Project Identity helper for Temple of Fiends fixed-tile encounter pressure.
	///
	/// Boundary:
	/// - Chests define tools/access and are owned by ProjectToFLabContainer.
	/// - Fixed battle tiles define pressure and are owned by this helper.
	///
	/// Default behavior is Vanilla. That makes this a safe staged container first:
	/// wiring can exist without changing the current Gargoyle pressure until the
	/// active preset is intentionally switched.
	/// </summary>
	public enum ProjectToFLabFixedTileEncounterPreset
	{
		Vanilla,
		LowLevelSafe,
		Pressure,
		ConversionBodyTest,
	}

	public static class ProjectToFLabFixedTileEncounterContainer
	{
		// EDIT THIS ONE LINE FOR LAB PRESSURE SWAPS.
		// Default is intentionally Vanilla: stage the container without changing pressure.
		public const ProjectToFLabFixedTileEncounterPreset ActivePreset =
			ProjectToFLabFixedTileEncounterPreset.LowLevelSafe;

		// Fixed encounter formation IDs.
		// 0x10 is the current vanilla Gargoyle-pressure candidate observed on the
		// TOF locked-room battle tiles in the generated lab ROM.
		public const byte VanillaGargoyleFormation = 0x10;

		// Low-level socket/access verification preset.
		// 0x01 is a confirmed valid A-side encounter byte in the project's trap-tile
		// code paths, unlike 0x00 which is used as the random-encounter sentinel after
		// trap-tile normalization.
		public const byte LowLevelSafeFormation = 0x01;

		// Pressure preset.
		// 0x44 appears as an existing TOF/SeaShrine fixed-tile battle value in the
		// current ROM shape and is staged as a stronger-than-safe pressure slot.
		// Adjust this value later if Senses/Forge confirms a better pressure body.
		public const byte PressureFormation = 0x44;

		// Conversion/body-test preset is deliberately staged but not activated yet.
		// Once a typed/HURT/tool conversion body is selected, add a named const here
		// and return it from GetFormationForPreset(ProjectToFLabFixedTileEncounterPreset.ConversionBodyTest).
		// public const byte ConversionBodyTestFormation = 0x00;

		// TOF locked-room Gargoyle-pressure tile IDs in the ToF/SeaShrine tileset.
		// These are tile definitions, not map coordinates and not random encounter zones.
		public static readonly IReadOnlyList<byte> ToFLockedRoomGargoyleTileIds =
			new byte[] { 0x20, 0x54 };

		public static void Apply(TileSetsData tileSetsData, Flags flags)
		{
			Apply(tileSetsData, flags, ActivePreset);
		}

		public static void Apply(TileSetsData tileSetsData, Flags flags, ProjectToFLabFixedTileEncounterPreset preset)
		{
			if (tileSetsData == null)
			{
				throw new ArgumentNullException(nameof(tileSetsData));
			}

			if (flags == null)
			{
				throw new ArgumentNullException(nameof(flags));
			}

			// Guardrail: this helper is for the vanilla/improved-vanilla TOF lab only.
			if (flags.GameMode == GameModes.DeepDungeon)
			{
				return;
			}

			byte formationId = GetFormationForPreset(preset);
			ApplyFormationToToFLockedRoomTiles(tileSetsData, formationId);
		}

		public static byte GetFormationForPreset(ProjectToFLabFixedTileEncounterPreset preset)
		{
			switch (preset)
			{
				case ProjectToFLabFixedTileEncounterPreset.Vanilla:
					return VanillaGargoyleFormation;

				case ProjectToFLabFixedTileEncounterPreset.LowLevelSafe:
					return LowLevelSafeFormation;

				case ProjectToFLabFixedTileEncounterPreset.Pressure:
					return PressureFormation;

				case ProjectToFLabFixedTileEncounterPreset.ConversionBodyTest:
					// Intentionally preserves vanilla until an exact conversion/body-test
					// formation is chosen. This keeps the preset visible without hiding a
					// speculative combat change.
					return VanillaGargoyleFormation;

				default:
					throw new ArgumentOutOfRangeException(nameof(preset), preset, null);
			}
		}

		private static void ApplyFormationToToFLockedRoomTiles(TileSetsData tileSetsData, byte formationId)
		{
			var toFSeaShrineTiles = tileSetsData[(int)TileSets.ToFSeaShrine].Tiles;

			foreach (byte tileId in ToFLockedRoomGargoyleTileIds)
			{
				var tile = toFSeaShrineTiles[tileId];

				if (!IsFixedBattleTile(tile))
				{
					throw new InvalidOperationException(
						$"TOF lab fixed-tile target 0x{tileId:X2} is not a fixed battle tile. " +
						"Refusing to write encounter pressure into a non-battle tile.");
				}

				tile.PropertyValue = formationId;
			}
		}

		private static bool IsFixedBattleTile(TileSM tile)
		{
			return (((TilePropFunc)tile.PropertyType & TilePropFunc.TP_SPEC_MASK) == TilePropFunc.TP_SPEC_BATTLE);
		}
	}
}
