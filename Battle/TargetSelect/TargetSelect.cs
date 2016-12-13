using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class TargetSelect
	{
		public virtual void Init(SkillObj skillObj, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			return;
		}
		public virtual IEnumerable<SkillObj> GetTargets(SkillObj skillObj, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			return null;
		}
		public virtual void Init(SkillObj skillObj, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			return;
		}
		public virtual IEnumerable<SkillObj> GetTargets(SkillObj skillObj, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			return null;
		}
	}
}
