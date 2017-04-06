using System.Linq;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class AddStateTimeBuffSuperpositionLogic : BuffSuperpositionLogic
	{
		private const int Key_AddTime_SkillConfig = 0;
		override public BuffSuperpositionRet OnBuffSuperposition(SkillObj tarObj, SkillObj srcObj, BattleReason reason, BuffConfig_New buffConfig)
		{
			BuffSuperpositionRet ret;
			ret.bType = BuffSuperpositionType.None;
			ret.buff = null;
			var tarBuff = tarObj.GetBuffList().Where(b => tarObj.GetBuffID(b) == tarObj.GetBuffID(buffConfig));
			if (tarBuff != null && tarBuff.FirstOrDefault() != null)
			{
				ret.bType = BuffSuperpositionType.AddStateTime;
				var buff = tarBuff.First();
				ret.buff =buff;
				double addTime = tarObj.GetBuffSuperpositionDoubleParam(buffConfig, Key_AddTime_SkillConfig);
				tarObj.SetBuffTime(buff, tarObj.GetBuffTime(buff) + addTime);
				tarObj.SetBuffStateTime(buff, tarObj.GetBuffStateTime(buff) + addTime);
				tarObj.NotifyBuffInfo(buff, BattleInfoNotifyType.Time_Buff, BattleNotifyTime.TickEnd);
			}
			else
			{
				ret.bType = BuffSuperpositionType.Add;
				ret.buff = new BuffInfo_New();
			}
			return ret;
		}
	}
}
