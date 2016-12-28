using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public interface FrontRectTargetSelect
	{
		IEnumerable<SkillObj> GetTarListNearby();
	}
	[hzyBattleBase]
	public class FrontRect : TargetSelect
	{
		private const int key_Width = 0;
		private const int key_Height = 1;
		public override IEnumerable<SkillObj> GetTargets(SkillObj skillObj, SkillInfo_New skillInfo, SkillConfig_New skillConfig)
		{
			double width = skillObj.GetTargetSelectDoubleParam(skillConfig, key_Width);
			double height = skillObj.GetTargetSelectDoubleParam(skillConfig, key_Height);
			return GetTargets(skillObj, width, height);
		}
		public override IEnumerable<SkillObj> GetTargets(SkillObj skillObj, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			double width = skillObj.GetTargetSelectDoubleParam(buffConfig, key_Width);
			double height = skillObj.GetTargetSelectDoubleParam(buffConfig, key_Height);
			return GetTargets(skillObj, width, height);
		}
		private IEnumerable<SkillObj> GetTargets(SkillObj skillObj, double width, double height)
		{
			Vector3_Hzy tarVec;
			Vector3_Hzy srcVec = skillObj.GetPos();
			var tList = skillObj.GetTarListNearby().Where(t => !t.IsDead());
			List<SkillObj> tarList = new List<SkillObj>();
			double srcDirRadian = skillObj.GetDirRadian();
			foreach (var tar in tList)
			{
				tarVec = tar.GetPos();
				if (BattleHelper.IsInRect_2D(srcVec, srcDirRadian, tarVec, width, height, skillObj))
				{
					tarList.Add(tar);
				}
			}
			return tarList;
		}

	}
}
