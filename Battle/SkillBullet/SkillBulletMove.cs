

using System;
using System.Collections.Generic;

namespace ZzServer.Battle
{

	[hzyBattleBase]
	public interface SkillObj_Move
	{
		bool CellCanMoveByIndex(float x, float y);
		bool CellCanMoveByIndex(SkillVector3 vec);
		SkillVector3 GetCellIndex(float x, float y, float z);
 		SkillVector3 GetCellIndex(SkillVector3 vec);

		SkillVector3 GetCellLocalPosition(float x, float y, float z);
 		SkillVector3 GetCellLocalPosition(SkillVector3 vec);
		SkillVector3 GetCellSpeedFix(SkillVector3 spd, SkillVector3 pos);
		SkillVector3 GetPositionByLocal(SkillVector3 loc, SkillVector3 index);
		float GetCellSize();
		SkillBulletMove GetSkillMove();
		//int GetCellIndexSize();
	}
	[hzyBattleBase]
	public struct SkillVector3
	{
		public float x;
		public float y;
		public float z;
		public SkillVector3(float _x, float _y, float _z)
		{
			x = _x;
			y = _y;
			z = _z;
		}
		public SkillVector3(float f) : this(f,f,f){}

		public static SkillVector3 operator *(SkillVector3 vec1, float multi)
		{
			vec1.x *= multi;
			vec1.y *= multi;
			vec1.z *= multi;
			return vec1;
		}
		public static SkillVector3 operator *(float multi, SkillVector3 vec1)
		{
			vec1.x *= multi;
			vec1.y *= multi;
			vec1.z *= multi;
			return vec1;
		}
		public static SkillVector3 operator +(SkillVector3 vec1, SkillVector3 vec2)
		{
			vec1.x += vec2.x;
			vec1.y += vec2.y;
			vec1.z += vec2.z;
			return vec1;
		}
		public static SkillVector3 operator -(SkillVector3 vec1, SkillVector3 vec2)
		{
			vec1 += -vec2;
			return vec1;
		}
		public static SkillVector3 operator -(SkillVector3 vec1)
		{
			vec1.x = -vec1.x;
			vec1.y = -vec1.y;
			vec1.z = -vec1.z;
			return vec1;
		}
		public static bool operator !=(SkillVector3 vec1, SkillVector3 vec2)
		{
			return !(vec1.Equals(vec2));
		}
		public static bool operator ==(SkillVector3 vec1, SkillVector3 vec2)
		{
			return vec1.Equals(vec2);
		}
		public override int GetHashCode()
		{
			return x.GetHashCode() + y.GetHashCode() + z.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			if (GetType() != obj.GetType())
				return false;
			SkillVector3 vec2 = (SkillVector3)obj;
			return x == vec2.x && y == vec2.y && z == vec2.z;
		}

		public float DisPow2()
		{
			return x * x + y * y;
		}
		public double Distance2D()
		{
			return Math.Sqrt(DisPow2());
		}
		public double Distance(SkillVector3 tar)
		{
			double deltaX = x - tar.x;
			double deltaY = y - tar.y;
			double deltaZ = z - tar.z;
			return Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
		}
		public float Dot2D(SkillVector3 tar)
		{
			return x * tar.x + y * tar.y;
		}
		public double GetRadian_2D()
		{
			double add = 0;
			if (y == 0)
			{
				if (x > 0)
				{
					return Math.PI / 2;
				}
				else if (x < 0)
				{
					return -Math.PI / 2;
				}
				else
				{
					System.Diagnostics.Debug.Assert(false, "SpeedDirection error");
				}
			}
			else if (y < 0)
			{
				add = Math.PI;
			}
			return Math.Atan(x / y) + add;
		}
	}
	[hzyBattleBase]
	public class SkillBulletMove
	{
		public SkillVector3 Direction
		{
			get
			{
				return skillObjMoveStruct.direction;
			}
			set
			{
				if(value != skillObjMoveStruct.direction)
				{
					skillObjMoveStruct.direction = value;
					NeedNotifySelf = true;
				}
			}
		}
		public SkillVector3 SpeedDirection
		{
			get
			{
				return skillObjMoveStruct.speedDirection;
			}
			set
			{
				if (value != skillObjMoveStruct.speedDirection)
				{
					skillObjMoveStruct.speedDirection = value;
					NeedNotifySelf = true;
				}
			}
		}
		public float SpeedValue
		{
			get
			{
				return skillObjMoveStruct.speedValue;
			}
			set
			{
				if(skillObjMoveStruct.speedValue != value)
				{
					skillObjMoveStruct.speedValue = value;
					NeedNotifySelf = true;
				}
			}
		}
		public SkillVector3 Speed
		{
			get
			{
				return skillObjMoveStruct.speedDirection * skillObjMoveStruct.speedValue + Self.GetCellSpeedFix(skillObjMoveStruct.speedDirection * skillObjMoveStruct.speedValue, Position);
			}
		}
		public SkillVector3 Position
		{
			get
			{
				return skillObjMoveStruct.position;
			}
			set
			{
// 				var index = Self.GetCellIndex(value);
// 				if (!Self.CellCanMoveByIndex(index) &&
// 					!Self.CellCanMoveByIndex(index.x - 1, index.y) &&
// 					!Self.CellCanMoveByIndex(index.x, index.y - 1) &&
// 					!Self.CellCanMoveByIndex(index.x - 1, index.y - 1))
// 				{
// 					skillObjMoveStruct.position = value;
// 				}
// 				if (value.y - 2 == skillObjMoveStruct.position.y)
// 				{
// 					skillObjMoveStruct.position = value;
// 				}
// 				index = Self.GetCellIndex(value);
// 				var loc = Self.GetCellLocalPosition(value);
// 				if (!Self.CellCanMoveByIndex(index) &&
// 					loc.x > 0 && loc.y > 0)
// 				{
// 					skillObjMoveStruct.position = value;
// 				}
				skillObjMoveStruct.position = value;
				MovingTrajectory.Add(value);
			}
		}
		public float TimeMS
		{
			get
			{
				return skillObjMoveStruct.TimeMS;
			}
			set
			{
				skillObjMoveStruct.TimeMS = value;
			}
		}
		public struct SkillObjMoveStruct
		{
			public SkillVector3 position;
			public SkillVector3 direction;
			public SkillVector3 speedDirection;
			public float speedValue;
			public float TimeMS;
		} 
		public SkillObj_Move Self;
// 		public SkillVector3 position;
// 		private SkillVector3 direction;
// 		private SkillVector3 speedDirection;
// 		private float speedValue;
		protected SkillObjMoveStruct skillObjMoveStruct;
		protected SkillObjMoveStruct skillObjMoveStructCopy;
		public bool NeedNotifySelf = false;

		// 		protected float movingDis = 0;
		// 		protected float MovingDisMax = -1;
		//		public float LifeTime;
		protected List<SkillVector3> MovingTrajectory = new List<SkillVector3>();

		public SkillBulletMove(SkillObj_Move self){ Self = self; }
		public SkillBulletMove(SkillObj_Move self, float timeMS, SkillVector3 position, SkillVector3 dir, SkillVector3 spdDir, float spdValue/*, float movingDisMax = -1*/) : this(self)
		{
			Position = position;
			Direction = dir;
			SpeedDirection = spdDir;
			SpeedValue = spdValue;
			TimeMS = timeMS;
			//MovingDisMax = movingDisMax;
		}

		public List<SkillVector3> MoveTraceList = new List<SkillVector3>();

		public void PushCopy()
		{
			skillObjMoveStructCopy = skillObjMoveStruct;
		}
		public SkillObjMoveStruct GetMoveStruct()
		{
			return skillObjMoveStruct;
		}
		public SkillObjMoveStruct GetMoveStructCopy()
		{
			return skillObjMoveStructCopy;
		}
		public List<SkillVector3> GetMovingTrajectory()
		{
			return MovingTrajectory;
		}
		public double GetSpeedDirection_Radian()
		{
			double add = 0;
			if (SpeedDirection.y == 0)
			{
				if(SpeedDirection.x > 0)
				{
					return Math.PI/2;
				}
				else if(SpeedDirection.x < 0)
				{
					return -Math.PI/2;
				}
				else
				{
					System.Diagnostics.Debug.Assert(false, "SpeedDirection error");
				}
			}
			else if(SpeedDirection.y < 0)
			{
				add = Math.PI;
			}
			return Math.Atan(SpeedDirection.x / SpeedDirection.y) + add;
		}
		public void ChangeSpeedDirection(SkillVector3 dir)
		{
			SpeedDirection = dir;
		}
		public void ChangeSpeedDirection(float changeRadian)
		{
			var radian = GetSpeedDirection_Radian();
			radian += changeRadian;
			var dir = new SkillVector3((float)Math.Sin(radian), (float)Math.Cos(radian), 0);
			ChangeSpeedDirection(dir);
		}
		public void SetSpeedDirection(float changeRadian)
		{
			var dir = new SkillVector3((float)Math.Sin(changeRadian), (float)Math.Cos(changeRadian), 0);
			ChangeSpeedDirection(dir);
		}
		/// <summary>
		/// 2D移动的update
		/// </summary>
		/// <param name="deltaTime"></param>
		/// <returns>本次update移动的距离</returns>
		public float UpdateMove2D(float deltaTime, float timeMS)
		{
			MovingTrajectory.Clear();
			TimeMS = timeMS;
			//if(movingDis >= MovingDisMax && MovingDisMax > 0)
			//{
			//	return 0;
			//}
			PushCopy();
			float retDis = 0;
			if (Speed.x == 0 && Speed.y == 0)
			{
				//movingDis += retDis;
				return retDis;
			}
			SkillVector3 beginPos = Position;

			SkillVector3 targetPos_NoFix = beginPos + Speed * deltaTime;
			//double deltaDis = targetPos_NoFix.Distance(beginPos);
			float deltaDisX = Math.Abs(targetPos_NoFix.x - beginPos.x);
			float deltaDisY = Math.Abs(targetPos_NoFix.y - beginPos.y);
			SkillVector3? FinalPos = null;

			var beginIndex = Self.GetCellIndex(beginPos);
			var targetIndex = Self.GetCellIndex(targetPos_NoFix);

			{
				float deltaX_NoFix = targetPos_NoFix.x - beginPos.x;
				float deltaY_NoFix = targetPos_NoFix.y - beginPos.y;
				float cellSize = Self.GetCellSize();

				float deltaX_StepByStep = 0;
				float deltaY_StepByStep = 0;
				// 			int girdSize = Self.GetCellIndexSize();
				float x = beginPos.x;
				float xl = beginPos.x;
				float y = beginPos.y;
				float yl = beginPos.y;

				//bool hasNextCell = true;
				while (true)
				{
					SkillVector3 speedTemp = Speed;
					if (speedTemp.x == 0 && speedTemp.y == 0)
					{
						//movingDis += retDis;
						return retDis;
					}
					float k = speedTemp.y;
					if (speedTemp.x != 0)
					{
						k /= speedTemp.x;
					}
					else
					{
						k = 0;
					}
					//SkillVector3 targetPos_Temp = ;
					float signX = speedTemp.x;
					if (signX != 0)
					{
						signX /= Math.Abs(signX);
					}
					float signY = speedTemp.y;
					if (signY != 0)
					{
						signY /= Math.Abs(signY);
					}
					SkillVector3 targetTemp = Position + new SkillVector3(signX * deltaDisX, signY * deltaDisY, 0);
					if (x*y < 0)
					{
						var temp = new SkillVector3(x,y,0);
						if (y < 0)
						{
							temp.y = y;
						}
						if (x < 0)
						{
							temp.x = x;
						}
						FinalPos = temp;
						deltaDisX -= Math.Abs(FinalPos.Value.x - Position.x);
						deltaDisY -= Math.Abs(FinalPos.Value.y - Position.y);
						//deltaDis -= (float)Position.Distance(FinalPos.Value);
						retDis += (float)Position.Distance(FinalPos.Value);
						Position = FinalPos.Value;
						break;
					}
					
					var localPos = Self.GetCellLocalPosition(x, y, 0);
					var localIndex = Self.GetCellIndex(x, y, 0);
					if (localPos.x == 0 && signX < 0)
					{
						localPos.x = 1;
						localIndex.x--;
					}
					if (localPos.y == 0 && signY < 0)
					{
						localPos.y = 1;
						localIndex.y--;
					}
					//y = kx + b;
					float b = localPos.y - k * localPos.x;
					float k_m1 = 0;
					if(k != 0)
					{
						k_m1 = 1 / k;
					}
					SkillVector3 p1 = new SkillVector3(-b*k_m1, 0, 0);						//y = 0
					SkillVector3 p2 = new SkillVector3((cellSize-b)* k_m1, cellSize, 0);    //y = cellSize
					SkillVector3 p3 = new SkillVector3(cellSize, k*cellSize+b, 0);      //x = cellSize
					SkillVector3 p4 = new SkillVector3(0, b, 0);                        //x = 0
					SkillVector3?[] ps = new SkillVector3?[2];
					if (signX > 0)
					{
						ps[0] = p3;
					}
					else if (signX<0)
					{
						ps[0] = p4;
					}
					else
					{
						ps[0] = null;
					}
					if(signY > 0)
					{
						ps[1] = p2;
					}
					else if (signY < 0)
					{
						ps[1] = p1;
					}
					else
					{
						ps[1] = null;
					}
					SkillVector3? pEdge = null;
					foreach (var p in ps)
					{
						if(p == null)
						{
							continue;
						}
						if(p.Value.x >= 0 && p.Value.x <= cellSize && p.Value.y >= 0 && p.Value.y <= cellSize)
						{
							pEdge = p.Value;
							break;
						}
					}

					if(pEdge == null)
					{
						//Logger.Log.Info("ddddddddddddddddddddddd43543565ryrthtrfhgfhgf");
						//movingDis += retDis;
						return retDis;
					}
					
					var pos = Self.GetPositionByLocal(pEdge.Value, localIndex);
					//var checkIndex = Self.GetCellIndex(pos);
// 					if (pEdge.Value.x == 0 && signX < 0)
// 					{
// 						checkIndex.x--;
// 					}
// 					if (pEdge.Value.y== 0 && signY < 0)
// 					{
// 						checkIndex.y--;
// 					}
					float deltaXSign = (pos.x - targetTemp.x) * (x - targetTemp.x);
					float deltaYSign = (pos.y - targetTemp.y) * (y - targetTemp.y);
					if (deltaXSign < 0 || deltaYSign < 0)
					{
						if(Self.CellCanMoveByIndex(localIndex))
						{
							if (deltaXSign < 0 && deltaYSign < 0)
							{
								FinalPos = targetTemp;
							}
							else if (deltaXSign < 0)
							{
								//Logger.Log.Info("errorrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr3");
								FinalPos = new SkillVector3(targetTemp.x, pos.y, 0);
							}
							else if (deltaYSign < 0)
							{
								//Logger.Log.Info("errorrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr4");
								FinalPos = new SkillVector3(pos.x, targetTemp.y, 0);
							}
							else
							{
								//FinalPos = ;
								//Logger.Log.Info("errorrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr1");
							}
						}
						else
						{
							if (deltaXSign < 0 && deltaYSign < 0)
							{//这个很奇怪，先这么放着
								FinalPos = pos;
							}
							else if (deltaXSign < 0)
							{
								FinalPos = new SkillVector3(targetTemp.x, pos.y, 0);
							}
							else if (deltaYSign < 0)
							{
								FinalPos = new SkillVector3(pos.x, targetTemp.y, 0);
							}
							else
							{
								//Logger.Log.Info("errorrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr222222");
								//FinalPos = ;
							}
						}
						deltaDisX -= Math.Abs(FinalPos.Value.x - Position.x);
						deltaDisY -= Math.Abs(FinalPos.Value.y - Position.y);
						//deltaDis -= (float)Position.Distance(FinalPos.Value);
						retDis += (float)Position.Distance(FinalPos.Value);
						Position = FinalPos.Value;
						break;
					}
					

					deltaX_StepByStep += Math.Abs(pos.x-xl);
					deltaY_StepByStep += Math.Abs(pos.y-yl);
					if(deltaX_StepByStep > Math.Abs(deltaX_NoFix) || deltaY_StepByStep > Math.Abs(deltaY_NoFix))
					{
						FinalPos = pos;
						deltaDisX -= Math.Abs(FinalPos.Value.x - Position.x);
						deltaDisY -= Math.Abs(FinalPos.Value.y - Position.y);
						//deltaDis -= (float)Position.Distance(FinalPos.Value);
						retDis += (float)Position.Distance(FinalPos.Value);
						Position = FinalPos.Value;
						break;
					}
					if(pos.x == xx && pos.y == yy)
					{
						//Logger.Log.Info("ddddddddddddddddddddddd43543565ryrthtrfhgfhgf");
					}
					xl = x;
					yl = y;
					x = pos.x;
					y = pos.y;
					MoveTraceList.Add(pos);
					deltaDisX -= Math.Abs(pos.x - Position.x);
					deltaDisY -= Math.Abs(pos.y - Position.y);
					//deltaDis -= (float)Position.Distance(pos); 
					retDis += (float)Position.Distance(pos);
					Position = pos;

				}
				//retDis += (float)Position.Distance(FinalPos.Value);
				//MoveTraceList.Add(FinalPos.Value);
				//Position = FinalPos.Value;
			}
			//else
			{
				//竖着的直线或者一个点
			}
			//movingDis += retDis;
			return retDis;
		}
		static float xx = 59;
		static float yy = 27;
	}
}
