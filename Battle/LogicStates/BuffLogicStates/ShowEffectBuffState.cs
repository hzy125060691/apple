using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	class ShowEffectBuffState : BuffLogicState
	{
		public override void InitState(SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig, double fixTime)
		{
			int index = self.GetBuffStateIndex(buffInfo);
			var time = self.GetBuffStateTime(buffConfig, index);
			self.SetBuffStateTime(buffInfo, time + fixTime);
		}
		public override LogicStateTickRet Tick(SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			LogicStateTickRet ret = base.Tick(self, buffInfo, buffConfig);
			if (ret == LogicStateTickRet.TimeFinish)
			{
				return ret;
			}
			self.SetBuffStateTime(buffInfo, self.GetBuffStateTime(buffInfo) - self.GetDeltaTime());
			if (self.GetBuffStateTime(buffInfo) <= 0)
			{
				ret = LogicStateTickRet.NextState;
				self.LogInfo("ChargeState:buffObj[{0}] buff:[{1}] ShowEffectState Finish".F(self.GetID(), self.GetBuffID(buffInfo)));
			}
			return ret;
		}
	}
}
