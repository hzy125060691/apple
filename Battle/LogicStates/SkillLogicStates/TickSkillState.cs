using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	/// <summary>
	/// 技能的tick状态
	/// </summary>
	[hzyBattleBase]
	public class TickSkillState : SkillLogicState
	{
		private const int key_ticlLeftTime_SkillInfo = 0;
		private const int key_EffectTickTime = 0;
		public override void InitState(SkillObj self, SkillInfo_New biffInfo, SkillConfig_New skillConfig, double fixTime)
		{
			int index = self.GetSkillStateIndex(biffInfo);
			var time = self.GetSkillStateTime(skillConfig, index) + fixTime;
			self.SetSkillStateTime(biffInfo, time);
			self.SetSkillDoubleParam(biffInfo, time, key_ticlLeftTime_SkillInfo);
		}
		public override LogicStateTickRet Tick(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			LogicStateTickRet ret = base.Tick(self, skillInfo, skillConfig);
			if (ret == LogicStateTickRet.TimeFinish)
			{
				return ret;
			}
			double effectTickTime = self.GetSkillStateDoubleParam(skillConfig, key_EffectTickTime, self.GetSkillStateIndex(skillInfo));
			if (effectTickTime < 0)
			{
				self.LogInfo("TickSkillState:skillObj[{0}] skill:[{1}] Tick Effect effectTickTime< 0".F(self.GetID(), self.GetSkillID(skillInfo)));
				return LogicStateTickRet.TimeFinish;
			}
			self.SetSkillStateTime(skillInfo, self.GetSkillStateTime(skillInfo) - self.GetDeltaTime());
			var leftTickTime = self.GetSkillDoubleParam(skillInfo, key_ticlLeftTime_SkillInfo);
			if (leftTickTime - self.GetSkillStateTime(skillInfo) >= effectTickTime)
			{
				self.SetSkillDoubleParam(skillInfo, leftTickTime - effectTickTime, key_ticlLeftTime_SkillInfo);
				self.LogInfo("TickSkillState:skillObj[{0}] skill:[{1}] Tick Effect".F(self.GetID(), self.GetSkillID(skillInfo)));
				ret = LogicStateTickRet.OnEffect;
			}
			else if (self.GetSkillStateTime(skillInfo) <= 0)
			{
				ret = LogicStateTickRet.NextState;
				self.LogInfo("TickSkillState:skillObj[{0}] skill:[{1}] Tick Finish".F(self.GetID(), self.GetSkillID(skillInfo)));
			}
			else
			{
				ret = LogicStateTickRet.None;
			}
			return ret;
		}
	}
}
