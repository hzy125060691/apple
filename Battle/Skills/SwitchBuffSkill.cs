using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleUndetermined]
	public class SwitchBuffSkill : SkillLogic
	{
 		private const int key_BuffId1_SkillConfig = 0;
 		private const int key_BuffId2_SkillConfig = 1;
// 		private const int key_BuffId3_SkillConfig = 2;
		public override bool OnEffect(SkillObj self, SkillObj target, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			var buffId1 = self.GetSkillIntParam(skillConfig, key_BuffId1_SkillConfig);
			var buffId2 = self.GetSkillIntParam(skillConfig, key_BuffId2_SkillConfig);
			var tarBuff = target.GetBuffList().Where(b => target.GetBuffID(b) == buffId1);
			if (tarBuff != null && tarBuff.FirstOrDefault() != null)
			{
				BattleModule.AddBuff(target, self, buffId2, BattleReason.Skill);
				BattleModule.RemoveBuff(target, self, buffId1, BattleReason.Skill);
			}
			else
			{
				BattleModule.AddBuff(target, self, buffId1, BattleReason.Skill);
				BattleModule.RemoveBuff(target, self, buffId2, BattleReason.Skill);
			}

			return true;
		}
	}
}
