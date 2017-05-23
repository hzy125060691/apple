using System;
using System.Collections.Generic;

using UnityEngine;

using ZzServer.Battle;

/**
* Created by huangzhenyu on 2017/5/15.
*/
public class BaseMoveController
{
	public MagicMoveRecord CurMagicRecord = new MagicMoveRecord();
	//private Tuple4<Long, Float, Float, Float> LastCommand = null;
	private SkillVector3 NextPos = new SkillVector3();
	private long NextPosTime = -1;

	private List<MagicMoveRecord> HistoryRecords = new List<MagicMoveRecord>();

	private List<MagicMoveRecord> FutureExeMoves = new List<MagicMoveRecord>();
	private List<MagicMoveRecord> CurNeedExeMoves = new List<MagicMoveRecord>();

	private List<SkillVector3> BacktrackingPoints = new List<SkillVector3>();
	private List<SkillVector3> TrackingPoints = new List<SkillVector3>();

	private List<MagicMoveRecord> recordsCache = new List<MagicMoveRecord>();

	//public List<string> Infos = new List<string>();

	public SkillVector3 GetCalcResult()
	{
		return NextPos;
	}
	public BaseMoveController()
	{
		Clear();
	}

	private float MinX = -1;
	private float MaxX = -1;
	private float MinZ = -1;
	private float MaxZ = -1;

	//public static  org.slf4j.Logger logger = null;
	public void SetInitPos(float x, float z, long NowTime_Ms_Long, float dirX, float dirZ, float speed, float minX, float maxX, float minZ, float maxZ)
	{
		//只有这里可以设置速度和方向为0
		Clear();

		MinX = minX;
		MaxX = maxX;
		MinZ = minZ;
		MaxZ = maxZ;

		CurMagicRecord.SetInitPos(x, z, NowTime_Ms_Long, dirX, dirZ, speed, NowTime_Ms_Long, MinX, MaxX, MinZ, MaxZ);
	}

	public void Clear()
	{
		NextPos = NextPos * 0;
		NextPosTime = -1;
		CurMagicRecord.Clear();
		foreach (MagicMoveRecord tmp in HistoryRecords)
		{
			tmp.Clear();
			recordsCache.Add(tmp);
		}
		HistoryRecords.Clear();

		FutureExeMoves.Clear();
		CurNeedExeMoves.Clear();
		BacktrackingPoints.Clear();
		TrackingPoints.Clear();
	}

	public void TestAddMoveCtrlCommand(long time, float dirX, float dirZ, float speed/*, long NowTime_Ms_Long*/, int index)
	{
		//检查是否是合法参数
		//如果这个时间点比服务器时间更早
		//if(time <= CurMagicRecord.GetTime())
		//{
		// throw new Exception("这个时间点比服务器时间更早:" + time + "@" + NowTime_Ms_Long );
		//}
		//先找到该时间点前一个MagicMoveRecord是哪个，可能是当前的MagicMoveRecord,也可能是缓存队列里某一个，所以遍历一下
		//上边已经排除了一个不可能的情况，现在看其他情况
		{
			MagicMoveRecord cmdLastRecord = null;
			if(HistoryRecords.Count > 0)
			{
				MagicMoveRecord tmp = HistoryRecords[HistoryRecords.Count - 1];
				if (tmp.GetTime() <= time && tmp.GetCMDTime() <= time)
				{
					cmdLastRecord = tmp;
				}
			}
			// 			foreach (var tmp in HistoryRecords)
			// 			{
			// 
			// 				//Log("HistoryRecords:" + tmp.ToString());
			// 				if (tmp.GetTime() <= time && tmp.GetCMDTime() <= time)
			// 				{
			// 					cmdLastRecord = tmp;
			// 				}
			// 				else
			// 				{
			// 					//这是个优先权队列，所以一旦发现不满足了就都不用检查了
			// 					break;
			// 				}
			// 			}
			if (CurMagicRecord.GetTime() <= time && CurMagicRecord.GetCMDTime() <= time)
			{
				cmdLastRecord = CurMagicRecord;
				//Log("CurMagicRecord:" + CurMagicRecord.ToString());
			}
			if (FutureExeMoves.Count > 0)
			{
				foreach (var tmp in FutureExeMoves)
				{
					//Log("FutureExeMoves:" + tmp.ToString());
					if (tmp.GetTime() <= time && tmp.GetCMDTime() <= time)
					{
						cmdLastRecord = tmp;
					}
					else
					{
						//这是个优先权队列，所以一旦发现不满足了就都不用检查了
						break;
					}
				}
			}

			Log("LastMoveCmd:" + cmdLastRecord.ToString());
			//根据上一个关键节点计算这个关键节点
			//强调一下，目前，也就是2017/05/17,单条命令只能是改速度或者是改方向，而且改变成的速度方向不可能是0，0只允许在初始值中
			if (speed == 0 || (dirX == 0 && dirZ == 0))
			{
				Log("speed == 0 || (dirX == 0 && dirZ == 0)");
				return;
			}
			SkillVector3 curDir = cmdLastRecord.GetDir();
			float curSpeed = cmdLastRecord.GetSpeed();
			if (curDir.x == dirX && curDir.z == dirZ && curSpeed == speed)
			{
				Log("curDir.x == dirX && curDir.z == dirZ && curSpeed == speed");
				return;
			}
			MagicMoveRecord newRecord = null;
			//速度改变的情况
			if (curSpeed != speed)
			{
				SkillVector3 beginPos = new SkillVector3();
				cmdLastRecord.GetNextPos_2D(time, ref beginPos);
				newRecord = GetNewRecord();
				newRecord.SetInitPos(beginPos.x, beginPos.z, time, dirX, dirZ, speed, time, MinX, MaxX, MinZ, MaxZ);
				Log(index + " Speed Changed : " + curSpeed + "---->>>----" + speed + " BeginPos:" + beginPos.ToString() + " NowPos:" + NextPos.ToString());
			}
			else
			{
				//方向改变的情况,由于我们的版本现在，也就是2017/05/17只允许整格子的0处改变方向，所以时间会略微不一样
				SkillVector3 beginPos = new SkillVector3();
				long nextTime = cmdLastRecord.GetNextMagicPosAndTime_2D(time, ref beginPos);
				newRecord = GetNewRecord();
				newRecord.SetInitPos(beginPos.x, beginPos.z, nextTime, dirX, dirZ, speed, time, MinX, MaxX, MinZ, MaxZ);
				Log(index + " Dir Changed : " + "(" + curDir.x + "," + curDir.z + ")" + "---->>>----" + "(" + dirX + "," + dirZ + ")" + " BeginPos:" + beginPos.ToString() + " NowPos:" + NextPos.ToString());
			}
			if (newRecord.GetTime() > cmdLastRecord.GetTime())
			{

				bool bAdd = true;
				foreach (var tmp in FutureExeMoves)
				{
					if (tmp.GetTime() == newRecord.GetTime())
					{
						bAdd = false;
					}
				}
				if (CurMagicRecord.GetTime() == newRecord.GetTime())
				{
					bAdd = false;
				}
				if (bAdd)
				{
					AddFutureExeMoves(newRecord);
				}
				else
				{
					Log("abandon 222 " + newRecord.ToString());
				}
			}
			else
			{
				Log("abandon " + newRecord.ToString());
			}

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
	public bool IsBK()
	{
		return BacktrackingPoints.Count > 0;
	}
	public bool IsTracking()
	{
		return TrackingPoints.Count > 0;
	}
	public List<SkillVector3> GetBKPs()
	{
		return BacktrackingPoints;
	}
	public List<SkillVector3> GetTPs()
	{
		return TrackingPoints;
	}
	public bool CalcNextPos(long NowTime_Ms_Long)
	{
		BacktrackingPoints.Clear();
		TrackingPoints.Clear();
		//检查当前的命令序列，是否有需要执行的，有几个需要执行
		FindNeedExeMove(NowTime_Ms_Long);

		//由于有可能有回退的点，所以这里需要计算一下
		CalcPoints();
		bool bBacktracking = BacktrackingPoints.Count > 0;

		CurMagicRecord.GetNextPos_2D(NowTime_Ms_Long, ref NextPos);
		NextPosTime = NowTime_Ms_Long;
		TrackingPoints.Add(NextPos);
		return bBacktracking;
	}

	private void FindNeedExeMove(long NowTime_Ms_Long)
	{
		while (FutureExeMoves.Count > 0)
		{
			MagicMoveRecord tmp = FutureExeMoves[0];
			if (tmp != null && tmp.GetTime() < NowTime_Ms_Long)
			{
				FutureExeMoves.RemoveAt(0);
				CurNeedExeMoves.Add(tmp);
			}
			else
			{
				break;
			}
		}
	}
	private void CalcPoints()
	{
		if (CurNeedExeMoves.Count > 0)
		{
			MagicMoveRecord tmp = CurNeedExeMoves[0];
			if (tmp.GetTime() < NextPosTime)
			{
				BacktrackingPoints.Add(tmp.GetPos());
				BacktrackingPoints.Add(NextPos);
			}
		}
		if (BacktrackingPoints.Count <= 0)
		{
			TrackingPoints.Add(NextPos);
		}
		while (CurNeedExeMoves.Count > 0)
		{
			MagicMoveRecord tmp = CurNeedExeMoves[0];
			CurNeedExeMoves.RemoveAt(0);

			TrackingPoints.Add(tmp.GetPos());

			HistoryRecords.Add(CurMagicRecord);
			CurMagicRecord = tmp;

		}
		return;
	}
	public void Log(string str)
	{
		Debug.LogError(str);
	}

	private void AddFutureExeMoves(MagicMoveRecord add)
	{
		FutureExeMoves.Add(add);
		FutureExeMoves.Sort(moveRecordComparator);
		Log(add.ToString());
	}

	private static MoveRecordComparator moveRecordComparator = new MoveRecordComparator();
	public class MoveRecordComparator : IComparer<MagicMoveRecord>
	{
		public int Compare(MagicMoveRecord m1, MagicMoveRecord m2)
		{
			return (int)(m1.GetTime() - m2.GetTime());
		}
	}


	public void ForceFreshMMR(List<ZzSocketShare.Protocol.PlayerMagicMoveRecord> pmmrs)
	{
		Clear();

		if(pmmrs.Count > 0)
		{
			var pmmr = pmmrs[0];
			pmmrs.RemoveAt(0);
			CurMagicRecord.SetInitPos(pmmr.PosX, pmmr.PosZ, pmmr.TickTimeMsLong, pmmr.DirX, pmmr.DirZ, pmmr.Speed, pmmr.CMDTime, MinX, MaxX, MinZ, MaxZ);
		}

		foreach(var pmmr in pmmrs)
		{
			var mmr = GetNewRecord();
			mmr.SetInitPos(pmmr.PosX, pmmr.PosZ, pmmr.TickTimeMsLong, pmmr.DirX, pmmr.DirZ, pmmr.Speed, pmmr.CMDTime, MinX, MaxX, MinZ, MaxZ);
			FutureExeMoves.Add(mmr);
		}
	}

}
