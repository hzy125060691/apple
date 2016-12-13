using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleUndetermined]
	public class SummonAndAddBuff : SkillLogic
	{
		private const int key_SummonId = 0;
		private const int key_BuffId = 1;
		public override bool OnEffect(SkillObj self, SkillObj target, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			int id = self.GetSkillIntParam(skillConfig, key_SummonId);
			var summonTar = BattleModule.Summon(id, self, target, skillInfo, skillConfig);
			if(summonTar != null)
			{
				var buffIds = self.GetSkillIntParams(skillConfig);
				foreach (var buffId in buffIds.Skip(key_SummonId + 1))
				{
					BattleModule.AddBuff(summonTar, self, buffId, BattleReason.Skill);
				}
			}
			{
				//int iBuffId = self.GetSkillIntParam(skillConfig, key_BuffId);
				//if (iBuffId > 0)
				//{
				//	BattleModule.AddBuff(summonTar, self, iBuffId, BattleReason.Skill);
				//}
			}
			return true;
		}
	}
}
