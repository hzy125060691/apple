using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleUndetermined]
	public class SelfAndHarm : TargetType
	{
		public override bool IsTarget(SkillObj srcObj, SkillObj tarObj)
		{
			var srcCamp = srcObj.GetCamp();
			var tarCamp = tarObj.GetCamp();
			if (IsEnemy(srcCamp, tarCamp) || srcObj == tarObj)
			{
				return true;
			}
			return false;
		}
	}
}
