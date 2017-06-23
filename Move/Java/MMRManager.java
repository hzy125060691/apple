package com.cyou.fusion.game.Battle.Move;

import com.cyou.fusion.example.protocol.GamePacket;
import com.cyou.fusion.example.protocol.GamePacketStruct;
import com.cyou.fusion.game.Battle.SkillBullet.SkillVector3;
import com.github.jlinqer.collections.List;

import java.util.Comparator;
import java.util.PriorityQueue;

/**
 * Created by huangzhenyu on 2017/6/23.
 */

public class MMRManager
{
    public enum FindCMDType
    {
        None,
        History,
        Now,
        Future,
    }
    private MagicMoveRecord CurMagicRecord = null;
//    private MagicMoveRecord CurMagicRecord
//    {
//        get
//        {
//            return curMagicRecord;
//        }
//        set
//        {
//            curMagicRecord = value;
//            //curIndex = Records.IndexOf(curMagicRecord);
//        }
//    }
    public MMRManager()
    {
        CurMagicRecord = new MagicMoveRecord();
        Records.add(CurMagicRecord);
    }
    //private int curIndex = -1;

    private List<MagicMoveRecord> Records = new List<MagicMoveRecord>();

    private List<MagicMoveRecord> recordsCache = new List<MagicMoveRecord>();

    public static Comparator<MagicMoveRecord> moveRecordComparator = new Comparator<MagicMoveRecord>()
    {
        @Override
        public int compare(MagicMoveRecord m1, MagicMoveRecord m2)
        {
            int ret = (int)(m1.GetTime() - m2.GetTime());
            return ret == 0? m1.GetCMDIndex() - m2.GetCMDIndex():ret;
        }
    };

    private float MinX = -1;
    private float MaxX = -1;
    private float MinZ = -1;
    private float MaxZ = -1;

    public void Clear()
    {
        CurMagicRecord.Clear();
        for (MagicMoveRecord tmp : Records.where(r->!r.equals(CurMagicRecord)))
        {
            tmp.Clear();
            recordsCache.add(tmp);
        }
        Records.removeIf((m)->!m.equals(CurMagicRecord));
        //curIndex = -1;
    }

    public void SetInitPos(float x, float z, long NowTime_Ms_Long, float dirX, float dirZ, float speed, long cmdTime, float minX, float maxX, float minZ, float maxZ, MagicMoveRecord.SpeedChangeType speedType, int cmdIdx)
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
        if (add.GetSpeedType() == MagicMoveRecord.SpeedChangeType.None)
        {
            for(int i = Records.size() - 1;i >= 0 ;i--)
            //for (MagicMoveRecord r : Records.reverse()<MagicMoveRecord>())
            {
                MagicMoveRecord r = Records.get(i);
                if (r.GetTime() == add.GetTime())
                {
                    equalList.add(r);
                }
                else if (r.GetTime() < add.GetTime())
                {
                    break;
                }
            }
            for (MagicMoveRecord e : equalList)
            {
                if (e.GetSpeedType() == add.GetSpeedType())
                {
                    Log("mutli SpeedChangeType.None :" + add.toString());
                    return null;
                }
            }
        }


        {
            Records.add(add);
            Records.sort(moveRecordComparator);
            int idx = Records.indexOf(add) - 1;
            if(idx < 0)
            {
                Log("if(idx < 0):"+idx);
                return null;
            }
//            MagicMoveRecord l = null;
//            for (MagicMoveRecord r : Records)
//            {
//                if (l != null && l.GetTime() == r.GetTime())
//                {
//                    boolean a = false;
//                    while(a)
//                    {
//
//                    }
//                }
//                l = r;
//            }
            return Records.get(idx);
        }
    }
    private MagicMoveRecord GetNewRecord()
    {
        MagicMoveRecord temp = null;
        if (recordsCache.size() > 0)
        {
            temp = recordsCache.get(0);
            recordsCache.remove(0);
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
        Records.remove(remove);
        remove.Clear();
        recordsCache.add(remove);
    }
    public void FindCurExeMove(long NowTime_Ms_Long)
    {
        MagicMoveRecord tmp = FindPreviousMMR(NowTime_Ms_Long);
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
    public void GetNextPos_2D(long NowTime_Ms_Long, SkillVector3 pos) throws Exception
    {
        CurMagicRecord.GetNextPos_2D(NowTime_Ms_Long, pos);
    }

    private MagicMoveRecord CalcNextMMR(MagicMoveRecord last, float dirX, float dirZ, MagicMoveRecord.SpeedChangeType speedType, long time, int index) throws Exception
    {
        return CalcNextMMR(last, dirX, dirZ, speedType, time, index, null);
    }
    private MagicMoveRecord CalcNextMMR(MagicMoveRecord last, float dirX, float dirZ, MagicMoveRecord.SpeedChangeType speedType, long time, int index, MagicMoveRecord newRecord)throws  Exception
    {
        //MagicMoveRecord newRecord = null;
        SkillVector3 curDir = last.GetDir();
        float curSpeed = last.GetSpeed();
        if (newRecord == null)
        {
            newRecord = GetNewRecord();
        }
        if (speedType != MagicMoveRecord.SpeedChangeType.None)
        {
            SkillVector3 beginPos = new SkillVector3();
            last.GetNextPos_2D(time, beginPos);
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
            long nextTime = last.GetNextMagicPosAndTime_2D(time, beginPos);
            //newRecord = GetNewRecord();
            newRecord.SetInitPos(beginPos.x, beginPos.z, nextTime, dirX, dirZ, last.GetSpeed(), time, MinX, MaxX, MinZ, MaxZ, speedType, index);
            Log(index + " Dir Changed : " + "(" + curDir.x + "," + curDir.z + ")" + "---->>>----" + "(" + dirX + "," + dirZ + ")" + " :" + newRecord);
            //Log(index + " Dir Changed : " + "(" + curDir.x + "," + curDir.z + ")" + "---->>>----" + "(" + dirX + "," + dirZ + ")" + " BeginPos:" + beginPos.ToString() + /*" NowPos:" + NextPos.ToString() +*/ " Time:" + newRecord.GetTime());
        }
        return newRecord;
    }
    private void FixMMRs(MagicMoveRecord first) throws Exception
    {
        Log("FixMMRs Enter:");
        int firstIdx = Records.indexOf(first);

        if(firstIdx < 0)
        {
            while(true)
            {
                Log("if(firstIdx < 0)");
            }
            //return;
        }
        MagicMoveRecord previous = Records.get(firstIdx);
        Log("Fix Begin:");
        for (int i = firstIdx + 1; i < Records.size(); i++)
        {
            MagicMoveRecord tmp = Records.get(i);
            CalcNextMMR(previous, tmp.GetDir().x, tmp.GetDir().z, tmp.GetSpeedType(), tmp.GetCMDTime(), tmp.GetCMDIndex(), tmp);
            previous = tmp;
        }
        Log("Fix End:");
        Log("FixMMRs Leave:");
    }
    float CalcSpeed(MagicMoveRecord.SpeedChangeType spdType, float nowSpd)
    {
        switch (spdType)
        {
            case SkillUp:
                return nowSpd * 2f;
            case SkillDown:
                return nowSpd / 2f;
            case ItemUp:
                return nowSpd * 8f;
            case ItemDown:
                return nowSpd / 8f;
            case None:
                return nowSpd;
            default:
                Log("error spdType:" + spdType);
                return 0;
        }
    }

    private MagicMoveRecord FindSpecialMMR(int mmrIdx)
    {
        for(int i = Records.size() - 1;i >= 0 ;i--)
        //foreach (var r in Records.Reverse<MagicMoveRecord>())
        {
            MagicMoveRecord r = Records.get(i);
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
        for(int i = Records.size() - 1;i >= 0 ;i--)
        //foreach (var r in Records.Reverse<MagicMoveRecord>())
        {
            MagicMoveRecord r = Records.get(i);
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
            tmp = Records.last();
        }
        return tmp;
    }
    public boolean AddMoveCtrlCommand(long time, float dirX, float dirZ, MagicMoveRecord.SpeedChangeType speedType, int index) throws  Exception
    {
        Log("Enter AddMoveCtrlCommand:" +  time);
        if (dirX == 0 && dirZ == 0 && speedType == MagicMoveRecord.SpeedChangeType.None)
        {
            Log("dirX == 0 && dirZ == 0 && speedType == SpeedChangeType.None");
            Log("Leave AddMoveCtrlCommand:" + time);
            return false;
        }

        MagicMoveRecord  previous = FindPreviousMMR(time);
        Log("previous:" + previous.toString());

        SkillVector3 curDir = previous.GetDir();
        if (curDir.x == dirX && curDir.z == dirZ && speedType == MagicMoveRecord.SpeedChangeType.None)
        {
            Log("curDir.x == dirX && curDir.z == dirZ && speedType == SpeedChangeType.None");
            Log("Leave AddMoveCtrlCommand:" + time);
            return false;
        }

        MagicMoveRecord newRecord = CalcNextMMR(previous, dirX, dirZ, speedType, time, index);

        MagicMoveRecord realPrevious = AddRecord(newRecord);
        if(realPrevious == null)
        {
            Log(" AddRecord(newRecord) failed:");
            Log("Leave AddMoveCtrlCommand:" + time);
            return false;
        }
        Log("RealPrevious:" + realPrevious.toString());

//        var beforeTime = newRecord.GetTime();
        FixMMRs(realPrevious);

//        if (beforeTime != newRecord.GetTime())
//        {
//            Log("Lesdfsdfave sfaf:" + newRecord);
//        }
        Log("Leave AddMoveCtrlCommand:" + time);
        return true;
    }
    public void CancelMoveCtrlCommand(long time, int index, int deleteIndex) throws Exception
    {
        MagicMoveRecord specialMMR = FindSpecialMMR(deleteIndex);
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
        boolean ret = AddMoveCtrlCommand(time, 0,0, MagicMoveRecord.SpeedChangeType.SkillDown, index);
        if(!ret)
        {
            while(true)
            {
                Log("var ret = AddMoveCtrlCommand(time, 0,0, SpeedChangeType.SkillDown, index);");
            }
            //return;
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
    public String GetCurMMRString()
    {
        return CurMagicRecord.toString();
    }
    public void ForceFreshMMR(List<GamePacketStruct.PlayerMagicMoveRecord> pmmrs, long time)
    {
        Clear();
        for (GamePacketStruct.PlayerMagicMoveRecord pmmr : pmmrs)
        {
            MagicMoveRecord mmr = GetNewRecord();

            mmr.SetInitPos(pmmr.getPosX(), pmmr.getPosZ(), pmmr.getTickTimeMsLong(), pmmr.getDirX(), pmmr.getDirZ(), pmmr.getSpeed(), pmmr.getCMDTime(), MinX, MaxX, MinZ, MaxZ,  MagicMoveRecord.SpeedChangeType.values()[pmmr.getSpeedType()], pmmr.getCMDIdx());
            Records.add(mmr);
        }
        RemoveRecord(CurMagicRecord);
        FindCurExeMove(time);
    }
    public void Log(String str)
    {
        AdvancedMoveController.logger.error(str);
        //Debug.LogError(str);
    }

}
