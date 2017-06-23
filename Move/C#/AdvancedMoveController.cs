using System;
using System.Collections.Generic;

using UnityEngine;

using ZzServer.Battle;



public class AdvancedMoveController
{
	protected MMRManager MMRs = new MMRManager();

	private SkillVector3 NextPos = new SkillVector3();
	private long NextPosTime = -1;

	private List<SkillVector3> BacktrackingPoints = new List<SkillVector3>();
	private List<SkillVector3> TrackingPoints = new List<SkillVector3>();

	private float MinX = -1;
	private float MaxX = -1;
	private float MinZ = -1;
	private float MaxZ = -1;

	

	public void Clear()
	{
		MMRs.Clear();
		NextPos = NextPos * 0;
		NextPosTime = -1;
		
		BacktrackingPoints.Clear();
		TrackingPoints.Clear();

		MinX = -1;
		MaxX = -1;
		MinZ = -1;
		MaxZ = -1;
	}

	public SkillVector3 GetCalcResult()
	{
		return NextPos;
	}
	public AdvancedMoveController()
	{
		Clear();
	}

	public void SetInitPos(float x, float z, long NowTime_Ms_Long, float dirX, float dirZ, float speed, float minX, float maxX, float minZ, float maxZ)
	{
		//只有这里可以设置速度和方向为0
		Clear();

		MinX = minX;
		MaxX = maxX;
		MinZ = minZ;
		MaxZ = maxZ;

		MMRs.SetInitPos(x, z, NowTime_Ms_Long, dirX, dirZ, speed, NowTime_Ms_Long, MinX, MaxX, MinZ, MaxZ, SpeedChangeType.None, -1);
	}

	public bool CalcNextPos(long NowTime_Ms_Long)
	{
		BacktrackingPoints.Clear();
		TrackingPoints.Clear();
		//检查当前的命令序列，是否有需要执行的，有几个需要执行
		MMRs.FindCurExeMove(NowTime_Ms_Long);

		//由于有可能有回退的点，所以这里需要计算一下
		CalcPoints();
		bool bBacktracking = BacktrackingPoints.Count > 0;

		MMRs.GetNextPos_2D(NowTime_Ms_Long, ref NextPos);
		NextPosTime = NowTime_Ms_Long;
		TrackingPoints.Add(NextPos);
		return bBacktracking;
	}
	private void CalcPoints()
	{

	}

	private long lastCMDTime = 0;
	private bool CheckCMDTime(long time)
	{
		if (time <= lastCMDTime)
		{
			Log("if(time <= lastCMDTime) : " + time + " last:" + lastCMDTime);
			return false;
		}
		lastCMDTime = time;
		return true;
	}
	public void TestAddMoveCtrlCommand(long time, float dirX, float dirZ, SpeedChangeType speedType, int index, bool CheckTime = true)
	{
		//检查是否是合法参数
		if(CheckTime && !CheckCMDTime(time))
		{
			return;
		}
		MMRs.AddMoveCtrlCommand(time, dirX, dirZ, speedType, index);
	}
	public void TestCancelMoveCtrlCommand(long time, int index, int deleteIndex)
	{
		//检查是否是合法参数
		if (!CheckCMDTime(time))
		{
			return;
		}
		MMRs.CancelMoveCtrlCommand(time, index, deleteIndex);
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

	public void Log(string str)
	{
		Debug.LogWarning(str);
	}
	public SkillVector3 GetCurDir()
	{
		return MMRs.GetCurDir();
	}
	public float GetCurSpeed()
	{
		return MMRs.GetCurSpeed();
	}
	public string GetCurMMRString()
	{
		return MMRs.GetCurMMRString();
	}

	public void ForceFreshMMR(List<ZzSocketShare.Protocol.PlayerMagicMoveRecord> pmmrs, long time)
	{
		MMRs.ForceFreshMMR(pmmrs, time);
	}
	public int DebugSkillUpDown(string kaitou, bool log = true)
	{
		return -1;
	}
}
