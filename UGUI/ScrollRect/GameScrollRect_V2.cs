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
	[ExecuteInEditMode]
	public sealed partial class GameScrollRect_V2 : ScrollRect
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
		//水平对齐方式
		public enum HorizontalAlignType
		{
			None = 0,
			Left,
			Center,
			Right
		}
		//竖直对齐方式
		public enum VerticalAlignType
		{
			None = 0,
			Upper,
			Middle,
			Lower
		}
		[SerializeField]
		public RectTransform.Edge FirstEdge = RectTransform.Edge.Right;
		[SerializeField]
		public RectTransform.Edge SecondEdge = RectTransform.Edge.Bottom;
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
		[NonSerialized]
		private Boolean IsDPsChanged = false;//DataAndPosProviders列表是否有变化
		[NonSerialized]
		private Boolean IsMoveDirty = true;

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

			SetDPsChanged();
		}

		#region Start Awake OnEnable OnDisable LateUpdate等等
		protected sealed override void Start()
		{
			base.Start();
			//这个空的节点必须有，如果没有指定，那么自己创建一个
			if (EmptyRoot == null)
			{
				GameObject empty = new GameObject("EmptyRecycle");
				empty.transform.SetParent(viewport ?? this.transform);
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
		/// 修改一点父类的逻辑，本类自己TweenMove的时候不执行父类的移动
		/// 还有就是判断本类哪个子节点应该显示
		/// </summary>
		protected sealed override void LateUpdate()
		{
			if (Time.time >= SelfTweenMoveTime)
			{
				base.LateUpdate();
			}
			RefreshContentSize();
			if (IsMoveDirty || Time.time < SelfTweenMoveTime)
			{
				//Debug.Log("LateUpdate Dirty:" + this.name);
				IsMoveDirty = false;
				CheckItemsPos();
			}
		}

		#endregion

		/// <summary>
		/// 拖动Scrollbar但是不直接拖拽列表时会有用的一个方法
		/// </summary>
		/// <param name="val"></param>
		private void _ScrollRect_OnValueChanged(Vector2 val)
		{
			//Debug.LogError("_ScrollRect_OnValueChanged:" + val);
			IsMoveDirty = true;
		}
		#region Recycle GetEmpty
		/// <summary>
		/// 回收所有
		/// </summary>
		private void RecycleAll()
		{
			RefreshContentSize();
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
			RefreshContentSize();
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
		#endregion
		#region 基础的增删改查方法
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
			SetDPsChanged();
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
			RefreshContentSize();
			return DataAndPosProviders[index].RectReal.position;
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
				RefreshContentSize();
				var count = DataAndPosProviders.Count;
				DataPos dp = null;
				for (var i = 0; i < count; i++)
				{
					dp = DataAndPosProviders[i];
					if (dp.VisableGO == go)
					{
						ret = dp.RectReal.position;
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
				RefreshContentSize();
				var count = DataAndPosProviders.Count;
				DataPos dp = null;
				for (var i = 0; i < count; i++)
				{
					dp = DataAndPosProviders[i];
					if (dp.Data == data)
					{
						ret = dp.RectReal.position;
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
			SetDPsChanged();
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
				RefreshContentSize();
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
				RefreshContentSize();
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
			RefreshContentSize();
			var dp = DataAndPosProviders[index];
			if(dp.VisableGO != null)
			{
				RecycleChildItem(dp.VisableGO, dp.Data);
				dp.VisableGO = null;
			}
			DataAndPosProviders.RemoveAt(index);
			SetDPsChanged();
		}
		/// <summary>
		/// 列表删除数据
		/// </summary>
		/// <param name="data"></param>
		public void RemoveData(System.Object data)
		{
			if (data != null && DataAndPosProviders != null)
			{
				RefreshContentSize();
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
					SetDPsChanged();
					break;
				}
			}
		}
		#endregion
#if UNITY_EDITOR
		public void ForceRefresh()
		{
			SetDPsChanged();
		}
#endif
		private void SetDPsChanged()
		{
			IsDPsChanged = true;
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
	}
}
