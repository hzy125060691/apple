using System.Collections.Generic;
using System.Linq;

namespace ZzServer.Battle
{
	[hzyBattleUndetermined]
	public class ActionCollisionLogic : SkillBulletCollisionLogic
	{
		public override OnCollideRet OnCollide(SkillObj_Collision self, SkillObj_Collision target)
		{
			var action = self.GetCallBackAction();
			if(action != null)
			{
				action(target.GetSkillObj());
			}
			return OnCollideRet.None;
		}
	}

}
