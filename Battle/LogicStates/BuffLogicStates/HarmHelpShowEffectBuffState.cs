using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	class HarmHelpShowEffectBuffState : BuffLogicState
	{
		private const int Key_Int__BuffConfig = 0;
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
			//help harm 判断是否停留
			int harmHelpType = self.GetBuffStateIntParam(buffConfig, Key_Int__BuffConfig, self.GetBuffStateIndex(buffInfo));
			bool needNextState = false;
			switch(harmHelpType)
			{
				case 1:
					//这个是友好，没错
					needNextState = self.GetCamp() != self.GetSrcCamp(buffInfo);
					break;
				case 2:
					//这个是友好，没错敌对
					needNextState = self.GetCamp() == self.GetSrcCamp(buffInfo);
					break;
			}
			if (self.GetBuffStateTime(buffInfo) <= 0 || needNextState)
			{
				ret = LogicStateTickRet.NextState;
				self.LogInfo("ChargeState:buffObj[{0}] buff:[{1}] ShowEffectState Finish".F(self.GetID(), self.GetBuffID(buffInfo)));
			}
			return ret;
		}
	}
}
