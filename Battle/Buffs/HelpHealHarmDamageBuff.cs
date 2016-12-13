using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleUndetermined]
	public class HelpHealHarmDamageBuff : BuffLogic
	{
		private const int Key_DamageValue_SkillConfig = 0;

		public override void OnEffect(SkillObj self, SkillObj tarObj, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			int damageValue = self.GetBuffIntParam(buffConfig, Key_DamageValue_SkillConfig);
			if(damageValue > 0 && self.GetSrcCamp(buffInfo) >= 0)
			{
				if (tarObj.GetCamp() == self.GetSrcCamp(buffInfo))
				{
					Damage heal = BattleModule.CreateDamage(-damageValue, self.GetSrcID(buffInfo));
					BattleModule.DamageTarget(tarObj, self, heal);
				}
				else
				{
					Damage damage = BattleModule.CreateDamage(damageValue, self.GetSrcID(buffInfo));
					BattleModule.DamageTarget(tarObj, self, damage);
				}

			}
		}
	}
}
