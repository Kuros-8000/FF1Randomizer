namespace FF1Lib
{
	/// <summary>
	/// Small project bridge for the current item-cast cleanup checkpoint.
	///
	/// Intentional current policy:
	/// - active item-casts live on weapons
	/// - armor active casts are disabled for now
	/// - Power Gauntlets should not be routed through CraftPowerItem
	/// - Power Gauntlets should receive a worn Death + Poison resist package
	///
	/// This is deliberately thin. It is not the final item identity system.
	/// </summary>
	internal static class ProjectItemMagicBridge
	{
		internal static bool AllowGuaranteedPowerItemCast(Flags flags)
		{
			return false;
		}

		internal static bool AllowArmorActiveCasts(Flags flags)
		{
			return false;
		}

		internal static bool TryGetForcedArmorResistPackage(Item item, out byte elementalResist)
		{
			elementalResist = 0;

			if (item == Item.PowerGauntlets)
			{
				elementalResist = (byte)((byte)SpellElement.Death | (byte)SpellElement.Poison);
				return true;
			}

			return false;
		}
	}
}
