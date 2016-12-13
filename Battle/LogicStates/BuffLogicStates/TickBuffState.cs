using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	/// <summary>
	/// Buff的tick状态
	/// </summary>
	[hzyBattleBase]
	public class TickBuffState : BuffLogicState
	{
		private const int key_ticlLeftTime_BuffInfo = 0;
		private const int key_EffectTickTime = 0;
		public override void InitState(SkillObj self, BuffInfo_New biffInfo, BuffConfig_New buffConfig, double fixTime)
		{
			int index = self.GetBuffStateIndex(biffInfo);
			var time = self.GetBuffStateTime(buffConfig, index) + fixTime;
			self.SetBuffStateTime(biffInfo, time);
			self.SetBuffDoubleParam(biffInfo, time, key_ticlLeftTime_BuffInfo);
		}
		public override LogicStateTickRet Tick(SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			LogicStateTickRet ret = base.Tick(self, buffInfo, buffConfig);
			if (ret == LogicStateTickRet.TimeFinish)
			{
				return ret;
			}
			double effectTickTime = self.GetBuffStateDoubleParam(buffConfig, key_EffectTickTime, self.GetBuffStateIndex(buffInfo));
			if(effectTickTime < 0)
			{
				self.LogInfo("TickBuffState:buffObj[{0}] buff:[{1}] Tick Effect effectTickTime< 0".F(self.GetID(), self.GetBuffID(buffInfo)));
				return LogicStateTickRet.TimeFinish;
			}
			self.SetBuffStateTime(buffInfo, self.GetBuffStateTime(buffInfo) - self.GetDeltaTime());
			var leftTickTime = self.GetBuffDoubleParam(buffInfo, key_ticlLeftTime_BuffInfo);
			if (leftTickTime - self.GetBuffStateTime(buffInfo) >= effectTickTime)
			{
				self.SetBuffDoubleParam(buffInfo, leftTickTime - effectTickTime, key_ticlLeftTime_BuffInfo);
				self.LogInfo("TickBuffState:buffObj[{0}] buff:[{1}] Tick Effect".F(self.GetID(), self.GetBuffID(buffInfo)));
				ret = LogicStateTickRet.OnEffect;
			}
			else if (self.GetBuffStateTime(buffInfo) <= 0)
			{
				ret = LogicStateTickRet.NextState;
				self.LogInfo("TickBuffState:buffObj[{0}] buff:[{1}] Tick Finish".F(self.GetID(), self.GetBuffID(buffInfo)));
			}
			else
			{
				ret = LogicStateTickRet.None;
			}
			return ret;
		}
	}
}
