using System;

namespace FF1Lib
{
	public partial class FF1Rom : NesRom
	{
		/// <summary>
		/// Temporary lab harness for Crown Relic Proof 01 - Were Law.
		///
		/// Suggested path:
		/// FF1Lib/ProjectIdentity/ProjectWereLawLabHarness.cs
		///
		/// V3 purpose:
		/// - Keep the working Were Sword starting-equipment shortcut.
		/// - Stop relying on overworld/start-area encounter zones.
		/// - Put the proof encounter on a Deep Dungeon floor domain instead.
		///
		/// This is lab-only and removable. It is not final NPC delivery, not final lair flow,
		/// not final reward/fanfare logic, and not flee/wipe logic.
		/// </summary>
		public void EnableProjectWereLawFloorHarness(
			ProjectWereLawLabEncounter encounter = ProjectWereLawLabEncounter.WrWolf,
			int deepDungeonFloor = 2)
		{
			ProjectWereLawLab_GiveWereSwordAsStartingEquipment();
			ProjectWereLawLab_ForceDeepDungeonFloorEncounters(deepDungeonFloor, encounter);
		}

		/// <summary>
		/// Legacy V1/V2 harness. Kept for compile compatibility, but avoid it for DD lab testing.
		/// It targets overworld starting zones, which can fail when the start/entrance state is odd.
		/// Prefer EnableProjectWereLawFloorHarness(..., deepDungeonFloor: 2).
		/// </summary>
		public void EnableProjectWereLawLabHarness(ProjectWereLawLabEncounter encounter = ProjectWereLawLabEncounter.Catman)
		{
			ProjectWereLawLab_GiveWereSwordAsStartingEquipment();
			ProjectWereLawLab_ForceStartingAreaEncounters(encounter);
		}

		/// <summary>
		/// Lab shortcut: put Were Sword into each character's first starting weapon slot.
		///
		/// This avoids NPC scripting for the first proof. It writes the same starting-equipment
		/// table that StartingEquipment.Write() uses, and also installs the copy routine in case
		/// this helper is called in a flag path that would not otherwise enable starting gear.
		///
		/// Call after StartingItems.Write() so this lab state wins.
		/// </summary>
		public void ProjectWereLawLab_GiveWereSwordAsStartingEquipment()
		{
			byte wereSwordStartingWeaponId = (byte)(Item.WereSword - Item.Soft);

			// Starting-equipment table is four 8-byte character records in bank 1B:
			// [4 weapon slots][4 armor slots]. Zero means empty.
			// Put Were Sword in slot 1 for every character and leave all other slots empty.
			for (int character = 0; character < 4; character++)
			{
				byte[] startingEquipmentRecord = new byte[8];
				startingEquipmentRecord[0] = wereSwordStartingWeaponId;
				PutInBank(0x1B, 0x8520 + 0x08 * character, startingEquipmentRecord);
			}

			// Install/refresh Copy_StartingEquipment so the table above is actually copied into SRAM.
			// Same routine body used by StartingEquipment.Write()/SetStartingEquipment().
			PutInBank(0x1B, 0x8540, Blob.FromHex("A200A000205785A040205785A080205785A0C020578560BD2085991861E8C898290FC908D0F160"));
		}

		/// <summary>
		/// V3 lab shortcut: force one Deep Dungeon floor's encounter domain to the Were proof encounter.
		///
		/// DeepDungeon.CreateDomains writes dungeon floor domains into zone indexes 64 + mapIndex.
		/// The DD spoiler names floors as DeepDungeon(currentMap - 7)B, so:
		///   floor 1 -> mapIndex 8  -> zone 0x48
		///   floor 2 -> mapIndex 9  -> zone 0x49
		///   floor N -> mapIndex N+7 -> zone 0x47 + N
		///
		/// Call after ZoneFormations.Write(this), because this writes the final ROM domain bytes directly.
		/// </summary>
		public void ProjectWereLawLab_ForceDeepDungeonFloorEncounters(
			int deepDungeonFloor = 2,
			ProjectWereLawLabEncounter encounter = ProjectWereLawLabEncounter.WrWolf)
		{
			int floor = Math.Clamp(deepDungeonFloor, 1, 53);
			int zone = ProjectWereLawLabDeepDungeonZoneBase + floor;
			byte encounterId = ProjectWereLawLab_GetEncounterId(encounter);

			Put(ProjectWereLawLabZoneFormationsOffset + zone * ProjectWereLawLabZoneFormationsSize,
				new byte[] { encounterId, encounterId, encounterId, encounterId, encounterId, encounterId, encounterId, encounterId });
		}

		/// <summary>
		/// V1/V2 lab shortcut: force the overworld starting encounter zones to the Were proof encounter.
		/// Kept only for compatibility. Floor harness is safer for Deep Dungeon testing.
		/// </summary>
		public void ProjectWereLawLab_ForceStartingAreaEncounters(ProjectWereLawLabEncounter encounter = ProjectWereLawLabEncounter.Catman)
		{
			byte encounterId = ProjectWereLawLab_GetEncounterId(encounter);

			foreach (byte zone in ProjectWereLawLabStartingZones)
			{
				Put(ProjectWereLawLabZoneFormationsOffset + zone * ProjectWereLawLabZoneFormationsSize,
					new byte[] { encounterId, encounterId, encounterId, encounterId, encounterId, encounterId, encounterId, encounterId });
			}
		}

		/// <summary>
		/// Optional static readback helper for local debugging. This does not prove gameplay feel,
		/// but it confirms the lab harness wrote the expected starting-equipment and floor-zone bytes.
		/// </summary>
		public (byte firstStartingWeapon, byte[] floorZone) ProjectWereLawFloorLab_Readback(int deepDungeonFloor = 2)
		{
			int floor = Math.Clamp(deepDungeonFloor, 1, 53);
			int zone = ProjectWereLawLabDeepDungeonZoneBase + floor;

			return (
				GetFromBank(0x1B, 0x8520, 1)[0],
				Get(ProjectWereLawLabZoneFormationsOffset + zone * ProjectWereLawLabZoneFormationsSize, ProjectWereLawLabZoneFormationsSize).ToBytes()
			);
		}

		private static byte ProjectWereLawLab_GetEncounterId(ProjectWereLawLabEncounter encounter)
		{
			return encounter == ProjectWereLawLabEncounter.WrWolf
				? ProjectWereLawLabWrWolfFormation
				: ProjectWereLawLabCatmanFormation;
		}

		private const int ProjectWereLawLabZoneFormationsOffset = 0x2C000;
		private const int ProjectWereLawLabZoneFormationsSize = 8;

		// DeepDungeon floor N uses zone 0x47 + N. Floor 2 => 0x49.
		private const int ProjectWereLawLabDeepDungeonZoneBase = 0x47;

		// Same starting zones excluded by ZoneFormations.InterZone shuffle. Avoid for current DD testing.
		private static readonly byte[] ProjectWereLawLabStartingZones = { 0x1B, 0x1C, 0x24, 0x2C };

		// Senses trace consolidation:
		// Formation 11-1 = WrWOLF 3-6, clean literal Werewolf proof.
		// Formation 39-1 = CATMAN 2-4, best playable trial body.
		private const byte ProjectWereLawLabWrWolfFormation = 0x0B;
		private const byte ProjectWereLawLabCatmanFormation = 0x27;
	}

	public enum ProjectWereLawLabEncounter
	{
		WrWolf,
		Catman,
	}
}
