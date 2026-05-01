using System.Collections.Generic;

namespace FF1Lib.ProjectIdentity
{
	/// <summary>
	/// Suggested path:
	/// FF1Lib/ProjectIdentity/ProjectItemIdentityCatalog.cs
	///
	/// Suggested namespace:
	/// FF1Lib.ProjectIdentity
	///
	/// Purpose:
	/// Captures project-owned item identity notes in code-readable form.
	///
	/// Helper family:
	/// analysis-only helper
	///
	/// What this does:
	/// - Gives Forge / future tooling a stable place to record item role reads.
	/// - Separates current item meaning from future stability.
	/// - Does not mutate items, pools, treasure, permissions, or ROM data.
	///
	/// What this does NOT do:
	/// - It does not affect generation.
	/// - It does not replace ItemLists, Weaponizer, ArmorCrafter, or ItemMagic.
	/// - It does not decide actual treasure placement.
	///
	/// Intended first use:
	/// - Side-by-side inspection with spoilers and treasure windows.
	/// - Later input to a TreasureWindowEvaluator.
	/// </summary>
	public static class ProjectItemIdentityCatalog
	{
		public static readonly IReadOnlyDictionary<Item, ProjectItemIdentityProfile> Profiles =
			new Dictionary<Item, ProjectItemIdentityProfile>
			{
				[Item.PowerRod] = new ProjectItemIdentityProfile(
					item: Item.PowerRod,
					treasureJob: ProjectTreasureJob.TacticalCarry,
					liveValue: ProjectLiveValue.CarriedLive,
					futureStability: ProjectFutureStability.Stable,
					convergenceRisk: ProjectConvergenceRisk.Medium,
					summary: "Deliberate defensive cast carrier; currently the cleanest example of tactical defensive permission on a weapon.",
					notes: "ItemMagic can write RUSE / INV / INV2 onto Power Rod through guaranteed defense item logic. Best read as carry-tech before defining weapon identity."),

				[Item.PowerGauntlets] = new ProjectItemIdentityProfile(
					item: Item.PowerGauntlets,
					treasureJob: ProjectTreasureJob.TacticalCarry,
					liveValue: ProjectLiveValue.CarriedLive,
					futureStability: ProjectFutureStability.ExperimentalVolatile,
					convergenceRisk: ProjectConvergenceRisk.Medium,
					summary: "Offensive race/tool candidate, but often risks pseudo-identity if SABR/TMPR action economy does not pay off.",
					notes: "Currently attached to guaranteed power item logic. Needs careful read against action economy and glove-slot competition."),

				[Item.Defense] = new ProjectItemIdentityProfile(
					item: Item.Defense,
					treasureJob: ProjectTreasureJob.TacticalCarry,
					liveValue: ProjectLiveValue.CarriedLive,
					futureStability: ProjectFutureStability.LikelyLimited,
					convergenceRisk: ProjectConvergenceRisk.Medium,
					summary: "RUSE-bearing tactical carry that can later become a hybrid identity weapon.",
					notes: "Useful because it can be worth carrying before it is worth equipping. Future value depends on buff-family cleanup."),

				[Item.WhiteShirt] = new ProjectItemIdentityProfile(
					item: Item.WhiteShirt,
					treasureJob: ProjectTreasureJob.IdentityLoot,
					liveValue: ProjectLiveValue.CarriedLive,
					futureStability: ProjectFutureStability.LikelyLimited,
					convergenceRisk: ProjectConvergenceRisk.High,
					summary: "Currently a major survival-grammar item, but volatile because broad reusable defensive buffs may be limited or relocated.",
					notes: "Strong present behavior should not automatically become long-term intended source structure."),

				[Item.BlackShirt] = new ProjectItemIdentityProfile(
					item: Item.BlackShirt,
					treasureJob: ProjectTreasureJob.IdentityLoot,
					liveValue: ProjectLiveValue.CarriedLive,
					futureStability: ProjectFutureStability.ExperimentalVolatile,
					convergenceRisk: ProjectConvergenceRisk.Medium,
					summary: "Offensive/control identity carry; future stability depends on broad AOE damage and item-cast cleanup.",
					notes: "Likely more acceptable if it remains angled and does not become routine trash-clear infrastructure."),

				[Item.ZeusGauntlets] = new ProjectItemIdentityProfile(
					item: Item.ZeusGauntlets,
					treasureJob: ProjectTreasureJob.TacticalCarry,
					liveValue: ProjectLiveValue.CarriedLive,
					futureStability: ProjectFutureStability.ExperimentalVolatile,
					convergenceRisk: ProjectConvergenceRisk.Medium,
					summary: "Stable recognizable cast item with matchup value; more tactical carry than full identity item.",
					notes: "Volatile only because broad reusable AOE damage may need cleanup."),

				[Item.HealRod] = new ProjectItemIdentityProfile(
					item: Item.HealRod,
					treasureJob: ProjectTreasureJob.TractionLoot,
					liveValue: ProjectLiveValue.CarriedLive,
					futureStability: ProjectFutureStability.LikelyConsolidated,
					convergenceRisk: ProjectConvergenceRisk.Low,
					summary: "Anti-cruelty sustain piece; useful for keeping the run alive but not usually a lane revealer.",
					notes: "May stay, but repeated broad sustain sources should be watched."),

				[Item.HealHelm] = new ProjectItemIdentityProfile(
					item: Item.HealHelm,
					treasureJob: ProjectTreasureJob.TractionLoot,
					liveValue: ProjectLiveValue.CarriedLive,
					futureStability: ProjectFutureStability.LikelyConsolidated,
					convergenceRisk: ProjectConvergenceRisk.Low,
					summary: "Anti-cruelty sustain piece; similar role to Heal Rod but armor-side.",
					notes: "Useful, but not a strong identity signal unless slot pressure makes it meaningful."),

				[Item.Ribbon] = new ProjectItemIdentityProfile(
					item: Item.Ribbon,
					treasureJob: ProjectTreasureJob.ApexLoot,
					liveValue: ProjectLiveValue.EquipNow,
					futureStability: ProjectFutureStability.Stable,
					convergenceRisk: ProjectConvergenceRisk.High,
					summary: "Apex anti-nonsense item; powerful inherited certainty.",
					notes: "Important to preserve as a known truth, but dangerous if too common or too early."),

				[Item.Masamune] = new ProjectItemIdentityProfile(
					item: Item.Masamune,
					treasureJob: ProjectTreasureJob.ApexLoot,
					liveValue: ProjectLiveValue.EquipNow,
					futureStability: ProjectFutureStability.Stable,
					convergenceRisk: ProjectConvergenceRisk.High,
					summary: "Universal apex weapon; solves broad offensive shortage and can collapse role scarcity.",
					notes: "Best treated as convergence-risk apex rather than identity reveal."),

				[Item.Xcalber] = new ProjectItemIdentityProfile(
					item: Item.Xcalber,
					treasureJob: ProjectTreasureJob.ApexLoot,
					liveValue: ProjectLiveValue.EquipNow,
					futureStability: ProjectFutureStability.Stable,
					convergenceRisk: ProjectConvergenceRisk.High,
					summary: "Directed apex weapon; crowns somebody more than it repairs everyone.",
					notes: "Healthier than Masamune when its permissions keep it angled."),

				[Item.CatClaw] = new ProjectItemIdentityProfile(
					item: Item.CatClaw,
					treasureJob: ProjectTreasureJob.IdentityLoot,
					liveValue: ProjectLiveValue.EquipNow,
					futureStability: ProjectFutureStability.Stable,
					convergenceRisk: ProjectConvergenceRisk.Medium,
					summary: "Sharp identity weapon; helps define who becomes an attacker.",
					notes: "Good example of role-specific offensive identity rather than universal apex."),

				[Item.ThorHammer] = new ProjectItemIdentityProfile(
					item: Item.ThorHammer,
					treasureJob: ProjectTreasureJob.IdentityLoot,
					liveValue: ProjectLiveValue.CarriedLive,
					futureStability: ProjectFutureStability.Stable,
					convergenceRisk: ProjectConvergenceRisk.Medium,
					summary: "Layered iconic weapon: recognizable LIT2 cast layer plus separate Thor Master awakening seam.",
					notes: "Current best read: Thor Master is battle-start melee augmentation, not a cast-damage hook."),

				[Item.DragonArmor] = new ProjectItemIdentityProfile(
					item: Item.DragonArmor,
					treasureJob: ProjectTreasureJob.ApexLoot,
					liveValue: ProjectLiveValue.EquipNow,
					futureStability: ProjectFutureStability.Stable,
					convergenceRisk: ProjectConvergenceRisk.High,
					summary: "Heavy inherited-certainty armor with broad elemental answer value.",
					notes: "Stable meaning, but high convergence risk if it appears too freely."),

				[Item.ProRing] = new ProjectItemIdentityProfile(
					item: Item.ProRing,
					treasureJob: ProjectTreasureJob.TractionLoot,
					liveValue: ProjectLiveValue.EquipNow,
					futureStability: ProjectFutureStability.Stable,
					convergenceRisk: ProjectConvergenceRisk.Medium,
					summary: "Anti-cruelty defensive answer; often more traction/stability than identity.",
					notes: "Can feel apex-like depending on death pressure, but usually does not declare a whole run by itself."),
			};

		public static bool TryGetProfile(Item item, out ProjectItemIdentityProfile profile)
		{
			return Profiles.TryGetValue(item, out profile);
		}
	}

	public sealed class ProjectItemIdentityProfile
	{
		public Item Item { get; }
		public ProjectTreasureJob TreasureJob { get; }
		public ProjectLiveValue LiveValue { get; }
		public ProjectFutureStability FutureStability { get; }
		public ProjectConvergenceRisk ConvergenceRisk { get; }
		public string Summary { get; }
		public string Notes { get; }

		public ProjectItemIdentityProfile(
			Item item,
			ProjectTreasureJob treasureJob,
			ProjectLiveValue liveValue,
			ProjectFutureStability futureStability,
			ProjectConvergenceRisk convergenceRisk,
			string summary,
			string notes)
		{
			Item = item;
			TreasureJob = treasureJob;
			LiveValue = liveValue;
			FutureStability = futureStability;
			ConvergenceRisk = convergenceRisk;
			Summary = summary;
			Notes = notes;
		}
	}

	public enum ProjectTreasureJob
	{
		Unknown = 0,
		SurvivalCurrency = 1,
		TractionLoot = 2,
		TacticalCarry = 3,
		IdentityLoot = 4,
		ApexLoot = 5,
	}

	public enum ProjectLiveValue
	{
		Unknown = 0,
		EquipNow = 1,
		CarriedLive = 2,
		BackupUseful = 3,
		DeadInventoryBudget = 4,
	}

	public enum ProjectFutureStability
	{
		Unknown = 0,
		Stable = 1,
		LikelyLimited = 2,
		LikelyRelocated = 3,
		LikelyConsolidated = 4,
		ExperimentalVolatile = 5,
	}

	public enum ProjectConvergenceRisk
	{
		Unknown = 0,
		Low = 1,
		Medium = 2,
		High = 3,
	}
}
