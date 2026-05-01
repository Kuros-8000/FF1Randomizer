using System;

namespace FF1Lib
{
	public partial class FF1Rom : NesRom
	{
		/// <summary>
		/// Project proof patch: Were-coded target + Were-effective weapon pierces the target's
		/// physical Absorb for this attack only.
		///
		/// Suggested path:
		/// FF1Lib/ProjectIdentity/ProjectWereLaw.cs
		///
		/// V2 safety note:
		/// - This version does NOT bank-swap out of bank 0C while executing code in bank 0C.
		/// - The first lab version could freeze because it swapped the currently executing bank.
		/// - This version installs the whole Were check inline at bank 0C:$A821.
		/// </summary>
		public void EnableProjectWereLawAbsorbPierce()
		{
			// Bank 0C $A821-A836 lives inside DoPhysicalAttack's base-damage calculation cleanup area.
			// Existing Wake-on-Hit/Sleep cleanup replaces this range with NOPs and a final:
			//   LDA math_dmgcalc ($6860)
			//
			// We replace the same footprint with:
			//   if ((btl_attacker_category & WERE) != 0 &&
			//       (btl_defender_category & WERE) != 0)
			//   {
			//       btl_defender_absorb = 0;
			//   }
			//   LDA math_dmgcalc
			//
			// RAM addresses from Assembly/Symbols.cs:
			// btl_attacker_category = $686D
			// btl_defender_category = $6876
			// btl_defender_absorb  = $6879
			// math_dmgcalc         = $6860
			// MonsterType.WERE     = $10
			PutInBank(0x0C, 0xA821, Blob.FromHex(
				"AD6D68" + // LDA $686D ; btl_attacker_category
				"2910" +   // AND #$10  ; MonsterType.WERE
				"F00C" +   // BEQ +12   ; skip to LDA math_dmgcalc
				"AD7668" + // LDA $6876 ; btl_defender_category
				"2910" +   // AND #$10  ; MonsterType.WERE
				"F005" +   // BEQ +5    ; skip to LDA math_dmgcalc
				"A900" +   // LDA #$00
				"8D7968" + // STA $6879 ; btl_defender_absorb = 0
				"AD6068"   // LDA $6860 ; preserve expected load of math_dmgcalc
			));
		}

		/// <summary>
		/// Lab helper: make sure vanilla Were Sword actually carries the Were target bit.
		/// Safe to call even if the current randomizer branch already preserves it.
		/// </summary>
		public void ProjectWereLaw_EnsureWereSwordTargetsWere()
		{
			int wereSwordWeaponIndex = (int)Item.WereSword - (int)Item.WoodenNunchucks;
			Weapon wereSword = new Weapon(wereSwordWeaponIndex, this);
			wereSword.TypeWeakness |= (byte)MonsterType.WERE;
			wereSword.writeWeaponMemory(this);
		}

		/// <summary>
		/// Lab helper: tune WrWOLF and CATMAN as proof bodies without changing formation flow.
		/// Start with these values to verify the law, then soften once the proof is readable.
		/// </summary>
		public void ProjectWereLaw_TuneWereProofBodiesForLab(
			int wrWolfHp = 180,
			byte wrWolfAbsorb = 120,
			int catmanHp = 420,
			byte catmanAbsorb = 180)
		{
			TuneWereProofEnemy(Enemy.WrWolf, wrWolfHp, wrWolfAbsorb, ensureWereType: true);
			TuneWereProofEnemy(Enemy.Catman, catmanHp, catmanAbsorb, ensureWereType: true);
		}

		private void TuneWereProofEnemy(int enemyId, int hp, byte absorb, bool ensureWereType)
		{
			Blob raw = Get(EnemyOffset + enemyId * EnemySize, EnemySize);
			var enemy = new EnemyInfo();
			enemy.decompressData(raw);

			enemy.hp = Math.Clamp(hp, 1, 65535);
			enemy.absorb = absorb;

			if (ensureWereType)
			{
				enemy.monster_type = (byte)(enemy.monster_type | (byte)MonsterType.WERE);
			}

			Put(EnemyOffset + enemyId * EnemySize, enemy.compressData());
		}
	}
}
