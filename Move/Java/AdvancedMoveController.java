package com.cyou.fusion.game.Battle.Move;

import com.cyou.fusion.example.protocol.GamePacketStruct;
import com.cyou.fusion.game.Battle.SkillBullet.SkillVector3;
import com.github.jlinqer.collections.List;

import java.util.Comparator;
import java.util.PriorityQueue;


/**
 * Created by huangzhenyu on 2017/5/15.
 */
public class AdvancedMoveController
{
    public static  org.slf4j.Logger logger = null;

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
        NextPos = SkillVector3.OpMultiply(NextPos, 0) ;
        NextPosTime = -1;

        BacktrackingPoints.clear();
        TrackingPoints.clear();

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

        MMRs.SetInitPos(x, z, NowTime_Ms_Long, dirX, dirZ, speed, NowTime_Ms_Long, MinX, MaxX, MinZ, MaxZ, MagicMoveRecord.SpeedChangeType.None, -1);
    }

    public boolean CalcNextPos(long NowTime_Ms_Long) throws  Exception
    {
        BacktrackingPoints.clear();
        TrackingPoints.clear();
        //检查当前的命令序列，是否有需要执行的，有几个需要执行
        MMRs.FindCurExeMove(NowTime_Ms_Long);

        //由于有可能有回退的点，所以这里需要计算一下
        CalcPoints();
        boolean bBacktracking = BacktrackingPoints.size() > 0;

        MMRs.GetNextPos_2D(NowTime_Ms_Long, NextPos);
        NextPosTime = NowTime_Ms_Long;
        TrackingPoints.add(NextPos);
        return bBacktracking;
    }
    private void CalcPoints()
    {

    }

    private long lastCMDTime = 0;
    private boolean CheckCMDTime(long time)
    {
        if (time <= lastCMDTime)
        {
            Log("if(time <= lastCMDTime) : " + time + " last:" + lastCMDTime);
            return false;
        }
        lastCMDTime = time;
        return true;
    }
    public void TestAddMoveCtrlCommand(long time, float dirX, float dirZ, MagicMoveRecord.SpeedChangeType speedType, int index) throws Exception
    {
        TestAddMoveCtrlCommand(time, dirX, dirZ, speedType, index, true);
    }
    public void TestAddMoveCtrlCommand(long time, float dirX, float dirZ, MagicMoveRecord.SpeedChangeType speedType, int index, boolean CheckTime) throws Exception
    {
        //检查是否是合法参数
        if(CheckTime && !CheckCMDTime(time))
        {
            return;
        }
        MMRs.AddMoveCtrlCommand(time, dirX, dirZ, speedType, index);
    }
    public void TestCancelMoveCtrlCommand(long time, int index, int deleteIndex) throws  Exception
    {
        //检查是否是合法参数
        if (!CheckCMDTime(time))
        {
            return;
        }
        MMRs.CancelMoveCtrlCommand(time, index, deleteIndex);
    }

    public boolean IsBK()
    {
        return BacktrackingPoints.size() > 0;
    }
    public boolean IsTracking()
    {
        return TrackingPoints.size() > 0;
    }
    public List<SkillVector3> GetBKPs()
    {
        return BacktrackingPoints;
    }
    public List<SkillVector3> GetTPs()
    {
        return TrackingPoints;
    }

    public void Log(String str)
    {
        logger.warn(str);
        //Debug.LogWarning(str);
    }
    public SkillVector3 GetCurDir()
    {
        return MMRs.GetCurDir();
    }
    public float GetCurSpeed()
    {
        return MMRs.GetCurSpeed();
    }
    public String GetCurMMRString()
    {
        return MMRs.GetCurMMRString();
    }

    public void ForceFreshMMR(List<GamePacketStruct.PlayerMagicMoveRecord> pmmrs, long time)
    {
        MMRs.ForceFreshMMR(pmmrs, time);
    }
    public int DebugSkillUpDown(String kaitou)
    {
        return DebugSkillUpDown(kaitou, true);
    }

    public int DebugSkillUpDown(String kaitou, boolean log)
    {
        return -1;
    }

    public List<MagicMoveRecord> GetNotifyMMRs()
    {
        List<MagicMoveRecord> list = new List<MagicMoveRecord>();
        return list;
    }
}
