using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	class InstantBuffState : BuffLogicState
	{
		private const int key = 0;
		private const int NotEffect = 0;
		private const int Effected = 1;
		public override void InitState(SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig, double fixTime)
		{
			self.SetBuffStateIntParam(buffInfo, key, NotEffect);
		}
		public override LogicStateTickRet Tick(SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			LogicStateTickRet ret = base.Tick(self, buffInfo, buffConfig);
			if (ret == LogicStateTickRet.TimeFinish)
			{
				return ret;
			}
			if (self.GetBuffStateIntParam(buffInfo, key) == NotEffect)
			{
				self.SetBuffStateIntParam(buffInfo, key, Effected);
				return LogicStateTickRet.OnEffect;
			}
			else
			{
				return LogicStateTickRet.NextState;
			}
			return ret;
		}
	}
}
