using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZzServer.Battle;
using System.Linq;

public class ShadowFollow
{
	private long LastTime;
	private SkillVector3 LastPos;
	private SkillVector3 LastDir;
	private float LastSpeed;

	private List<SkillVector3> FollowList = new List<SkillVector3>();

	public void InitShadow(long time, SkillVector3 pos, SkillVector3 dir, float shadowSpeed)
	{
		LastTime = time;
		LastPos = pos;
		LastDir = dir;
		LastSpeed = shadowSpeed;
		FollowList.Clear();
	}

	public SkillVector3 GetNextPos(long time, SkillVector3 shadowPos, SkillVector3 shadowDir, float shadowSpeed, List<SkillVector3> bkps, List<SkillVector3> tps)
	{
		long DeltaTime = time - LastTime;
		if (DeltaTime <= 0 || LastPos.Distance_Horizontal_Vertical(shadowPos) <= shadowSpeed * (time - LastTime))
		{
			InitShadow(time, shadowPos, shadowDir, shadowSpeed);
		}
		else
		{
			//这里说明位置距离比较大，需要加速追上影子,离影子越远速度就越大

			if (bkps.Count > 0)
			{
				FollowList.AddRange(bkps.Reverse<SkillVector3>());
			}
			if (tps.Count > 0)
			{
				FollowList.AddRange(tps);
			}
			if (FollowList.Count > 0)
			{
				//有的时候点比较多，中间还可能有重复经过的点，就把中间的部分直接干掉，省略一些路径
				FollowListSkip();

				//直接求位置，跳过求速度，但是会记录速度
				var dis = CalcAllDis();
				if (dis > shadowSpeed * DeltaTime)
				{
					var moveDis = shadowSpeed * 2 * DeltaTime;
					if (moveDis >= dis)
					{
						//Debug.LogError("if(moveDis >= dis) followed shadow :" + FollowList[FollowList.Count - 1]);
						LastDir *= 0;//需要计算一下
						LastSpeed = 0;
						LastPos = FollowList[FollowList.Count - 1];
						FollowList.Clear();
					}
					else
					{
						//Debug.LogError("move " + moveDis + " Alldis:" + dis + " DeltaTime:" + DeltaTime);
						LastDir *= 0;//需要计算一下
						LastSpeed = 0;
						LastPos = Move(moveDis);
					}
				}
				else
				{
					//Debug.LogWarning("followed shadow :" + FollowList[FollowList.Count - 1]);
					LastDir *= 0;//需要计算一下
					LastSpeed = 0;
					LastPos = FollowList[FollowList.Count - 1];
					FollowList.Clear();
				}
			}
			else
			{
				Debug.LogError("! if (FollowList.Count > 0)");
			}


			LastTime = time;
		}
		return LastPos;
	}
//	static int mm = 0;
	public SkillVector3 Move(double dis)
	{
// 		mm++;
// 		foreach (var tmp in FollowList)
// 		{
// 			Debug.LogError(mm + ":Next Pos:" + tmp.ToString());
// 		}

		var pos = LastPos;
		for (var i = 0; i < FollowList.Count;)
		{
			var tmp = FollowList[i];
			var lineDis = tmp.Distance_Horizontal_Vertical(pos);
			if (lineDis < dis)
			{
				Debug.LogError(pos.ToString() + "------->move over :" + tmp.ToString());
				FollowList.RemoveAt(i);
				dis -= lineDis;
				pos = tmp;
			}
			else
			{
				var tmpPos = pos;
				pos = SpecialNormalize(tmp - pos) * (float)dis + pos;
				//Debug.LogError(tmpPos.ToString() + "------->move end :" + pos.ToString());
				break;
			}
		}
		return pos;
	}
	public SkillVector3 SpecialNormalize(SkillVector3 input)
	{
		input.x = input.x == 0 ? 0 : input.x / Mathf.Abs(input.x);
		input.y = input.y == 0 ? 0 : input.y / Mathf.Abs(input.y);
		input.z = input.z == 0 ? 0 : input.z / Mathf.Abs(input.z);
		return input;
	}
	public void FollowListSkip()
	{
		//现在并不省略，只是检查一下是否有连续2个相同的点出现，删除掉
		for (var i = 0; i < FollowList.Count - 1;)
		{
			var front = FollowList[i];
			var back = FollowList[i + 1];
			float decimalPartX = front.x - Mathf.Floor(front.x);
			float decimalPartZ = front.z - Mathf.Floor(front.z);
			if (decimalPartX != MagicMoveRecord.CenterPoint || decimalPartZ != MagicMoveRecord.CenterPoint)
			{
				FollowList.RemoveAt(i);
				//Debug.LogError("not MagicMoveRecord.CenterPoint Remove follow point:" + front.ToString());
			}
			else if (front.Equals(back))
			{
				FollowList.RemoveAt(i);
				//Debug.LogError("Remove follow point:" + back.ToString());
			}
			else
			{
				i++;
			}
		}
	}
	public double CalcAllDis()
	{
		double ret = 0;
		var tmp = LastPos;
		for (var i = 0; i < FollowList.Count; i++)
		{
			var front = FollowList[i];
			//Debug.LogWarning("Alldis Some:" + front.Distance_Horizontal_Vertical(tmp) + " :" + tmp.ToString() + "------->" + front.ToString());
			ret += front.Distance_Horizontal_Vertical(tmp);
			tmp = front;
		}
		return ret;
	}
}
