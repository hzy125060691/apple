using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class Help : TargetType
	{
		public override bool IsTarget(SkillObj srcObj, SkillObj tarObj)
		{
			var srcCamp = srcObj.GetCamp();
			var tarCamp = tarObj.GetCamp();
			if (IsFriend(srcCamp, tarCamp))
			{
				return true;
			}
			return false;
		}
	}
}
