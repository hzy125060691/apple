using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zz.NotifySystem;
namespace Zz.NotifySystem
{
	public class UINotifyRedDot : MonoBehaviour
	{
		private List<INotifyEvent> customEvents = new List<INotifyEvent>();
		public List<NotifyType> NotifyTypes;
		public GameObject RedDot;
		private bool mInited = false;

		// Use this for initialization
		void Start()
		{
			if (!mInited)
			{
				mInited = true;
				foreach (NotifyType type in NotifyTypes)
				{
					NotifyManager.Instance().RegistListener((int)type, this.OnChange);
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
			foreach (NotifyType type in NotifyTypes)
			{
				if (NotifyManager.Instance().IsNeedNotify((int)type))
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

			Debug.Log("UINotifyRedDot::OnChange after calculate show value.");
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
			foreach (NotifyType type in NotifyTypes)
			{
				NotifyManager.Instance().UnRegistListener((int)type, this.OnChange);
			}
			customEvents.Clear();
			customEvents = null;
		}

		public void UnregisterCustomAction()
		{
			if (customEvents != null)
				customEvents.Clear();
		}

		public void RegisterCustomAction(INotifyEvent _customEvent)
		{
			if (customEvents == null)
			{
				customEvents = new List<INotifyEvent>();
			}
			customEvents.Add(_customEvent);

			OnChange();
		}
	}
}