using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 0612

namespace ZzServer.Battle
{
	[hzyBattleUndetermined]
	public class KillSelfDamageOtherBuff : BuffLogic
	{
		private const int Key_Int_DamageTarPer_BuffConfig = 0;
		private const int Key_Int_DamageTarPerByMyHP_BuffConfig = 1;
		private const int Key_Int_SummonId_BuffConfig = 2;
		private const int Key_Double_AddTime_BuffConfig = 0;
		private const int Key_Int_DamageByMyHP_BuffInfo = 0;
		private const int Key_Int_Bombed_BuffInfo = 1;

		private const int Key_Int_Bombed_Value = 9527;
		public override void Tick(SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			Dictionary<int, int> keyValues = new Dictionary<int, int>();
			foreach(var buff in self.GetBuffIntParams(buffConfig).Skip(Key_Int_SummonId_BuffConfig+1))
			{
				keyValues.Add(buff, buff);
			}
			bool bAddTime = false;
			foreach (var buff in self.GetBuffList(true))
			{
				int buffId = self.GetBuffID(buff);
				if(keyValues.ContainsKey(buffId))
				{
					bAddTime = true;
					break;
				}
			}
			if (bAddTime)
			{
				self.SetBuffTime(buffInfo, self.GetBuffTime(buffInfo) + self.GetDeltaTime());
				self.SetBuffStateTime(buffInfo, self.GetBuffStateTime(buffInfo) + self.GetDeltaTime());
				self.NotifyBuffInfo(buffInfo, BattleInfoNotifyType.Time_Buff, BattleNotifyTime.TickEnd);
			}
			base.Tick(self, buffInfo, buffConfig);

		}
		public override void OnDamageTarget(SkillObj self, SkillObj target, Damage damage, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			if(damage.value > 0 && self != target)
			{
				double addTime = self.GetBuffDoubleParam(buffConfig, Key_Double_AddTime_BuffConfig);
				self.SetBuffTime(buffInfo, self.GetBuffTime(buffInfo) + addTime);
				self.SetBuffStateTime(buffInfo, self.GetBuffStateTime(buffInfo) + addTime);
				self.NotifyBuffInfo(buffInfo, BattleInfoNotifyType.Time_Buff, BattleNotifyTime.TickEnd);
			}
		}
		public override void OnDie(SkillObj self, SkillObj attacker, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			return;
			if (self.GetBuffIntParam(buffInfo, Key_Int_Bombed_BuffInfo) != Key_Int_Bombed_Value)
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
				foreach (var tar in targets.Where(t => !t.IsDead()))
				{
					OnEffect(self, tar, buffInfo, buffConfig);
				}
			}
		}
		public override void OnEffect(SkillObj self, SkillObj tarObj, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			int damageValue = 999999999;
			int damageByMyHP = self.GetBuffIntParam(buffInfo, Key_Int_DamageByMyHP_BuffInfo);
			int damageTarPerByMyHP = self.GetBuffIntParam(buffConfig, Key_Int_DamageTarPerByMyHP_BuffConfig); 
			if (self == tarObj)
			{
				if (damageByMyHP <= 0)
				{
					damageByMyHP = self.GetHP()* damageTarPerByMyHP /10000;
					self.SetBuffIntParam(buffInfo, Key_Int_DamageByMyHP_BuffInfo, damageByMyHP);
				}
				if (damageValue > 0)
				{
					Damage damage = BattleModule.CreateDamage(damageValue, bNeedCalc : false);
					BattleModule.DamageTarget(tarObj, self, damage);
				}

				int summonId = self.GetBuffIntParam(buffConfig, Key_Int_SummonId_BuffConfig);
				if(summonId > 0)
				{
					var summonTar = BattleModule.Summon(summonId, self, self, null, null);
				}
			}
			else
			{
				if (damageByMyHP <= 0)
				{
					damageByMyHP = self.GetHP() * damageTarPerByMyHP / 10000;
					self.SetBuffIntParam(buffInfo, Key_Int_DamageByMyHP_BuffInfo, damageByMyHP);
					if(damageByMyHP < 0 )
					{
						damageByMyHP = 0;
					}
				}
				damageValue = self.GetBuffIntParam(buffConfig, Key_Int_DamageTarPer_BuffConfig) * tarObj.GetMaxHP()/10000 + damageByMyHP;
				if (damageValue > 0)
				{
					Damage damage = BattleModule.CreateDamage(damageValue, bNeedCalc: false);
					BattleModule.DamageTarget(tarObj, self, damage);
				}
			}
			self.SetBuffIntParam(buffInfo, Key_Int_Bombed_BuffInfo, Key_Int_Bombed_Value);
		}
	}
}

#pragma warning restore 0612
