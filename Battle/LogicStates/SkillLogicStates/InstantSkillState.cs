using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	class InstantSkillState : SkillLogicState
	{
		private const int key = 0;
		private const int NotEffect = 0;
		private const int Effected = 1;
		public override void InitState(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig, double fixTime)
		{
			self.SetSkillStateIntParam(skillInfo, key, NotEffect);
		}
		public override LogicStateTickRet Tick(SkillObj self, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			LogicStateTickRet ret = base.Tick(self, skillInfo, skillConfig);
			if (ret == LogicStateTickRet.TimeFinish)
			{
				return ret;
			}
			if(self.GetSkillStateIntParam(skillInfo, key) == NotEffect)
			{
				self.SetSkillStateIntParam(skillInfo, key, Effected);
				return LogicStateTickRet.OnEffect;
			}
			else
			{
				return LogicStateTickRet.NextState;
			}
			return ret;
		}
	}
}
