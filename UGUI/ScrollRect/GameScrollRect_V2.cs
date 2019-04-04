using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.Core
{

	/// <summary>
	/// 这是一个新的复用滚动列表
	/// </summary>
	public class GameScrollRect_V2 : ScrollRect
	{
		/// <summary>
		/// item的布局方式
		/// </summary>
		public enum LayoutType
		{
			Grid,//使用Grid参数设置大小
			Horizontal,//使用子对象的宽度来设置
			Vertical//使用子对象的高度来设置
		}
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
		//生成的列表的布局方式
		[SerializeField]
		public LayoutType LayoutMode = LayoutType.Grid;
		#region Grid参数
		//Grid布局下的约束，与GridLayoutGroup基本一致
		[SerializeField]
		public GridLayoutGroup.Constraint GridConstraint = GridLayoutGroup.Constraint.Flexible;
		//GRID约束的数量
		[SerializeField]
		public Int32 ConstraintCount = 1;
		/// <summary>
		/// 如果要使用grid模式，理论上不允许size小于等于0，所以会自动设置到0.01f上
		/// </summary>
		[SerializeField]
		public Vector2 CellSize;
		[SerializeField]
		public Vector2 SpacingSize;//todo这个还没加入，还可以外带个padding
		[SerializeField]
		public Vector2 ViewSizeMinExt;//有的时候不并想超出框就移除，可以在viewsize的四个边缘方向增加一定大小
		[SerializeField]
		public Vector2 ViewSizeMaxExt;//有的时候不并想超出框就移除，可以在viewsize的四个边缘方向增加一定大小
		#endregion

		//如果没有从外部指定GetEmptyItemAction，那么就不停的实例化childitem去当新的列表项目
		[SerializeField]
		public GameObject ChildItem = null;
		//RecycleAction如果不为空，这个就是回收后的item的父节点
		[SerializeField]
		public Transform EmptyRoot;
		//这个是运行时所有的数据，有序的List
		[NonSerialized]
		private List<DataPos> DataAndPosProviders = new List<DataPos>();

		//这个是RecycleAction为null时，回收的item存放的地方
		[NonSerialized]
		private List<GameObject> EmptyChildItems = new List<GameObject>();

		//运行时列表的行数
		[NonSerialized]
		private Int32 RowCount = 1;
		//运行时列表的列数
		[NonSerialized]
		private Int32 ColumnCount = 1;

		#region 外部传进来的可以修改一些逻辑的方法
		//如果不用本类自己的缓存和回收方法，可以自己从外部传别的方法进来
		//这个一般来说都是需要从外部传进来的，根据data设置GO的对应状态的
		[NonSerialized]
		private Action<GameObject, System.Object> SetDataAction = null;
		//根据data清理GO的对应状态的
		[NonSerialized]
		private Action<GameObject, System.Object> ClearDataAction = null;
		//通过GameObject。Instance实例化childitem后的回调
		[NonSerialized]
		private Action<GameObject, System.Object> InstancePostAction = null;
		//回收item时需要调用的函数，如果不为null，请自己更改父节点
		[NonSerialized]
		private Action<GameObject, System.Object> RecycleAction = null;
		//获得一个空的item的方法，如果为null，去实例化childitem
		[NonSerialized]
		private Func<System.Object, GameObject> GetEmptyItemAction = null;
		/// <summary>
		/// 这个方法在LayoutType.Grid模式下不会使用
		/// 用来计算GO应该占的大小，在那种data种类不唯一，或者GO种类不唯一的情况
		/// </summary>
		[NonSerialized]
		private Func<System.Object, Vector2> CalcSizeAction = null;
		#endregion
		#region Drag相关方法，可以自己设置3个拖拽类的回调实现一些特别的功能
		[NonSerialized]
		private Action<PointerEventData> OnBeginDragAction = null;
		[NonSerialized]
		private Action<PointerEventData> OnDragAction = null;
		[NonSerialized]
		private Action<PointerEventData> OnEndDragAction = null;
		/// <summary>
		/// 设置拖拽类回调，不是必须调用
		/// </summary>
		/// <param name="beginAction"></param>
		/// <param name="dragAction"></param>
		/// <param name="endAction"></param>
		public void SetDragAction(Action<PointerEventData> beginAction = null, Action<PointerEventData> dragAction = null, Action<PointerEventData> endAction = null)
		{
			OnBeginDragAction = beginAction;
			OnDragAction = dragAction;
			OnEndDragAction = endAction;
		}
		public override void OnBeginDrag(PointerEventData eventData)
		{
			base.OnBeginDrag(eventData);
			//Debug.LogError("OnBeginDrag:" + this.name + "," + eventData.ToString());
			if (OnBeginDragAction != null)
			{
				OnBeginDragAction.Invoke(eventData);
			}
		}
		public override void OnDrag(PointerEventData eventData)
		{
			base.OnDrag(eventData);
			//Debug.LogError("OnDrag:" + this.name + "," + eventData.ToString());
			if (OnDragAction != null)
			{
				OnDragAction.Invoke(eventData);
			}
		}

		public override void OnEndDrag(PointerEventData eventData)
		{
			base.OnEndDrag(eventData);
			//Debug.LogError("OnEndDrag:" + this.name + "," + eventData.ToString());
			if (OnEndDragAction != null)
			{
				OnEndDragAction.Invoke(eventData);
			}
		}
		#endregion
		protected sealed override void Start()
		{
			base.Start();
			//这个空的节点必须有，如果没有指定，那么自己创建一个
			if (EmptyRoot == null)
			{
				GameObject empty = new GameObject("EmptyRecycle");
				empty.transform.SetParent(viewport);
				RectTransform emptyrect = empty.AddComponent<RectTransform>();
				emptyrect.sizeDelta = new Vector2(0f, 0f);
				emptyrect.localScale = Vector3.one;
				emptyrect.localEulerAngles = Vector3.zero;
				EmptyRoot = emptyrect;
				empty.SetActive(false);
			}
		}
		/// <summary>
		/// 这个是为了适应Scrollbar设置的
		/// </summary>
		protected sealed override void OnEnable()
		{
			base.OnEnable();
			// when the scroller is disabled, remove the listener
			onValueChanged.AddListener(_ScrollRect_OnValueChanged);

	#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return;
			}
	#endif
			if (EmptyRoot != null && EmptyRoot.gameObject.activeSelf)
			{
				EmptyRoot.gameObject.SetActive(false);
			}
		}
		/// <summary>
		/// 这个是为了适应Scrollbar设置的
		/// </summary>
		protected sealed override void OnDisable()
		{
			base.OnDisable();
			// when the scroller is disabled, remove the listener
			onValueChanged.RemoveListener(_ScrollRect_OnValueChanged);
		}
		/// <summary>
		/// 拖动Scrollbar但是不直接拖拽列表时会有用的一个方法
		/// </summary>
		/// <param name="val"></param>
		private void _ScrollRect_OnValueChanged(Vector2 val)
		{
			//Debug.LogError("_ScrollRect_OnValueChanged:" + val);
			bMoveDirty = true;
		}
		public GameScrollRect_V2()
		{
			EmptyChildItems.Clear();
		}
		/// <summary>
		/// 初始化的方法，理论上没有哪个参数是必须的……但是建议setDataAction都设置好
		/// 注：视野指的是viewport的范围
		/// </summary>
		/// <param name="setDataAction">这个主要是用来item进入视野时，需要设置相关信息的回调</param>
		/// <param name="clearDataAction">这个是item出视野时进行一些东西的清理的回调用</param>
		/// <param name="instancePostAction">这个是在调用GameObject.Instantiate childitem后调用的回调（实例化后处理），
		///									当有GetEmptyItemAction的时候不会回调改方法</param>
		/// <param name="recycleAction">当一个item出视野后回收可复用item时的回调，如果设置则不会自动处理item的回收，设置父节点等操作；如果不设置默认把item放到EmptyRoot节点下</param>
		/// <param name="getEmptyItemAction">这是一个获得item的方法，如果不设置，就从自己的emptylist中获得item，如果list里也没有就GameObject.Instantiate childitem</param>
		/// <param name="calcSizeAction">这是一个计算每个item大小的方法，可以针对不data显示出不同大小，LayoutType.Grid模式下无效</param>
		public void InitScrollRect(Action<GameObject, System.Object> setDataAction, Action<GameObject, System.Object> clearDataAction, Action<GameObject, System.Object> instancePostAction, Action<GameObject, System.Object> recycleAction, Func<System.Object, GameObject> getEmptyItemAction, Func<System.Object, Vector2> calcSizeAction = null)
		{
			SelfTweenMoveTime = 0;

			RecycleAll();
	#if UNITY_EDITOR
			if (setDataAction == null)
			{
				Debug.LogWarning("setDataAction == null, Are you OK?");
			}
			if (clearDataAction == null)
			{
				Debug.LogWarning("clearDataAction == null, Are you OK?");
			}
	#endif
			SetDataAction = setDataAction;
			ClearDataAction = clearDataAction;
			InstancePostAction = instancePostAction;
			RecycleAction = recycleAction;
			GetEmptyItemAction = getEmptyItemAction;
			CalcSizeAction = calcSizeAction;

			RowCount = 1;
			ColumnCount = 1;
			DataAndPosProviders.Clear();

			RefreshContentSize();
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
		/// 回收所有
		/// </summary>
		private void RecycleAll()
		{
			foreach (var dp in DataAndPosProviders)
			{
				if (dp.VisableGO != null)
				{
					RecycleChildItem(dp.VisableGO, dp.Data);
				}
			}
		}
		/// <summary>
		/// 回收childitem
		/// </summary>
		/// <param name="item"></param>
		private void RecycleChildItem(GameObject item, System.Object data)
		{
			if (ClearDataAction != null)
			{
				ClearDataAction.Invoke(item, data);
			}
			if (RecycleAction != null)
			{
				RecycleAction.Invoke(item, data);
			}
			else
			{
				item.transform.SetParent(EmptyRoot);
				EmptyChildItems.Add(item);
			}
		}
		/// <summary>
		/// 获得一个空闲的childitem
		/// 没有空闲的就创建一个
		/// </summary>
		/// <returns></returns>
		private GameObject GetEmptyChildItem(System.Object data)
		{
			GameObject ret = null;
			if (GetEmptyItemAction != null)
			{
				return GetEmptyItemAction.Invoke(data);
			}
			else
			{
				if (EmptyChildItems.Count > 0)
				{
					ret = EmptyChildItems[0];
					EmptyChildItems.RemoveAt(0);
					return ret;
				}
				if (ChildItem == null)
				{
					return null;
				}
				ret = GameObject.Instantiate(ChildItem);
				if (InstancePostAction != null)
				{
					InstancePostAction.Invoke(ret, data);
				}
			}
			return ret;
		}
		/// <summary>
		/// 向列表添加数据
		/// </summary>
		/// <param name="data"></param>
		/// <param name="index">指定位置</param>
		public void InsertData(System.Object data, Int32 index)
		{
			if (data != null && DataAndPosProviders != null)
			{
				DataAndPosProviders.Insert(index, new DataPos() { Data = data });
			}
			//这个可以不用立刻执行
			RefreshContentSize();
		}
		/// <summary>
		/// 获得某个item的位置
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public Vector2 GetPos(Int32 index)
		{
			if (index < 0 || index >= DataAndPosProviders.Count)
			{
				return Vector2.negativeInfinity;
			}
			return DataAndPosProviders[index].RectPos.position;
		}
		/// <summary>
		/// 获得某个item的位置
		/// </summary>
		/// <param name="go"></param>
		/// <returns></returns>
		public Boolean TryGetPos(GameObject go, out Vector2 ret)
		{
			if (go != null && DataAndPosProviders != null)
			{
				var count = DataAndPosProviders.Count;
				DataPos dp = null;
				for (var i = 0; i < count; i++)
				{
					dp = DataAndPosProviders[i];
					if (dp.VisableGO == go)
					{
						ret = dp.RectPos.position;
						return true;
					}
				}
			}
			ret = Vector2.negativeInfinity;
			return false;
		}
		/// <summary>
		/// 获得某个item的位置
		/// </summary>
		/// <param name="go"></param>
		/// <returns></returns>
		public Boolean TryGetPos(System.Object data, out Vector2 ret)
		{
			if (data != null && DataAndPosProviders != null)
			{
				var count = DataAndPosProviders.Count;
				DataPos dp = null;
				for (var i = 0; i < count; i++)
				{
					dp = DataAndPosProviders[i];
					if (dp.Data == data)
					{
						ret = dp.RectPos.position;
						return true;
					}
				}
			}
			ret = Vector2.negativeInfinity;
			return false;
		}
		/// <summary>
		/// 向列表添加数据
		/// </summary>
		/// <param name="data"></param>
		public void AddData(System.Object data)
		{
			if (data != null && DataAndPosProviders != null)
			{
				DataAndPosProviders.Add(new DataPos() { Data = data });
			}
			//这个可以不用立刻执行
			RefreshContentSize();
		}

		/// <summary>
		/// 获得一个GO对应的index
		/// </summary>
		/// <param name="go"></param>
		/// <returns></returns>
		public Int32 GetGameObjectIndex(GameObject go)
		{
			if (go != null && DataAndPosProviders != null)
			{
				var count = DataAndPosProviders.Count;
				DataPos dp = null;
				for (var i = 0; i < count; i++)
				{
					dp = DataAndPosProviders[i];
					if (dp.VisableGO == go)
					{
						return i;
					}
				}
			}
			return -1;
		}
		/// <summary>
		/// 获得一个data对应的index
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public Int32 GetDataIndex(System.Object data)
		{
			if (data != null && DataAndPosProviders != null)
			{
				var count = DataAndPosProviders.Count;
				DataPos dp = null;
				for (var i = 0; i < count; i++)
				{
					dp = DataAndPosProviders[i];
					if (dp.Data == data)
					{
						return i; 
					}
				}
			}
			return -1;
		}
		/// <summary>
		/// 列表删除数据
		/// </summary>
		/// <param name="data"></param>
		public void RemoveDataAt(Int32 index)
		{
			if (index < 0 || index >= DataAndPosProviders.Count)
			{
				Debug.LogError("RemoveDataAt idx Error " + index);
				return;
			}
			var dp = DataAndPosProviders[index];
			if(dp.VisableGO != null)
			{
				RecycleChildItem(dp.VisableGO, dp.Data);
				dp.VisableGO = null;
			}
			DataAndPosProviders.RemoveAt(index);
			//这个可以不用立刻执行
			RefreshContentSize();
		}
		/// <summary>
		/// 列表删除数据
		/// </summary>
		/// <param name="data"></param>
		public void RemoveData(System.Object data)
		{
			if (data != null && DataAndPosProviders != null)
			{
				var count = DataAndPosProviders.Count;
				DataPos dp = null;
				for (var i = 0; i < count; i++)
				{
					dp = DataAndPosProviders[i];
					if(dp.Data != data)
					{
						continue;
					}
					if (dp.VisableGO != null)
					{
						RecycleChildItem(dp.VisableGO, dp.Data);
						dp.VisableGO = null;
					}
					DataAndPosProviders.Remove(dp);
					//这个可以不用立刻执行
					RefreshContentSize();
					break;
				}
			}
		}
		/// <summary>
		/// 根据当前的数据，重新计算一下Content的大小
		/// </summary>
		private void RefreshContentSize()
		{
			bMoveDirty = true;
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
		/// 默认的迭代模式，只遍历视野范围内的GO
		/// </summary>
		/// <returns></returns>
		public IEnumerator<GameObject> GetEnumerator()
		{
			GameObject ret = null;
			var itor = DataAndPosProviders.GetEnumerator();
			while (itor.MoveNext())
			{
				ret = itor.Current.VisableGO;
				if (ret != null)
				{
					yield return ret;
				}
			}
			yield break;
		}
		[NonSerialized]
		private Boolean bMoveDirty = true;
		/// <summary>
		/// 修改一点父类的逻辑，本类自己TweenMove的时候不执行父类的移动
		/// 还有就是判断本类哪个子节点应该显示
		/// </summary>
		protected sealed override void LateUpdate()
		{
			if (Time.time >= SelfTweenMoveTime)
			{
				base.LateUpdate();
			}
			if (bMoveDirty)
			{
				//Debug.Log("LateUpdate Dirty:" + this.name);
				bMoveDirty = false;
				CheckItemsPos();
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
		public void LocateAtTarget(GameObject obj, Single flyTime, Action endCallback = null)
		{
			foreach (var dp in DataAndPosProviders)
			{
				if (dp.VisableGO == obj)
				{
					LocateAtTarget(dp.Index, flyTime, endCallback);
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
		public void LocateAtTarget(Int32 idx, Single flyTime, Action endCallback = null)
		{
			if (idx < 0 || idx >= DataAndPosProviders.Count)
			{
				Debug.LogError("LocateAtTarget idx Error " + idx);
				return;
			}
			if (TweenCoroutine != null)
			{
				StopCoroutine(TweenCoroutine);
				TweenCoroutine = null;
			}
			//有负号
			Vector2 tarPos = -DataAndPosProviders[idx].RectPos.position;
			var ct = content;
			var vt = viewport;
			//三种模式下设置的目标掉不太一样
			switch (movementType)
			{
				case MovementType.Unrestricted://无限制模式目标点不做修改
					break;
				case MovementType.Clamped://这个需要把目标点限制在范围内
				case MovementType.Elastic://惯性模式虽然可以先弹出viewport范围，但是没什么用，修改成与Clamped模式相同
					if (vertical)
					{
						tarPos.y = Mathf.Min(Mathf.Max(0, tarPos.y), ct.rect.height - vt.rect.height);
					}
					if (horizontal)
					{
						tarPos.x = Mathf.Max(Mathf.Min(0, tarPos.x),  vt.rect.width - ct.rect.width);
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
				content.anchoredPosition = tarPos;
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
		protected IEnumerator TweenMoveToPos(Vector2 pos, Vector2 v2Pos, Single flyTime, Action endCallback = null)
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
				bMoveDirty = false;
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
