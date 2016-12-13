using System.Linq;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class UnqueBuffKeyBuffSuperpositionLogic : BuffSuperpositionLogic
	{
		private const int Key_AddTime_SkillConfig = 0;
		override public BuffSuperpositionRet OnBuffSuperposition(SkillObj tarObj, SkillObj srcObj, BattleReason reason, BuffConfig_New buffConfig)
		{
			BuffSuperpositionRet ret;
			ret.bType = BuffSuperpositionType.None;
			ret.buff = null;
			var buffKey = tarObj.GetBuffKey(buffConfig);
			foreach (var tarBuff in tarObj.GetBuffList())
			{
				var tarBuffConfig = tarObj.GetBuffConfig(tarObj.GetBuffID(tarBuff));
				if(tarBuffConfig != null && tarObj.GetBuffKey(tarBuffConfig).Equals(buffKey))
				{
					BattleModule.RemoveBuff(tarObj, srcObj, tarObj.GetBuffID(tarBuff), BattleReason.Replace);
					//BattleModule.DetachBuff(tarObj, srcObj, tarBuff, tarBuffConfig);
				}
			}

			ret.bType = BuffSuperpositionType.Add;
			ret.buff = new BuffInfo_New();
			return ret;
		}
	}
}
