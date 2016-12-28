using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class Range : TargetSelect
	{
		private const int key_Range = 0;
		public override IEnumerable<SkillObj> GetTargets(SkillObj skillObj, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			double range = skillObj.GetTargetSelectDoubleParam(skillConfig, key_Range);
			return GetTargets(skillObj, range);
		}
		public override IEnumerable<SkillObj> GetTargets(SkillObj skillObj, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			double range = skillObj.GetTargetSelectDoubleParam(buffConfig, key_Range);
			return GetTargets(skillObj, range);
		}
		private IEnumerable<SkillObj> GetTargets(SkillObj skillObj, double range)
		{
			Vector3_Hzy tarVec;
			Vector3_Hzy srcVec = skillObj.GetPos();
			var tList = skillObj.GetTarListNearby().Where(t => !t.IsDead());
			List<SkillObj> tarList = new List<SkillObj>();
			double srcDirRadian = skillObj.GetDirRadian();
			foreach (var tar in tList)
			{
				tarVec = tar.GetPos();
				double distance = BattleHelper.Distance_2D(tarVec, srcVec);
				if(distance <= range)
				{
					tarList.Add(tar);
				}
			}
			return tarList;
		}
	}
}
