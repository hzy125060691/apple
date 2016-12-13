using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ZzServer.Battle
{
	// 	using SkillInfo = ZzSocketShare.Protocol.SkillUnitInfo;
	// 	using SkillConfig = ZzConfig.Table.SkillConfigTable;
	// 	using BuffInfo = BUff;
	// 	using BuffConfig = ZzConfig.Table.SkillConfigTable;
	[hzyBattleBase]
	public enum UseSkillRet
	{
		/// <summary>
		/// 这就是一个初始值，返回这个大概是哪个地方漏写了啥去查查吧
		/// </summary>
		None,
		Success,
		Deplete,	// 消耗不满足条件
		CD,         // CD中
		ActionLimit,         // 行动被限制
		Dead,	//已经死了
	}
	/// <summary>
	/// 这个类是通过反射初始化用的，需要某个地方有调用下；
	/// </summary>
	[hzyBattleBase]
	public class BattleInitClass
	{
		public static BattleInitClass something = new BattleInitClass();
		private BattleInitClass()
		{
			var asm = Assembly.GetExecutingAssembly();

			foreach (var b in asm.GetTypes())
			{
				if(b.IsSubclassOf(typeof(SkillLogic)))
				{
					//自动填充SkillLogics
					SkillLogic ins = asm.CreateInstance(b.FullName) as SkillLogic;
					BattleModule.SkillLogics.Add(b.Name, ins);
				}
				else if(b.IsSubclassOf(typeof(SkillLogicState)))
				{
					//自动填充SkillLogicStates
					SkillLogicState ins = asm.CreateInstance(b.FullName) as SkillLogicState;
					BattleModule.SkillLogicStates.Add(b.Name, ins);
				}
				else if(b.IsSubclassOf(typeof(TargetSelect)))
				{
					//自动填充TargetSelect
					TargetSelect ins = asm.CreateInstance(b.FullName) as TargetSelect;
					BattleModule.TargetSelects.Add(b.Name, ins);
				}
				else if (b.IsSubclassOf(typeof(TargetType)))
				{
					//自动填充TargetType
					TargetType ins = asm.CreateInstance(b.FullName) as TargetType;
					BattleModule.TargetTypes.Add(b.Name, ins);
				}
				else if (b.IsSubclassOf(typeof(BuffLogicState)))
				{

					//自动填充BuffLogicStates
					BuffLogicState ins = asm.CreateInstance(b.FullName) as BuffLogicState;
					BattleModule.BuffLogicStates.Add(b.Name, ins);
				}
				else if (b.IsSubclassOf(typeof(BuffLogic)))
				{
					//自动填充BuffLogicStates
					BuffLogic ins = asm.CreateInstance(b.FullName) as BuffLogic;
					BattleModule.BuffLogics.Add(b.Name, ins);
				}
				else if(b.IsSubclassOf(typeof(BuffSuperpositionLogic)))
				{
					//自动填充BuffSuperpositionLogics
					BuffSuperpositionLogic ins = asm.CreateInstance(b.FullName) as BuffSuperpositionLogic;
					BattleModule.BuffSuperpositionLogics.Add(b.Name, ins);
				}
				else if(b.IsSubclassOf(typeof(SkillBulletMovingLogic)))
				{
					//自动填充SkillBulletMovingLogic
					SkillBulletMovingLogic ins = asm.CreateInstance(b.FullName) as SkillBulletMovingLogic;
					BattleModule.SkillBulletMovingLogics.Add(b.Name, ins);
				}
				else if (b.IsSubclassOf(typeof(SkillBulletCollisionLogic)))
				{
					//自动填充SkillBulletCollisionLogic
					SkillBulletCollisionLogic ins = asm.CreateInstance(b.FullName) as SkillBulletCollisionLogic;
					BattleModule.SkillBulletCollisionLogics.Add(b.Name, ins);
				}
			}
		}
		public void OutputSomething()
		{
			if(BattleInitClass.something != null)
			{
			}
		}
	}
	[hzyBattleBase]
	public static class BattleModule
	{
		private static int battleID = (int)DateTime.Now.Ticks;
		private static DateTime battleBeginTime = DateTime.Now;
		public static int BattleID
		{
			get
			{
				return battleID++;
			}
		}
		public static int BattleTickCount = 0;
		public static void ResetBattleID()
		{
			battleID = (int)DateTime.Now.Ticks;
		}
		public static void ResetBattleTime()
		{
			battleBeginTime = DateTime.Now;
		}
		public static void NotifyTimeOnEnter(SkillObj player)
		{
			player.NotifyBattleTime(battleBeginTime.Ticks / (10000f));
		}
		public static bool RemoveBuff(SkillObj tarObj, SkillObj srcObj, int deleteBuffId, BattleReason reason)
		{
			if (tarObj == null)
			{
				return false;
			}
			var buffConfig = tarObj.GetBuffConfig(deleteBuffId);
			if (buffConfig == null)
			{
				return false;
			}

			var tarBuffList = tarObj.GetBuffList();
			BuffInfo_New buff = null;
			foreach (var b in tarBuffList)
			{
				if (tarObj.GetBuffID(b) == deleteBuffId)
				{
					buff = b;
					break;
				}
			}
			if (buff == null)
			{
				return false;
			}
			//DetachBuff(tarObj, srcObj, buff, buffConfig);
			var buffLogicId = tarObj.GetBuffLogicId(buffConfig);
			if (!BuffLogics.ContainsKey(buffLogicId))
			{
				return false;
			}
			BuffLogic buffLogic = BuffLogics[buffLogicId];
			buffLogic.BuffOnEnd(tarObj, buff, buffConfig);

			////先从src方进行修正
			//if (srcObj != null)
			//{
			//	var srcSkillList = srcObj.GetSkillList();
			//	var srcBuffList = srcObj.GetBuffList();
			//	if (srcSkillList != null)
			//	{
			//		foreach (var skillInfo in srcSkillList)
			//		{
			//			int skillId = srcObj.GetSkillID(skillInfo);
			//			SkillConfig_New skillConfig = srcObj.GetSkillConfig(skillId);
			//			var logic = GetSkillLogic(skillInfo, srcObj);
			//			logic.OnClearBuff(tarObj, srcObj, buff, skillInfo, skillConfig);
			//		}
			//	}
			//	if (srcBuffList != null)
			//	{
			//		foreach (var buffInfo in srcBuffList)
			//		{
			//			int buffId = srcObj.GetBuffID(buffInfo);
			//			BuffConfig_New srcBuffConfig = srcObj.GetBuffConfig(buffId);
			//			var logic = GetBuffLogic(buffInfo, srcObj);
			//			logic.OnClearBuff(tarObj, srcObj, buff, buffInfo, srcBuffConfig);
			//		}
			//	}
			//}
			//{
			//	var tarSkillList = tarObj.GetSkillList();
			//	if (tarSkillList != null)
			//	{
			//		foreach (var skillInfo in tarSkillList)
			//		{
			//			int skillId = tarObj.GetSkillID(skillInfo);
			//			SkillConfig_New skillConfig = tarObj.GetSkillConfig(skillId);
			//			var logic = GetSkillLogic(skillInfo, tarObj);
			//			logic.OnDetachBuff(tarObj, srcObj, buff, skillInfo, skillConfig);
			//		}
			//	}
			//	if (tarBuffList != null)
			//	{
			//		foreach (var buffInfo in tarBuffList)
			//		{
			//			int buffId = tarObj.GetBuffID(buffInfo);
			//			BuffConfig_New tarBuffConfig = tarObj.GetBuffConfig(buffId);
			//			var logic = GetBuffLogic(buffInfo, tarObj);
			//			logic.OnDetachBuff(tarObj, srcObj, buff, buffInfo, tarBuffConfig);
			//		}
			//	}
			//}
			//buffLogic.BuffOnEnd(tarObj, buff, buffConfig);
			return true;
		}
		public static bool AddBuff(SkillObj tarObj, SkillObj srcObj, int newBuffId, BattleReason reason)
		{
			if (tarObj == null)
			{
				return false;
			}
			var buffConfig = tarObj.GetBuffConfig(newBuffId);
			if(buffConfig == null)
			{
				return false;
			}

			var buffSuperpositionLogicId = tarObj.GetBuffSuperpositionLogicId(buffConfig);
			if (!BuffSuperpositionLogics.ContainsKey(buffSuperpositionLogicId))
			{
				return false;
			}
			BuffSuperpositionLogic buffSuperpositionLogic = BuffSuperpositionLogics[buffSuperpositionLogicId];

			var buffRet = buffSuperpositionLogic.OnBuffSuperposition(tarObj, srcObj, reason, buffConfig);
			BuffInfo_New buff = buffRet.buff;
			if(buffRet.bType == BuffSuperpositionType.Refresh)
			{
				var buffLogicId = tarObj.GetBuffLogicId(buffConfig);
				if (!BuffLogics.ContainsKey(buffLogicId))
				{
					return false;
				}
				BuffLogic buffLogic = BuffLogics[buffLogicId];
				buffLogic.InitBuffInfo(tarObj, srcObj, reason, buff, buffConfig, false);
			}
			else if(buffRet.bType == BuffSuperpositionType.Add)
			{
				var buffLogicId = tarObj.GetBuffLogicId(buffConfig);
				if (!BuffLogics.ContainsKey(buffLogicId))
				{
					return false;
				}
				BuffLogic buffLogic = BuffLogics[buffLogicId];
				buffLogic.InitBuffInfo(tarObj, srcObj, reason, buff, buffConfig);

				//先从src方进行修正
				if (srcObj != null)
				{
					var srcSkillList = srcObj.GetSkillList();
					var srcBuffList = srcObj.GetBuffList();
					if (srcSkillList != null)
					{
						foreach (var skillInfo in srcSkillList)
						{
							int skillId = srcObj.GetSkillID(skillInfo);
							SkillConfig_New skillConfig = srcObj.GetSkillConfig(skillId);
							var logic = GetSkillLogic(skillInfo, srcObj);
							logic.OnSendBuff(tarObj, srcObj, buff, skillInfo, skillConfig);
						}
					}
					if (srcBuffList != null)
					{
						foreach (var buffInfo in srcBuffList)
						{
							int buffId = srcObj.GetBuffID(buffInfo);
							BuffConfig_New srcBuffConfig = srcObj.GetBuffConfig(buffId);
							var logic = GetBuffLogic(buffInfo, srcObj);
							logic.OnSendBuff(tarObj, srcObj, buff, buffInfo, srcBuffConfig);
						}
					}
				}
				{
					var tarSkillList = tarObj.GetSkillList();
					var tarBuffList = tarObj.GetBuffList();
					if (tarSkillList != null)
					{
						foreach (var skillInfo in tarSkillList)
						{
							int skillId = tarObj.GetSkillID(skillInfo);
							SkillConfig_New skillConfig = tarObj.GetSkillConfig(skillId);
							var logic = GetSkillLogic(skillInfo, tarObj);
							logic.OnAttachBuff(tarObj, srcObj, buff, skillInfo, skillConfig);
						}
					}
					if (tarBuffList != null)
					{
						foreach (var buffInfo in tarBuffList)
						{
							int buffId = tarObj.GetBuffID(buffInfo);
							BuffConfig_New tarBuffConfig = tarObj.GetBuffConfig(buffId);
							var logic = GetBuffLogic(buffInfo, tarObj);
							logic.OnAttachBuff(tarObj, srcObj, buff, buffInfo, tarBuffConfig);
						}
					}
				}
				buffLogic.OnAttach(tarObj, srcObj, buff, buffConfig);
				tarObj.AddTempBuffList(buff);
			}
			tarObj.NotifyBuffInfo(buff, BattleInfoNotifyType.All_Buff, BattleNotifyTime.TickEnd);
			return true;
		}
		private static BuffLogic GetBuffLogic(BuffInfo_New buffInfo, SkillObj logObj)
		{
			int buffId = logObj.GetBuffID(buffInfo);
			BuffConfig_New buffConfig = logObj.GetBuffConfig(buffId);
			if (buffConfig == null)
			{
				logObj.LogInfo("BuffConfig not found buffId[{0}]".F(buffId));
				return null;
			}
			string buffLogicId = logObj.GetBuffLogicId(buffConfig);
			if (!BuffLogics.ContainsKey(buffLogicId))
			{
				logObj.LogInfo("BuffLogic not found buffId[{0}] buffLogicId[{1}]".F(buffId, buffLogicId));
				return null;
			}
			BuffLogic buffLogic = BuffLogics[buffLogicId];
			return buffLogic;
		}
		public static SkillLogicState GetSkillLogicState(SkillInfo_New skillInfo, SkillObj logObj)
		{
			string stateName = logObj.GetSkillLogicStateName(skillInfo);
			SkillLogicState state = GetSkillLogicState(stateName);
			if (state == null && logObj != null)
			{
				logObj.LogInfo("SkillLogicStates not found stateName[{0}]".F(stateName));
			}
			return state;
		}
		public static BuffLogicState GetBuffLogicState(BuffInfo_New buffInfo, SkillObj logObj)
		{
			string stateName = logObj.GetBuffLogicStateName(buffInfo);
			BuffLogicState state = GetBuffLogicState(stateName);
			if (state == null && logObj != null)
			{
				logObj.LogInfo("BuffLogicState not found stateName[{0}]".F(stateName));
			}
			return state;
		}
		public static SkillLogicState GetSkillLogicState(string name)
		{
			if (!SkillLogicStates.ContainsKey(name))
			{
				return null;
			}
			SkillLogicState state = SkillLogicStates[name];
			return state;
		}
		public static BuffLogicState GetBuffLogicState(string name)
		{
			if (!BuffLogicStates.ContainsKey(name))
			{
				return null;
			}
			BuffLogicState state = BuffLogicStates[name];
			return state;
		}
		public static TargetSelect GetTargetSelect(string name)
		{
			if (!TargetSelects.ContainsKey(name))
			{
				return null;
			}
			TargetSelect select = TargetSelects[name];
			return select;
		}
		public static TargetType GetTargetType(string name)
		{
			if (!TargetTypes.ContainsKey(name))
			{
				return null;
			}
			TargetType tType = TargetTypes[name];
			return tType;
		}
		private static SkillLogic GetSkillLogic(SkillInfo_New skillInfo, SkillObj logObj)
		{
			int skillId = logObj.GetSkillID(skillInfo);
			SkillConfig_New buffConfig = logObj.GetSkillConfig(skillId);
			if (buffConfig == null)
			{
				logObj.LogInfo("SkillConfig not found skillid[{0}]".F(skillId));
				return null;
			}
			string skillLogicId = logObj.GetSkillLogicId(buffConfig);
			if (!SkillLogics.ContainsKey(skillLogicId))
			{
				logObj.LogInfo("SkillLogic not found skillid[{0}] SkillLogicId[{1}]".F(skillId, skillLogicId));
				return null;
			}
			SkillLogic skillLogic = SkillLogics[skillLogicId];
			return skillLogic;
		}
		public static IEnumerable<SkillObj> GetTargets(SkillObj skillObj, TargetSelect select, TargetType tType, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			if (select == null || tType == null)
			{
				return null;
			}
			var ret = select.GetTargets(skillObj, skillInfo, skillConfig).Where(t=> tType.IsTarget(skillObj, t));
			return ret;
		}
		public static IEnumerable<SkillObj> GetTargets(SkillObj skillObj, TargetSelect select, TargetType tType, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			if (select == null || tType == null)
			{
				return null;
			}
			var ret = select.GetTargets(skillObj, buffInfo, buffConfig).Where(t => tType.IsTarget(skillObj, t));
			return ret;
		}
		/// <summary>
		/// 使用技能，这个需要手动调用
		/// </summary>
		/// <param name="self"></param>
		/// <param name="skillId"></param>
		/// <returns></returns>
		public static UseSkillRet UseSkill(SkillObj self, int skillId)
		{
			UseSkillRet ret = UseSkillRet.Success;
			if (BattleModule.IsActionLimited(self, ActionLimitType.UseSkill))
			{
				return UseSkillRet.ActionLimit;
			}
			if(self.IsDead())
			{
				return UseSkillRet.Dead;
			}
			//检查特殊状态
			if (!self.CanUseSkill(skillId))
			{
				return UseSkillRet.None;
			}
			SkillConfig_New skillConfig = self.GetSkillConfig(skillId);
			if (skillConfig == null)
			{
				return UseSkillRet.None;
			}
			//检查技能消耗
			if (!UseSkillCheckDeplete(self, skillConfig))
			{
				return UseSkillRet.Deplete;
			}
			string skillLogicId = self.GetSkillLogicId(skillConfig);
			if (!SkillLogics.ContainsKey(skillLogicId))
			{
				return UseSkillRet.None;
			}

			SkillLogic skillLogic = SkillLogics[skillLogicId];
			if(skillLogic != null)
			{
				//检查CD
				if (!skillLogic.CheckCD(self, skillConfig))
				{
					string key = self.GetSkillCDKey(skillConfig);
					self.LogInfo("skill:[{0}] CD ing [{1}]:[{2}]".F(self.GetSkillID(skillConfig), key, self.GetCD(key)));
					return UseSkillRet.CD;
				}
				SkillInfo_New skillInfo = skillLogic.Begin(self, skillConfig);
				if (skillInfo != null)
				{
					skillLogic.BeginCD(self, skillInfo, skillConfig);
					self.AddSkillList(skillInfo);
					self.LogInfo("Skill Begin Successed [{0}]".F(skillId));
				}
				else
				{
					self.LogInfo("Skill Begin Failed [{0}]".F(skillId));
				}
			}
			return ret;
		}
		/// <summary>
		/// 单人技能的tick，这个需要手动调用
		/// </summary>
		/// <param name="self"></param>
		public static void Tick_Battle(SkillObj self)
		{
			if (self.IsDead())
			{
				return;
			}
			var skillList = self.GetSkillList(true);
			var buffList = self.GetBuffList(true);
			var CDKeyList = self.GetCDKeyList();
			//buff
			if (buffList != null)
			{
				foreach (var buffInfo in buffList)
				{
					BuffLogic buffLogic = GetBuffLogic(buffInfo, self);
					if (buffLogic != null)
					{
						int buffId = self.GetBuffID(buffInfo);
						BuffConfig_New buffConfig = self.GetBuffConfig(buffId);
						buffLogic.Tick(self, buffInfo, buffConfig);
					}
					else
					{
						self.LogInfo("something error {0}".F(self.GetBuffID(buffInfo)));
					}
				}
			}
			//技能
			if (skillList != null)
			{
				foreach (var skillInfo in skillList)
				{
					SkillLogic skillLogic = GetSkillLogic(skillInfo, self);
					if (skillLogic != null)
					{
						int skillId = self.GetSkillID(skillInfo);
						SkillConfig_New skillConfig = self.GetSkillConfig(skillId);
						skillLogic.Tick(self, skillInfo, skillConfig);
					}
					else
					{
						self.LogInfo("something error2 {0}".F(self.GetSkillID(skillInfo)));
					}
				}
			}
			//CD
			if(CDKeyList != null)
			{
				foreach (var key in CDKeyList)
				{
					self.SetCD(key, self.GetCD(key) - self.GetDeltaTime());
				}
			}
		}
		/// <summary>
		/// 技能的late tick，这是处理一些技能时间到，buff时间到之后的后续处理，需要手动调用
		/// </summary>
		/// <param name="self"></param>
		public static void Tick_Battle_Late(SkillObj self)
		{
			self.Tick_Battle_Late();
			//这个时候已经不再使用buff的迭代器，可以改变集合了,因为这里边的buff已经认为是被加在角色身上了，所以需要有后续处理得先加进去
			foreach (var b in self.GetTempBuffList())
			{
				self.AddBuffList(b);
			}
			self.ClearTempBuffList();
			IEnumerable<BuffInfo_New> detachBuffs = null;
			if (self.IsDead())
			{
				detachBuffs = self.RemoveAllBuffs((b) => true);
				self.RemoveAllSkills((s) => true);
				self.RemoveAllCDKey((c) => true);
			}else
			{
				detachBuffs = self.RemoveAllBuffs(b => self.GetBuffTime(b) <= 0);
				self.RemoveAllSkills(s => self.GetSkillTime(s) <= 0);
				self.RemoveAllCDKey(s => self.GetCD(s) <= 0);
			}
			foreach (var delBuf in detachBuffs)
			{
				var buffConfig = self.GetBuffConfig(self.GetBuffID(delBuf));
				if (buffConfig == null)
				{
					continue;
				}
				//foreach (var buff in GetTankData().BuffInfos)
				{
					BattleModule.DetachBuff(self, null, delBuf, buffConfig);
				}
			}
		}
		public static void DetachBuff(SkillObj tarObj, SkillObj srcObj, BuffInfo_New buff, BuffConfig_New buffConfig)
		{
			var buffLogicId = tarObj.GetBuffLogicId(buffConfig);
			if (!BuffLogics.ContainsKey(buffLogicId))
			{
				return ;
			}
			BuffLogic buffLogic = BuffLogics[buffLogicId];

			//先从src方进行修正
			if (srcObj != null)
			{
				var srcSkillList = srcObj.GetSkillList();
				var srcBuffList = srcObj.GetBuffList();
				if (srcSkillList != null)
				{
					foreach (var skillInfo in srcSkillList)
					{
						int skillId = srcObj.GetSkillID(skillInfo);
						SkillConfig_New skillConfig = srcObj.GetSkillConfig(skillId);
						var logic = GetSkillLogic(skillInfo, srcObj);
						logic.OnClearBuff(tarObj, srcObj, buff, skillInfo, skillConfig);
					}
				}
				if (srcBuffList != null)
				{
					foreach (var buffInfo in srcBuffList)
					{
						int buffId = srcObj.GetBuffID(buffInfo);
						BuffConfig_New srcBuffConfig = srcObj.GetBuffConfig(buffId);
						var logic = GetBuffLogic(buffInfo, srcObj);
						logic.OnClearBuff(tarObj, srcObj, buff, buffInfo, srcBuffConfig);
					}
				}
			}
			{
				var tarSkillList = tarObj.GetSkillList();
				var tarBuffList = tarObj.GetBuffList();
				if (tarSkillList != null)
				{
					foreach (var skillInfo in tarSkillList)
					{
						int skillId = tarObj.GetSkillID(skillInfo);
						SkillConfig_New skillConfig = tarObj.GetSkillConfig(skillId);
						var logic = GetSkillLogic(skillInfo, tarObj);
						logic.OnDetachBuff(tarObj, srcObj, buff, skillInfo, skillConfig);
					}
				}
				if (tarBuffList != null)
				{
					foreach (var buffInfo in tarBuffList.Where(b=>b.buff!=buff.buff))
					{
						int buffId = tarObj.GetBuffID(buffInfo);
						BuffConfig_New tarBuffConfig = tarObj.GetBuffConfig(buffId);
						var logic = GetBuffLogic(buffInfo, tarObj);
						logic.OnDetachBuff(tarObj, srcObj, buff, buffInfo, tarBuffConfig);
					}
				}
			}
			buffLogic.BuffOnEnd(tarObj, buff, buffConfig);
			buffLogic.OnDetach(tarObj, srcObj, buff, buffConfig);
		}
		public static bool IsActionLimited(SkillObj self, ActionLimitType limit)
		{
			var skillList = self.GetSkillList();
			var buffList = self.GetBuffList();
			//buff
			if (buffList != null)
			{
				foreach (var buffInfo in buffList)
				{
					BuffLogic buffLogic = GetBuffLogic(buffInfo, self);
					if (buffLogic != null)
					{
						int buffId = self.GetBuffID(buffInfo);
						BuffConfig_New buffConfig = self.GetBuffConfig(buffId);
						if(buffLogic.IsActionLimited(self, limit, buffInfo, buffConfig))
						{
							return true;
						}
					}
					else
					{
						self.LogInfo("something error {0}".F(self.GetBuffID(buffInfo)));
					}
				}
			}
			//技能
			if (skillList != null)
			{
				foreach (var skillInfo in skillList)
				{
					SkillLogic skillLogic = GetSkillLogic(skillInfo, self);
					if (skillLogic != null)
					{
						int skillId = self.GetSkillID(skillInfo);
						SkillConfig_New skillConfig = self.GetSkillConfig(skillId);
						if (skillLogic.IsActionLimited(self, limit, skillInfo, skillConfig))
						{
							return true;
						}
					}
					else
					{
						self.LogInfo("something error2 {0}".F(self.GetSkillID(skillInfo)));
					}
				}
			}
			return false;
		}
		/// <summary>
		/// 消耗检查
		/// </summary>
		/// <param name="self"></param>
		/// <param name="skillConfig"></param>
		/// <returns></returns>
		private static bool UseSkillCheckDeplete(SkillObj self, SkillConfig_New skillConfig)
		{
			return true;
		}

		public static Damage CreateDamage(int iValue, int iSrcId = -1,bool bNeedCalc = true, DamageType eType = DamageType.None, BattleReason eReason = BattleReason.None)
		{
			return new Damage() { value = iValue, type = eType, reason = eReason, needCalc = bNeedCalc, srcId = iSrcId };
		}
		private static void CalcDamage(SkillObj tarObj, SkillObj srcObj, Damage damage)
		{
			if (tarObj == null)
			{
				return;
			}
			tarObj.CalcDamage(damage, srcObj);
		}
		private static void DamageFix(SkillObj tarObj, SkillObj srcObj, Damage damage)
		{
			if(tarObj == null)
			{
				return;
			}
			if(srcObj != null)
			{
				var srcSkillList = srcObj.GetSkillList();
				var srcBuffList = srcObj.GetBuffList();
				//Source skill fix
				if(srcSkillList != null)
				{
					foreach (var skillInfo in srcSkillList)
					{
						SkillLogic skillLogic = GetSkillLogic(skillInfo, srcObj);
						int skillId = srcObj.GetSkillID(skillInfo);
						if (skillLogic != null)
						{
							SkillConfig_New skillConfig = srcObj.GetSkillConfig(skillId);
							skillLogic.DamageTargetFix(srcObj, tarObj, damage, skillInfo, skillConfig);
						}
						else
						{
							srcObj.LogInfo("something error in BattleModule.DamageFix src skill:[{0}]".F(skillId));
						}
					}
				}
				//Source buff fix
				if (srcBuffList != null)
				{
					foreach (var buffInfo in srcBuffList)
					{
						BuffLogic buffLogic = GetBuffLogic(buffInfo, srcObj);
						int buffId = srcObj.GetBuffID(buffInfo);
						if (buffLogic != null)
						{
							BuffConfig_New skillConfig = srcObj.GetBuffConfig(buffId);
							buffLogic.DamageTargetFix(srcObj, tarObj, damage, buffInfo, skillConfig);
						}
						else
						{
							srcObj.LogInfo("something error in BattleModule.DamageFix src buff:[{0}]".F(buffId));
						}
					}
				}
			}
			{
				var tarBuffList = tarObj.GetBuffList();
				var tarSkillList = tarObj.GetSkillList();
				//Target skill fix
				if (tarSkillList != null)
				{
					foreach (var skillInfo in tarSkillList)
					{
						SkillLogic skillLogic = GetSkillLogic(skillInfo, tarObj);
						int skillId = tarObj.GetSkillID(skillInfo);
						if (skillLogic != null)
						{
							SkillConfig_New skillConfig = tarObj.GetSkillConfig(skillId);
							skillLogic.BeHurtDamageFix(tarObj, srcObj, damage, skillInfo, skillConfig);
						}
						else
						{
							tarObj.LogInfo("something error in BattleModule.DamageFix tar skill:[{0}]".F(skillId));
						}
					}
				}
				//Target buff fix
				if (tarBuffList != null)
				{
					foreach (var buffInfo in tarBuffList)
					{
						BuffLogic buffLogic = GetBuffLogic(buffInfo, tarObj);
						int buffId = tarObj.GetBuffID(buffInfo);
						if (buffLogic != null)
						{
							BuffConfig_New skillConfig = tarObj.GetBuffConfig(buffId);
							buffLogic.BeHurtDamageFix(tarObj, srcObj, damage, buffInfo, skillConfig);
						}
						else
						{
							tarObj.LogInfo("something error in BattleModule.DamageFix tar buff:[{0}]".F(buffId));
						}
					}
				}
			}
		}
		/// <summary>
		/// 造成伤害或治疗
		/// </summary>
		/// <param name="tarObj"></param>
		/// <param name="srcObj"></param>
		/// <param name="damage">大于0是伤害，小于0是治疗</param>
		public static void DamageTarget(SkillObj tarObj, SkillObj srcObj, Damage damage)
		{
			SkillObj realSrcObj = srcObj;
			if(tarObj == null || tarObj.IsDead())
			{
				return;
			}
			if(realSrcObj != null && damage.srcId > 0 && realSrcObj.GetID() != damage.srcId)
			{
				realSrcObj = tarObj.GetSkillObj(damage.srcId);
			}
			BattleModule.CalcDamage(tarObj, realSrcObj, damage);
			//先修正伤害
			BattleModule.DamageFix(tarObj, realSrcObj, damage);
			//质量和造成伤害都是这个
			//造成伤害
			if(damage.value >= 0)
			{
				tarObj.OnDamage(damage, realSrcObj);
			}
			else
			{
				tarObj.OnDamage(damage, realSrcObj);
			}
			//检查技能与BUFF的相应触发
			if (realSrcObj != null && !realSrcObj.IsDead())
			{
				var srcSkillList = realSrcObj.GetSkillList();
				var srcBuffList = realSrcObj.GetBuffList();
				//Source skill fix
				if (srcSkillList != null)
				{
					foreach (var skillInfo in srcSkillList)
					{
						SkillLogic skillLogic = GetSkillLogic(skillInfo, realSrcObj);
						int skillId = realSrcObj.GetSkillID(skillInfo);
						if (skillLogic != null)
						{
							SkillConfig_New skillConfig = realSrcObj.GetSkillConfig(skillId);
							if (damage.value >= 0)
							{
								skillLogic.OnDamageTarget(realSrcObj, tarObj, damage, skillInfo, skillConfig);
							}
							else
							{
								skillLogic.OnHealTarget(realSrcObj, tarObj, damage, skillInfo, skillConfig);
							}
						}
						else
						{
							realSrcObj.LogInfo("something error in BattleModule.DamageTarget src skill:[{0}]".F(skillId));
						}
					}
				}
				//Source buff fix
				if (srcBuffList != null)
				{
					foreach (var buffInfo in srcBuffList)
					{
						BuffLogic buffLogic = GetBuffLogic(buffInfo, realSrcObj);
						int buffId = realSrcObj.GetBuffID(buffInfo);
						if (buffLogic != null)
						{
							BuffConfig_New buffConfig = realSrcObj.GetBuffConfig(buffId);
							if (damage.value >= 0)
							{
								buffLogic.OnDamageTarget(realSrcObj, tarObj, damage, buffInfo, buffConfig);
							}
							else
							{
								buffLogic.OnHealTarget(realSrcObj, tarObj, damage, buffInfo, buffConfig);
							}
						}
						else
						{
							realSrcObj.LogInfo("something error in BattleModule.DamageTarget src buff:[{0}]".F(buffId));
						}
					}
				}
			}

			var tarSkillList = tarObj.GetSkillList();
			var tarBuffList = tarObj.GetBuffList();
			//Target skill fix
			if(!tarObj.IsDead())
			{
				if (tarSkillList != null)
				{
					foreach (var skillInfo in tarSkillList)
					{
						SkillLogic skillLogic = GetSkillLogic(skillInfo, tarObj);
						int skillId = tarObj.GetSkillID(skillInfo);
						if (skillLogic != null)
						{
							SkillConfig_New skillConfig = tarObj.GetSkillConfig(skillId);
							if (damage.value >= 0)
							{
								skillLogic.OnBeHurt(tarObj, realSrcObj, damage, skillInfo, skillConfig);
							}
							else
							{
								skillLogic.OnBeHeal(tarObj, realSrcObj, damage, skillInfo, skillConfig);
							}
						}
						else
						{
							tarObj.LogInfo("something error in BattleModule.DamageTarget tar skill:[{0}]".F(skillId));
						}
					}
				}
				//Target buff fix
				if (tarBuffList != null)
				{
					foreach (var buffInfo in tarBuffList)
					{
						BuffLogic buffLogic = GetBuffLogic(buffInfo, tarObj);
						int buffId = tarObj.GetBuffID(buffInfo);
						if (buffLogic != null)
						{
							BuffConfig_New buffConfig = tarObj.GetBuffConfig(buffId);
							if (damage.value >= 0)
							{
								buffLogic.OnBeHurt(tarObj, realSrcObj, damage, buffInfo, buffConfig);
							}
							else
							{
								buffLogic.OnBeHeal(tarObj, realSrcObj, damage, buffInfo, buffConfig);
							}
						}
						else
						{
							tarObj.LogInfo("something error in BattleModule.DamageTarget tar buff:[{0}]".F(buffId));
						}
					}
				}
			}

			if (tarObj.IsDead())
			{
				if (tarSkillList != null)
				{
					foreach (var skillInfo in tarSkillList)
					{
						SkillLogic skillLogic = GetSkillLogic(skillInfo, tarObj);
						int skillId = tarObj.GetSkillID(skillInfo);
						if (skillLogic != null)
						{
							SkillConfig_New skillConfig = tarObj.GetSkillConfig(skillId);
							skillLogic.OnDie(tarObj, realSrcObj, skillInfo, skillConfig);
						}
						else
						{
							tarObj.LogInfo("something error in BattleModule.DamageTarget tar skill:[{0}]".F(skillId));
						}
					}
				}
				//Target buff fix
				if (tarBuffList != null)
				{
					foreach (var buffInfo in tarBuffList)
					{
						BuffLogic buffLogic = GetBuffLogic(buffInfo, tarObj);
						int buffId = tarObj.GetBuffID(buffInfo);
						if (buffLogic != null)
						{
							BuffConfig_New buffConfig = tarObj.GetBuffConfig(buffId);
							buffLogic.OnDie(tarObj, realSrcObj, buffInfo, buffConfig);
						}
						else
						{
							tarObj.LogInfo("something error in BattleModule.DamageTarget tar buff:[{0}]".F(buffId));
						}
					}
				}
				bool ret = tarObj.OnDie(realSrcObj);
			}
		}
		public static SkillObj Summon(int id, SkillObj srcObj, SkillObj tarObj, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			SkillObj summonObj = null;
			if (srcObj != null)
			{
				summonObj = srcObj.Summon(id, tarObj, skillInfo, skillConfig);
				var srcBuffList = srcObj.GetBuffList();
				//Source buff fix
				if (srcBuffList != null)
				{
					foreach (var buffInfo in srcBuffList)
					{
						BuffLogic buffLogic = GetBuffLogic(buffInfo, srcObj);
						int buffId = srcObj.GetBuffID(buffInfo);
						if (buffLogic != null)
						{
							BuffConfig_New buffConfig = srcObj.GetBuffConfig(buffId);
							buffLogic.OnSummon(id, srcObj, summonObj, buffInfo, buffConfig);
						}
						else
						{
							srcObj.LogInfo("something error in BattleModule.Summon src buff:[{0}]".F(buffId));
						}
					}
				}
			}
			return summonObj;
		}
		public static double DataFix(SkillObj obj, PropertyType pType, double pValue)
		{
// 			var srcSkillList = obj.GetSkillList();
// 			var srcBuffList = obj.GetBuffList();
// 			//Source skill fix
// 			if (srcSkillList != null)
// 			{
// 				foreach (var skillInfo in srcSkillList)
// 				{
// 					SkillLogic skillLogic = GetSkillLogic(skillInfo, obj);
// 					int skillId = obj.GetSkillID(skillInfo);
// 					if (skillLogic != null)
// 					{
// 						SkillConfig_New skillConfig = obj.GetSkillConfig(skillId);
// 						skillLogic.DataFix(obj, pType, pValue, skillInfo, skillConfig);
//
// 					}
// 					else
// 					{
// 						obj.LogInfo("something error in BattleModule.DamageTarget src skill:[{0}]".F(skillId));
// 					}
// 				}
// 			}
// 			//Source buff fix
// 			if (srcBuffList != null)
// 			{
// 				foreach (var buffInfo in srcBuffList)
// 				{
// 					BuffLogic buffLogic = GetBuffLogic(buffInfo, obj);
// 					int buffId = obj.GetBuffID(buffInfo);
// 					if (buffLogic != null)
// 					{
// 						BuffConfig_New buffConfig = obj.GetBuffConfig(buffId);
// 						buffLogic.DataFix(obj, pType, pValue, buffInfo, buffConfig);
// 					}
// 					else
// 					{
// 						obj.LogInfo("something error in BattleModule.DamageTarget src buff:[{0}]".F(buffId));
// 					}
// 				}
// 			}
			return pValue;
		}
		/// <summary>
		/// 格式化字符串，防止别的地方没有
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string F(this string format, params object[] args)
		{
			try
			{
				return string.Format(format, args);
			}
			catch (Exception )
			{
				return format;
			}
		}

		private static Dictionary<string, SkillLogic> g_SkillLogics = new Dictionary<string, SkillLogic>();
		public static Dictionary<string, SkillLogic> SkillLogics
		{
			get
			{
				return g_SkillLogics;
			}
		}
		private static Dictionary<string, BuffLogic> g_BuffLogics = new Dictionary<string, BuffLogic>();
		public static Dictionary<string, BuffLogic> BuffLogics
		{
			get
			{
				return g_BuffLogics;
			}
		}

		private static Dictionary<string, SkillLogicState> g_SkillLogicStates = new Dictionary<string, SkillLogicState>();
		public static Dictionary<string, SkillLogicState> SkillLogicStates
		{
			get
			{
				return g_SkillLogicStates;
			}
		}
		private static Dictionary<string, BuffLogicState> g_BuffLogicStates = new Dictionary<string, BuffLogicState>();
		public static Dictionary<string, BuffLogicState> BuffLogicStates
		{
			get
			{
				return g_BuffLogicStates;
			}
		}
		// 		private static Dictionary<string, BuffLogicState> g_BuffLogicStates = new Dictionary<string, BuffLogicState>();
		// 		public static Dictionary<string, BuffLogicState> BuffLogicStates
		// 		{
		// 			get
		// 			{
		// 				return g_BuffLogicStates;
		// 			}
		// 		}

		private static Dictionary<string, TargetSelect> g_TargetSelects = new Dictionary<string, TargetSelect>();
		public static Dictionary<string, TargetSelect> TargetSelects
		{
			get
			{
				return g_TargetSelects;
			}
		}

		private static Dictionary<string, TargetType> g_TargetTypes = new Dictionary<string, TargetType>();
		public static Dictionary<string, TargetType> TargetTypes
		{
			get
			{
				return g_TargetTypes;
			}
		}

		private static Dictionary<string, BuffSuperpositionLogic> g_BuffSuperpositionLogics = new Dictionary<string, BuffSuperpositionLogic>();
		public static Dictionary<string, BuffSuperpositionLogic> BuffSuperpositionLogics
		{
			get
			{
				return g_BuffSuperpositionLogics;
			}
		}

		private static Dictionary<string, SkillBulletMovingLogic> g_SkillBulletMovingLogics = new Dictionary<string, SkillBulletMovingLogic>();
		public static Dictionary<string, SkillBulletMovingLogic> SkillBulletMovingLogics
		{
			get
			{
				return g_SkillBulletMovingLogics;
			}
		}

		private static Dictionary<string, SkillBulletCollisionLogic> g_SkillBulletCollisionLogics = new Dictionary<string, SkillBulletCollisionLogic>();
		public static Dictionary<string, SkillBulletCollisionLogic> SkillBulletCollisionLogics
		{
			get
			{
				return g_SkillBulletCollisionLogics;
			}
		}
	}
}
