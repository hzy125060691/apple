using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core
{
    // 添加到AddCompentMenu中
    [AddComponentMenu("游戏UI/GameScrollRect")]
    public class GameScrollRect : ScrollRect
    {

        //自动移动相关属性
        static bool m_isMoveStop = false;
        static bool isRight = false;
        //System.Action m_callBack = null;
        float childItemWidth = 0f;
        //float childItemHeight = 0f;
        int childCount = 0;
        int currIndex = 0;
        int doubleChildCount = 0;
        GameObject isStop = null;  //当该对象显示时，则停止滑动

        //
        List<GameToggle> pointList = new List<GameToggle>();

        #region 编辑器代码 开始
#if UNITY_EDITOR
        protected sealed override void Reset()
        {
            base.Reset();
            ChildItem = null;
            LeftArrowBtn = null;
            RightArrowBtn = null;
            AutoMoveIntervalsTimes = 0;
        }
#endif
        #endregion 编辑器代码 结束

        [SerializeField]
        public GameObject ChildItem = null;
        [SerializeField]
        public GameButton LeftArrowBtn = null;
        [SerializeField]
        public GameButton RightArrowBtn = null;
        [SerializeField]
        public float AutoMoveIntervalsTimes = 3.0f;
        [SerializeField]
        public GameObject PointListObj = null;
        [SerializeField]
        public GameObject PointObj = null;
        /// <summary>
        /// 实例化一个子对象
        /// </summary>
        /// <typeparam name="T">Item上面挂着的脚本类型</typeparam>
        /// <param name="parent">父节点</param>
        /// <param name="list">存放生成的Item上面的脚本</param>
        /// <returns></returns>
        public T  CreateChildItem<T>(Transform parent,int index = -1) where T : BaseUICom
        {          
            if (ChildItem == null) return null;
            GameObject obj = GameObject.Instantiate(ChildItem) as GameObject;
            if (obj != null)
            {
                if (index >= 0)
                {
                    obj.name = "Item" + index.ToString();
                }                
                obj.transform.SetParent(parent);
                obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = Vector3.zero;
                T ctrl = obj.GetComponent<BaseUICom>() as T;
                if(ctrl == null)
                {
                    Debug.LogError("GameScrollRect ChildItem BaseUICom is Null!");
                }
                else
                {                   
                    return ctrl;
                }                
            }
            return null;
        }

        //注册滑动列表的点击箭头按钮
        public void RegisterArrowClick(bool isBtn = false,System.Action callBack = null)
        {
            if (LeftArrowBtn == null || RightArrowBtn == null)
                return;
            InitData();
            SetArrowShow();
            if (isBtn)
            {
                LeftArrowBtn.onClick = () => { OnArrowClick(false, callBack); };
                RightArrowBtn.onClick = () => { OnArrowClick(true, callBack); };
            }                  
        }
        private void SetArrowShow()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            LeftArrowBtn.gameObject.SetActive(false);
            RightArrowBtn.gameObject.SetActive(false);
            ResetArrowShow();
            onValueChanged.AddListener((e) =>{ ResetArrowShow();});
        }

        private void ResetArrowShow()
        {
            if (content.rect.width > viewRect.rect.width)
            {
                if (horizontalNormalizedPosition <= 0)
                {
                    LeftArrowBtn.gameObject.SetActive(false);
                    RightArrowBtn.gameObject.SetActive(true);
                }
                else if (horizontalNormalizedPosition >= 1)
                {
                    LeftArrowBtn.gameObject.SetActive(true);
                    RightArrowBtn.gameObject.SetActive(false);
                }
                else
                {
                    LeftArrowBtn.gameObject.SetActive(true);
                    RightArrowBtn.gameObject.SetActive(true);
                }
            }
            else
            {
                LeftArrowBtn.gameObject.SetActive(false);
                RightArrowBtn.gameObject.SetActive(false);
            }
        }
        private void OnArrowClick(bool isRightMove,System.Action callBack)
        {
            float newX = 0;
            if (isRightMove)
            {
                newX = content.anchoredPosition.x - childItemWidth;
            }
            else
            {
                newX = content.anchoredPosition.x + childItemWidth;
            }
            currIndex++;
            Vector2 newVec = new Vector2(newX, content.anchoredPosition.y);
            DOTween.To(() => content.anchoredPosition, x => content.anchoredPosition = x, newVec, 0.5f);

            if (callBack != null)
                callBack.Invoke();
        }

        private void InitData()
        {
            RectTransform childRect = ChildItem.GetComponent<RectTransform>();
            childItemWidth = childRect.rect.width;
            //childItemHeight = childRect.rect.height;
            currIndex = 0;            
        }

        private void InitPointList()
        {
            if (pointList != null && pointList.Count > 0)
            {
                for (int i = 0; i < pointList.Count; i++)
                {
                    pointList[i].gameObject.SetActive(false);
                }                
            }

            if (PointObj != null && PointListObj != null)
            {
                childCount = content.childCount;
                GameObject point = null;
                for (int i = 0; i < childCount; i++)
                {
                    if (i >= pointList.Count)
                    {
                        point = GameObject.Instantiate(PointObj) as GameObject;
                    }
                    else
                    {
                        point = pointList[i].gameObject;
                    }
                    if (point != null)
                    {
                        point.name = i.ToString();
                        point.transform.SetParent(PointListObj.transform);
                        point.transform.localScale = Vector3.one;
                        point.transform.localPosition = Vector3.zero;
                        point.SetActive(true);
                        GameToggle toggle = point.GetComponent<GameToggle>();
                        if (toggle != null)
                        {
                            pointList.Add(toggle);
                            //int index = i;
                            //toggle.onValueChanged.AddListener((bool value) => OnToggleClick(toggle, value, index));                            
                        }                        
                    }
                }
                SetPointLight(0);
            }
        }


        //自动移动位置
        long TimeGuid = 0;
        public void RegisterAutoMove(GameObject isStopObj = null, System.Action callBack = null)
        {
            InitData();
            isStop = isStopObj;
            GameTimeAndUpdateSystem.Inst.RemoveUpdateListener(TimeGuid);
            TimeGuid = GameTimeAndUpdateSystem.Inst.AddFrameUpdateListenerDepend(1, 1, gameObject,
                (e) =>
                {
                    InitPointList();
                    Circle(callBack);
                });           
        }


        long id_AnimCallback;
        private void Circle(System.Action callBack)
        {
            GameTimeAndUpdateSystem.Inst.RemoveUpdateListener(id_AnimCallback);
            id_AnimCallback = GameTimeAndUpdateSystem.Inst.AddTimeUpdateListenerDepend(AutoMoveIntervalsTimes, -1, TimeType.scaledTime, gameObject,
                (e) =>
                {
                    SetAutoMoveStopOrMove();
                    ToMove(callBack);
                });
        }
        
        private void ToMove(System.Action callBack)
        {
            if (m_isMoveStop)
                return;
            if (currIndex == doubleChildCount - 2)
            {
                currIndex = 0;
            }
            int moveTarIndex = 0;
            if (currIndex < childCount - 1)
            {
                //向右移动
                isRight = true;
                moveTarIndex = currIndex + 1;
            }
            else {
                //向左移动
                isRight = false;
                moveTarIndex = currIndex - 1;
            }
            SetPointLight(moveTarIndex);
            OnArrowClick(isRight, callBack);
        }

        //设置移动暂停或者开始移动
        private void SetAutoMoveStopOrMove()
        {
            if (isStop != null)
            {
                m_isMoveStop = isStop.activeSelf;
            }
            if (m_isMoveStop) return;
            childCount = content.childCount;
            doubleChildCount = childCount * 2;
            if (childCount < 2)
            {
                m_isMoveStop = true;
                return;
            }
            else
            {
                m_isMoveStop = false;
            }
        }

        private void SetPointLight(int index)
        {
            if (pointList != null && pointList.Count > index)
            {
                pointList[index].isOn = true;
            }
        }

        public void OnToggleClick(GameToggle toggle, bool value, int index)
        {
            if (value && currIndex != index)
            {

            }
            else {

            }

        }
		/// <summary>
		/// 竖直方向上设置显示到目标位置，需要计算，不能简单设置
		/// 横向的谁用谁自己写吧
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <param name="cellHeight"></param>
		/// <param name="viewHeight"></param>
		/// <param name="space"></param>
		public void ToTarIndexVertical(Int32 index, Int32 count, Single cellHeight, Single viewHeight, Single space)
		{
			if(count <= 0 || index < 0)
			{
				return;
			}
			if(count == 1 || index >= count)
			{
				verticalScrollbar.value = 0;
			}
			if(index == 0)
			{
				verticalScrollbar.value = 1;
			}
			var tarHeight = 0f;
			var maxHeight = (count - 1) * (cellHeight + space) + cellHeight;
			if (viewHeight < cellHeight)
			{
				maxHeight -= viewHeight;
			}
			else
			{
				var lastCount = Mathf.FloorToInt(viewHeight / (cellHeight + space));
				var zeroRemainder = viewHeight % (cellHeight + space);
				var maxIndex = Mathf.Max(count - lastCount - 1, 0);
				maxHeight -= lastCount * (cellHeight + space) + zeroRemainder;
				//Item底边要贴住View底边，所以先算一下，index大于几时直接设置0即可
				if (index > maxIndex)
				{
					index = maxIndex;
				}
			}
			tarHeight = index * (cellHeight + space);
            if (verticalScrollbar != null)
            {
                verticalScrollbar.value = 1 - tarHeight / maxHeight;
            }
               
			//Debug.LogError("verticalScrollbar.value = " + (1 - tarHeight / maxHeight).ToString("F3"));
		}
	}
}
