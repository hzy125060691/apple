using System.Collections.Generic;

namespace ZzServer.Battle
{
	/// <summary>
	/// 暂时未用到
	/// </summary>
	[hzyBattleBase]
	public class SkillObjCollision : SkillCollision
	{
		public SkillObjCollision(SkillObj_Collision self, SkillVector3 v) : this(self, null, v)
		{

		}
		public SkillObjCollision(SkillObj_Collision self, SkillBulletMove m, SkillVector3 v) : base(self, m, v)
		{
			CollisionType = SkillCollisionType.SkillObj;

			ActiveCollisions.Add(SkillCollisionType.SkillObj, SkillCollisionType.SkillObj);

			PassiveCollision.Add(SkillCollisionType.SkillObj, SkillCollisionType.SkillObj);
			PassiveCollision.Add(SkillCollisionType.SkillBullet, SkillCollisionType.SkillBullet);
		}

		public override bool CollisionDetect(SkillCollision target)
		{
			return false;
		}
	}

}
