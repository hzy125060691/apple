using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public static class BattleHelper
	{
		public static double Distance_2D(Vector3_Hzy srcVec, Vector3_Hzy tarVec)
		{
			double x = srcVec.x - tarVec.x;
			double z = srcVec.z - tarVec.z;
			return Math.Sqrt(x * x + z * z);
		}
		public static bool IsInRect_2D(Vector3_Hzy srcVec, double srcDirRadian, Vector3_Hzy tarVec, double width, double height, SkillObj logObj)
		{
			//                                                                                   0度
			//			|←   width   →|                                                          ↑Y
			//			A--------------B  ——————                                                 |
			//			 |            |     ↑                                                    |
			//			 |            |     |                                                    |
			//			 |            |                                                          |
			//			 |            |    Height                       -270度 ——————————————————+———————————————→ X  90度
			//			 |     dir    |                                                          |
			//			 |      ↑     |     |                                                    |
			//			 |      |     |     ↓                                                    |
			//			D-------*------C  ——————                                                180度
			//			        ↑
			//			     srcVec

			//double dirRadian = skillObj.GetDirRadian();
			//Vector2 pos = skillObj.GetPos();
			double sin = Math.Sin(srcDirRadian);
			double cos = Math.Cos(srcDirRadian);
			Vector3_Hzy A, B, C, D;
			D.x = srcVec.x - cos * width / 2;
			D.z = srcVec.z + sin * width / 2;
			D.y = 0;

			C.x = srcVec.x + cos * width / 2;
			C.z = srcVec.z - sin * width / 2;
			C.y = 0;

			A.x = D.x + sin * height;
			A.z = D.z + cos * height;
			A.y = 0;

			B.x = C.x + sin * height;
			B.z = C.z + cos * height;
			B.y = 0;

			//M = tarObj.GetPos();

			string dis = Math.Sqrt((srcVec.x - tarVec.x) * (srcVec.x - tarVec.x) + (srcVec.z - tarVec.z) * (srcVec.z - tarVec.z)).ToString("F2");
			logObj.LogInfo("A([{0},{1}]),B([{2},{3}]),C([{4},{5}]),D([{6},{7}]),M([{8},{9}]),dis({10})".F(A.x.ToString("F2"), A.z.ToString("F2"), B.x.ToString("F2"), B.z.ToString("F2"), C.x.ToString("F2"), C.z.ToString("F2"), D.x.ToString("F2"), D.z.ToString("F2"), tarVec.x.ToString("F2"), tarVec.z.ToString("F2"), dis));
			return isContain_2D(A, B, C, D, tarVec);
		}
		internal static bool isContain_2D(Vector3_Hzy mp1, Vector3_Hzy mp2, Vector3_Hzy mp3, Vector3_Hzy mp4, Vector3_Hzy mp)
		{
			if (Multiply(mp, mp1, mp2) * Multiply(mp, mp4, mp3) <= 0

				&& Multiply(mp, mp4, mp1) * Multiply(mp, mp3, mp2) <= 0)
				return true;

			return false;
		}
		/// <summary>
		/// 计算叉乘 |P0P1| × |P0P2|
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <param name="p0"></param>
		/// <returns></returns>
		internal static double Multiply(Vector3_Hzy p1, Vector3_Hzy p2, Vector3_Hzy p0)
		{
			return ((p1.x - p0.x) * (p2.z - p0.z) - (p2.x - p0.x) * (p1.z - p0.z));
		}

		/// <summary>
		/// 由a点指向b点的射线方向
		/// 返回与Z轴角度，在正负派之间
		/// </summary>
		/// <param name="b"></param>
		/// <param name="a"></param>
		/// <returns>弧度</returns>
		public static double RadianA2B_2D(Vector3_Hzy b, Vector3_Hzy a)
		{
			double dis = Vector2Dis_2D(b, a);
			double x = b.x - a.x;
			double ra = Math.Asin(x / dis);

			Vector3_Hzy temp = new Vector3_Hzy() { x = b.x - a.x, z = b.z - a.z, y = 0 };
			if (temp.x > 0 && temp.z > 0) { }
			else if (temp.x < 0 && temp.z > 0) { }
			else if (temp.x > 0 && temp.z < 0)
			{
				ra = Math.PI - ra;
			}
			else if (temp.x < 0 && temp.z < 0)
			{
				ra = -Math.PI - ra;
			}
			return ra;
		}
		/// <summary>
		/// 求两点间距离
		/// </summary>
		/// <param name="a">a点</param>
		/// <param name="b">b点</param>
		/// <returns></returns>
		public static double Vector2Dis_2D(Vector3_Hzy a, Vector3_Hzy b)
		{
			return Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z));
		}

		/// <summary>
		/// 把弧度限制在正负派之内
		/// </summary>
		/// <param name="radian">弧度</param>
		/// <returns></returns>
		public static double Radian2PI(double radian)
		{
			while (true)
			{
				if (radian <= -Math.PI)
				{
					radian += Math.PI * 2;
				}
				else if (radian > Math.PI)
				{
					radian -= Math.PI * 2;
				}
				else
				{
					break;
				}
			}
			return radian;
		}

		public static double RadianA2B_Sym(double bb, double aa, double scale = 1)
		{
			double ret = 0;
			double ang = bb - aa;
			double abs = Math.Abs(ang);
			if (ang > 0 && abs < Math.PI)
			{
				ret = 1;
			}
			else if (ang > 0 && abs >= Math.PI)
			{
				ret = -1;
			}
			else if (ang < 0 && abs < Math.PI)
			{
				ret = -1;
			}
			else if (ang < 0 && abs >= Math.PI)
			{
				ret = 1;
			}
			return ret * scale;
		}
	}



	/// <summary>
	/// 辅助小坐标
	/// </summary>
	public struct Vector3_Hzy
	{
		public double x;
		public double y;
		public double z;
	}
}
