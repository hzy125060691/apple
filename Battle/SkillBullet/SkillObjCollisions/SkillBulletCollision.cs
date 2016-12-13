using System.Collections.Generic;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class SkillBulletCollision : SkillCollision
	{
		public SkillBulletCollision(SkillObj_Collision self, SkillBulletMove m, SkillVector3 v) : base(self, m, v)
		{
			CollisionType = SkillCollisionType.SkillBullet;

			ActiveCollisions.Add(SkillCollisionType.SkillObj, SkillCollisionType.SkillObj);
		}

		public override bool CollisionDetect(SkillCollision target)
		{
			return CollisionDetect2DBox_NoMove(target);
		}
	}

}
