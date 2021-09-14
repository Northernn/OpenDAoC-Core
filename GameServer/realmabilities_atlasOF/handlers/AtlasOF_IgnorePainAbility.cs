using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Self heal that heals 100% of the user's health, and can be used in combat.
	/// </summary>
	public class AtlasOF_IgnorePainAbility : L1RealmAbility
	{
		public AtlasOF_IgnorePainAbility(DBAbility dba, int level) : base(dba, level) { }

		public override bool CheckRequirement(GamePlayer player) { return AtlasRAHelpers.HasFirstAidLevel(player, 2); }

        public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			int heal = 0;
			int currentCharMaxHealth = living.MaxHealth;

			int healed = living.ChangeHealth(living, eHealthChangeType.Spell, currentCharMaxHealth);

			SendCasterSpellEffectAndCastMessage(living, 7004, healed > 0);

			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				if (healed > 0) player.Out.SendMessage("You heal yourself for " + healed + " hit points.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				if (heal > healed)
				{
					player.Out.SendMessage("You are fully healed.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}
			}
			if (healed > 0) DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 1800;	// 30 mins
		}

		public override void AddEffectsInfo(IList<string> list)
		{
			{
				list.Add("Level 1: Value: 100%");
				list.Add("");
				list.Add("Target: Self");
				list.Add("Casting time: instant");
				list.Add("Reuse Timer : 30 mins");
				list.Add("Pre-Requisit : First Aid lvl 2");
			}
		}

    }
}