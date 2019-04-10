using UnityEngine.UI;

namespace Game.Core
{
	/// <summary>
	/// 这是一个新的复用滚动列表
	/// 本文件主要是一些无法在扩展方法里实现的东西，但是与滚动列表核心部分又不那么紧密关联
	/// </summary>
	public sealed partial class GameScrollRect_V2 : ScrollRect
	{
		/// <summary>
		/// 移动到最下端
		/// </summary>
		public void SetToBottom()
		{
			if (!vertical)
			{
				//不能竖直方向移动的直接返回
				return;
			}
			//移动到最底端
			SetNormalizedPosition(0, 1);
			StopMovement();
		}
	}
}
