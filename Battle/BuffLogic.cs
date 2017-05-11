using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	// 	using SkillInfo = ZzSocketShare.Protocol.SkillUnitInfo;
	// 	using SkillConfig = ZzConfig.Table.SkillConfigTable;
	// 	using BuffInfo = BUff;
	// 	using BuffConfig = ZzConfig.Table.SkillConfigTable;
	// 	using Damage = System.Int32;
	[hzyBattleBase]
	public class BuffLogic
	{
		public virtual void OnSendBuff(SkillObj self, SkillObj tarObj, BuffInfo_New buff, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { }
		public virtual void OnClearBuff(SkillObj self, SkillObj tarObj, BuffInfo_New buff, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { }
		public virtual void OnAttachBuff(SkillObj self, SkillObj srcObj, BuffInfo_New newBuff, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { }
		public virtual void OnDetachBuff(SkillObj self, SkillObj srcObj, BuffInfo_New deleteBuff, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { }
		public virtual void OnAttach(SkillObj self, SkillObj srcObj, BuffInfo_New buff, BuffConfig_New buffConfig) { }
		public virtual void OnDetach(SkillObj self, SkillObj srcObj, BuffInfo_New buff, BuffConfig_New buffConfig) { }
		public virtual void OnBeHurt(SkillObj self, SkillObj attacker, Damage damage, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { }
		public virtual void OnBeHeal(SkillObj self, SkillObj attacker, Damage heal, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			//OnBeHurt(self, attacker, -heal, buffInfo, buffConfig);
		}
		public virtual void OnDamageTarget(SkillObj self, SkillObj target, Damage damage, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { }
		public virtual void OnHealTarget(SkillObj self, SkillObj target, Damage heal, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			//OnDamageTarget(self, target, -heal, buffInfo, buffConfig);
		}
		public virtual void OnDie(SkillObj self, SkillObj attacker, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { }
		public virtual void OnUseSkill(SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { }
		public virtual void OnSummon(int id, SkillObj self, SkillObj summonObj, BuffInfo_New buffInfo, BuffConfig_New buffConfig) {}
		//public virtual double HitRateRefix(SkillObj self, int skillId, double rate, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { return rate; }
		//public virtual double CriticalRateRefix(SkillObj self, int skillId, double rate, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { return rate; }
		public virtual Damage DamageTargetFix(SkillObj self, SkillObj target, Damage damage, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { return damage; }
		public virtual Damage BeHurtDamageFix(SkillObj self, SkillObj source, Damage damage, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { return damage; }
		public virtual Damage HealFix(SkillObj self, SkillObj target, Damage heal, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { return heal; }
		public virtual Damage BeHealFix(SkillObj self, SkillObj source, Damage heal, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { return heal; }
		//public virtual void MarkModifiedAttrDirtyFlag(SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { }
		public virtual void OnEffect(SkillObj self, SkillObj tarObj, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { }
		public virtual bool InitBuffInfo(SkillObj self, SkillObj srcObj, BattleReason reason, BuffInfo_New buffInfo, BuffConfig_New buffConfig, bool RefreshGUID = true)
		{
			var buffId = self.GetBuffID(buffConfig);
			self.SetBuffId(buffInfo, buffId);
			if (RefreshGUID)
			{
				var guid = BattleModule.BattleID;
				self.SetBuffGuid(buffInfo, guid);
			}
			self.SetBuffStateIndex(buffInfo, 0);
			if(srcObj != null)
			{
				if(srcObj.GetParentID() > 0)
				{
					self.SetSrcID(buffInfo, srcObj.GetParentID());
				}
				else
				{
					self.SetSrcID(buffInfo, srcObj.GetID());
				}
			}
			var startLogicStateName = self.GetLogicState(buffConfig, 0);
			var startLogicState = BattleModule.GetBuffLogicState(startLogicStateName);
			if (startLogicState == null)
			{
				self.LogInfo("startLogicState == null BuffId:[{0}] startLogicStateName:[{1}]".F(buffId, startLogicStateName));
				return false;
			}
			//init
			startLogicState.InitBuff(self, buffInfo, buffConfig, 0);
			startLogicState.InitState(self, buffInfo, buffConfig, 0);

			var targetSelectName = self.GetTargetSelect(buffConfig);
			if (targetSelectName != null)
			{
				var targetSelect = BattleModule.GetTargetSelect(targetSelectName);
				if (targetSelect == null)
				{
					self.LogInfo("targetSelect == null buffId:[{0}] targetSelectName:[{1}]".F(buffId, targetSelectName));
					return false;
				}
				targetSelect.Init(self, buffInfo, buffConfig);
			}
			return true;
		}
		public virtual void Tick(SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			if (!CommonCheck(self, buffInfo, buffConfig))
			{
				return;
			}
			var logicState = BattleModule.GetBuffLogicState(buffInfo, self);
			if (logicState == null)
			{
				return;
			}
			var ret = logicState.Tick(self, buffInfo, buffConfig);
			switch (ret)
			{
				case LogicStateTickRet.TimeFinish:
					BuffOnEnd(self, buffInfo, buffConfig);
					break;
				case LogicStateTickRet.NextState:
					int index = self.GetBuffStateIndex(buffInfo);
					string nextStateName = self.GetLogicState(buffConfig, ++index);
					if (nextStateName == null || nextStateName.Equals(""))
					{
						BuffOnEnd(self, buffInfo, buffConfig);
						return;
					}
					var nextLogicState = BattleModule.GetBuffLogicState(nextStateName);
					if (nextLogicState == null)
					{
						self.LogInfo("startLogicState {0} not found".F(nextStateName));
						return;
					}
					double fixTime = logicState.OnStateChanged(nextStateName, self, buffInfo, buffConfig);
					self.SetBuffStateIndex(buffInfo, index);
					nextLogicState.InitState(self, buffInfo, buffConfig, fixTime);
					self.NotifyBuffInfo(buffInfo, BattleInfoNotifyType.ChangeState_Buff, BattleNotifyTime.TickEnd);
					break;
				case LogicStateTickRet.OnEffect:
					//var srcObj = self.GetBuffSrcObj(buffInfo);
					{
						var targetSelectName = self.GetTargetSelect(buffConfig);
						var targetSelect = BattleModule.GetTargetSelect(targetSelectName);
						if (targetSelect == null)
						{
							self.LogInfo("targetSelect == null buffId:[{0}] targetSelectName:[{1}]".F(self.GetBuffID(buffInfo), targetSelectName));
							return;
						}
						var targetTypeName = self.GetTargetType(buffConfig);
						var targetType = BattleModule.GetTargetType(targetTypeName);
						if (targetType == null)
						{
							self.LogInfo("targetSelect == null buffId:[{0}] targetType:[{1}]".F(self.GetBuffID(buffInfo), targetTypeName));
							return;
						}
						var targets = BattleModule.GetTargets(self, targetSelect, targetType, buffInfo, buffConfig);
						foreach (var tar in targets)
						{
							OnEffect(self, tar, buffInfo, buffConfig);
						}
					}
					break;
			}
		}
		public virtual bool CommonCheck(SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			if (!self.CommonCheck() || self.IsDead())
			{
				return false;
			}
			if (IsEnd(self, buffInfo, buffConfig))
			{
				return false;
			}
			return true;
		}
		public virtual bool IsEnd(SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			return self.GetBuffTime(buffInfo) <= 0;
		}
		public virtual void BuffOnEnd(SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			if (self.GetBuffTime(buffInfo) > 0)
			{
				self.SetBuffTime(buffInfo, 0);
			}
			self.NotifyBuffInfo(buffInfo, BattleInfoNotifyType.Time_Buff, BattleNotifyTime.TickEnd);
		}

		public virtual bool IsActionLimited(SkillObj self, ActionLimitType limit, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			if (!CommonCheck(self, buffInfo, buffConfig))
			{
				return false;
			}
			var logicState = BattleModule.GetBuffLogicState(buffInfo, self);
			if (logicState == null)
			{
				return false;
			}
			var ret = logicState.IsActionLimited(self, limit, buffInfo, buffConfig);
			return ret;
		}

		public virtual double OnDataFix(SkillObj self, PropertyType pType, double pValue, BuffInfo_New buffInfo, BuffConfig_New buffConfig) { return pValue; }
		public virtual double DataFix(SkillObj self, PropertyType pType, double pValue, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			if (!CommonCheck(self, buffInfo, buffConfig))
			{
				return pValue;
			}
			var logicState = BattleModule.GetBuffLogicState(buffInfo, self);
			if (logicState == null)
			{
				return pValue;
			}
			if(logicState.NeedDataFix(self, pType, pValue, buffInfo, buffConfig))
			{
				pValue = OnDataFix(self, pType, pValue, buffInfo, buffConfig);
			}
			return pValue;
		}
	}
}
