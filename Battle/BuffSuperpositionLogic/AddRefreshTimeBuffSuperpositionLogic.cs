using System.Linq;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class AddRefreshTimeBuffSuperpositionLogic : BuffSuperpositionLogic
	{
		override public BuffSuperpositionRet OnBuffSuperposition(SkillObj tarObj, SkillObj srcObj, BattleReason reason, BuffConfig_New buffConfig)
		{
			BuffSuperpositionRet ret;
			ret.bType = BuffSuperpositionType.None;
			ret.buff = null;
			var tarBuff = tarObj.GetBuffList().Where(b => tarObj.GetBuffID(b) == tarObj.GetBuffID(buffConfig));
			if (tarBuff != null && tarBuff.FirstOrDefault() != null)
			{
				ret.bType = BuffSuperpositionType.Refresh;
				var buff = tarBuff.First();
				ret.buff = buff;
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
