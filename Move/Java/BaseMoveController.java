package com.cyou.fusion.Battle.Move;

import com.cyou.fusion.Battle.Base.Tuple4;
import com.cyou.fusion.Battle.SkillBullet.SkillVector3;
import com.github.jlinqer.collections.List;

import java.util.Comparator;
import java.util.PriorityQueue;

/**
 * Created by huangzhenyu on 2017/5/15.
 */
public class BaseMoveController
{
    private MagicMoveRecord CurMagicRecord = new MagicMoveRecord();
    //private Tuple4<Long, Float, Float, Float> LastCommand = null;
    private SkillVector3 NextPos = new SkillVector3();
    private long NextPosTime = -1;

    private List<MagicMoveRecord> HistoryRecords = new List<MagicMoveRecord>();
    //private List<MagicCommandRecord> HistoryCommands = new List<MagicCommandRecord>();
    private List<MagicMoveRecord> FutureExeMoves = new List<MagicMoveRecord>();
    private List<MagicMoveRecord> CurNeedExeMoves = new List<MagicMoveRecord>();

    private List<SkillVector3> BacktrackingPoints = new List<SkillVector3>();
    private List<SkillVector3> TrackingPoints = new List<SkillVector3>();

    private List<MagicMoveRecord> recordsCache = new List<MagicMoveRecord>();
    //private List<MagicCommandRecord> commandsCache = new List<MagicCommandRecord>();

    public List<String> Infos = new List<String>();
    public SkillVector3 GetCalcResult()
    {
        return  NextPos;
    }
    public BaseMoveController()
    {
        Clear();
    }

    private float MinX = -1;
    private float MaxX = -1;
    private float MinZ = -1;
    private float MaxZ = -1;

    public static  org.slf4j.Logger logger = null;
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
        SkillVector3.OpMultiply(NextPos, 0);
        NextPosTime = -1;
        CurMagicRecord.Clear();
        for (MagicMoveRecord tmp : HistoryRecords)
        {
            tmp.Clear();
            recordsCache.add(tmp);
        }
        HistoryRecords.clear();

        FutureExeMoves.clear();
        CurNeedExeMoves.clear();
        BacktrackingPoints.clear();
        TrackingPoints.clear();
    }
    public void TestAddMoveCtrlCommand(long time, float dirX, float dirZ, float speed/*, long NowTime_Ms_Long*/, int index) throws Exception
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
            if(HistoryRecords.size() > 0)
            {
                MagicMoveRecord tmp = HistoryRecords.get(HistoryRecords.size() - 1);
                 if(tmp.GetTime() <= time && tmp.GetCMDTime() <= time)
                {
                    cmdLastRecord = tmp;
                }
            }

//            for(MagicMoveRecord tmp : HistoryRecords)
//            {
//                //Log("HistoryRecords:"+tmp.toString());
//                if(tmp.GetTime() <= time && tmp.GetCMDTime() <= time)
//                {
//                    cmdLastRecord = tmp;
//                }
//                else
//                {
//                    //这是个优先权队列，所以一旦发现不满足了就都不用检查了
//                    break;
//                }
//            }
            if(CurMagicRecord.GetTime() <= time && CurMagicRecord.GetCMDTime() <= time)
            {
                cmdLastRecord = CurMagicRecord;
                //Log("CurMagicRecord:"+CurMagicRecord.toString());
            }
            if(FutureExeMoves.size() > 0)
            {
                for (MagicMoveRecord tmp : FutureExeMoves)
                {
                    //Log("FutureExeMoves:"+tmp.toString());
                    if(tmp.GetTime() <= time && tmp.GetCMDTime() <= time)
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

            Log("LastMoveCmd:"+cmdLastRecord.toString());
            //根据上一个关键节点计算这个关键节点
            //强调一下，目前，也就是2017/05/17,单条命令只能是改速度或者是改方向，而且改变成的速度方向不可能是0，0只允许在初始值中
            if(speed == 0 || (dirX == 0 && dirZ == 0))
            {
                Log("speed == 0 || (dirX == 0 && dirZ == 0)");
                return;
            }
            SkillVector3 curDir = cmdLastRecord.GetDir();
            float curSpeed = cmdLastRecord.GetSpeed();
            if(curDir.x == dirX && curDir.z == dirZ && curSpeed == speed)
            {
                Log("curDir.x == dirX && curDir.z == dirZ && curSpeed == speed");
                return;
            }
            MagicMoveRecord newRecord = null;

            //速度改变的情况
            if(curSpeed != speed)
            {
                SkillVector3 beginPos = new SkillVector3();
                cmdLastRecord.GetNextPos_2D(time, beginPos);
                newRecord = GetNewRecord();
                newRecord.SetInitPos(beginPos.x, beginPos.z, time, dirX, dirZ, speed, time, MinX, MaxX, MinZ, MaxZ);
                Log(index+" Speed Changed : "+curSpeed +"---->>>----"+ speed + " BeginPos:" + beginPos.toString() + " NowPos:" + NextPos.toString());
            }
            else
            {
                //方向改变的情况,由于我们的版本现在，也就是2017/05/17只允许整格子的0.5处改变方向，所以时间会略微不一样
                SkillVector3 beginPos = new SkillVector3();
                long nextTime = cmdLastRecord.GetNextMagicPosAndTime_2D(time, beginPos);
                newRecord = GetNewRecord();
                newRecord.SetInitPos(beginPos.x, beginPos.z, nextTime, dirX, dirZ, speed, time, MinX, MaxX, MinZ, MaxZ);
                Log(index+" Dir Changed : "+" ("+curDir.x+","+curDir.z+")" +"---->>>----" + "("+dirX+","+dirZ+")"  + " BeginPos:" + beginPos.toString() + " NowPos:" + NextPos.toString());
            }
            if(newRecord.GetTime() > cmdLastRecord.GetTime())
            {
                boolean bAdd = true;
                for (MagicMoveRecord tmp : FutureExeMoves)
                {
                    if(tmp.GetTime() == newRecord.GetTime())
                    {
                        bAdd = false;
                    }
                }
                if (CurMagicRecord.GetTime() == newRecord.GetTime())
                {
                    bAdd = false;
                }
                if(bAdd)
                {
                    AddFutureExeMoves(newRecord);
                }
                else
                {
                    Log("abandon 222 "+newRecord.toString());
                }

            }
            else
            {
                Log("abandon "+newRecord.toString());
            }
        }
    }

    private MagicMoveRecord GetNewRecord()
    {
        MagicMoveRecord temp = null;
        if(recordsCache.size() > 0)
        {
            temp = recordsCache.remove(0);
            temp.Clear();
        }

        if(temp == null)
        {
            temp = new MagicMoveRecord();
        }
        return temp;
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
    public boolean CalcNextPos(long NowTime_Ms_Long) throws Exception
    {
        BacktrackingPoints.clear();
        TrackingPoints.clear();
        //检查当前的命令序列，是否有需要执行的，有几个需要执行
        FindNeedExeMove(NowTime_Ms_Long);

        //由于有可能有回退的点，所以这里需要计算一下
        CalcPoints();
        boolean bBacktracking = BacktrackingPoints.size()>0;

        CurMagicRecord.GetNextPos_2D(NowTime_Ms_Long, NextPos);
        NextPosTime = NowTime_Ms_Long;
        TrackingPoints.add(NextPos.clone());
        return bBacktracking;
    }

    private void FindNeedExeMove(long NowTime_Ms_Long) throws Exception
    {
        while (FutureExeMoves.size() > 0)
        {
            MagicMoveRecord tmp = FutureExeMoves.get(0);
            if(tmp != null && tmp.GetTime() < NowTime_Ms_Long)
            {
                FutureExeMoves.remove(0);
                CurNeedExeMoves.add(tmp);
            }
            else
            {
                break;
            }
        }
    }

    private void CalcPoints()
    {
        if(CurNeedExeMoves.size() > 0)
        {
            MagicMoveRecord tmp = CurNeedExeMoves.get(0);
            if(tmp.GetTime() < NextPosTime)
            {
                BacktrackingPoints.add(tmp.GetPos().clone());
                BacktrackingPoints.add(NextPos.clone());
            }
        }
        if(BacktrackingPoints.size() <= 0)
        {
            TrackingPoints.add(NextPos.clone());
        }
        while (CurNeedExeMoves.size() > 0)
        {
            MagicMoveRecord tmp = CurNeedExeMoves.remove(0);

            TrackingPoints.add(tmp.GetPos().clone());

            HistoryRecords.add(CurMagicRecord);
            CurMagicRecord = tmp;

        }
        return ;
    }
    private void Log(String str)
    {
        logger.error(str);
    }

    private void AddFutureExeMoves(MagicMoveRecord add)
    {
        FutureExeMoves.add(add);
        FutureExeMoves.sort(moveRecordComparator);
        Log(add.toString());
    }

    public static Comparator<MagicMoveRecord> moveRecordComparator = new Comparator<MagicMoveRecord>()
    {
        @Override
        public int compare(MagicMoveRecord m1, MagicMoveRecord m2) {
            return (int) (m1.GetTime() - m2.GetTime());
        }
    };


    public List<MagicMoveRecord> GetNotifyMMRs()
    {
        List<MagicMoveRecord> list = new List<MagicMoveRecord>();
        list.add(CurMagicRecord);
        list.addAll(FutureExeMoves);
        return list;
    }
}
