using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	class DamageAndAddBuffSkill : SkillLogic
	{
		private const int Key_Damege_SkillConfig = 0;
		//private const int Key_Double_BuffBegin_SkillConfig = 0;
		public override bool OnEffect(SkillObj self, SkillObj target, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			if(self.GetCamp()!=target.GetCamp())
			{
				int iDamamge = self.GetSkillIntParam(skillConfig, Key_Damege_SkillConfig);
				Damage damage = BattleModule.CreateDamage(iDamamge);
				BattleModule.DamageTarget(target, self, damage);
				var buffIds = self.GetSkillIntParams(skillConfig).Skip(Key_Damege_SkillConfig + 1);
				foreach (var buffId in buffIds)
				{
					BattleModule.AddBuff(target, self, buffId, BattleReason.Skill);
				}
			}
			else
			{
				var buffIds = self.GetSkillDoubleParams(skillConfig).Select(d=>(int)d);
				foreach (var buffId in buffIds)
				{
					BattleModule.AddBuff(target, self, buffId, BattleReason.Skill);
				}
			}
			return true;
		}
	}
}
