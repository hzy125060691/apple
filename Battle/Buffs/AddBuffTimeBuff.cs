using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleUndetermined]
	public class AddBuffTimeBuff : BuffLogic
	{
		private const int Key_Int_BuffId_BuffConfig = 0;
		private const int Key_Double_AddTime1_BuffConfig = 0;
		private const int Key_Double_AddTime2_BuffConfig = 1;
		public override void OnEffect(SkillObj self, SkillObj tarObj, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			int key = Key_Double_AddTime1_BuffConfig;
			if (self.GetSrcID(buffInfo) == tarObj.GetID() || self.GetSrcParentID(buffInfo) == tarObj.GetID())
			{
				key = Key_Double_AddTime1_BuffConfig;
			}
			else
			{
				key = Key_Double_AddTime2_BuffConfig;
			}
			foreach (var buff in self.GetBuffList(true))
			{
				int buffId = self.GetBuffID(buff);
				if (buffId > 0 && buffId == self.GetBuffIntParam(buffConfig, Key_Int_BuffId_BuffConfig))
				{
					double addTime = self.GetBuffDoubleParam(buffConfig, key);
					self.SetBuffTime(buff, self.GetBuffTime(buff) + addTime);
					self.SetBuffStateTime(buff, self.GetBuffStateTime(buff) + addTime);
					self.NotifyBuffInfo(buff, BattleInfoNotifyType.Time_Buff, BattleNotifyTime.TickEnd);
				}
			}
		}
	}
}
