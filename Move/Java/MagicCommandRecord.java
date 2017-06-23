package com.cyou.fusion.game.Battle.Move;

/**
 * Created by huangzhenyu on 2017/5/22.
 */
public class MagicCommandRecord
{
    public long time;
    public float dirX;
    public float dirZ;
    public float speed;
    public int index;

    public MagicCommandRecord()
    {
        Clear();
    }

    public MagicCommandRecord(long t, float dx, float dz, float spd, int idx)
    {
        time = t;
        dirX = dx;
        dirZ = dz;
        speed = spd;
        index = idx;
    }
    public void Clear()
    {
        time = 0;
        dirX = 0;
        dirZ = 0;
        speed = -1;
        index = -1;
    }
    public MagicCommandRecord Set(long t, float dx, float dz, float spd, int idx)
    {
        time = t;
        dirX = dx;
        dirZ = dz;
        speed = spd;
        index = idx;
        return this;
    }
}
