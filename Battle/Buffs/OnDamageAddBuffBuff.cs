using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public class OnDamageAddBuffBuff : BuffLogic
	{
		private const int Key_Int_AddBuffSummonObjId_BuffConfig = 0;
		private const int Key_Int_SummonObjBuffId_BuffConfig = 1;
		public override void OnDamageTarget(SkillObj self, SkillObj target, Damage damage, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			var buffIds = self.GetBuffIntParams(buffConfig).Skip(Key_Int_SummonObjBuffId_BuffConfig+1);
			foreach (var buffId in buffIds.Where(b=>b>0))
			{
				BattleModule.AddBuff(target, self, buffId, BattleReason.Buff);
			}
			return ;
		}
		public override void OnSummon(int id, SkillObj self, SkillObj summonObj, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			if(summonObj != null)
			{
				var summonId = self.GetBuffIntParam(buffConfig, Key_Int_AddBuffSummonObjId_BuffConfig);
				var buffId = self.GetBuffIntParam(buffConfig, Key_Int_SummonObjBuffId_BuffConfig);
				if(summonId == id)
				{
					BattleModule.AddBuff(summonObj, self, buffId, BattleReason.Buff);
				}
			}
		}
	}
}
