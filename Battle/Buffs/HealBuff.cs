using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class HealBuff : BuffLogic
	{
		private const int HealValueKey = 0;

		public override void OnEffect(SkillObj self, SkillObj tarObj, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			int healValue = self.GetBuffIntParam(buffConfig, HealValueKey);
			if (healValue > 0)
			{
				Damage heal = BattleModule.CreateDamage(-healValue);
				BattleModule.DamageTarget(tarObj, self, heal);
			}
		}
	}
}
