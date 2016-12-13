using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class AddBuffBuff : BuffLogic
	{
		// 		private const int key_BuffId1_SkillConfig = 0;
		// 		private const int key_BuffId2_SkillConfig = 1;
		// 		private const int key_BuffId3_SkillConfig = 2;
		public override void OnEffect(SkillObj self, SkillObj tarObj, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			var buffIds = self.GetBuffIntParams(buffConfig);
			foreach(var buffId in buffIds)
			{
				BattleModule.AddBuff(tarObj, self, buffId, BattleReason.Skill);
			}
			return ;
		}
	}
}
