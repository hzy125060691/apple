using System;

using UnityEngine;

using ZzServer.Battle;
/**
* Created by huangzhenyu on 2017/5/15.
*/
public class MagicMoveRecord
{
	private long TickTime_Ms_Long = -1;
	private long CMDTime = -1;

	private SkillVector3 Pos = new SkillVector3(-1, -1, -1);

	private SkillVector3 Dir = new SkillVector3(0, 0, 0);


	private float Speed = -1;   //单位：格子/毫秒,          建议是一个当除数非常好的数

	private float MinX = -1;
	private float MaxX = -1;
	private float MinZ = -1;
	private float MaxZ = -1;

	private static float OutdateTime = 1000 * 1000;//毫秒
	public static float CenterPoint = 0f; //每个格子可以拐弯的中心点，是一个[0,1)的值
	private static float f05MinusCenterPoint = 0.5f - CenterPoint;

	public float GetSpeed()
	{
		return Speed;
	}
	public SkillVector3 GetDir()
	{
		return  Dir;
	}
	public SkillVector3 GetPos()
	{
		return Pos;
	}
	public long GetTime()
	{
		return TickTime_Ms_Long;
	}
	public long GetCMDTime()
	{
		return CMDTime;
	}


	public MagicMoveRecord()
	{
		Clear();
	}
	public void Clear()
	{
		TickTime_Ms_Long = -1;
		CMDTime = -1;

		Pos.x = -1;
		Pos.y = -1;
		Pos.z = -1;

		Dir.x = 0;
		Dir.y = 0;
		Dir.z = 0;

		Speed = -1;
	}
	public void SetInitPos(float x, float z, long NowTime_Ms_Long, float dirX, float dirZ, float speed, long cmdTime, float minX, float maxX, float minZ, float maxZ)
	{
		Pos.x = x;
		Pos.y = 0;
		Pos.z = z;
		Dir.x = dirX == 0 ? 0 : Mathf.Abs(dirX) / dirX;
		Dir.y = 0;
		Dir.z = dirZ == 0 ? 0 : Mathf.Abs(dirZ) / dirZ;
		Speed = speed;
		TickTime_Ms_Long = NowTime_Ms_Long;
		CMDTime = cmdTime;

		MinX = minX + CenterPoint;
		MaxX = maxX + CenterPoint - 1;
		MinZ = minZ + CenterPoint;
		MaxZ = maxZ + CenterPoint - 1;

	}


	public bool IsOutdate(long otherTime)
	{
	return (otherTime - GetTime()) >= OutdateTime;
	}
	public bool IsOutdate(MagicMoveRecord other)
	{
	return IsOutdate(other.GetTime());
	}

	public long Compare(long otherTime)
	{
	return otherTime - GetTime();
	}
	public long Compare(MagicMoveRecord other)
	{
	return Compare(other.GetTime());
	}
	public bool IsDirOrSpdChanged(float dirX, float dirZ, float spd)
	{
		return Dir.x != dirX || Dir.z != dirZ || Speed != spd;
	}
	public static float Clamp(float value, float min, float max)
	{
		return Mathf.Max(min, Mathf.Min(max, value));
	}
	public void GetNextPos_2D(long NowTime_Ms_Long, ref SkillVector3 pos) 
	{
		long diffTime = Compare(NowTime_Ms_Long);
		if (diffTime > 0)
		{
			double r = Mathf.Sqrt(Mathf.Pow(Dir.x, 2) + Mathf.Pow(Dir.z, 2));
			double sin = r == 0 ? 0 : Dir.z / r;
			double cos = r == 0 ? 0 : Dir.x / r;
			pos.x = Clamp((float)(Pos.x + cos * Speed * diffTime), MinX, MaxX);
			pos.z = Clamp((float)(Pos.z + sin * Speed * diffTime), MinZ, MaxZ);
		}
		else if (diffTime <= 0)
		{
			pos.x = Pos.x;
			pos.z = Pos.z;
			//throw new Exception("else if(diffTime < 0)");
		}
		// 		else
		// 		{
		// 			throw new Exception("需要辅助参数来确定顺序，否则会出现客户端服务器不一致 3333333333333333333");
		// 		}
	}
	public long GetNextMagicPosAndTime_2D(long nowTime, ref SkillVector3 nextPos)
	{
		if(Speed <= 0 || (Dir.x == 0 && Dir.z == 0) || Dir.x * Dir.z != 0)
		{
		return  -1;
		}
		long time = -1;
		SkillVector3 nowPos = new SkillVector3();
		GetNextPos_2D(nowTime, ref nowPos);
		if (Dir.x != 0)
		{
			nextPos.x = Clamp(UnifyRound(nowPos.x + f05MinusCenterPoint) + 0.5f * Dir.x - f05MinusCenterPoint, MinX, MaxX);
			nextPos.z = nowPos.z;
			float dis = Mathf.Abs(nextPos.x - nowPos.x);
			time = nowTime + UnifyRound(dis/Speed);
		}
		else if(Dir.z != 0)
		{
			nextPos.z = Clamp(UnifyRound(nowPos.z + f05MinusCenterPoint) + 0.5f * Dir.z - f05MinusCenterPoint, MinX, MaxX);
			nextPos.x = nowPos.x;
			float dis = Mathf.Abs(nextPos.z - nowPos.z);
			time = nowTime + UnifyRound(dis/Speed);
		}
		else
		{
			throw new Exception("");
		}
		return time;

	}
	override public string ToString()
	{
		return "Time:" + TickTime_Ms_Long + " Pos:" + Pos.ToString() + " Dir:" + Dir.ToString() + " Speed:" + Speed + " CMDTime:" + CMDTime;
	}

	private int UnifyRound(float f)
	{
		return (int)Mathf.Floor(f + 0.5f);
	}
}
