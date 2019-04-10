using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Core
{
	/// <summary>
	/// 这是一个新的复用滚动列表
	/// 这个文件的功能主要是计算各种位置与大小，和判断相交之类的
	/// </summary>
	public sealed partial class GameScrollRect_V2 : ScrollRect
	{
		/// <summary>
		/// 数据与位置类
		/// 保存了对应位置的数据与坐标位置
		/// 用来判断是否需要显示item
		/// </summary>
		private class DataPos
		{
			//本数据在列表中的位置
			public Int32 Index;
			//真正的数据
			public System.Object Data;
			//如果在视野范围内就指向对应的GO
			public GameObject VisableGO;
			//对内用来判断相交的Rect
			private Rect RectOverlap;
			//对外用来显示位置的Rect，与上边那个不一样，差一个Rect的高度
			public Rect RectPos { get; private set; }
			/// <summary>
			/// 设置进入视野的GO的Transform信息
			/// </summary>
			/// <param name="vGO"></param>
			public void SetGO(GameObject vGO)
			{
				if (vGO == null) return;
				VisableGO = vGO;
				var rectT = VisableGO.transform as RectTransform;
				rectT.pivot = rectT.anchorMin = rectT.anchorMax = new Vector2(0, 1);
				rectT.localScale = Vector3.one;
				rectT.localEulerAngles = Vector3.zero;
				//VisableGO.name = Index.ToString();
				rectT.anchoredPosition3D = Vector3.zero;
				rectT.anchoredPosition = RectPos.position;
			}
			/// <summary>
			/// 设置自己的两个Rect
			/// </summary>
			/// <param name="rect"></param>
			/// <param name="lt"></param>
			public void SetRect(Rect rect, LayoutType lt)
			{
				switch (lt)
				{
					case LayoutType.Grid:
						RectOverlap = new Rect(rect.x, rect.y - rect.height, rect.width, rect.height);
						break;
					case LayoutType.Horizontal:
						RectOverlap = new Rect(rect.x, rect.y - rect.height, rect.width, rect.height);
						break;
					case LayoutType.Vertical:
						RectOverlap = new Rect(rect.x, rect.y - rect.height, rect.width, rect.height);
						break;
				}
				RectPos = rect;
			}
			/// <summary>
			/// 判断是否相交
			/// </summary>
			/// <param name="otherData"></param>
			/// <returns></returns>
			public Boolean Overlaps(DataPos otherData)
			{
				return RectOverlap.Overlaps(otherData.RectOverlap);
			}
			/// <summary>
			/// 判断是否相交
			/// </summary>
			/// <param name="otherRect"></param>
			/// <returns></returns>
			public Boolean Overlaps(Rect otherRect)
			{
				return RectOverlap.Overlaps(otherRect);
			}
			public override String ToString()
			{
				return String.Format("index:{0},x:{1},y:{2},w:{3},h:{4}\n realPos:{5}", Index, RectOverlap.x, RectOverlap.y, RectOverlap.width, RectOverlap.height, RectPos);
			}
		}

		private float GetGridBlockSizeY() { return CellSize.y + SpacingSize.y; }
		private float GetGridBlockSizeX() { return CellSize.x + SpacingSize.x; }
		private Vector2 GetGridBlockSize() { return CellSize + SpacingSize; }

		/// <summary>
		/// 计算某一位置的item应该占多大
		/// </summary>
		/// <param name="dp"></param>
		/// <param name="column"></param>
		/// <param name="row"></param>
		/// <param name="lastItemRect"></param>
		/// <returns></returns>
		private Rect GetBlockRect(DataPos dp, Int32 column, Int32 row, Rect lastItemRect)
		{
			switch (LayoutMode)
			{
				case LayoutType.Grid:
					return new Rect(column * GetGridBlockSizeX(), -row * GetGridBlockSizeY(), CellSize.x, CellSize.y);
				case LayoutType.Horizontal:
				case LayoutType.Vertical:
					var size = Vector2.zero;
					if (CalcSizeAction != null)
					{
						size = CalcSizeAction.Invoke(dp.Data);
					}
					else
					{
						size = (ChildItem.transform as RectTransform).sizeDelta;
					}
					size += SpacingSize;
					return new Rect(LayoutMode == LayoutType.Horizontal ?
									(lastItemRect.position + new Vector2(lastItemRect.width, 0))
									:
									(lastItemRect.position + new Vector2(0, -lastItemRect.height))
									, size);
			}
			return Rect.zero;
		}
		/// <summary>
		/// 根据当前的数据，重新计算一下Content的大小
		/// </summary>
		private void RefreshContentSize()
		{
			if (!IsDPsChanged)
			{
				return;
			}
			IsDPsChanged = false;
			IsMoveDirty = true;
			if (DataAndPosProviders == null)
			{
				return;
			}
			var size = Vector2.zero;
			var dataCount = DataAndPosProviders.Count;
			if (dataCount > 0)
			{
				switch (LayoutMode)
				{
					case LayoutType.Grid:
						/// 如果要使用grid模式，理论上不允许size小于等于0，所以会自动设置到0.01f上
						CellSize = new Vector2(Mathf.Max(CellSize.x, 0.01f), Mathf.Max(CellSize.y, 0.01f));
						var cellSize = GetGridBlockSize();
						RowCount = 1;
						ColumnCount = 1;
						switch (GridConstraint)
						{
							case GridLayoutGroup.Constraint.Flexible:
								//优先排满一行
								var vp = viewport;
								ColumnCount = Mathf.Max(Mathf.FloorToInt(vp.rect.width / cellSize.x), 1);
								RowCount = Mathf.Max(Mathf.CeilToInt((Single)dataCount / ColumnCount), 1);
								var tmp = dataCount * cellSize;
								break;
							case GridLayoutGroup.Constraint.FixedColumnCount:
								ColumnCount = Mathf.Max(ConstraintCount, 1);
								RowCount = Mathf.Max(Mathf.CeilToInt((Single)dataCount / ColumnCount), 1);
								break;
							case GridLayoutGroup.Constraint.FixedRowCount:
								RowCount = Mathf.Max(ConstraintCount, 1);
								ColumnCount = Mathf.Max(Mathf.CeilToInt((Single)dataCount / RowCount), 1);
								break;
							default:
								Debug.LogError("InitScrollRect Error GridConstraint:" + GridConstraint);
								break;
						}
						size = new Vector2(cellSize.x * ColumnCount, cellSize.y * RowCount);
						break;
					case LayoutType.Vertical:
					case LayoutType.Horizontal:
						//水平和竖直布局就很随意了，按照item自己的size进行计算
						//如果有通过data计算CalcSizeAction方法就使用它进行计算
						var tmpMax = Vector2.zero;
						var tmpAll = Vector2.zero;
						if (CalcSizeAction != null)
						{
							var tmp = Vector2.zero;
							foreach (var data in DataAndPosProviders)
							{
								tmp = CalcSizeAction.Invoke(data.Data) + SpacingSize;
								tmpAll += new Vector2(tmp.x, tmp.y);
								tmpMax = new Vector2(Mathf.Max(tmpMax.x, tmp.x), Mathf.Max(tmpMax.y, tmp.y));
							}
						}
						else if (ChildItem != null)
						{
							var rectT = ChildItem.transform as RectTransform;
							tmpAll = new Vector2(dataCount * rectT.rect.width, dataCount * rectT.rect.height) + SpacingSize;
							tmpMax = new Vector2(rectT.rect.width, rectT.rect.height) + SpacingSize;
						}
						else
						{
							Debug.LogError("Error CalcSizeAction is null, and  Childitem is null too");
						}
						size = LayoutMode == LayoutType.Vertical ? new Vector2(tmpMax.x, tmpAll.y) : new Vector2(tmpAll.x, tmpMax.y);
						ColumnCount = LayoutMode == LayoutType.Vertical ? dataCount : 1;
						RowCount = LayoutMode == LayoutType.Vertical ? 1 : dataCount;
						break;
					default:
						Debug.LogError("InitScrollRect LayoutMode:" + LayoutMode);
						break;
				}
			}
			//Debug.LogError("Content size:" + size);
			var ct = this.content;
			ct.sizeDelta = size;
			var row = 1;
			var column = 1;
			DataPos tmpDP = null;
			var lastRect = Rect.zero;
			for (int i = 0; i < dataCount; ++i)
			{
				tmpDP = DataAndPosProviders[i];
				row = i / ColumnCount;
				column = i % ColumnCount;
				tmpDP.Index = i;
				var tmpRect = GetBlockRect(tmpDP, column, row, lastRect);
				tmpDP.SetRect(tmpRect, LayoutMode);
				lastRect = tmpRect;
				//Debug.LogError("data :" + i + " , " + tmpDP.ToString() + "," +tmpDP.Data.ToString());
			}
		}
		/// <summary>
		/// 判断所有数据，检查哪些item应该显示，哪些应该不显示
		/// </summary>
		private void CheckItemsPos()
		{
			//Debug.Log("CheckItemsPos:");
			if (viewport == null)
			{
				Debug.LogError("Error viewport is null,GameObject Name:" + gameObject.name);
				return;
			}
			GameObject tmp = null;
			var viewSize = viewport.rect.size;
			//这里我们要求content左上角相对view节点左上角的位移，现在默认view是content的父节点
			//1。anchoredPosition是pivot相对于&&&四个锚点的中心&&&的位移
			//我们要用三个向量加一起计算出偏移量
			var ctPivot2TopLeftOffset = new Vector2(-content.pivot.x, 1 - content.pivot.y) * content.rect.size;
			var ctParentTopLeft2Anchored = new Vector2(content.anchorMin.x + content.anchorMax.x, content.anchorMin.y + content.anchorMax.y - 2) * viewSize * 0.5f;
			var ctAnchoredPos = ctParentTopLeft2Anchored + ctPivot2TopLeftOffset + content.anchoredPosition;

			//Debug.LogError("PIVOT：" + (content.pivot * viewSize).ToString() + ",pos:" + content.anchoredPosition );
			Rect viewRect = new Rect(-ctAnchoredPos.x + ViewSizeMinExt.x, -viewSize.y - ctAnchoredPos.y + ViewSizeMinExt.y, viewSize.x - ViewSizeMinExt.x + ViewSizeMaxExt.x, viewSize.y - ViewSizeMinExt.y + ViewSizeMaxExt.y);
			foreach (var dp in DataAndPosProviders)
			{
				if (dp.Overlaps(viewRect))
				{
					if (dp.VisableGO == null)
					{
						tmp = GetEmptyChildItem(dp.Data);
						tmp.transform.SetParent(content);
						dp.SetGO(tmp);
						if (SetDataAction != null)
						{
							SetDataAction.Invoke(tmp, dp.Data);
#if UNITY_EDITOR
							tmp.name = "EditorRuntime_" + content.name + "_" + dp.Index.ToString();
#endif
						}
						else
						{
#if UNITY_EDITOR
							tmp.name = "NoSetDataAction_" + dp.Index.ToString();
#endif
						}
						//Debug.LogError(dp.Index+" In");
					}
					else
					{
						//暂时这么写，目前有个insert功能，可能导致已显示的GO位置不太正确，靠这个无脑刷新一下
						dp.SetGO(dp.VisableGO);
					}
				}
				else
				{
					if (dp.VisableGO != null)
					{
						tmp = dp.VisableGO;
						dp.VisableGO = null;
						RecycleChildItem(tmp, dp.Data);
						//Debug.LogError(dp.Index + " Out");
					}
				}
			}
		}

		[NonSerialized]
		private Coroutine TweenCoroutine = null;
		/// <summary>
		/// 移动列表使之能定位到给定数据的位置上
		/// 线性插值的移动，带结束回调
		/// </summary>
		/// <param name="obj">目标的GO</param>
		/// <param name="flyTime">飞行时间，负数就是直接设置过去</param>
		/// <param name="endCallback">结束时的回调</param>
		public void LocateAtTarget(GameObject obj, Single flyTime, HorizontalAlignType hAlignment = HorizontalAlignType.Left, VerticalAlignType vAlignment = VerticalAlignType.Upper, Action endCallback = null)
		{
			RefreshContentSize();
			foreach (var dp in DataAndPosProviders)
			{
				if (dp.VisableGO == obj)
				{
					LocateAtTarget(dp.Index, flyTime, hAlignment, vAlignment, endCallback);
					break;
				}
			}
		}

		/// <summary>
		/// 移动列表使之能定位到给定数据的位置上
		/// 线性插值的移动，带结束回调
		/// </summary>
		/// <param name="idx">目标index</param>
		/// <param name="flyTime">飞行时间，负数就是直接设置过去</param>
		/// <param name="endCallback">结束时的回调</param>
		public void LocateAtTarget(Int32 idx, Single flyTime, HorizontalAlignType hAlignment = HorizontalAlignType.Left, VerticalAlignType vAlignment = VerticalAlignType.Upper, Action endCallback = null)
		{
			if (idx < 0 || idx >= DataAndPosProviders.Count)
			{
				Debug.LogError("LocateAtTarget idx Error " + idx);
				return;
			}
			RefreshContentSize();
			if (TweenCoroutine != null)
			{
				StopCoroutine(TweenCoroutine);
				TweenCoroutine = null;
			}
			var ct = content;
			var vt = viewport;
			var tarDP = DataAndPosProviders[idx];
			//有负号
			Vector2 tarPos = ct.anchoredPosition;
			Vector2 alignOffset = Vector2.zero;
			if (vertical)
			{
				tarPos.y = -tarDP.RectPos.position.y;
				switch (vAlignment)
				{
					case VerticalAlignType.None:
					case VerticalAlignType.Upper:
						break;
					case VerticalAlignType.Lower:
						alignOffset.y = (tarDP.RectPos.size.y - vt.rect.size.y);
						break;
					case VerticalAlignType.Middle:
						alignOffset.y = (tarDP.RectPos.size.y - vt.rect.size.y) * 0.5f;
						break;
					default:
						throw new NotImplementedException("VerticalAlignType:" + vAlignment + " not Implemented");
				}
			}
			if (horizontal)
			{
				tarPos.x = -tarDP.RectPos.position.x;
				switch (hAlignment)
				{
					case HorizontalAlignType.None:
					case HorizontalAlignType.Left:
						break;
					case HorizontalAlignType.Right:
						alignOffset.x = (vt.rect.size.x - tarDP.RectPos.size.x);
						break;
					case HorizontalAlignType.Center:
						alignOffset.x = (vt.rect.size.x - tarDP.RectPos.size.x) * 0.5f;
						break;
					default:
						throw new NotImplementedException("HorizontalAlignType:" + hAlignment + " not Implemented");
				}
			}
			tarPos += alignOffset;
			//三种模式下设置的目标掉不太一样
			switch (movementType)
			{
				case MovementType.Unrestricted://无限制模式目标点不做修改
					break;
				case MovementType.Clamped://这个需要把目标点限制在范围内
				case MovementType.Elastic://惯性模式虽然可以先弹出viewport范围，但是没什么用，修改成与Clamped模式相同
					if (vertical)
					{
						tarPos.y = Mathf.Min(Mathf.Max(0, tarPos.y), Mathf.Max(ct.rect.height - vt.rect.height, 0));//TODO现在都是左上开始排列的玩法，才能用这个0做处理
					}
					if (horizontal)
					{
						tarPos.x = Mathf.Max(Mathf.Min(0, tarPos.x), Mathf.Min(vt.rect.width - ct.rect.width, 0));//TODO现在都是左上开始排列的玩法，才能用这个0做处理
					}
					break;
				default:
					Debug.LogError("new movementType found:" + movementType);
					break;
			}
			if (flyTime > 0)
			{
				TweenCoroutine = StartCoroutine(TweenMoveToPos(ct.anchoredPosition, tarPos, flyTime, endCallback));
			}
			else
			{
				ct.anchoredPosition = tarPos;
				if (endCallback != null)
				{
					endCallback.Invoke();
				}
			}
		}
		[NonSerialized]
		private Single SelfTweenMoveTime = 0;
		/// <summary>
		/// 线性插值的移动，带结束回调
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="v2Pos"></param>
		/// <param name="flyTime"></param>
		/// <param name="endCallback"></param>
		/// <returns></returns>
		private IEnumerator TweenMoveToPos(Vector2 pos, Vector2 v2Pos, Single flyTime, Action endCallback = null)
		{
			var running = true;
			var passedTime = 0f;
			if (flyTime == 0)
			{
				flyTime = 1;
			}
			SelfTweenMoveTime = Time.time + flyTime;
			while (running)
			{
				IsMoveDirty = false;
				yield return new WaitForEndOfFrame();
				passedTime += Time.deltaTime;
				Vector2 vCur;
				if (passedTime >= flyTime)
				{
					vCur = v2Pos;
					running = false;
					SelfTweenMoveTime = 0;
					velocity = Vector2.zero;
					if (endCallback != null)
					{
						endCallback.Invoke();
					}
					StopCoroutine(TweenCoroutine);
					TweenCoroutine = null;
				}
				else
				{
					vCur = Vector2.Lerp(pos, v2Pos, passedTime / flyTime);
				}
				content.anchoredPosition = vCur;
			}

		}
	}
}
