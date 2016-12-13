using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class AddBuffSkill : SkillLogic
	{
// 		private const int key_BuffId1_SkillConfig = 0;
// 		private const int key_BuffId2_SkillConfig = 1;
// 		private const int key_BuffId3_SkillConfig = 2;
		public override bool OnEffect(SkillObj self, SkillObj target, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			var buffIds = self.GetSkillIntParams(skillConfig);
			foreach(var buffId in buffIds)
			{
				BattleModule.AddBuff(target, self, buffId, BattleReason.Skill);
			}
			return true;
		}
	}
}
