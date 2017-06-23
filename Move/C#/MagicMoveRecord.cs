using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using ZzServer.Battle;
/**
* Created by huangzhenyu on 2017/5/15.
*/
public enum SpeedChangeType
{
	None = 0,
	SkillUp = 1,//技能加速
	SkillDown = 2,//技能减速
	ItemUp = 3,//加速物品
	ItemDown = 4,//加速减速
}

public class MagicMoveRecord
{
	private long TickTime_Ms_Long = -1;
	private long CMDTime = -1;
	private SpeedChangeType SpeedType = SpeedChangeType.None;
	private int CMDIdx = -1;

	private SkillVector3 Pos = new SkillVector3(-1, -1, -1);

	private SkillVector3 Dir = new SkillVector3(0, 0, 0);


	private float Speed = -1;   //单位：格子/毫秒,          建议是一个当除数非常好的数

	private float MinX = -1;
	private float MaxX = -1;
	private float MinZ = -1;
	private float MaxZ = -1;

	public static readonly float OutdateTime = 1000 * 1000;//毫秒
	public static readonly float CenterPoint = 0f; //每个格子可以拐弯的中心点，是一个[0,1)的值
	public static readonly float f05MinusCenterPoint = 0.5f - CenterPoint;
	public static readonly float EdgeWidth = 1f;

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
		SpeedType = SpeedChangeType.None;
		CMDIdx = -1;
	}
	public void SetInitPos(float x, float z, long NowTime_Ms_Long, float dirX, float dirZ, float speed, long cmdTime, float minX, float maxX, float minZ, float maxZ, SpeedChangeType speedType, int cmdIdx)
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

		MinX = minX + CenterPoint + EdgeWidth;
		MaxX = maxX + CenterPoint - 1 - EdgeWidth;
		MinZ = minZ + CenterPoint + EdgeWidth;
		MaxZ = maxZ + CenterPoint - 1 - EdgeWidth;

		SpeedType = speedType;
		CMDIdx = cmdIdx;
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
			nextPos.x = Pos.x;
			nextPos.z = Pos.z;
			return nowTime;
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
		return "Index:" + CMDIdx + " Time:" + TickTime_Ms_Long + " Pos:" + Pos.ToString() + " Dir:" + Dir.ToString() + " Speed:" + Speed + " CMDTime:" + CMDTime + " Type:" + SpeedType;
	}

	private int UnifyRound(float f)
	{
		return (int)Mathf.Floor(f + 0.5f);
	}

	public SpeedChangeType GetSpeedType()
	{
		return SpeedType;
	}

	public int GetCMDIndex()
	{
		return CMDIdx;
	}
}
public class MMRManager
{
	public enum FindCMDType
	{
		None,
		History,
		Now,
		Future,
	}
	private MagicMoveRecord curMagicRecord = null;
	private MagicMoveRecord CurMagicRecord
	{
		get
		{
			return curMagicRecord;
		}
		set
		{
			curMagicRecord = value;
			//curIndex = Records.IndexOf(curMagicRecord);
		}
	}
	public MMRManager()
	{
		curMagicRecord = new MagicMoveRecord();
		Records.Add(curMagicRecord);
	}
	//private int curIndex = -1;

	private List<MagicMoveRecord> Records = new List<MagicMoveRecord>();

	private List<MagicMoveRecord> recordsCache = new List<MagicMoveRecord>();

	private static MoveRecordComparator moveRecordComparator = new MoveRecordComparator();
	public class MoveRecordComparator : IComparer<MagicMoveRecord>
	{
		public int Compare(MagicMoveRecord m1, MagicMoveRecord m2)
		{
			int ret = (int)(m1.GetTime() - m2.GetTime());
			return ret == 0? m1.GetCMDIndex() - m2.GetCMDIndex():ret;
		}
	}

	private float MinX = -1;
	private float MaxX = -1;
	private float MinZ = -1;
	private float MaxZ = -1;

	public void Clear()
	{
		CurMagicRecord.Clear();
		foreach (MagicMoveRecord tmp in Records.Where(r=>!r.Equals(CurMagicRecord)))
		{
			tmp.Clear();
			recordsCache.Add(tmp);
		}
		Records.RemoveAll((m)=>!m.Equals(CurMagicRecord));
		//curIndex = -1;
	}

	public void SetInitPos(float x, float z, long NowTime_Ms_Long, float dirX, float dirZ, float speed, long cmdTime, float minX, float maxX, float minZ, float maxZ, SpeedChangeType speedType, int cmdIdx)
	{
		//只有这里可以设置速度和方向为0
		Clear();
		MinX = minX;
		MaxX = maxX;
		MinZ = minZ;
		MaxZ = maxZ;
		CurMagicRecord.SetInitPos(x, z, NowTime_Ms_Long, dirX, dirZ, speed, cmdTime, minX, maxX, minZ, maxZ, speedType, cmdIdx);
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="add"></param>
	/// <returns>真正意义上的前一个mmr</returns>
	private MagicMoveRecord AddRecord(MagicMoveRecord add)
	{
		//需要判断一下是否可以加入，因为有的时候会有两个连续的转向命令，但是在同一个拐点只能接受第1个命令,后来的要抛弃，加速减速也可能会有这个问题，但是如果是提前设置的减速点和拐点重合，那么可以add成功
		List<MagicMoveRecord> equalList = new List<MagicMoveRecord>();
		if (add.GetSpeedType() == SpeedChangeType.None)
		{
			foreach (var r in Records.Reverse<MagicMoveRecord>())
			{
				if (r.GetTime() == add.GetTime())
				{
					equalList.Add(r);
				}
				else if (r.GetTime() < add.GetTime())
				{
					break;
				}
			}
			foreach (var e in equalList)
			{
				if (e.GetSpeedType() == add.GetSpeedType())
				{
					Log("mutli SpeedChangeType.None :" + add.ToString());
					return null;
				}
			}
		}
		

		{
			Records.Add(add);
			Records.Sort(moveRecordComparator);
			var idx = Records.IndexOf(add) - 1;
			if(idx < 0)
			{
				Log("if(idx < 0):"+idx);
				return null;
			}
// 			MagicMoveRecord l = null;
// 			foreach (var r in Records)
// 			{
// 				if (l != null && l.GetTime() == r.GetTime())
// 				{
// 					bool a = false;
// 					while(a)
// 					{
// 
// 					}
// 				}
// 				l = r;
// 			}
			return Records[idx];
		}
	}
	private MagicMoveRecord GetNewRecord()
	{
		MagicMoveRecord temp = null;
		if (recordsCache.Count > 0)
		{
			temp = recordsCache[0];
			recordsCache.RemoveAt(0);
			temp.Clear();
		}

		if (temp == null)
		{
			temp = new MagicMoveRecord();
		}
		return temp;
	}
	private void RemoveRecord(MagicMoveRecord remove)
	{
		Records.Remove(remove);
		remove.Clear();
		recordsCache.Add(remove);
	}
	public void FindCurExeMove(long NowTime_Ms_Long)
	{
		var tmp = FindPreviousMMR(NowTime_Ms_Long);
		SetCurMMR(tmp);
	}
	private void SetCurMMR(MagicMoveRecord mmr)
	{
		if(mmr != CurMagicRecord)
		{
			Log("MMR changed:" + CurMagicRecord.GetCMDIndex() + "->" + mmr.GetCMDIndex());
			CurMagicRecord = mmr;
		}
	}
	public void GetNextPos_2D(long NowTime_Ms_Long, ref SkillVector3 pos)
	{
		CurMagicRecord.GetNextPos_2D(NowTime_Ms_Long, ref pos);
	}

	private MagicMoveRecord CalcNextMMR(MagicMoveRecord last, float dirX, float dirZ, SpeedChangeType speedType, long time, int index, MagicMoveRecord newRecord = null)
	{
		//MagicMoveRecord newRecord = null;
		SkillVector3 curDir = last.GetDir();
		float curSpeed = last.GetSpeed();
		if (newRecord == null)
		{
			newRecord = GetNewRecord();
		}
		if (speedType != SpeedChangeType.None)
		{
			SkillVector3 beginPos = new SkillVector3();
			last.GetNextPos_2D(time, ref beginPos);
			//newRecord = GetNewRecord();
			newRecord.SetInitPos(beginPos.x, beginPos.z, time, curDir.x, curDir.z, CalcSpeed(speedType, curSpeed), time, MinX, MaxX, MinZ, MaxZ, speedType, index);

			//FixMMRs(newRecord);
			Log(index + " Speed Changed : " + curSpeed + "---->>>----" + newRecord.GetSpeed() + " :" + newRecord);
			//Log(index + " Speed Changed : " + curSpeed + "---->>>----" + newRecord.GetSpeed() + " BeginPos:" + beginPos.ToString() + /*" NowPos:" + NextPos.ToString() +*/ " Time:" + newRecord.GetTime());
		}
		else
		{
			SkillVector3 beginPos = new SkillVector3();
			if (last.GetTime() == newRecord.GetTime())
			{
				return null;
			}
			long nextTime = last.GetNextMagicPosAndTime_2D(time, ref beginPos);
			//newRecord = GetNewRecord();
			newRecord.SetInitPos(beginPos.x, beginPos.z, nextTime, dirX, dirZ, last.GetSpeed(), time, MinX, MaxX, MinZ, MaxZ, speedType, index);
			Log(index + " Dir Changed : " + "(" + curDir.x + "," + curDir.z + ")" + "---->>>----" + "(" + dirX + "," + dirZ + ")" + " :" + newRecord);
			//Log(index + " Dir Changed : " + "(" + curDir.x + "," + curDir.z + ")" + "---->>>----" + "(" + dirX + "," + dirZ + ")" + " BeginPos:" + beginPos.ToString() + /*" NowPos:" + NextPos.ToString() +*/ " Time:" + newRecord.GetTime());
		}
		return newRecord;
	}
	private void FixMMRs(MagicMoveRecord first)
	{
		Log("FixMMRs Enter:");
		int firstIdx = Records.IndexOf(first);

		if(firstIdx < 0)
		{
			while(true)
			{
				Log("if(firstIdx < 0)");
			}
			return;
		}
		MagicMoveRecord previous = Records[firstIdx];
		Log("Fix Begin:");
		for (int i = firstIdx + 1; i < Records.Count; i++)
		{
			MagicMoveRecord tmp = Records[i];
			CalcNextMMR(previous, tmp.GetDir().x, tmp.GetDir().z, tmp.GetSpeedType(), tmp.GetCMDTime(), tmp.GetCMDIndex(), tmp);
			previous = tmp;
		}
		Log("Fix End:");
		Log("FixMMRs Leave:");
	}
	float CalcSpeed(SpeedChangeType spdType, float nowSpd)
	{
		switch (spdType)
		{
			case SpeedChangeType.SkillUp:
				return nowSpd * 2f;
			case SpeedChangeType.SkillDown:
				return nowSpd / 2f;
			case SpeedChangeType.ItemUp:
				return nowSpd * 8f;
			case SpeedChangeType.ItemDown:
				return nowSpd / 8f;
			case SpeedChangeType.None:
				return nowSpd;
			default:
				Log("error spdType:" + spdType);
				return 0;
		}
	}

	private MagicMoveRecord FindSpecialMMR(int mmrIdx)
	{
		foreach (var r in Records.Reverse<MagicMoveRecord>())
		{
			if(r.GetCMDIndex() == mmrIdx)
			{
				return r;
			}
		}
		return null;
	}

	private MagicMoveRecord FindPreviousMMR(long time)
	{
		MagicMoveRecord tmp = null;
		//List<MagicMoveRecord> equalsList = new List<MagicMoveRecord>();
		foreach (var r in Records.Reverse<MagicMoveRecord>())
		{
			//if(r.GetTime() == time)
			//{
			//	Log("有两个相同:" + time);
			//	equalsList.Add(r);
			//}
			//else 
			if (r.GetTime() <= time)
			{
				tmp = r;
				break;
			}
		}
		//if(equalsList.Count > 0)
		//{
		//	tmp = equalsList.Max();
		//	Log("有多个个相同，最大的是:" + tmp.ToString());
		//	equalsList.ForEach((e)=> Log("每个是:" + e.ToString()));
		//}
		if(tmp == null)
		{
			tmp = Records.Last();
		}
		return tmp;
	}
	public bool AddMoveCtrlCommand(long time, float dirX, float dirZ, SpeedChangeType speedType, int index)
	{
		Log("Enter AddMoveCtrlCommand:" +  time);
		if (dirX == 0 && dirZ == 0 && speedType == SpeedChangeType.None)
		{
			Log("dirX == 0 && dirZ == 0 && speedType == SpeedChangeType.None");
			Log("Leave AddMoveCtrlCommand:" + time);
			return false;
		}

		var previous = FindPreviousMMR(time);
		Log("previous:" + previous.ToString());

		SkillVector3 curDir = previous.GetDir();
		if (curDir.x == dirX && curDir.z == dirZ && speedType == SpeedChangeType.None)
		{
			Log("curDir.x == dirX && curDir.z == dirZ && speedType == SpeedChangeType.None");
			Log("Leave AddMoveCtrlCommand:" + time);
			return false;
		}

		MagicMoveRecord newRecord = CalcNextMMR(previous, dirX, dirZ, speedType, time, index);

		var realPrevious = AddRecord(newRecord);
		if(realPrevious == null)
		{
			Log(" AddRecord(newRecord) failed:");
			Log("Leave AddMoveCtrlCommand:" + time);
			return false;
		}
		Log("RealPrevious:" + realPrevious.ToString());

		var beforeTime = newRecord.GetTime();
		FixMMRs(realPrevious);

		if (beforeTime != newRecord.GetTime())
		{
			Log("Lesdfsdfave sfaf:" + newRecord);
		}
		Log("Leave AddMoveCtrlCommand:" + time);
		return true;
	}
	public void CancelMoveCtrlCommand(long time, int index, int deleteIndex)
	{
		var specialMMR = FindSpecialMMR(deleteIndex);
		if(specialMMR == null)
		{
			Log("deleteIndex not find : " + deleteIndex);
			return;
		}
		if(time >= specialMMR.GetTime())
		{
			Log("time >= specialMMR.GetTime() : " + time + "->" + specialMMR.GetTime());
			return;
		}

		RemoveRecord(specialMMR);
		var ret = AddMoveCtrlCommand(time, 0,0, SpeedChangeType.SkillDown, index);
		if(!ret)
		{
			while(true)
			{
				Log("var ret = AddMoveCtrlCommand(time, 0,0, SpeedChangeType.SkillDown, index);");
			}
			return;
		}
	}
	public SkillVector3 GetCurDir()
	{
		return CurMagicRecord.GetDir();
	}
	public float GetCurSpeed()
	{
		return CurMagicRecord.GetSpeed();
	}
	public string GetCurMMRString()
	{
		return CurMagicRecord.ToString();
	}
	public void ForceFreshMMR(List<ZzSocketShare.Protocol.PlayerMagicMoveRecord> pmmrs, long time)
	{
		Clear();
		foreach (var pmmr in pmmrs)
		{
			var mmr = GetNewRecord();
			mmr.SetInitPos(pmmr.PosX, pmmr.PosZ, pmmr.TickTimeMsLong, pmmr.DirX, pmmr.DirZ, pmmr.Speed, pmmr.CMDTime, MinX, MaxX, MinZ, MaxZ, (SpeedChangeType)Enum.ToObject(typeof(SpeedChangeType),pmmr.SpeedType), pmmr.CMDIdx);
			Records.Add(mmr);
		}
		RemoveRecord(CurMagicRecord);
		FindCurExeMove(time);
	}
	public void Log(string str)
	{
		Debug.LogError(str);
	}

}
