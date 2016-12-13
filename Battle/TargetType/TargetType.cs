using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public interface SkillObjTargetType
	{
		int GetCamp();
	}
	[hzyBattleBase]
	public class TargetType
	{
		public virtual bool IsTarget(SkillObj srcObj, SkillObj tarObj)
		{
			return false;
		}

		public bool IsEnemy(int camp1, int camp2)
		{
			if (camp1 < 0 || camp2 < 0)
			{
				return false;
			}
			if (camp1 != camp2)
			{
				return true;
			}
			return false;
		}
		public bool IsFriend(int camp1, int camp2)
		{
			if (camp1 < 0 || camp2 < 0)
			{
				return false;
			}
			if (camp1 == camp2)
			{
				return true;
			}
			return false;
		}
	}
}
