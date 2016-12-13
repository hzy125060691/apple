using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleUndetermined]
	public interface SummonAndTraceTarget_SkillLogic
	{
		int GetHP();
		SkillObj GetTargetById(int id);
	}
	[hzyBattleUndetermined]
	public class SummonAndTraceTarget : SkillLogic
	{
		private const int key_SummonId_SkillConfig = 0;
		private const int key_SummonCount_SkillConfig = 1;
		private const int key_SummonIdx_SkillInfo = 0;
		private const int key_SummonTraceTarget_SkillInfo = 1;
		public override bool InitSkillInfo(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			bool ret = base.InitSkillInfo(self, skillInfo, skillConfig);

			var targetSelectName = "Range";
			var targetSelect = BattleModule.GetTargetSelect(targetSelectName);
			if (targetSelect == null)
			{
				self.LogInfo("targetSelect == null skillId:[{0}] targetSelectName:[{1}]".F(self.GetSkillID(skillInfo), targetSelectName));
				Debug.Assert(false, "targetSelect == null skillId:[{0}] targetSelectName:[{1}]".F(self.GetSkillID(skillInfo), targetSelectName));
				return false;
			}
			var targetTypeName = "Harm";
			var targetType = BattleModule.GetTargetType(targetTypeName);
			if (targetType == null)
			{
				self.LogInfo("targetSelect == null skillId:[{0}] targetType:[{1}]".F(self.GetSkillID(skillInfo), targetTypeName));
				Debug.Assert(false, "targetSelect == null skillId:[{0}] targetType:[{1}]".F(self.GetSkillID(skillInfo), targetTypeName));
				return false;
			}
			var targets = BattleModule.GetTargets(self, targetSelect, targetType, skillInfo, skillConfig).ToList();
			if(targets == null || targets.Count <= 0)
			{
				return true;
			}
			int count = self.GetSkillIntParam(skillConfig, key_SummonCount_SkillConfig);
			int realCount = 0;
			int index = 0;
			foreach (var tar in targets)
			{
				if(index < count)
				{
					self.SetSkillIntParam(skillInfo, key_SummonTraceTarget_SkillInfo + index++, tar.GetID());
					realCount++;
				}
				else
				{
					break;
				}
			}
			if(index < count)
			{
				var tank = targets.OrderBy(t => t.GetHP()).FirstOrDefault();
				for (int i = index; i < count; i++)
				{
					self.SetSkillIntParam(skillInfo, key_SummonTraceTarget_SkillInfo + i, tank.GetID());
					realCount++;
				}
			}
			self.SetSkillIntParam(skillInfo, key_SummonIdx_SkillInfo, 0);
			ret = true;
			return ret;
		}
		public override bool OnEffect(SkillObj self, SkillObj target, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			int id = self.GetSkillIntParam(skillConfig, key_SummonId_SkillConfig);
			int count = self.GetSkillIntParam(skillConfig, key_SummonCount_SkillConfig);
			int idx = self.GetSkillIntParam(skillInfo, key_SummonIdx_SkillInfo);
			if(idx < count)
			{
				int tarId = self.GetSkillIntParam(skillInfo, key_SummonTraceTarget_SkillInfo + idx);
				var tar = self.GetTargetById(tarId);
				if(tar != null)
				{
					var summonTar = BattleModule.Summon(id, self, tar, skillInfo, skillConfig);
				}
				self.SetSkillIntParam(skillInfo, key_SummonIdx_SkillInfo, idx + 1);
			}
			
			return true;
		}
	}
}
