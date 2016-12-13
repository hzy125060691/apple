using System;
using System.Collections.Generic;

namespace ZzServer.Battle
{
	[hzyBattleUndetermined]
	public interface SkillObj_Collision
	{
		Vector3_Hzy GetPos();
		IEnumerable<SkillObj_Collision> GetCollisionListNearby();
		SkillCollision GetCollision();
		SkillObj GetSkillObj();
		SkillBulletMove GetSkillMove();
		Action<SkillObj> GetCallBackAction();

	}
	[hzyBattleUndetermined]
	public enum SkillCollisionType
	{
		None,
		SkillObj,
		SkillBullet,
	}
	[hzyBattleUndetermined]
	public class SkillCollision
	{
		protected SkillObj_Collision Self;
		protected SkillBulletMove move;
		protected SkillVector3 CollisionInfo;
		protected SkillCollisionType CollisionType = SkillCollisionType.None;
		protected Dictionary<SkillCollisionType, SkillCollisionType> ActiveCollisions = new Dictionary<SkillCollisionType, SkillCollisionType>();
		protected Dictionary<SkillCollisionType, SkillCollisionType> PassiveCollision = new Dictionary<SkillCollisionType, SkillCollisionType>();
		public SkillCollision(SkillObj_Collision self, SkillBulletMove m, SkillVector3 v)
		{
			Self = self;
			move = m;
			CollisionInfo = v;
		}
		//public SkillCollision(){}
		public SkillCollisionType GetCollisionType()
		{
			return CollisionType;
		}
		public virtual bool IsActiveCollision(SkillCollision targetCollision)
		{
			if(this.CollisionType == SkillCollisionType.None || targetCollision.CollisionType == SkillCollisionType.None)
			{
				return false;
			}
			return this.ActiveCollisions.ContainsKey(targetCollision.CollisionType) &&
				targetCollision.PassiveCollision.ContainsKey(this.CollisionType);
		}
		public virtual bool IsPassiveCollision(SkillCollision sourceCollision)
		{
			return sourceCollision.IsActiveCollision(this);
		}
		public virtual bool ActiveCollisionDetection(SkillCollision target)
		{
			if(IsActiveCollision(target))
			{
				return CollisionDetect(target);
			}
			return false;
		}
		public virtual bool CollisionDetect(SkillCollision target)
		{
			return false;
		}
		public bool CollisionDetect2DBox(SkillCollision target)
		{
			var deltaP2P = move.Position - target.move.Position;

			if(Math.Abs(deltaP2P.x) <= CollisionInfo.x + target.CollisionInfo.x &&
				Math.Abs(deltaP2P.y) <= CollisionInfo.y + target.CollisionInfo.y )
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// 这是一个在tank并不使用SkillBulletMove功能的情况下进行碰撞判断的方法
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool CollisionDetect2DBox_NoMove(SkillCollision target)
		{
			if(move != null && target.move != null)
			{
				return CollisionDetect2DBox(target);
			}
			var pos = Self.GetPos();
			var tarPos = target.Self.GetPos();

			if (Math.Abs(pos.x - tarPos.x) <= CollisionInfo.x + target.CollisionInfo.x &&
				Math.Abs(pos.z - tarPos.z) <= CollisionInfo.y + target.CollisionInfo.y)
			{
				return true;
			}
			return false;
		}
	}

}
