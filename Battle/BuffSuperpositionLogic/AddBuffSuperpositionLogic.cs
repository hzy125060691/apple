
namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class AddBuffSuperpositionLogic : BuffSuperpositionLogic
	{
		override public BuffSuperpositionRet OnBuffSuperposition(SkillObj tarObj, SkillObj srcObj, BattleReason reason, BuffConfig_New buffConfig)
		{
			BuffSuperpositionRet ret;
			ret.bType = BuffSuperpositionType.Add;
			ret.buff = new BuffInfo_New();
			return ret;
		}
	}
}
