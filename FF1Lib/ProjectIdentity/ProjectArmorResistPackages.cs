namespace FF1Lib.ProjectIdentity
{
	/// <summary>
	/// Suggested path:
	/// FF1Lib/ProjectIdentity/ProjectArmorResistPackages.cs
	///
	/// Purpose:
	/// Defines project-owned armor-side resist packages.
	///
	/// Helper family:
	/// generation-facing source helper
	///
	/// Current first use:
	/// Power Gauntlets should stop being an active armor-cast item and become a
	/// guaranteed Poison + Death resist item for now.
	///
	/// What this does:
	/// - Defines named resist masks.
	/// - Keeps armor-side identity readable and separate from weapon active casts.
	///
	/// What this does NOT do:
	/// - It does not mutate armor by itself.
	/// - It does not write ROM data.
	/// - It does not change ArmorCrafter until wired.
	/// </summary>
	public static class ProjectArmorResistPackages
	{
		public static byte PowerGauntletsResistMask =>
			(byte)((byte)SpellElement.Poison | (byte)SpellElement.Death);

		public static byte PoisonDeathResistMask =>
			(byte)((byte)SpellElement.Poison | (byte)SpellElement.Death);
	}
}
