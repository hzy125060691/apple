using System;

namespace ZzServer.Battle
{
	/// <summary>
	/// 这是战斗逻辑中未定部分，可以看情况处理带该特性的代码
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Method
		, Inherited = false, AllowMultiple = false)]
	public class hzyBattleUndeterminedAttribute : Attribute
	{

	}
}