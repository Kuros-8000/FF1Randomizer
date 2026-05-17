using System;
using System.Collections.Generic;
using FF1Lib;

namespace FF1Lib.ProjectIdentity
{
	/// <summary>
	/// Project-owned Temple of Fiends chest lab helper.
	///
	/// This helper runs after normal item placement and before item placement is written back.
	/// It only rewrites entries in the provided placed-items list. It does not alter fixed-tile
	/// encounter behavior, Randomize.cs wiring, Deep Dungeon generation, map data, NPCs, shops,
	/// or the Mystic Key / TOF container call site.
	/// </summary>
	public enum ProjectToFLabContainerPreset
	{
		/// <summary>
		/// Existing baseline TOF lab shape: keep a compact success path with Mystic Key plus Opal Bangle.
		/// </summary>
		Baseline,

		/// <summary>
		/// Complete TOF equipment-kit lab:
		/// - 1 weapon slot
		/// - Mystic Key fixed in an accessible chest
		/// - 4 armor/tool posture slots across accessible and gated TOF chests
		/// </summary>
		FullEquipmentKitTest,
	}

	public static class ProjectToFLabContainer
	{
		// Keep the current baseline preset active by default.
		// To run the new equipment-kit lab, temporarily change this to FullEquipmentKitTest.
		private const ProjectToFLabContainerPreset ActivePreset = ProjectToFLabContainerPreset.FullEquipmentKitTest;

		private const Item FallbackDisplacedItem = Item.Cabin;

		public static void Apply(List<IRewardSource> placedItems)
		{
			Apply(placedItems, ActivePreset);
		}

		public static void Apply(List<IRewardSource> placedItems, ProjectToFLabContainerPreset preset)
		{
			if (placedItems == null)
			{
				throw new ArgumentNullException(nameof(placedItems));
			}

			switch (preset)
			{
				case ProjectToFLabContainerPreset.Baseline:
					ApplyBaselinePreset(placedItems);
					break;

				case ProjectToFLabContainerPreset.FullEquipmentKitTest:
					ApplyFullEquipmentKitTestPreset(placedItems);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(preset), preset, null);
			}
		}

		private static void ApplyBaselinePreset(List<IRewardSource> placedItems)
		{
			// Baseline success path. Do not expand this preset while adding new labs.
			// The new FullEquipmentKitTest preset below is intentionally separate.
			MoveOrPlace(placedItems, ItemLocations.ToFTopLeft1, Item.Key);   // Mystic Key, accessible.
			MoveOrPlace(placedItems, ItemLocations.ToFTopLeft2, Item.Opal);  // Opal Bangle success path, accessible.
		}

		private static void ApplyFullEquipmentKitTestPreset(List<IRewardSource> placedItems)
		{
			// Accessible room: entry kit plus the key needed to open the gated TOF rooms.
			MoveOrPlace(placedItems, ItemLocations.ToFTopLeft1, Item.SunSword); // Weapon slot.
			MoveOrPlace(placedItems, ItemLocations.ToFTopLeft2, Item.Key);       // Mystic Key fixed.
			MoveOrPlace(placedItems, ItemLocations.ToFBottomLeft, Item.Opal);    // Armor/tool posture slot 1.

			// Gated rooms: three more armor/tool posture slots, for four total across TOF.
			MoveOrPlace(placedItems, ItemLocations.ToFBottomRight, Item.ProCape); // Armor/tool posture slot 2.
			MoveOrPlace(placedItems, ItemLocations.ToFTopRight1, Item.ProRing);         // Armor/tool posture slot 3.
			MoveOrPlace(placedItems, ItemLocations.ToFTopRight2, Item.Ribbon);          // Armor/tool posture slot 4.
		}

		private static void MoveOrPlace(List<IRewardSource> placedItems, IRewardSource targetLocation, Item desiredItem)
		{
			int targetIndex = FindPlacementIndexByAddress(placedItems, targetLocation.Address);
			Item displacedItem = targetIndex >= 0 ? placedItems[targetIndex].Item : FallbackDisplacedItem;

			if (targetIndex >= 0 && placedItems[targetIndex].Item == desiredItem)
			{
				return;
			}

			int currentItemIndex = FindPlacementIndexByItem(placedItems, desiredItem, targetLocation.Address);

			if (currentItemIndex >= 0)
			{
				IRewardSource currentItemLocation = placedItems[currentItemIndex];
				placedItems[currentItemIndex] = ItemPlacement.NewItemPlacement(currentItemLocation, displacedItem);
			}

			IRewardSource replacement = ItemPlacement.NewItemPlacement(targetLocation, desiredItem);

			if (targetIndex >= 0)
			{
				placedItems[targetIndex] = replacement;
			}
			else
			{
				placedItems.Add(replacement);
			}
		}

		private static int FindPlacementIndexByAddress(List<IRewardSource> placedItems, int address)
		{
			for (int i = 0; i < placedItems.Count; i++)
			{
				if (placedItems[i].Address == address)
				{
					return i;
				}
			}

			return -1;
		}

		private static int FindPlacementIndexByItem(List<IRewardSource> placedItems, Item item, int excludedAddress)
		{
			for (int i = 0; i < placedItems.Count; i++)
			{
				if (placedItems[i].Address != excludedAddress && placedItems[i].Item == item)
				{
					return i;
				}
			}

			return -1;
		}
	}
}
