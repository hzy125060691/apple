
namespace ZzServer.Battle
{
	[hzyBattleBase]
	public enum BuffSuperpositionType
	{
		None,
		Add,
		AddStateTime,
		Refresh,
	}

	[hzyBattleBase]
	public struct BuffSuperpositionRet
	{
		public BuffSuperpositionType bType;
		public BuffInfo_New buff;
	}
	[hzyBattleBase]
	public class BuffSuperpositionLogic
	{
		virtual public BuffSuperpositionRet OnBuffSuperposition(SkillObj tarObj, SkillObj srcObj, BattleReason reason, BuffConfig_New buffConfig)
		{
			BuffSuperpositionRet ret;
			ret.bType = BuffSuperpositionType.None;
			ret.buff = new BuffInfo_New();
			return ret;
		}
	}
}
