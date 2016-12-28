using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace ZzServer.Battle
{
	[hzyBattleEx]
	public class SkillInfo_New
	{
		public SkillInfo_New()
		{
			skill = new ZzSocketShare.Protocol.BattleLogicInfo() { BattleLogicState = new ZzSocketShare.Protocol.StateInfo()};
		}
		public ZzSocketShare.Protocol.BattleLogicInfo skill;
	}
	//using SkillInfo = ZzSocketShare.Protocol.SkillUnitInfo;
	[hzyBattleEx]
	public class SkillConfig_New
	{
		public ZzConfig.Table.SkillConfig_NewTable config;
	}
	//using SkillConfig = ZzConfig.Table.SkillConfigTable;
	[hzyBattleEx]
	public class BuffInfo_New
	{
		public BuffInfo_New()
		{
			buff = new ZzSocketShare.Protocol.BattleLogicInfo() { BattleLogicState = new ZzSocketShare.Protocol.StateInfo() };
		}
		public ZzSocketShare.Protocol.BattleLogicInfo buff;
	}
	//using BuffInfo = BUff;
	[hzyBattleEx]
	public class BuffConfig_New
	{
		public ZzConfig.Table.BuffConfig_NewTable config;
	}
	//using BuffConfig = ZzConfig.Table.SkillConfigTable;
	[hzyBattleBase]
	public enum BattleReason
	{
		None = 0,
		Skill,
		Buff,
		System,
		Bullet,
		Replace,
		Attack,
	}
	[hzyBattleBase]
	public enum DamageType
	{
		None = 0,
	}
	[hzyBattleBase]
	public class Damage
	{
		public System.Int32 value;
		public DamageType type;
		public BattleReason reason;
		public bool needCalc;
		public int srcId;
	}
	//Damage = System.Int32;

// 	public interface AttributeObj
// 	{
// 
// 	}
	[hzyBattleBase]
	public interface SkillObj_BuffConfig
	{
		int GetBuffID(BuffConfig_New buffConfig);
		BuffConfig_New GetBuffConfig(int buffID);
		string GetBuffLogicId(BuffConfig_New buffConfig);
		string GetBuffSuperpositionLogicId(BuffConfig_New buffConfig);
		double GetBuffTime(BuffConfig_New buffConfig);
		string GetBuffKey(BuffConfig_New buffConfig);
		//double GetBuffTickTime(BuffConfig buffConfig);
		//int GetActiveNum(BuffConfig_New buffConfig);
		string GetLogicState(BuffConfig_New buffConfig, int index);
		string GetTargetSelect(BuffConfig_New buffConfig);
		double GetTargetSelectDoubleParam(BuffConfig_New buffConfig, int key);
		string GetTargetType(BuffConfig_New buffConfig);
		double GetBuffStateTime(BuffConfig_New buffConfig, int index);
		int GetBuffIntParam(BuffConfig_New buffConfig, int key);
		IEnumerable<int> GetBuffIntParams(BuffConfig_New buffConfig);
		double GetBuffDoubleParam(BuffConfig_New buffConfig, int key);
		int GetBuffStateIntParam(BuffConfig_New buffConfig, int key, int index);
		double GetBuffStateDoubleParam(BuffConfig_New buffConfig, int key, int index);
		double GetBuffSuperpositionDoubleParam(BuffConfig_New buffConfig, int index);
	}
	[hzyBattleBase]
	public interface SkillObj_BuffInfo
	{
		int GetBuffID(BuffInfo_New buffInfo);
		void SetBuffId(BuffInfo_New buffInfo, int buffId);
		long GetBuffGuid(BuffInfo_New buffInfo);
		void SetBuffGuid(BuffInfo_New buffInfo, long guid);
		void SetSrcID(BuffInfo_New buffInfo, int srcId);
		int GetSrcID(BuffInfo_New buffInfo);
		int GetSrcCamp(BuffInfo_New buffInfo);
		void SetReason(BuffInfo_New buffInfo, BattleReason reason);
		BattleReason GetReason(BuffInfo_New buffInfo);
		double GetBuffTime(BuffInfo_New buffInfo);
		void SetBuffTime(BuffInfo_New buffInfo, double time);
		double GetBuffStateTime(BuffInfo_New buffInfo);
		void SetBuffStateTime(BuffInfo_New buffInfo, double time);
		int GetBuffStateIndex(BuffInfo_New buffInfo);
		void SetBuffStateIndex(BuffInfo_New buffInfo, int index);
		string GetBuffLogicStateName(BuffInfo_New buffInfo);

		int GetBuffIntParam(BuffInfo_New buffInfo, int key);
		void SetBuffIntParam(BuffInfo_New buffInfo, int key, int i);
		double GetBuffDoubleParam(BuffInfo_New buffInfo, int key);
		void SetBuffDoubleParam(BuffInfo_New buffInfo, double d, int key);

		int GetBuffStateIntParam(BuffInfo_New buffInfo, int key);
		void SetBuffStateIntParam(BuffInfo_New buffInfo, int key, int i);
		int GetSrcParentID(BuffInfo_New buffInfo);

		void NotifyBuffInfo(BuffInfo_New buffInfo, BattleInfoNotifyType nType, BattleNotifyTime nTime);
	}
	[hzyBattleBase]
	public interface SkillObj_Buff : SkillObj_BuffInfo, SkillObj_BuffConfig, SkillObj_BuffMagic
	{
		IEnumerable<BuffInfo_New> GetBuffList(bool bSelfBuffs = false);
		IEnumerable<BuffInfo_New> GetTempBuffList();
		void AddTempBuffList(BuffInfo_New buff);
		void ClearTempBuffList();
		void AddBuffList(BuffInfo_New buff);
		IEnumerable<BuffInfo_New> RemoveAllBuffs(Predicate<BuffInfo_New> match);
	}
	[hzyBattleBase]
	public interface SkillObj_BuffMagic : DamageBuff_BuffLogic { }

	[hzyBattleBase]
	public interface SkillObj_SkillConfig
	{
		int GetSkillID(SkillConfig_New skillConfig);
		SkillConfig_New GetSkillConfig(int skillID);
		string GetSkillLogicId(SkillConfig_New skillConfig);
		double GetSkillTime(SkillConfig_New skillConfig);
		double GetSkillCD(SkillConfig_New skillConfig);
		string GetSkillCDKey(SkillConfig_New skillConfig);
		//double GetSkillTickTime(SkillConfig skillConfig);
		//int GetActiveNum(SkillConfig_New skillConfig);
		string GetLogicState(SkillConfig_New skillConfig, int index);
		string GetTargetSelect(SkillConfig_New skillConfig);
		double GetTargetSelectDoubleParam(SkillConfig_New skillConfig, int key);
		string GetTargetType(SkillConfig_New skillConfig);
		double GetSkillStateTime(SkillConfig_New skillConfig, int index);
		int GetSkillIntParam(SkillConfig_New skillConfig, int key);
		IEnumerable<int> GetSkillIntParams(SkillConfig_New skillConfig);
		int GetSkillStateIntParam(SkillConfig_New skillConfig, int key, int index);
		double GetSkillStateDoubleParam(SkillConfig_New skillConfig, int key, int index);
		IEnumerable<double> GetSkillDoubleParams(SkillConfig_New skillConfig);
	}
	[hzyBattleBase]
	public enum BattleInfoNotifyType
	{
		None,
		CD_Skill,
		Time_Skill,
		ChangeState_Skill,
		All_Skill,

		Time_Buff,
		ChangeState_Buff,
		All_Buff,

	}
	[hzyBattleBase]
	public enum BattleNotifyTime
	{
		None,
		Immediately,
		TickEnd,
	}
	[hzyBattleBase]
	public interface SkillObj_SkillInfo
	{
		int GetSkillID(SkillInfo_New skillInfo);
		void SetSkillId(SkillInfo_New skillInfo, int skillId);
		long GetSkillGuid(SkillInfo_New skillInfo);
		void SetSkillGuid(SkillInfo_New skillInfo, long guid);
		double GetSkillTime(SkillInfo_New skillInfo);
		void SetSkillTime(SkillInfo_New skillInfo, double time);
		double GetSkillStateTime(SkillInfo_New skillInfo);
		void SetSkillStateTime(SkillInfo_New skillInfo, double time);
		int GetSkillStateIndex(SkillInfo_New skillInfo);
		void SetSkillStateIndex(SkillInfo_New skillInfo, int index);
		string GetSkillLogicStateName(SkillInfo_New skillInfo);

		int GetSkillIntParam(SkillInfo_New skillInfo, int key);
		void SetSkillIntParam(SkillInfo_New skillInfo, int key, int i);
		double GetSkillDoubleParam(SkillInfo_New skillInfo, int key);
		void SetSkillDoubleParam(SkillInfo_New skillInfo, double d, int key);

		void NotifySkillInfo(SkillInfo_New skillInfo, BattleInfoNotifyType nType, BattleNotifyTime nTime);
		// 		Vector3 GetSkillPosParam(SkillInfo_New skillInfo, int key);
		// 		void SetSkillPosParam(SkillInfo_New skillInfo, Vector3 pos, int key);
		// 		long GetSkillLongParam(SkillInfo_New skillInfo, int key);
		// 		void SetSkillLongParam(SkillInfo_New skillInfo, long l, int key);
		//SkillObj GetSkillSrcObj(SkillInfo_New skillInfo);
		//void SetSkillSrcObj(SkillInfo_New skillInfo, SkillObj src);

		int GetSkillStateIntParam(SkillInfo_New skillInfo, int key);
		void SetSkillStateIntParam(SkillInfo_New skillInfo, int key, int i);

		double GetSkillStateDoubleParam(SkillInfo_New skillInfo, int key);
		void SetSkillStateDoubleParam(SkillInfo_New skillInfo, int key, double d);
		
		// 		Vector3 GetSkillPosParam(SkillInfo_New skillInfo, int key);
		// 		void SetSkillPosParam(SkillInfo_New skillInfo, Vector3 pos, int key);
		// 		long GetSkillLongParam(SkillInfo_New skillInfo, int key);
		// 		void SetSkillLongParam(SkillInfo_New skillInfo, long l, int key);
		// 		SkillObj GetSkillSrcObj(SkillInfo_New skillInfo);
		// 		void SetSkillSrcObj(SkillInfo_New skillInfo, SkillObj src);



		// 		int GetActiveNum(SkillInfo_New skillInfo);
		// 		void SetActiveNum(SkillInfo_New skillInfo, int num);



	}
	[hzyBattleBase]
	public interface SkillObj_Skill : SkillObj_SkillInfo, SkillObj_SkillConfig, SkillObj_SkillMagic
	{
		bool CanUseSkill(int skillId);
		IEnumerable<SkillInfo_New> GetSkillList(bool bSelfSkills = false);
		void AddSkillList(SkillInfo_New skill);
		void RemoveAllSkills(Predicate<SkillInfo_New> match);
		IEnumerable<string> GetCDKeyList();
		void RemoveAllCDKey(Predicate<string> match);
	}
	[hzyBattleBase]
	public interface SkillObj_SkillMagic : SummonAndTraceTarget_SkillLogic { }
	[hzyBattleBase]
	public interface SkillObj_Base : SkillObjTargetType, FrontRectTargetSelect
	{
		int GetID();
		int GetParentID();
		void SetParentID(int id);
		bool CommonCheck();
		double GetNowTime();
		double GetDeltaTime();
		Vector3_Hzy GetPos();
		double GetDirRadian();
		double GetCD(string key);
		void SetCD(string key, double time);
		void CalcDamage(Damage damage, SkillObj srcObj);
		void OnDamage(Damage damage, SkillObj srcObj);
		bool IsDead();
		bool OnDie(SkillObj srcObj);
		SkillObj Summon(int id, SkillObj target, SkillInfo_New skillInfo, SkillConfig_New skillConfig);
	}
	[hzyBattleBase]
	public interface SkillObjHelper
	{
		SkillObj GetSkillObj(int id);
	}
	[hzyBattleBase]
	public interface LogObj
	{
		/// <summary>
		/// 输出个日志
		/// </summary>
		/// <param name="logInfo">日志信息</param>
		/// <param name="file">调用者的文件名（自动填充）</param>
		/// <param name="line">调用者所在行号（自动填充）</param>
		/// <param name="member">调用者的方法名（自动填充）</param>
		void LogInfo(string logInfo, [CallerFilePath] string file = "NoFile", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "None");
	}
	[hzyBattleBase]
	public interface SkillObj : SkillObj_Base, SkillObj_Skill, SkillObj_Buff, LogObj, SkillObjHelper
	{
		void NotifyBattleTime(float timeMS);
		void Tick_Battle_Late();
		void Tick_Battle();
	}
}
