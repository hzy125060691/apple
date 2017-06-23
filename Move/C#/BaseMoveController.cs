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

		CurMagicRecord.SetInitPos(x, z, NowTime_Ms_Long, dirX, dirZ, speed, NowTime_Ms_Long, MinX, MaxX, MinZ, MaxZ, SpeedChangeType.None, -1);
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
		NeedFixRecords.Clear();
	}
	public enum FindCMDType
	{
		None,
		History,
		Now,
		Future,
	}
	private List<MagicMoveRecord> NeedFixRecords = new List<MagicMoveRecord>();
	private void FixToFuture()
	{
		FutureExeMoves.AddRange(NeedFixRecords);
		NeedFixRecords.Clear();
	}
	private MagicMoveRecord FixMMRs(MagicMoveRecord first)
	{
		MagicMoveRecord last = first;
		for (int i = 0; i < NeedFixRecords.Count;)
		{
			if (last != null)
			{
				MagicMoveRecord tmp = NeedFixRecords[i];
				var temp2 = CalcNextMMR(last, tmp.GetDir().x, tmp.GetDir().z, tmp.GetSpeedType(), tmp.GetTime(), tmp.GetCMDIndex(), tmp);
				if (temp2 != null)
				{
					i++;
				}
				else
				{
					if (last == first && first.GetCMDIndex() > tmp.GetCMDIndex())
					{
						first = tmp;
					}
					else
					{
						Log("if(last==first && first.GetCMDIndex() > tmp.GetCMDIndex())@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@22");
					}
					NeedFixRecords.RemoveAt(i);
					Log("discard:" + tmp.ToString());
				}
				last = tmp;
			}
		}
		return first;
	}
	private MagicMoveRecord CalcNextMMR(MagicMoveRecord last, float dirX, float dirZ, SpeedChangeType speedType, long time, int index, MagicMoveRecord newRecord = null)
	{
		//MagicMoveRecord newRecord = null;
		SkillVector3 curDir = last.GetDir();
		float curSpeed = last.GetSpeed();
		if(newRecord == null)
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
			Log(index + " Speed Changed : " + curSpeed + "---->>>----" + newRecord.GetSpeed() + " BeginPos:" + beginPos.ToString() + " NowPos:" + NextPos.ToString() + " Time:" + newRecord.GetTime());
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
			Log(index + " Dir Changed : " + "(" + curDir.x + "," + curDir.z + ")" + "---->>>----" + "(" + dirX + "," + dirZ + ")" + " BeginPos:" + beginPos.ToString() + " NowPos:" + NextPos.ToString() + " Time:" + newRecord.GetTime());
		}
		return newRecord;
	}
	public bool TestCancelMoveCtrlCommand(long time, float dirX, float dirZ, SpeedChangeType speedType, int index, int deleteIndex)
	{
		DebugSkillUpDown("Cancel Before ADD");
		bool ret = true;
		NeedFixRecords.Clear();

		//如果要取消的操作已经执行了，那么就不需要取消了
		if (FutureExeMoves.Count > 0)
		{
			int deleteIdx = -1;
			bool bAdd = false;
			MagicMoveRecord tar = null;
			for (int i = FutureExeMoves.Count - 1; i >= 0; i--)
			{
				var tmp = FutureExeMoves[i];
				if (tmp.GetCMDIndex() == deleteIndex && tmp.GetSpeedType() == SpeedChangeType.SkillDown)
				{
					bAdd = true;
					deleteIdx = i;
					tar = tmp;
					break;
				}
			}
			if(deleteIdx >= 0)
			{

				DebugConsole.LogWarning("discard 22222:" + tar.ToString());
				FutureExeMoves.RemoveAt(deleteIdx);
			}


			if(bAdd)
			{
				ret = true;
				TestAddMoveCtrlCommand(time, dirX, dirZ, speedType, index);
			}
			else
			{
				ret = false;
			}

		}
		DebugSkillUpDown("Cancel After ADD");
		return ret;
	}
	public void TestAddMoveCtrlCommand(long time, float dirX, float dirZ, SpeedChangeType speedType, int index)
	{
		DebugSkillUpDown("Before ADD");
		//检查是否是合法参数
		//如果这个时间点比服务器时间更早
		//if(time <= CurMagicRecord.GetTime())
		//{
		// throw new Exception("这个时间点比服务器时间更早:" + time + "@" + NowTime_Ms_Long );
		//}
		//先找到该时间点前一个MagicMoveRecord是哪个，可能是当前的MagicMoveRecord,也可能是缓存队列里某一个，所以遍历一下
		//上边已经排除了一个不可能的情况，现在看其他情况
		{
			NeedFixRecords.Clear();

			MagicMoveRecord cmdLastRecord = null;
			FindCMDType find = FindCMDType.None;
			int findIdx = -1;
			if (HistoryRecords.Count > 0)
			{
				for(int i = HistoryRecords.Count - 1; i >= 0; i--)
				{
					MagicMoveRecord tmp = HistoryRecords[i];
					if (tmp.GetTime() <= time && tmp.GetCMDTime() <= time)
					{
						cmdLastRecord = tmp;
						break;
					}
					else
					{
						findIdx = i;
						find = FindCMDType.History;
					}
				}
			}

			{
				if (find == FindCMDType.None)
				{
					if (CurMagicRecord != null && CurMagicRecord.GetTime() <= time && CurMagicRecord.GetCMDTime() <= time)
					{
						cmdLastRecord = CurMagicRecord;
						//Log("CurMagicRecord:" + CurMagicRecord.ToString());
					}
					else
					{
						find = FindCMDType.Now;
					}
				}
				if (find == FindCMDType.None)
				{
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
								findIdx = FutureExeMoves.IndexOf(tmp);
								find = FindCMDType.Future;
								//这是个优先权队列，所以一旦发现不满足了就都不用检查了
								break;
							}
						}
					}
				}
			}

			Log("LastMoveCmd:" + cmdLastRecord.ToString());
			//根据上一个关键节点计算这个关键节点
			if (dirX == 0 && dirZ == 0 && speedType == SpeedChangeType.None)
			{
				Log("dirX == 0 && dirZ == 0");
				return;
			}
			SkillVector3 curDir = cmdLastRecord.GetDir();
			float curSpeed = cmdLastRecord.GetSpeed();
			if (curDir.x == dirX && curDir.z == dirZ && speedType == SpeedChangeType.None)
			{
				Log("curDir.x == dirX && curDir.z == dirZ && speedType == SpeedChangeType.None");
				return;
			}
			MagicMoveRecord newRecord = CalcNextMMR(cmdLastRecord, dirX, dirZ, speedType, time, index);
			Log(" find ret :" + find);
			//速度改变的情况
			//if (speedType != SpeedChangeType.None)
			{
				switch (find)
				{
					case FindCMDType.Future:
						{
							if(FutureExeMoves.Count > findIdx)
							{
								NeedFixRecords.AddRange(FutureExeMoves.GetRange(findIdx, FutureExeMoves.Count - findIdx));
								FutureExeMoves.RemoveRange(findIdx, FutureExeMoves.Count - findIdx);
							}
						}
						break;
					case FindCMDType.History:
						{
							if (HistoryRecords.Count > findIdx)
							{
								NeedFixRecords.AddRange(HistoryRecords.GetRange(findIdx, HistoryRecords.Count - findIdx));
								HistoryRecords.RemoveRange(findIdx, HistoryRecords.Count - findIdx);
							}

							if (findIdx == 0)
							{
								throw new Exception("findIdx == 0");
							}

							NeedFixRecords.Add(CurMagicRecord);
							CurMagicRecord = HistoryRecords[findIdx - 1];
							HistoryRecords.RemoveAt(findIdx - 1);

							if (FutureExeMoves.Count > 0)
							{
								NeedFixRecords.AddRange(FutureExeMoves);
								FutureExeMoves.Clear();
							}
						}
						break;
					
					case FindCMDType.Now:
						{
							NeedFixRecords.Add(CurMagicRecord);
							if (HistoryRecords.Count == 0)
							{
								throw new Exception("HistoryRecords.Count() == 0");
							}
							CurMagicRecord = HistoryRecords[HistoryRecords.Count - 1];
							HistoryRecords.RemoveAt(HistoryRecords.Count - 1);
							if (FutureExeMoves.Count > 0)
							{
								NeedFixRecords.AddRange(FutureExeMoves);
								FutureExeMoves.Clear();
							}
						}
						break;
				}

				newRecord = FixMMRs(newRecord);
				//Log(index + " Speed Changed : " + curSpeed + "---->>>----" + speed + " BeginPos:" + beginPos.ToString() + " NowPos:" + NextPos.ToString());
			}
			if (newRecord.GetTime() > cmdLastRecord.GetTime() || (newRecord.GetTime() == cmdLastRecord.GetTime() && newRecord.GetSpeedType() != SpeedChangeType.None))
			{

				bool bAdd = true;
				foreach (var tmp in FutureExeMoves)
				{
					if (tmp.GetTime() == newRecord.GetTime() && tmp.GetSpeedType() == SpeedChangeType.None && newRecord.GetSpeedType() == SpeedChangeType.None)
					{
						bAdd = false;
					}
				}
				if (CurMagicRecord != null && CurMagicRecord.GetTime() == newRecord.GetTime() && CurMagicRecord.GetSpeedType() == SpeedChangeType.None && newRecord.GetSpeedType() == SpeedChangeType.None)
				{
					bAdd = false;
				}
				if (bAdd)
				{
					AddFutureExeMoves(newRecord);
					if(newRecord.GetSpeedType() == SpeedChangeType.SkillUp)
					{
						skillupcount++;
					}
					else if(newRecord.GetSpeedType() == SpeedChangeType.SkillDown)
					{
						skilldowncount++;
					}
				}
				else
				{
					Log("abandon 222 " + newRecord.ToString());
				}
			}
			else
			{
				Log("abandon " + newRecord.ToString());
				if(newRecord.GetSpeedType() != SpeedChangeType.None)
				{
					DebugConsole.LogError("SDFEARYTHRTHGFHGDSFSDFSDFSDAF");
				}
			}

		}
		{
			FixToFuture();
		}
		DebugSkillUpDown("After ADD");
	}
	static int skillupcount = 0;
	static int skilldowncount = 0;
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
			CurMagicRecord.SetInitPos(pmmr.PosX, pmmr.PosZ, pmmr.TickTimeMsLong, pmmr.DirX, pmmr.DirZ, pmmr.Speed, pmmr.CMDTime, MinX, MaxX, MinZ, MaxZ, SpeedChangeType.None, -4);
		}

		foreach(var pmmr in pmmrs)
		{
			var mmr = GetNewRecord();
			mmr.SetInitPos(pmmr.PosX, pmmr.PosZ, pmmr.TickTimeMsLong, pmmr.DirX, pmmr.DirZ, pmmr.Speed, pmmr.CMDTime, MinX, MaxX, MinZ, MaxZ, SpeedChangeType.None, -4);
			FutureExeMoves.Add(mmr);
		}
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
				DebugConsole.LogError("error spdType:" + spdType);
				return 0;
		}
	}

	public int DebugSkillUpDown(string kaitou, bool log=true)
	{
		int u = 0;
		int d = 0;
		int u1 = 0;
		int d1 = 0;
		int u2 = 0;
		int d2 = 0;
		int u3 = 0;
		int d3 = 0;
		foreach (var m in NeedFixRecords)
		{
			if (m.GetSpeedType() == SpeedChangeType.SkillUp)
			{
				u3++;
			}
			else if (m.GetSpeedType() == SpeedChangeType.SkillDown)
			{
				d3++;
			}
		}
		foreach (var m in HistoryRecords)
		{
			if(m.GetSpeedType() == SpeedChangeType.SkillUp)
			{
				u1++;
			}
			else if(m.GetSpeedType() == SpeedChangeType.SkillDown)
			{
				d1++;
			}
		}

		if (CurMagicRecord.GetSpeedType() == SpeedChangeType.SkillUp)
		{
			u++;
		}
		else if (CurMagicRecord.GetSpeedType() == SpeedChangeType.SkillDown)
		{
			d++;
		}

		foreach (var m in FutureExeMoves)
		{
			if (m.GetSpeedType() == SpeedChangeType.SkillUp)
			{
				u2++;
			}
			else if (m.GetSpeedType() == SpeedChangeType.SkillDown)
			{
				d2++;
			}
		}
		if(log)
		{
			DebugConsole.LogWarning(kaitou + " UPDOWN:History(" + u1 + ":" + d1 + ") " + "Future(" + u2 + ":" + d2 + ") " + "Now(" + u + ":" + d + ") " + "Fix(" + u3 + ":" + d3 + ") " + "Sum(" + (u + u1 + u2 + u3) + ":" + (d + d1 + d2 + d3) + ")");
		}
		return (u + u1 + u2 + u3) - (d + d1 + d2 + d3);
	}
}
