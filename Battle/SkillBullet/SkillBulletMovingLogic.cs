namespace ZzServer.Battle
{
	[hzyBattleBase]
	public interface SkillObjMoveController
	{
		int GetTarID();
		SkillObj GetTar();
		Vector3_Hzy GetPos();
		float GetMovingFloatParam(int index);
		void SetMovingFloatParam(int index, float value);
		float GetMovingDistance();
		void SetMovingDistance(float dis);
		float GetMovingMaxDistance();
		void SetMovingMaxDistance(float dis);
		float GetLifeTime();
		void SetLifeTime(float time);
		SkillVector3 GetVecParam();
		System.Action<SkillObjMoveController> GetMoveCallBackAction();
		//SkillBulletCollisionLogic GetCollision();
	}
	[hzyBattleBase]
	public class SkillBulletMovingLogic
	{
		//public virtual bool CheckSummon()
		//{
		//	return true;
		//}
		public virtual void Init(SkillObjMoveController ctrl, SkillBulletMove move, float lifeTime = -1)
		{
			ctrl.SetLifeTime(lifeTime);
		}
		public virtual void Update(SkillObjMoveController ctrl, SkillBulletMove move, float deltaTime, float timeMS)
		{
			float dis = move.UpdateMove2D(deltaTime, timeMS);
			ctrl.SetMovingDistance(ctrl.GetMovingDistance() + dis);
			ctrl.SetLifeTime(ctrl.GetLifeTime() - deltaTime / 1000f);
		}
	}

}
