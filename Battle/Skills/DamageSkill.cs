using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	class DamageSkill : SkillLogic
	{
		private const int DamegeKey = 0;
		public override bool OnEffect(SkillObj self, SkillObj target, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			int iDamamge = self.GetSkillIntParam(skillConfig, DamegeKey);
			Damage damage = BattleModule.CreateDamage(iDamamge);
			BattleModule.DamageTarget(target, self, damage);
			return true;
		}
	}
}
