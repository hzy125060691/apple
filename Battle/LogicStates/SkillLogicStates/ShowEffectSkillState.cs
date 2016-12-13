using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	class ShowEffectSkillState : SkillLogicState
	{
		public override void InitState(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig, double fixTime)
		{
			int index = self.GetSkillStateIndex(skillInfo);
			var time = self.GetSkillStateTime(skillConfig, index);
			self.SetSkillStateTime(skillInfo, time + fixTime);
		}
		public override LogicStateTickRet Tick(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			LogicStateTickRet ret = base.Tick(self, skillInfo, skillConfig);
			if (ret == LogicStateTickRet.TimeFinish)
			{
				return ret;
			}
			self.SetSkillStateTime(skillInfo, self.GetSkillStateTime(skillInfo) - self.GetDeltaTime());
			if (self.GetSkillStateTime(skillInfo) <= 0)
			{
				ret = LogicStateTickRet.NextState;
				self.LogInfo("ShowEffectSkillState:skillObj[{0}] skill:[{1}] ShowEffectState Finish".F(self.GetID(), self.GetSkillID(skillInfo)));
			}
			return ret;
		}
	}
}
