using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzServer.Battle
{
	[hzyBattleUndetermined]
	public class GFChangeBuff : BuffLogic
	{
		private const int Key_Int_GChangeType_BuffConfig = 0;
		private const int Key_Double_GChange_BuffConfig = 0;
		private const int Key_Int_FChangeType_BuffConfig = 1;
		private const int Key_Double_FChange_BuffConfig = 1;
		private const int Key_Int_GFChanged_BuffInfo = 0;
		public override void OnAttach(SkillObj self, SkillObj srcObj, BuffInfo_New buff, BuffConfig_New buffConfig) 
		//public override double OnDataFix(SkillObj self, PropertyType pType, double pValue, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		{
			if (self is Object.Tank)
			{
				Object.Tank tank = self as Object.Tank;
				//double pValue = tank.Spd2011;
				int GChangeType = self.GetBuffIntParam(buffConfig, Key_Int_GChangeType_BuffConfig);
				int FChangeType = self.GetBuffIntParam(buffConfig, Key_Int_FChangeType_BuffConfig);
				int bGFChanged = self.GetBuffIntParam(buff, Key_Int_GFChanged_BuffInfo);
				if (bGFChanged == 0)
				{
					self.SetBuffIntParam(buff, Key_Int_GFChanged_BuffInfo, 1);
					switch (GChangeType)
					{
						case 1:
							tank.AttAdd1004 += (float)self.GetBuffDoubleParam(buffConfig, Key_Double_GChange_BuffConfig);
							break;
						case 2:
							if (1 + (float)self.GetBuffDoubleParam(buffConfig, Key_Double_GChange_BuffConfig) != 0)
							{
								tank.AttMult1003 += (float)self.GetBuffDoubleParam(buffConfig, Key_Double_GChange_BuffConfig);
							}
							break;
					}
					switch (FChangeType)
					{
						case 1:
							tank.DefAdd1009 += (float)self.GetBuffDoubleParam(buffConfig, Key_Double_FChange_BuffConfig);
							break;
						case 2:
							if (1 + (float)self.GetBuffDoubleParam(buffConfig, Key_Double_FChange_BuffConfig) != 0)
							{
								tank.DefMult1008 += (float)self.GetBuffDoubleParam(buffConfig, Key_Double_FChange_BuffConfig);
							}
							break;
					}
				}

			}
			return ;
		}
		//public override void OnAttachBuff(SkillObj self, SkillObj srcObj, BuffInfo_New newBuff, BuffInfo_New buffInfo, BuffConfig_New buffConfig)
		//{
		//	if(self is Object.Tank)
		//	{
		//		Object.Tank tank = self as Object.Tank;
		//		//double pValue = tank.Spd2011;
		//		int speedChangeType = self.GetBuffIntParam(buffConfig, Key_Int_SpeedChangeType_SkillConfig);
		//		switch(speedChangeType)
		//		{
		//			case 1:
		//				tank.SpdAdd2010 += (float)self.GetBuffDoubleParam(buffConfig, Key_Double_SpeedChange_SkillConfig);
		//				break;
		//			case 2:
		//				if(1 + (float)self.GetBuffDoubleParam(buffConfig, Key_Double_SpeedChange_SkillConfig) != 0)
		//				{
		//					tank.SpdMult2009 *= 1 + (float)self.GetBuffDoubleParam(buffConfig, Key_Double_SpeedChange_SkillConfig);
		//				}
		//				break;
		//		}
		//	}

		//}
		public override void OnDetach(SkillObj self, SkillObj srcObj, BuffInfo_New buff, BuffConfig_New buffConfig)
		{
			if (self is Object.Tank)
			{
				Object.Tank tank = self as Object.Tank;
				//double pValue = tank.Spd2011;
				int GChangeType = self.GetBuffIntParam(buffConfig, Key_Int_GChangeType_BuffConfig);
				int FChangeType = self.GetBuffIntParam(buffConfig, Key_Int_FChangeType_BuffConfig);
				int bGFChanged = self.GetBuffIntParam(buff, Key_Int_GFChanged_BuffInfo);
				if (bGFChanged == 1)
				{
					switch (GChangeType)
					{
						case 1:
							tank.AttAdd1004 -= (float)self.GetBuffDoubleParam(buffConfig, Key_Double_GChange_BuffConfig);
							break;
						case 2:
							if (1 + (float)self.GetBuffDoubleParam(buffConfig, Key_Double_GChange_BuffConfig) != 0)
							{
								tank.AttMult1003 -= (float)self.GetBuffDoubleParam(buffConfig, Key_Double_GChange_BuffConfig);
							}
							break;
					}

					switch (FChangeType)
					{
						case 1:
							tank.DefAdd1009 -= (float)self.GetBuffDoubleParam(buffConfig, Key_Double_FChange_BuffConfig);
							break;
						case 2:
							if (1 + (float)self.GetBuffDoubleParam(buffConfig, Key_Double_FChange_BuffConfig) != 0)
							{
								tank.DefMult1008 -= (float)self.GetBuffDoubleParam(buffConfig, Key_Double_FChange_BuffConfig);
							}
							break;
					}
				}
			}
		}

	}
}
