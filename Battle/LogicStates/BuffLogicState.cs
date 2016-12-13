using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class BuffLogicState : LogicState<BuffInfo_New, BuffConfig_New>
	{
		public override void InitBuff(SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig, double fixTime)
		{
			var time = self.GetBuffTime(buffConfig);
			self.SetBuffTime(buffInfo, time + fixTime);
		}
		public override LogicStateTickRet Tick(SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			LogicStateTickRet ret = base.Tick(self, buffInfo, buffConfig);
			self.SetBuffTime(buffInfo, self.GetBuffTime(buffInfo) - self.GetDeltaTime());
			if (self.GetBuffTime(buffInfo) <= 0)
			{
				ret = LogicStateTickRet.TimeFinish;
				self.LogInfo("BuffLogicState:buffObj[{0}] buff:[{1}] [{2}]".F(self.GetID(), self.GetBuffID(buffInfo), ret.ToString()));
			}
			return ret;
		}
		public override double OnStateChanged(string tarState, SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			return self.GetBuffStateTime(buffInfo);
		}
	}
}
