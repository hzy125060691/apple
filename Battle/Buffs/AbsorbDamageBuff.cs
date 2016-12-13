using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class AbsorbDamageBuff : BuffLogic
	{
		private const int AbsorbDamageKey = 0;
		public override bool InitBuffInfo(SkillObj self, SkillObj srcObj, BattleReason reason, BuffInfo_New buffInfo, BuffConfig_New buffConfig, bool RefreshGUID = true)
		{
			base.InitBuffInfo(self, srcObj, reason, buffInfo, buffConfig, RefreshGUID);
			int absorbDamage = self.GetBuffIntParam(buffConfig, AbsorbDamageKey);
			if(absorbDamage > 0)
			{
				self.SetBuffIntParam(buffInfo, AbsorbDamageKey, absorbDamage);
			}
			return true;
		}

		public override Damage BeHurtDamageFix(SkillObj self, SkillObj source, Damage damage, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			int shield = self.GetBuffIntParam(buffInfo, AbsorbDamageKey);
			if(shield > damage.value)
			{
				shield -= damage.value;
				damage.value = 0;
			}
			else
			{
				damage.value -= shield;
				shield = 0;
				BuffOnEnd(self, buffInfo, buffConfig);
			}
			return damage;
		}
	}
}
