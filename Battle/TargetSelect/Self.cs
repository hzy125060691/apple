using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class Self : TargetSelect
	{
		public override IEnumerable<SkillObj> GetTargets(SkillObj skillObj, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			List<SkillObj> tarList = new List<SkillObj>();
			tarList.Add(skillObj);
			return tarList;
		}
		public override IEnumerable<SkillObj> GetTargets(SkillObj skillObj, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			List<SkillObj> tarList = new List<SkillObj>();
			tarList.Add(skillObj);
			return tarList;
		}
	}
}
