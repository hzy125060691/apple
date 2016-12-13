namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class TraceTarget : SkillBulletMovingLogic
	{
		private const int Key_TickRadian_MovingFloatParam = 0;

		public override void Init(SkillObjMoveController ctrl, SkillBulletMove move, float lifeTime = -1)
		{
			base.Init(ctrl, move, lifeTime);
		}
		public override void Update(SkillObjMoveController ctrl, SkillBulletMove move, float deltaTime, float timeMS)
		{
			double MoveDir = move.GetSpeedDirection_Radian();
			int tarId = ctrl.GetTarID();
			var target = ctrl.GetTar();
			if (target != null && !target.IsDead())
			{

				MoveDir = BattleHelper.Radian2PI(MoveDir);
				var tarPos = target.GetPos();
				Vector3_Hzy selfPos = ctrl.GetPos();
				double radian = BattleHelper.RadianA2B_2D(tarPos, selfPos);
				radian = BattleHelper.Radian2PI(radian);
				if (radian != MoveDir)
				{
					float tickRadian = ctrl.GetMovingFloatParam(Key_TickRadian_MovingFloatParam);
					double something = ((5 + tickRadian) * System.Math.PI / 180);
					double changeRadian = BattleHelper.RadianA2B_Sym(radian, MoveDir, something);
					tickRadian += deltaTime*0.6f/30;
					ctrl.SetMovingFloatParam(Key_TickRadian_MovingFloatParam, tickRadian);
					MoveDir = BattleHelper.Radian2PI(MoveDir + changeRadian);
					move.SetSpeedDirection((float)MoveDir);
				}
			}
			else
			{
				ctrl.SetLifeTime(-1);
				return;
			}
			base.Update(ctrl, move, deltaTime, timeMS);
		}
	}
}
