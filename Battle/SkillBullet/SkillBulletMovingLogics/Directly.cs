namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class Directly : SkillBulletMovingLogic
	{
		public override void Init(SkillObjMoveController ctrl, SkillBulletMove move, float lifeTime = -1)
		{
			base.Init(ctrl, move, lifeTime);
		}
		public override void Update(SkillObjMoveController ctrl, SkillBulletMove move, float deltaTime, float timeMS)
		{
			base.Update(ctrl, move, deltaTime, timeMS);
			return;
		}
	}
}
