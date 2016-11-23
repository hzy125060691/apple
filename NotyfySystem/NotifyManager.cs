using UnityEngine;
using System.Collections.Generic;
using System;

namespace Zz.NotifySystem
{
	public abstract class INotifyEvent
	{
		public abstract bool CheckStatus ();
	}
	
	public enum NotifyType
	{
		None = 0,
		Count,
	}
	
	public class NotifyManager
	{
		
		private static NotifyManager instance;
//		private bool  mReigsted = false;
		
		public static NotifyManager  Instance ()
		{
			if (null == instance) {
				instance = new NotifyManager ();
			}
//			if(!instance.mReigsted)
//				instance.RegistNotifyeListener();
			return instance;
		}
		
		private bool[] mStatusArr = new bool[ 0 ];
		private Dictionary<int, List<System.Action>>  mChangeListeners = new Dictionary<int, List<System.Action>> ();
		
		private NotifyManager ()
		{
			mStatusArr = new bool[ (int)NotifyType.Count ];
			setStatusAllOff ();
		}

		public void Clear ()
		{
			setStatusAllOff ();
			mChangeListeners.Clear ();
		}
		
		private void setStatusAllOff ()
		{
			if (mStatusArr != null) {
				for (int i=0; i< mStatusArr.Length; ++i) 
					mStatusArr [i] = false;
			}
		}
		
		public bool IsNeedNotify (int index)
		{
			if (index >= 0 && index < mStatusArr.Length) {
				return mStatusArr [index];
			} else {
				return false;
			}
		}

		public void NotifyStatusOff (int index)
		{
			if (index >= 0 && index < mStatusArr.Length) {
				if (mStatusArr [index] != false) {
					mStatusArr [index] = false;
					if (canNotify ((NotifyType)index)) {
						changeNotify (index);
					}
				}
			}
		}
		
		public void NotifyStatus (int index, bool status)
		{
//			if(status) {
//				DebugLog.LogError("---Notity ON :  "+index + " type = "+(NotifyType)index);
//			}
//			DebugLog.LogError("Notifystatus : " + index.ToString() + ", Status : " + status);
			if (index >= 0 && index < mStatusArr.Length) {
				if (mStatusArr [index] != status) {
					mStatusArr [index] = status;
					if (canNotify ((NotifyType)index)) {
						changeNotify (index);
					}
				}
			}
		}
		
		
		public void RegistListener (int index, System.Action onChange)
		{
			if (mChangeListeners.ContainsKey (index)) {
				mChangeListeners [index].Add (onChange);
			} else {
				List<System.Action> actions = new List<System.Action> ();
				actions.Add (onChange);
				mChangeListeners.Add (index, actions);
			}
		}
		
		public void UnRegistListener (int index, System.Action onChange)
		{
			if (mChangeListeners.ContainsKey (index)) {
				List<System.Action> actions = mChangeListeners [index];
				actions.Remove (onChange);
				if (actions.Count == 0)
					mChangeListeners.Remove (index);
			}
		}
		
		private void changeNotify (int index)
		{
			if (mChangeListeners.ContainsKey (index)) {
				List<System.Action> actions = mChangeListeners [index];
				foreach (Action a in actions) {
					try {
						a ();
					} catch (Exception) {
					}
				}
			}
		}

		private void changeNotify (List<int> changes)
		{
			HashSet<System.Action> actions = new HashSet<System.Action> ();
			
			foreach (int key in changes) {
				if (mChangeListeners.ContainsKey (key)) {
					List<Action> list = mChangeListeners [key];
					foreach (Action a in list) {
						actions.Add (a);
					}
				}
			}
			
			foreach (Action a in actions) {
				try {
					a ();
				} catch (Exception) {
				}
			}
			
		}

		private bool canNotify (NotifyType type)
		{
			bool b = true;
			return b;
		}
	}
}
