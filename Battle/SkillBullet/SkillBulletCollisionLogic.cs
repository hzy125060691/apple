using System.Collections.Generic;
using System.Linq;

namespace ZzServer.Battle
{
	[hzyBattleUndetermined]
	public enum OnCollideRet
	{
		None,
	}
	[hzyBattleUndetermined]
	public class SkillBulletCollisionLogic
	{
		public virtual void TickCollision(SkillObj_Collision self)
		{
			if(!IsCollisionEnable(self))
			{
				return;
			}
			var targets = self.GetCollisionListNearby();
			var selfCollision = self.GetCollision();
			foreach(var tar in targets.Where(t=>t.GetCollision() != selfCollision))
			{
				if(CheckTarget(self, tar))
				{
					var tarCollision = tar.GetCollision();
					if (selfCollision.CollisionDetect(tarCollision))
					{
						var ret = OnCollide(self, tar);
					}
				}
			}
			return ;
		}
		public virtual bool IsCollisionEnable(SkillObj_Collision self){return true;}
		public virtual bool CheckTarget(SkillObj_Collision self, SkillObj_Collision target){return true;}
		public virtual OnCollideRet OnCollide(SkillObj_Collision self, SkillObj_Collision target)
		{
			//Logger.LogError("命中命中命中命中命中命中命中" + BattleModule.BattleTickCount);
			return OnCollideRet.None;
		}
	}

}
