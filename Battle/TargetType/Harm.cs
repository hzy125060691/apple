using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class Harm : TargetType
	{
		public override bool IsTarget(SkillObj srcObj, SkillObj tarObj)
		{
			var srcCamp = srcObj.GetCamp();
			var tarCamp = tarObj.GetCamp();
			if (IsEnemy(srcCamp, tarCamp))
			{
				return true;
			}
			return false;
		}
	}
}
