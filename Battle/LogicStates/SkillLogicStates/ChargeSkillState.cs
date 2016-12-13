using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	/// <summary>
	/// 技能charge状态
	/// </summary>
	[hzyBattleBase]
	public class ChargeSkillState : SkillLogicState
	{
		private const int key_ActionMoveLimit_SkillConfig = 0;
		private const int key_ActionAttackLimit_SkillConfig = 1;
		private const int key_ActionUseSkillLimit_SkillConfig = 2;
		public override void InitState(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig, double fixTime)
		{
			int index = self.GetSkillStateIndex(skillInfo);
			var time = self.GetSkillStateTime(skillConfig, index);
			self.SetSkillStateTime(skillInfo, time + fixTime);
			self.LogInfo("State [{0}] NowTime bEGIN:[{1}]".F(self.GetSkillLogicStateName(skillInfo), self.GetNowTime()));
		}
		public override LogicStateTickRet Tick(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			LogicStateTickRet ret = base.Tick(self, skillInfo, skillConfig);
			if(ret == LogicStateTickRet.TimeFinish)
			{
				return ret;
			}
			self.SetSkillStateTime(skillInfo, self.GetSkillStateTime(skillInfo) - self.GetDeltaTime());
			//self.LogInfo("State [{0}] Time:[{1}]".F(self.GetSkillLogicStateName(skillInfo), self.GetSkillStateTime(skillInfo)));
			if (self.GetSkillStateTime(skillInfo) <= 0)
			{
				self.LogInfo("State [{0}] NowTime fINISH:[{1}]".F(self.GetSkillLogicStateName(skillInfo), self.GetNowTime()));
				ret = LogicStateTickRet.NextState;
				self.LogInfo("ChargeState:skillObj[{0}] skill:[{1}] Charge Finish".F(self.GetID(), self.GetSkillID(skillInfo)));
			}
			return ret;
		}
		public override double OnStateChanged(string tarState, SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			self.LogInfo("State [{0}] finish".F(self.GetSkillLogicStateName(skillInfo)));
			return self.GetSkillStateTime(skillInfo);
		}
		[hzyBattleUndetermined]
		public override bool IsActionLimited(SkillObj self, ActionLimitType limit, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			int iLimit = -1;
			switch (limit)
			{
				case ActionLimitType.Attack:
					iLimit = self.GetSkillStateIntParam(skillConfig, key_ActionAttackLimit_SkillConfig, self.GetSkillStateIndex(skillInfo));
					break;
				case ActionLimitType.Move:
					iLimit = self.GetSkillStateIntParam(skillConfig, key_ActionMoveLimit_SkillConfig, self.GetSkillStateIndex(skillInfo));
					break;
				case ActionLimitType.UseSkill:
					iLimit = self.GetSkillStateIntParam(skillConfig, key_ActionUseSkillLimit_SkillConfig, self.GetSkillStateIndex(skillInfo));
					break;
				default:
					System.Diagnostics.Debug.Assert(false, "limit == default IsActionLimited:[{0}]".F(limit));
					break;
			}
			if (iLimit == 1)
			{
				return true;
			}
			return false;
		}
	}
}
