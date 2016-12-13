namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class DontMove : SkillBulletMovingLogic
	{
		public override void Init(SkillObjMoveController ctrl, SkillBulletMove move, float lifeTime = -1)
		{
			base.Init(ctrl, move, lifeTime);
			move.SpeedValue = 0;
		}
		public override void Update(SkillObjMoveController ctrl, SkillBulletMove move, float deltaTime, float timeMS)
		{
			ctrl.SetLifeTime(ctrl.GetLifeTime() - deltaTime / 1000f);
			return;
		}
	}
}
