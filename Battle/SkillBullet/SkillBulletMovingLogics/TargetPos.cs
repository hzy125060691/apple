namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class TargetPos : SkillBulletMovingLogic
	{
		public override void Init(SkillObjMoveController ctrl, SkillBulletMove move, float lifeTime = -1)
		{
			base.Init(ctrl, move, lifeTime);
		}
		public override void Update(SkillObjMoveController ctrl, SkillBulletMove move, float deltaTime, float timeMS)
		{
			base.Update(ctrl, move, deltaTime, timeMS);
			var tarPos = ctrl.GetVecParam();

			if (move.SpeedDirection.x == 0 && move.SpeedDirection.y == 0)
			{
				SkillVector3 lastPos = move.GetMoveStruct().position;
				var disX = tarPos.x - lastPos.x;
				var disZ = tarPos.y - lastPos.y;
				if (disX * disX + disZ * disZ <= 0.3 * 0.3)
				{
					move.SpeedValue = 0;
					var action = ctrl.GetMoveCallBackAction();
					if (action != null)
					{
						action(ctrl);
					}
				}
			}
			else
			{
				//                      P(tarPos)
				//                     /|
				//                    / |
				//                   /  |
				//         (lastPos)A————*————————————B(p)
				//                      C(AP在AB上的投影点)
				SkillVector3? lastPos = null;
				foreach (var p in move.GetMovingTrajectory())
				{
					if(lastPos != null)
					{
						SkillVector3 AP = tarPos - lastPos.Value;
						SkillVector3 AB = p - lastPos.Value; ;
						var dot_AB_AP = AB.Dot2D(AP);
						var disAB_Pow_2 = AB.DisPow2();
						double min_P2AB = 0;
						if(dot_AB_AP <= 0)
						{
							//AP
							min_P2AB = AP.DisPow2();
						}
						else if(dot_AB_AP >= disAB_Pow_2)
						{
							//BP
							min_P2AB = (p - tarPos).DisPow2();
						}
						else
						{
							//PC = -AP + AC 
							min_P2AB = (-AP + (dot_AB_AP/ disAB_Pow_2)*AB).DisPow2();
						}
						if (min_P2AB <= 0.3 * 0.3)
						{
							move.SpeedValue = 0;
							var action = ctrl.GetMoveCallBackAction();
							if (action != null)
							{
								action(ctrl);
							}
							break;
						}
					}
					lastPos = p;
				}
			}

			return;
		}
	}
}
