using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleUndetermined]
	class ActionLimitBuffState : BuffLogicState
	{
		private const int key_MoveLimit_SkillConfig = 0;
		private const int key_AttackLimit_SkillConfig = 1;
		private const int key_UseSkillLimit_SkillConfig = 2;
		public override void InitState(SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig, double fixTime)
		{
			int index = self.GetBuffStateIndex(buffInfo);
			var time = self.GetBuffStateTime(buffConfig, index);
			self.SetBuffStateTime(buffInfo, time + fixTime);
		}
		public override LogicStateTickRet Tick(SkillObj self, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			LogicStateTickRet ret = base.Tick(self, buffInfo, buffConfig);
			if (ret == LogicStateTickRet.TimeFinish)
			{
				return ret;
			}
			self.SetBuffStateTime(buffInfo, self.GetBuffStateTime(buffInfo) - self.GetDeltaTime());
			if (self.GetBuffStateTime(buffInfo) <= 0)
			{
				ret = LogicStateTickRet.TimeFinish;
				self.LogInfo("ChargeState:buffObj[{0}] buff:[{1}] ShowEffectState Finish".F(self.GetID(), self.GetBuffID(buffInfo)));
			}
			return ret;
		}
		public override bool IsActionLimited(SkillObj self, ActionLimitType limit, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			int iLimit = -1;
			switch (limit)
			{
				case ActionLimitType.Attack:
					iLimit = self.GetBuffStateIntParam(buffConfig, key_AttackLimit_SkillConfig, self.GetBuffStateIndex(buffInfo));
					break;
				case ActionLimitType.Move:
					iLimit = self.GetBuffStateIntParam(buffConfig, key_MoveLimit_SkillConfig, self.GetBuffStateIndex(buffInfo));
					break;
				case ActionLimitType.UseSkill:
					iLimit = self.GetBuffStateIntParam(buffConfig, key_UseSkillLimit_SkillConfig, self.GetBuffStateIndex(buffInfo));
					break;
				default:
					Debug.Assert(false, "limit == default IsActionLimited:[{0}]".F(limit));
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
