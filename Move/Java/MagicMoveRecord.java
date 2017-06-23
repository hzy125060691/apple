package com.cyou.fusion.game.Battle.Move;

import com.cyou.fusion.game.Battle.SkillBullet.SkillVector3;
import com.github.jlinqer.collections.List;

/**
 * Created by huangzhenyu on 2017/5/15.
 */
public class MagicMoveRecord
{
    public enum SpeedChangeType
    {
        None(0),
        SkillUp(1),//技能加速
        SkillDown(2),//技能减速
        ItemUp(3),//加速物品
        ItemDown(4);//加速减速
        int num;
        SpeedChangeType(int n) {
            num = n;
        }
        int GetNum() {
            return num;
        }
    }
    public MagicCommandRecord Command = null;

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

    public static final float OutdateTime = 1000 * 1000;//毫秒
    public static final float CenterPoint = 0f;
    public static final float f05MinusCenterPoint = 0.5f - CenterPoint;
    public static final float EdgeWidth = 1f;

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
        return  Pos;
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


        Command = null;

    }
    public void SetInitPos(float x, float z, long NowTime_Ms_Long, float dirX, float dirZ, float speed, long cmdTime, float minX, float maxX, float minZ, float maxZ, SpeedChangeType speedType, int cmdIdx)
    {
        Pos.x = x;
        Pos.y = 0;
        Pos.z = z;
        Dir.x = dirX==0?0:Math.abs(dirX)/dirX;
        Dir.y = 0;
        Dir.z = dirZ==0?0:Math.abs(dirZ)/dirZ;
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


    public boolean IsOutdate(long otherTime)
    {
        return (otherTime - GetTime()) >= OutdateTime;
    }
    public boolean IsOutdate(MagicMoveRecord other)
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
    public boolean IsDirOrSpdChanged(float dirX, float dirZ, float spd)
    {
        return Dir.x != dirX || Dir.z != dirZ || Speed != spd;
    }
    public static float Clamp(float value, float min, float max)
    {
        return Math.max(min, Math.min(max, value));
    }
    public void GetNextPos_2D(long NowTime_Ms_Long, SkillVector3 pos) throws  Exception
    {
        long diffTime = Compare(NowTime_Ms_Long);
        if(diffTime>0)
        {
            double r = Math.sqrt(Math.pow(Dir.x,2)+Math.pow(Dir.z,2));
            double sin = r==0?0:Dir.z/r;
            double cos = r==0?0:Dir.x/r;
            pos.x = Clamp((float)(Pos.x + cos*Speed*diffTime), MinX, MaxX);
            pos.z = Clamp((float)(Pos.z + sin*Speed*diffTime), MinZ, MaxZ);
        }
        else if(diffTime <= 0)
        {
            pos.x = Pos.x;
            pos.z = Pos.z;
            //throw new Exception("else if(diffTime < 0)");
        }
//        else
//        {
//            throw new Exception("需要辅助参数来确定顺序，否则会出现客户端服务器不一致 3333333333333333333");
//        }
    }
    public long GetNextMagicPosAndTime_2D(long nowTime,SkillVector3 nextPos) throws  Exception
    {
        if(Speed <= 0 || (Dir.x == 0 && Dir.z == 0) || Dir.x * Dir.z != 0)
        {
            nextPos.x = Pos.x;
            nextPos.z = Pos.z;
            return nowTime;

        }
        long time = -1;
        SkillVector3 nowPos = new SkillVector3();
        GetNextPos_2D(nowTime, nowPos);

        if(Dir.x != 0)
        {
            nextPos.x = Clamp(UnifyRound(nowPos.x + f05MinusCenterPoint) + 0.5f * Dir.x - f05MinusCenterPoint, MinX, MaxX);
            nextPos.z = nowPos.z;
            float dis = Math.abs(nextPos.x - nowPos.x);
            time = nowTime + UnifyRound(dis/Speed);
        }
        else if(Dir.z != 0)
        {
            nextPos.z = Clamp(UnifyRound(nowPos.z + f05MinusCenterPoint) + 0.5f * Dir.z - f05MinusCenterPoint, MinX, MaxX);
            nextPos.x = nowPos.x;
            float dis = Math.abs(nextPos.z - nowPos.z);
            time = nowTime + UnifyRound(dis/Speed);
        }
        else
        {
            throw new Exception("");
        }
        return time;

    }

    @Override
    public String toString()
    {
        //java.text.DecimalFormat decimalFormat=new java.text.DecimalFormat(".00000000");//构造方法的字符格式这里如果小数不足x位,会以0补足.
        //String test2 = decimalFormat.format((float)Math.round(0.50000f));
        return "Index:"+ CMDIdx + " Time:"+TickTime_Ms_Long+" Pos:" + Pos.toString() + " Dir:" + Dir.toString() + " Speed:"+ Speed + " CMDTime:"+CMDTime + " Type:" + SpeedType;
    }

    private int UnifyRound(float f)
    {
        return (int)Math.floor(f+0.5f);
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
