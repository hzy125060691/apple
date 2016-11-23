using UnityEngine;
using System.Collections.Generic;

namespace Zz.GameUINotifySystem
{
	public class GameUINotifyRedDot : MonoBehaviour
	{
		private List<IGameUINotifyEvent> customEvents = new List<IGameUINotifyEvent>();
		public List<GameUINotifyType> NotifyTypes;
		public GameObject RedDot;
		private bool mInited = false;

		// Use this for initialization
		void Start()
		{
			if (!mInited)
			{
				mInited = true;
				foreach (GameUINotifyType type in NotifyTypes)
				{
					GameUINotifyManager.Instance.RegistListener((int)type, this.OnChange);
				}
			}

			OnChange();
		}

		public void OnEnable()
		{
			OnChange();
		}

		public void OnChange()
		{
			if (!enabled)
				return;

			bool show = false;
			foreach (GameUINotifyType type in NotifyTypes)
			{
				if (GameUINotifyManager.Instance.IsNeedNotify((int)type))
				{
					show = true;
					break;
				}
			}
			Debug.Log("UINotifyRedDot::OnChange after IsNeedNotify show value.");

			if (!show && customEvents != null)
			{
				foreach (var _cusEvent in customEvents)
				{
					if (_cusEvent != null && _cusEvent.CheckStatus())
					{
						show = true;
					}
				}
			}

			if (RedDot == null)
				return;

			if (show)
			{
				RedDot.SetActive(true);
			}
			else
			{
				RedDot.SetActive(false);
			}
		}

		void OnDestroy()
		{
			foreach (GameUINotifyType type in NotifyTypes)
			{
				GameUINotifyManager.Instance.UnRegistListener((int)type, this.OnChange);
			}
			customEvents.Clear();
			customEvents = null;
		}

		public void UnregisterCustomAction()
		{
			if (customEvents != null)
				customEvents.Clear();
		}

		public void RegisterCustomAction(IGameUINotifyEvent _customEvent)
		{
			if (customEvents == null)
			{
				customEvents = new List<IGameUINotifyEvent>();
			}
			customEvents.Add(_customEvent);

			OnChange();
		}
	}
}