using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public enum LogicStateChangeReason
	{
		None,
	}
	[hzyBattleBase]
	public enum LogicStateTickRet
	{
		None,
		NextState,
		TimeFinish,
		OnEffect,
	}
	[hzyBattleBase]
	public enum ActionLimitType
	{
		None = 0,
		Move,
		Attack,
		UseSkill,
	}
	[hzyBattleUndetermined]
	public enum PropertyType
	{
		None = 0,
		Speed,
	}
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="I">info</typeparam>
	/// <typeparam name="C">Config</typeparam>
	[hzyBattleBase]
	public class LogicState<I,C>
	{
		public virtual bool CheckConfig(C config) { return false; }
		public virtual LogicStateTickRet Tick(SkillObj self, I info, C config) { return LogicStateTickRet.None; }
		public virtual void InitSkill(SkillObj self, I info, C config, double timeFix) { }
		public virtual void InitBuff(SkillObj self, I info, C config, double timeFix) { }
		public virtual void InitState(SkillObj self, I info, C config, double timeFix) { }
		//public virtual string GetCurResult(SkillObj self, I info, C config) { return ""; }
		public virtual bool CanChangeState(LogicStateChangeReason reason, SkillObj self, I info, C config) { return false; }
		public virtual double OnStateChanged(string tarState, SkillObj self, I info, C config) { return 0; }
		public virtual bool IsActionLimited(SkillObj self, ActionLimitType limit, I info, C config) { return false; }
		public virtual bool NeedDataFix(SkillObj self, PropertyType pType, double pValue, I info, C config) { return true; }
	}
}
