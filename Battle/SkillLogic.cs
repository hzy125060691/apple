// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;

namespace ZzServer.Battle
{

	//下边这个需要修改成对应的技能信息
	// 	using SkillInfo = ZzSocketShare.Protocol.SkillUnitInfo;
	// 	using SkillConfig = ZzConfig.Table.SkillConfigTable;
	// 	using BuffInfo = BUff;
	// 	using BuffConfig = ZzConfig.Table.SkillConfigTable;
	// 	using Damage = System.Int32;
	[hzyBattleBase]
	public class SkillLogic
	{
		//public static Dictionary<int, SkillLogic> g_SkillLogics = new Dictionary<int, SkillLogic>();
		public virtual void OnSendBuff(SkillObj self, SkillObj tarObj, BuffInfo_New buff, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { }
		public virtual void OnClearBuff(SkillObj self, SkillObj tarObj, BuffInfo_New buff, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { }
		public virtual void OnAttachBuff(SkillObj self, SkillObj srcObj, BuffInfo_New buff, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { }
		public virtual void OnDetachBuff(SkillObj self, SkillObj srcObj, BuffInfo_New buff, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { }
		public virtual void OnBeHurt(SkillObj self, SkillObj attacker, Damage damage, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { }
		public virtual void OnBeHeal(SkillObj self, SkillObj attacker, Damage heal, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			//OnBeHurt(self, attacker, -heal, skillInfo, skillConfig);
		}
		public virtual void OnDamageTarget(SkillObj self, SkillObj target, Damage damage, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { }
		public virtual void OnHealTarget(SkillObj self, SkillObj target, Damage heal, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			//OnDamageTarget(self, target, -heal, skillInfo, skillConfig);
		}
		public virtual void OnDie(SkillObj self, SkillObj attacker, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { }
		public virtual void OnUseSkill(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { }
		//public virtual double HitRateRefix(SkillObj self, int skillId, double rate, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { return rate; }
		//public virtual double CriticalRateRefix(SkillObj self, int skillId, double rate, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { return rate; }
		public virtual Damage DamageTargetFix(SkillObj self, SkillObj target, Damage damage, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { return damage; }
		public virtual Damage BeHurtDamageFix(SkillObj self, SkillObj source, Damage damage, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { return damage; }
		public virtual Damage HealFix(SkillObj self, SkillObj target, Damage heal, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { return heal; }
		public virtual Damage BeHealFix(SkillObj self, SkillObj source, Damage heal, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { return heal; }
		public virtual bool CanUse(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { return false; }
		public virtual void BeginCD(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			string key = self.GetSkillCDKey(skillConfig);
			if (key == null || key.Equals(""))
			{
				return ;
			}
			double time = self.GetSkillCD(skillConfig);
			if (time <= 0)
			{
				return ;
			}
			self.SetCD(key, time);
			self.NotifySkillInfo(skillInfo, BattleInfoNotifyType.CD_Skill, BattleNotifyTime.TickEnd);
			self.LogInfo("skill:[{0}] CD Begin [{1}]".F(self.GetSkillID(skillInfo), self.GetCD(key)));
			return ;
		}
		public virtual bool CheckCD(SkillObj self, SkillConfig_New skillConfig)
		{
			string key = self.GetSkillCDKey(skillConfig);
			if(key == null || key.Equals(""))
			{
				return true;
			}
			double time = self.GetCD(key);
			if(time <= 0)
			{
				return true;
			}
			return false;
		}
		//public virtual void MarkModifiedAttrDirtyFlag(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { }
		public virtual bool OnEffect(SkillObj self, SkillObj target, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
// 			var activeNum = self.GetActiveNum(skillInfo);
// 			if(activeNum <= 0)
// 			{
// 				return false;
// 			}
// 			self.SetActiveNum(skillInfo, activeNum - 1);
			return false;
		}

		public virtual bool InitSkillInfo(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			var guid = BattleModule.BattleID;
			//var time = self.GetSkillTime(skillConfig);
			var skillId = self.GetSkillID(skillConfig);
			//var nowTime = self.GetNowTime();
			//var activeNum = self.GetActiveNum(skillConfig);
			self.SetSkillId(skillInfo, skillId);
			//self.SetSkillTime(skillInfo, time);
			self.SetSkillGuid(skillInfo, guid);
			//self.SetSkillStateTime(skillInfo, nowTime);
			//self.SetSkillSrcObj(skillInfo, self);
			//self.SetActiveNum(skillInfo, activeNum);
			self.SetSkillStateIndex(skillInfo, 0);
			var startLogicStateName = self.GetLogicState(skillConfig, 0);
			var startLogicState = BattleModule.GetSkillLogicState(startLogicStateName);
			if (startLogicState == null)
			{
				self.LogInfo("startLogicState == null skillId:[{0}] startLogicStateName:[{1}]".F(skillId, startLogicStateName));
				return false;
			}
			//init
			startLogicState.InitSkill(self, skillInfo, skillConfig, 0);
			startLogicState.InitState(self, skillInfo, skillConfig, 0);

			var targetSelectName = self.GetTargetSelect(skillConfig);
			var targetSelect = BattleModule.GetTargetSelect(targetSelectName);
			if(targetSelect == null)
			{
				self.LogInfo("targetSelect == null skillId:[{0}] targetSelectName:[{1}]".F(skillId, targetSelectName));
				return false;
			}
			targetSelect.Init(self, skillInfo, skillConfig);
			return true;
		}
		
		public virtual void Tick(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			if (!CommonCheck(self, skillInfo, skillConfig))
			{
				return ;
			}
			var logicState = BattleModule.GetSkillLogicState(skillInfo, self);
			if(logicState == null)
			{
				return;
			}
			var ret = logicState.Tick(self, skillInfo, skillConfig);
			switch(ret)
			{
				case LogicStateTickRet.TimeFinish:
					SkillOnEnd(self, skillInfo, skillConfig);
					break;
				case LogicStateTickRet.NextState:
					int index = self.GetSkillStateIndex(skillInfo);
					string nextStateName = self.GetLogicState(skillConfig, ++index);
					if (nextStateName == null || nextStateName.Equals(""))
					{
						SkillOnEnd(self, skillInfo, skillConfig);
						return;
					}
					var nextLogicState = BattleModule.GetSkillLogicState(nextStateName);
					if (nextLogicState == null)
					{
						self.LogInfo("startLogicState {0} not found".F(nextStateName));
						return;
					}
					double fixTime = logicState.OnStateChanged(nextStateName, self, skillInfo, skillConfig);
					self.SetSkillStateIndex(skillInfo, index);
					nextLogicState.InitState(self, skillInfo, skillConfig, fixTime);
					self.NotifySkillInfo(skillInfo, BattleInfoNotifyType.ChangeState_Skill, BattleNotifyTime.TickEnd);
					break;
				case LogicStateTickRet.OnEffect:
					//var srcObj = self.GetSkillSrcObj(skillInfo);
					{
						var targetSelectName = self.GetTargetSelect(skillConfig);
						var targetSelect = BattleModule.GetTargetSelect(targetSelectName);
						if (targetSelect == null)
						{
							self.LogInfo("targetSelect == null skillId:[{0}] targetSelectName:[{1}]".F(self.GetSkillID(skillInfo), targetSelectName));
							return;
						}
						var targetTypeName = self.GetTargetType(skillConfig);
						var targetType = BattleModule.GetTargetType(targetTypeName);
						if (targetType == null)
						{
							self.LogInfo("targetSelect == null skillId:[{0}] targetType:[{1}]".F(self.GetSkillID(skillInfo), targetTypeName));
							return;
						}
						var targets = BattleModule.GetTargets(self, targetSelect, targetType, skillInfo, skillConfig);
						foreach (var tar in targets)
						{
							OnEffect(self, tar, skillInfo, skillConfig);
						}
					}
					
					break;
			}
		}
		public virtual bool CommonCheck(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			if (!self.CommonCheck() || self.IsDead())
			{
				return false;
			}
			if (skillInfo != null && IsEnd(self, skillInfo, skillConfig))
			{
				return false;
			}
			return true;
		}
		public virtual bool IsEnd(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			return self.GetSkillTime(skillInfo) <= 0;
		}
		public virtual void SkillOnEnd(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			if (self.GetSkillTime(skillInfo) > 0)
			{
				self.SetSkillTime(skillInfo, 0);
				self.NotifySkillInfo(skillInfo, BattleInfoNotifyType.Time_Skill, BattleNotifyTime.TickEnd);
			}
		}

		public virtual bool IsActionLimited(SkillObj self, ActionLimitType limit, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			if (!CommonCheck(self, skillInfo, skillConfig))
			{
				return false;
			}
			var logicState = BattleModule.GetSkillLogicState(skillInfo, self);
			if (logicState == null)
			{
				return false;
			}
			var ret = logicState.IsActionLimited(self, limit, skillInfo, skillConfig);
			return ret;
		}

		public SkillInfo_New Begin(SkillObj self, SkillConfig_New skillConfig)
		{
			if (!CommonCheck(self, null, skillConfig))
			{
				return null;
			}
			SkillInfo_New skillInfo = new SkillInfo_New() { };
			bool ret = InitSkillInfo(self, skillInfo, skillConfig);
			if (ret)
			{
				self.NotifySkillInfo(skillInfo, BattleInfoNotifyType.All_Skill, BattleNotifyTime.TickEnd);
				return skillInfo;
			}
			return null;
		}
		public virtual double OnDataFix(SkillObj self, PropertyType pType, double pValue, SkillInfo_New skillInfo, SkillConfig_New skillConfig) { return pValue; }

		public virtual double DataFix(SkillObj self, PropertyType pType, double pValue, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			if (!CommonCheck(self, skillInfo, skillConfig))
			{
				return pValue;
			}
			var logicState = BattleModule.GetSkillLogicState(skillInfo, self);
			if (logicState == null)
			{
				return pValue;
			}
			if (logicState.NeedDataFix(self, pType, pValue, skillInfo, skillConfig))
			{
				pValue = OnDataFix(self, pType, pValue, skillInfo, skillConfig);
			}
			return pValue;
		}
	}
}
