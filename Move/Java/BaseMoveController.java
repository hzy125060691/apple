package com.cyou.fusion.game.Battle.Move;

import com.cyou.fusion.game.Battle.SkillBullet.SkillVector3;
import com.github.jlinqer.collections.List;

import java.util.Comparator;
import java.util.PriorityQueue;

/**
 * Created by huangzhenyu on 2017/5/15.
 */
public class BaseMoveController
{
//    public MagicMoveRecord CurMagicRecord = new MagicMoveRecord();
//    //private Tuple4<Long, Float, Float, Float> LastCommand = null;
//    private SkillVector3 NextPos = new SkillVector3();
//    private long NextPosTime = -1;
//
//    private List<MagicMoveRecord> HistoryRecords = new List<MagicMoveRecord>();
//    //private List<MagicCommandRecord> HistoryCommands = new List<MagicCommandRecord>();
//    private List<MagicMoveRecord> FutureExeMoves = new List<MagicMoveRecord>();
//    private List<MagicMoveRecord> CurNeedExeMoves = new List<MagicMoveRecord>();
//
//    private List<SkillVector3> BacktrackingPoints = new List<SkillVector3>();
//    private List<SkillVector3> TrackingPoints = new List<SkillVector3>();
//
//    private List<MagicMoveRecord> recordsCache = new List<MagicMoveRecord>();
//
//   // public List<String> Infos = new List<String>();
//   public enum SpeedChangeType
//   {
//       None(0),
//       SkillUp(1),//技能加速
//       SkillDown(2),//技能减速
//       ItemUp(3),//加速物品
//       ItemDown(4);//加速减速
//       int num;
//       SpeedChangeType(int n) {
//           num = n;
//       }
//       int GetNum() {
//           return num;
//       }
//   }
//
//    public SkillVector3 GetCalcResult()
//    {
//        return  NextPos;
//    }
//    public BaseMoveController()
//    {
//        Clear();
//    }
//
//    private float MinX = -1;
//    private float MaxX = -1;
//    private float MinZ = -1;
//    private float MaxZ = -1;
//
//    public static  org.slf4j.Logger logger = null;
//    public void SetInitPos(float x, float z, long NowTime_Ms_Long, float dirX, float dirZ, float speed, float minX, float maxX, float minZ, float maxZ)
//    {
//        //只有这里可以设置速度和方向为0
//        Clear();
//
//        MinX = minX;
//        MaxX = maxX;
//        MinZ = minZ;
//        MaxZ = maxZ;
//
//        CurMagicRecord.SetInitPos(x, z, NowTime_Ms_Long, dirX, dirZ, speed, NowTime_Ms_Long, MinX, MaxX, MinZ, MaxZ, SpeedChangeType.None, -1);
//    }
//
//    public void Clear()
//    {
//        SkillVector3.OpMultiply(NextPos, 0);
//        NextPosTime = -1;
//        CurMagicRecord.Clear();
//        for (MagicMoveRecord tmp : HistoryRecords)
//        {
//            tmp.Clear();
//            recordsCache.add(tmp);
//        }
//        HistoryRecords.clear();
//
//        FutureExeMoves.clear();
//        CurNeedExeMoves.clear();
//        BacktrackingPoints.clear();
//        TrackingPoints.clear();
//        NeedFixRecords.clear();
//    }
//    public enum FindCMDType
//    {
//        None,
//        History,
//        Now,
//        Future,
//    }
//    private List<MagicMoveRecord> NeedFixRecords = new List<MagicMoveRecord>();
//    private void FixToFuture()
//    {
//        FutureExeMoves.addAll(NeedFixRecords);
//        NeedFixRecords.clear();
//    }
//    private MagicMoveRecord FixMMRs(MagicMoveRecord first) throws Exception
//    {
//        MagicMoveRecord last = first;
//        for ( int i = 0;i < NeedFixRecords.size();)
//        {
//            if(last != null)
//            {
//                MagicMoveRecord tmp = NeedFixRecords.get(i);
//                MagicMoveRecord temp2 = CalcNextMMR(last, tmp.GetDir().x, tmp.GetDir().z, tmp.GetSpeedType(), tmp.GetTime(), tmp.GetCMDIndex(), tmp);
//                if(temp2 != null)
//                {
//                    i++;
//                }
//                else
//                {
//                    if(last==first && first.GetCMDIndex() > tmp.GetCMDIndex())
//                    {
//                        first = tmp;
//                    }
//                    else
//                    {
//                        logger.error("if(last==first && first.GetCMDIndex() > tmp.GetCMDIndex())@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@22");
//                    }
//                    NeedFixRecords.remove(i);
//                    Log("discard:" + tmp.toString());
//                }
//                last = tmp;
//            }
//        }
//        return first;
//    }
//    private MagicMoveRecord CalcNextMMR(MagicMoveRecord last, float dirX, float dirZ, SpeedChangeType speedType, long time, int index, MagicMoveRecord newRecord) throws Exception
//    {
//        //MagicMoveRecord newRecord = null;
//        SkillVector3 curDir = last.GetDir();
//        float curSpeed = last.GetSpeed();
//        if(newRecord == null)
//        {
//            newRecord = GetNewRecord();
//        }
//        if (speedType != SpeedChangeType.None)
//        {
//            SkillVector3 beginPos = new SkillVector3();
//            last.GetNextPos_2D(time, beginPos);
//            //newRecord = GetNewRecord();
//            newRecord.SetInitPos(beginPos.x, beginPos.z, time, curDir.x, curDir.z, CalcSpeed(speedType, curSpeed), time, MinX, MaxX, MinZ, MaxZ, speedType, index);
//
//            //FixMMRs(newRecord);
//            Log(index + " Speed Changed : " + curSpeed + "---->>>----" + newRecord.GetSpeed() + " BeginPos:" + beginPos.toString() + " NowPos:" + NextPos.toString() + " Time:" + newRecord.GetTime());
//        }
//        else
//        {
//            if(last.GetTime() == newRecord.GetTime())
//            {
//                return null;
//            }
//            SkillVector3 beginPos = new SkillVector3();
//            long nextTime = last.GetNextMagicPosAndTime_2D(time, beginPos);
//            //newRecord = GetNewRecord();
//            newRecord.SetInitPos(beginPos.x, beginPos.z, nextTime, dirX, dirZ, last.GetSpeed(), time, MinX, MaxX, MinZ, MaxZ, speedType, index);
//            Log(index + " Dir Changed : " + "(" + curDir.x + "," + curDir.z + ")" + "---->>>----" + "(" + dirX + "," + dirZ + ")" + " BeginPos:" + beginPos.toString() + " NowPos:" + NextPos.toString() + " Time:" + newRecord.GetTime());
//        }
//        return newRecord;
//    }
//    public boolean TestCancelMoveCtrlCommand(long time, float dirX, float dirZ, SpeedChangeType speedType, int index, int deleteIndex)throws Exception
//    {
//        //12312312
//        //todo s dfsdfsdfsdf
//        DebugSkillUpDown("Cancel Before ADD");
//        boolean ret = true;
//        NeedFixRecords.clear();
//
//        //如果要取消的操作已经执行了，那么就不需要取消了
//        if (FutureExeMoves.size() > 0)
//        {
//            int deleteIdx = -1;
//            boolean bAdd = false;
//            MagicMoveRecord tar = null;
//            for (int i = FutureExeMoves.size() - 1; i >= 0; i--)
//            {
//                MagicMoveRecord tmp = FutureExeMoves.get(i);
//                if (tmp.GetCMDIndex() == deleteIndex && tmp.GetSpeedType() == SpeedChangeType.SkillDown)
//                {
//                    bAdd = true;
//                    deleteIdx = i;
//                    tar = tmp;
//                    break;
//                }
//            }
//            if(deleteIdx >= 0)
//            {
////                if(tar.GetTime() <= time)
////                {
////                    return;
////                }
//                FutureExeMoves.remove(deleteIdx);
//            }
////            else if(CurMagicRecord.GetCMDIndex() == deleteIndex && CurMagicRecord.GetSpeedType() == SpeedChangeType.SkillDown)
////            {
////                tar = CurMagicRecord;
////                CurMagicRecord = null;
////                bAdd = true;
//////                if (tar.GetTime() <= time)
//////                {
//////                    return;
//////                }
////            }
////            else
////            {
////                for (int i = HistoryRecords.size() - 1; i >= 0; i--)
////                {
////                    MagicMoveRecord tmp = HistoryRecords.get(i);
////                    if (tmp.GetCMDIndex() == deleteIndex && tmp.GetSpeedType() == SpeedChangeType.SkillDown)
////                    {
////                        tar = tmp;
////                        bAdd = true;
////                        deleteIdx = i;
////                        break;
////                    }
////                }
////                if (deleteIdx >= 0)
////                {
//////                    if (tar.GetTime() <= time)
//////                    {
//////                        return;
//////                    }
////                    HistoryRecords.remove(deleteIdx);
////                }
////            }
//
//            if(bAdd)
//            {
//                ret = true;
//                TestAddMoveCtrlCommand(time, dirX, dirZ, speedType, index);
//            }
//            else
//            {
//                ret = false;
//            }
//
//        }
//        DebugSkillUpDown("Cancel After ADD");
//        return ret;
//    }
//
//    public void TestAddMoveCtrlCommand(long time, float dirX, float dirZ, SpeedChangeType speedType, int index) throws Exception
//    {
//        DebugSkillUpDown("Before ADD");
//        //检查是否是合法参数
//        //如果这个时间点比服务器时间更早
//        //if(time <= CurMagicRecord.GetTime())
//        //{
//           // throw new Exception("这个时间点比服务器时间更早:" + time + "@" + NowTime_Ms_Long );
//        //}
//        //先找到该时间点前一个MagicMoveRecord是哪个，可能是当前的MagicMoveRecord,也可能是缓存队列里某一个，所以遍历一下
//        //上边已经排除了一个不可能的情况，现在看其他情况
//        {
//            NeedFixRecords.clear();
//
//            MagicMoveRecord cmdLastRecord = null;
//            FindCMDType find = FindCMDType.None;
//            int findIdx = -1;
//
//            if(HistoryRecords.size() > 0)
//            {
//                for(int i = HistoryRecords.size() - 1; i >= 0; i--)
//                {
//                    MagicMoveRecord tmp = HistoryRecords.get(i);
//                    if (tmp.GetTime() <= time && tmp.GetCMDTime() <= time)
//                    {
//                        cmdLastRecord = tmp;
//                        break;
//                    }
//                    else
//                    {
//                        findIdx = i;
//                        find = FindCMDType.History;
//                    }
//                }
//
//            }
//
//            {
//                if(find == FindCMDType.None)
//                {
//                    if (CurMagicRecord != null && CurMagicRecord.GetTime() <= time && CurMagicRecord.GetCMDTime() <= time)
//                    {
//                        cmdLastRecord = CurMagicRecord;
//                        //Log("CurMagicRecord:" + CurMagicRecord.ToString());
//                    }
//                    else
//                    {
//                        find = FindCMDType.Now;
//                    }
//                }
//                if(find == FindCMDType.None)
//                {
//                    if (FutureExeMoves.size() > 0)
//                    {
//                        for (MagicMoveRecord tmp : FutureExeMoves)
//                        {
//                            //Log("FutureExeMoves:" + tmp.ToString());
//                            if (tmp.GetTime() <= time && tmp.GetCMDTime() <= time)
//                            {
//                                cmdLastRecord = tmp;
//                            }
//                            else
//                            {
//                                findIdx = FutureExeMoves.indexOf(tmp);
//                                find = FindCMDType.Future;
//                                //这是个优先权队列，所以一旦发现不满足了就都不用检查了
//                                break;
//                            }
//                        }
//                    }
//                }
//
//            }
//
//            Log("LastMoveCmd:"+cmdLastRecord.toString());
//            //根据上一个关键节点计算这个关键节点
//            if (dirX == 0 && dirZ == 0 && speedType == SpeedChangeType.None)
//            {
//                Log("dirX == 0 && dirZ == 0");
//                return;
//            }
//
//            SkillVector3 curDir = cmdLastRecord.GetDir();
//            float curSpeed = cmdLastRecord.GetSpeed();
//            if (curDir.x == dirX && curDir.z == dirZ && speedType == SpeedChangeType.None)
//            {
//                Log("curDir.x == dirX && curDir.z == dirZ && speedType == SpeedChangeType.None");
//                return;
//            }
//            MagicMoveRecord newRecord = CalcNextMMR(cmdLastRecord, dirX, dirZ, speedType, time, index, null);
//            Log(" find ret :"+find);
//            //速度改变的情况
//            //if (speedType != SpeedChangeType.None)
//            {
//                switch (find)
//                {
//                    case Future:
//                    {
//                        if(FutureExeMoves.size() > findIdx)
//                        {
//                            NeedFixRecords.addAll(FutureExeMoves.subList(findIdx, FutureExeMoves.size()));
//                            if(NeedFixRecords.size() > 1 && NeedFixRecords.get(NeedFixRecords.size()-1).equals(NeedFixRecords.get(NeedFixRecords.size() - 2)))
//                            {
//                                logger.error("error ******************************88");
//                            }
//                            for(int i = findIdx; i < FutureExeMoves.size();)
//                            {
//                                FutureExeMoves.remove(i);
//                            }
//                        }
//                    }
//                    break;
//                    case History:
//                    {
//                        if (HistoryRecords.size() > findIdx)
//                        {
//                            NeedFixRecords.addAll(HistoryRecords.subList(findIdx, HistoryRecords.size()));
//                            for(int i = findIdx; i < HistoryRecords.size();)
//                            {
//                                HistoryRecords.remove(i);
//                            }
//                        }
//                        if(findIdx == 0)
//                        {
//                            throw new Exception("findIdx == 0");
//                        }
//                        NeedFixRecords.add(CurMagicRecord);
//                        CurMagicRecord = HistoryRecords.get(findIdx-1);
//                        HistoryRecords.remove(findIdx-1);
//
//                        if (FutureExeMoves.size() > 0)
//                        {
//                            NeedFixRecords.addAll(FutureExeMoves);
//                            FutureExeMoves.clear();
//                        }
//                        break;
//                    }
//                    case Now:
//                    {
//                        NeedFixRecords.add(CurMagicRecord);
//                        if(HistoryRecords.size() == 0)
//                        {
//                            throw new Exception("HistoryRecords.size() == 0");
//                        }
//                        CurMagicRecord = HistoryRecords.get(HistoryRecords.size()-1);
//                        HistoryRecords.remove(HistoryRecords.size()-1);
//                        if (FutureExeMoves.size() > 0)
//                        {
//                            NeedFixRecords.addAll(FutureExeMoves);
//                            FutureExeMoves.clear();
//                        }
//                    }
//                    break;
//                }
//                if(NeedFixRecords.size() > 1 && NeedFixRecords.get(NeedFixRecords.size()-1).equals(NeedFixRecords.get(NeedFixRecords.size() - 2)))
//                {
//                    logger.error("error ******************************88");
//                }
//                newRecord = FixMMRs(newRecord);
//                //Log(index + " Speed Changed : " + curSpeed + "---->>>----" + speed + " BeginPos:" + beginPos.ToString() + " NowPos:" + NextPos.ToString());
//            }
//
//            if(newRecord.GetTime() > cmdLastRecord.GetTime() || (newRecord.GetTime() == cmdLastRecord.GetTime() && newRecord.GetSpeedType() != SpeedChangeType.None))
//            {
//                boolean bAdd = true;
//                for (MagicMoveRecord tmp : FutureExeMoves)
//                {
//                    if(tmp.GetTime() == newRecord.GetTime() && tmp.GetSpeedType() == SpeedChangeType.None && newRecord.GetSpeedType() == SpeedChangeType.None)
//                    {
//                        bAdd = false;
//                    }
//                }
//                if (CurMagicRecord != null && CurMagicRecord.GetTime() == newRecord.GetTime() && CurMagicRecord.GetSpeedType() == SpeedChangeType.None && newRecord.GetSpeedType() == SpeedChangeType.None)
//                {
//                    bAdd = false;
//                }
//                if(bAdd)
//                {
//                    AddFutureExeMoves(newRecord);
//                }
//                else
//                {
//                    Log("abandon 222 "+newRecord.toString());
//                }
//
//            }
//            else
//            {
//                Log("abandon "+newRecord.toString());
//            }
//        }
//        {
//            FixToFuture();
//        }
//        DebugSkillUpDown("After ADD");
//    }
//    static int skillupcount = 0;
//    static int skilldowncount = 0;
//    private MagicMoveRecord GetNewRecord()
//    {
//        MagicMoveRecord temp = null;
//        if(recordsCache.size() > 0)
//        {
//            temp = recordsCache.remove(0);
//            temp.Clear();
//        }
//
//        if(temp == null)
//        {
//            temp = new MagicMoveRecord();
//        }
//        return temp;
//    }
//    public boolean IsBK()
//    {
//        return BacktrackingPoints.size() > 0;
//    }
//    public boolean IsTracking()
//    {
//        return TrackingPoints.size() > 0;
//    }
//    public List<SkillVector3> GetBKPs()
//    {
//        return BacktrackingPoints;
//    }
//    public List<SkillVector3> GetTPs()
//    {
//        return TrackingPoints;
//    }
//    public boolean CalcNextPos(long NowTime_Ms_Long) throws Exception
//    {
//        BacktrackingPoints.clear();
//        TrackingPoints.clear();
//        //检查当前的命令序列，是否有需要执行的，有几个需要执行
//        FindNeedExeMove(NowTime_Ms_Long);
//
//        //由于有可能有回退的点，所以这里需要计算一下
//        CalcPoints();
//        boolean bBacktracking = BacktrackingPoints.size()>0;
//
//        CurMagicRecord.GetNextPos_2D(NowTime_Ms_Long, NextPos);
//        NextPosTime = NowTime_Ms_Long;
//        TrackingPoints.add(NextPos.clone());
//        return bBacktracking;
//    }
//
//    private void FindNeedExeMove(long NowTime_Ms_Long) throws Exception
//    {
//        while (FutureExeMoves.size() > 0)
//        {
//            MagicMoveRecord tmp = FutureExeMoves.get(0);
//            if(tmp != null && tmp.GetTime() < NowTime_Ms_Long)
//            {
//                FutureExeMoves.remove(0);
//                CurNeedExeMoves.add(tmp);
//            }
//            else
//            {
//                break;
//            }
//        }
//    }
//
//    private void CalcPoints()
//    {
//        if(CurNeedExeMoves.size() > 0)
//        {
//            MagicMoveRecord tmp = CurNeedExeMoves.get(0);
//            if(tmp.GetTime() < NextPosTime)
//            {
//                BacktrackingPoints.add(tmp.GetPos().clone());
//                BacktrackingPoints.add(NextPos.clone());
//            }
//        }
//        if(BacktrackingPoints.size() <= 0)
//        {
//            TrackingPoints.add(NextPos.clone());
//        }
//        while (CurNeedExeMoves.size() > 0)
//        {
//            MagicMoveRecord tmp = CurNeedExeMoves.remove(0);
//
//            TrackingPoints.add(tmp.GetPos().clone());
//            if(HistoryRecords.size() > 0)
//            {
//                if(CurMagicRecord.GetTime()< HistoryRecords.get(HistoryRecords.size()-1).GetTime())
//                {
//                    logger.error("Big Error Whats Up");
//                }
//            }
//            HistoryRecords.add(CurMagicRecord);
//            CurMagicRecord = tmp;
//
//        }
//        return ;
//    }
//    private void Log(String str)
//    {
//        logger.error(str);
//    }
//
//    private void AddFutureExeMoves(MagicMoveRecord add)
//    {
//        FutureExeMoves.add(add);
//        FutureExeMoves.sort(moveRecordComparator);
//        Log(add.toString());
//    }
//
//    public static Comparator<MagicMoveRecord> moveRecordComparator = new Comparator<MagicMoveRecord>()
//    {
//        @Override
//        public int compare(MagicMoveRecord m1, MagicMoveRecord m2) {
//            return (int) (m1.GetTime() - m2.GetTime());
//        }
//    };
//
//
//    public List<MagicMoveRecord> GetNotifyMMRs()
//    {
//        List<MagicMoveRecord> list = new List<MagicMoveRecord>();
//        list.add(CurMagicRecord);
//        list.addAll(FutureExeMoves);
//        return list;
//    }
//    float CalcSpeed(SpeedChangeType spdType, float nowSpd)
//    {
//        switch (spdType)
//        {
//            case SkillUp:
//                return nowSpd * 2f;
//            case SkillDown:
//                return nowSpd / 2f;
//            case ItemUp:
//                return nowSpd * 8f;
//            case ItemDown:
//                return nowSpd / 8f;
//            case None:
//                return nowSpd;
//            default:
//                Log("error spdType:" + spdType);
//                return 0;
//        }
//    }
//    public int DebugSkillUpDown(String kaitou)
//    {
//        return DebugSkillUpDown(kaitou, true);
//    }
//    public int DebugSkillUpDown(String kaitou, boolean log)
//    {
//        int u = 0;
//        int d = 0;
//        int u1 = 0;
//        int d1 = 0;
//        int u2 = 0;
//        int d2 = 0;
//        int u3 = 0;
//        int d3 = 0;
//        for (MagicMoveRecord m : NeedFixRecords)
//        {
//            if (m.GetSpeedType() == SpeedChangeType.SkillUp)
//            {
//                u3++;
//            }
//            else if (m.GetSpeedType() == SpeedChangeType.SkillDown)
//            {
//                d3++;
//            }
//        }
//        for (MagicMoveRecord m : HistoryRecords)
//        {
//            if(m.GetSpeedType() == SpeedChangeType.SkillUp)
//            {
//                u1++;
//            }
//            else if(m.GetSpeedType() == SpeedChangeType.SkillDown)
//            {
//                d1++;
//            }
//        }
//
//        if (CurMagicRecord.GetSpeedType() == SpeedChangeType.SkillUp)
//        {
//            u++;
//        }
//        else if (CurMagicRecord.GetSpeedType() == SpeedChangeType.SkillDown)
//        {
//            d++;
//        }
//
//        for (MagicMoveRecord m : FutureExeMoves)
//        {
//            if (m.GetSpeedType() == SpeedChangeType.SkillUp)
//            {
//                u2++;
//            }
//            else if (m.GetSpeedType() == SpeedChangeType.SkillDown)
//            {
//                d2++;
//            }
//        }
//        if(log)
//        {
//            Log(kaitou + " UPDOWN:History(" + u1 + ":" + d1 + ") " + "Future(" + u2 + ":" + d2 + ") " + "Now(" + u + ":" + d + ") " + "Fix(" + u3 + ":" + d3 + ") " + "Sum(" + (u + u1 + u2 + u3) + ":" + (d + d1 + d2 + d3) + ")");
//        }
//        return (u + u1 + u2 + u3) - (d + d1 + d2 + d3);
//    }
}
