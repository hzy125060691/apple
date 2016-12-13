using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleBase]
	public interface DamageBuff_BuffLogic
	{ 
		int GetMaxHP();
	}
	[hzyBattleBase]
	public class DamageBuff : BuffLogic
	{
		private const int Key_Int_DamageType_SkillConfig = 0;
		private const int Key_Double_DamageValue_SkillConfig = 0;

		public override void OnEffect(SkillObj self, SkillObj tarObj, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			int damageType = self.GetBuffIntParam(buffConfig, Key_Int_DamageType_SkillConfig);
			double damageValue = self.GetBuffDoubleParam(buffConfig, Key_Double_DamageValue_SkillConfig);
			switch (damageType)
			{
				case 1:
					if (damageValue > 0)
					{
						Damage damage = BattleModule.CreateDamage((int)damageValue, self.GetSrcID(buffInfo), eReason: BattleReason.Buff);
						BattleModule.DamageTarget(tarObj, self, damage);
					}
					break;
				case 2:
					if (damageValue > 0)
					{
						Damage damage = BattleModule.CreateDamage((int)damageValue * tarObj.GetMaxHP() / 10000, self.GetSrcID(buffInfo), eReason: BattleReason.Buff);
						BattleModule.DamageTarget(tarObj, self, damage);
					}
					break;
			}
			// 			Damage damage = BattleModule.CreateDamage((int)damageType, self.GetSrcID(buffInfo), eReason: BattleReason.Buff);
			// 			BattleModule.DamageTarget(tarObj, self, damage);

		}
	}
}
