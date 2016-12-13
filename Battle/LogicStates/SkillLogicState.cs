using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	/// <summary>
	/// 这个是技能的状态
	/// </summary>
	[hzyBattleBase]
	public class SkillLogicState : LogicState<SkillInfo_New, SkillConfig_New>
	{
		public override void InitSkill(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig, double fixTime)
		{
			var time = self.GetSkillTime(skillConfig);
			self.SetSkillTime(skillInfo, time + fixTime);
		}
		public override LogicStateTickRet Tick(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			LogicStateTickRet ret = base.Tick(self, skillInfo, skillConfig);
			self.SetSkillTime(skillInfo, self.GetSkillTime(skillInfo) - self.GetDeltaTime());
			if (self.GetSkillTime(skillInfo) <= 0)
			{
				ret = LogicStateTickRet.TimeFinish;
				self.LogInfo("SkillLogicState:skillObj[{0}] skill:[{1}] [{2}]".F(self.GetID(), self.GetSkillID(skillInfo), ret.ToString()));
			}
			return ret;
		}
		public override double OnStateChanged(string tarState, SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			return self.GetSkillStateTime(skillInfo);
		}
	}
}
