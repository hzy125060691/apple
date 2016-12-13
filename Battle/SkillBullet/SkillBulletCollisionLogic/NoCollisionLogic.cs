using System.Collections.Generic;
using System.Linq;

namespace ZzServer.Battle
{
	[hzyBattleUndetermined]
	public class NoCollisionLogic : SkillBulletCollisionLogic
	{
		public override bool IsCollisionEnable(SkillObj_Collision self)
		{
			return false;
		}
	}

}
