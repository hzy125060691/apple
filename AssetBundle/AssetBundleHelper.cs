using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading;
using System.Linq;
using System.Xml;

/// <summary>
/// 加载bundle的类，编辑器模式下也可以通过AssetDataBase直接加载资源(需要设置一下UseAssetBundle，详情看ABSettingHelper，就是类里边的那个，它是个配置文件)
/// </summary>
public class AssetBundleHelper : MonoBehaviour
{
	public AssetBundleSettingHelper ABSettingHelper = null;
	public AssetBundleUpdater ABUpdater = new AssetBundleUpdater();

	//private static Queue<KeyValuePair<string, KeyValuePair<bool,Action<UnityEngine.Object>>>> NeedLoadQueue = new Queue<KeyValuePair<string, KeyValuePair<bool, Action<UnityEngine.Object>>>>();
	private static Queue<Tuple<ABPInfo, bool, Action<UnityEngine.Object>>> NeedLoadQueue = new Queue<Tuple<ABPInfo, bool, Action<UnityEngine.Object>>>();

	private static AssetBundleManifest manifest = null;
	//private static object manifest_Sum_LockObj = new object();
	public class StageInfo
	{
		public string name;
		public DateTime beginTime;
		public StageInfo(string n = "Init")
		{
			name = n;
			beginTime = DateTime.Now;
		}
		public StageInfo(string n, DateTime time)
		{
			name = n;
			beginTime = time;
		}
	}
	private static StageInfo stage = new StageInfo("Default");
	//private static StageInfo lastStage = null;
	public static StageInfo Stage_C
	{
		get
		{
			return stage;
		}
	}
	public static string Stage
	{
		set
		{
			//lastStage = stage;
			stage = new StageInfo(value);

			AssetBundleDic.OutputCurABInfos();
			//先卸载不需要的AB，后设置预加载新的AB
			//var list = AssetBundleUseAnalysis.StageDiff(lastStage.name, stage.name);
			//AssetBundleDic.Unload(list);

			var preload = AssetBundleUseAnalysis.StagePreload(stage.name);
			CurNeedPreloadAB.Clear();
			if(preload != null)
			{
				CurNeedPreloadAB.AddRange(preload);
				if (CurNeedPreloadAB.Count > 0 && EnableUseRes())
				{
					Ins.BeginLoadRes();
				}
			}
		}
	}

	private static AssetBundleRecord AssetBundleDic = new AssetBundleRecord();
	#region 异步加载相关
	private static List<string> CurNeedPreloadAB = new List<string>();
	//private static Dictionary<string, AssetBundle> AssetBundleDic = new Dictionary<string, AssetBundle>();
	//private static Dictionary<string, AssetBundle> AssetBundleDic = new Dictionary<string, AssetBundle>();
	public static AssetBundleHelper Ins = null;
	private static Coroutine LoadResCor = null;
	private static bool bLoadingRes = false;
	//private Thread LoadThread = null;
	#endregion
	void Awake()
	{
		if (ABSettingHelper == null)
		{
			throw new Exception("ABSettingHelper == null; you need set it");
		}
		DontDestroyOnLoad(this);
		Ins = this;

// 		{
// 			FileInfo t = new FileInfo(Path.Combine(AssetBundleSettingHelper.GetStreamingAssetsPath(), "google/google"));
// 
// 			TestLog(t.FullName + ": exist :" + t.Exists);
// 			DirectoryInfo d = new DirectoryInfo(Application.persistentDataPath);
// 			TestLog(d.FullName + ": exist :" + d.Exists);
// 			d = new DirectoryInfo(AssetBundleSettingHelper.GetStreamingAssetsPath());
// 			TestLog(d.FullName + ": exist :" + d.Exists);
// 			d = new DirectoryInfo(Application.streamingAssetsPath);
// 			TestLog(d.FullName + ": exist :" + d.Exists);
// 			TestLog(Path.Combine(Application.streamingAssetsPath, "google") + ": exist :" + Directory.Exists(Path.Combine(Application.streamingAssetsPath, "google")));
// 			TestLog(Application.streamingAssetsPath + ": exist :" + Directory.Exists(Application.streamingAssetsPath));
// 			TestLog(AssetBundleSettingHelper.GetStreamingAssetsPath() + ": exist :" + Directory.Exists(AssetBundleSettingHelper.GetStreamingAssetsPath()));
// 		}
		//启动的时候做一些必要的检查和拷贝。这个以后要做成通过协程，需要界面更新的，别卡死界面
		if (ABUpdater != null)
		{
			//ABUpdater.CheckAndCopyABToPD(ABSettingHelper);

			//从网络段获得一份资源列表，检查一下是否有需要更新的。如果版本没有本地的新，那么就不更新了 ，防止CDN上放了一份老资源
			StartCoroutine(ABUpdater.CheckAndUpdateABToPD(ABSettingHelper, this,LateInit));
		}
		else
		{
			LateInit();
		}

	}

	public void LateInit()
	{
		//manifest的加载要在资源拷贝完成后进行。		
		try
		{
			LoadManifestAndPreloadInfo();
		}
		catch (Exception e)
		{
			Debug.LogError(e.Message);
			TestLog(e.Message);
		}
		if (ABSettingHelper.IsAsynLoadRes)
		{
			if (tempNeedLoadList.Count > 0)
			{
				foreach (var t in tempNeedLoadList)
				{
					Ins.PushResToNeedLoad_Real(t.t1, t.t2, t.t3);
				}
				tempNeedLoadList.Clear();
			}
		}

		//TestLog("Init Over Can Play");
	}

	public static bool EnableUseRes()
	{
		return Ins != null && 
				(Ins.ABUpdater.IsStage(AssetBundleUpdater.UpdaterStage.EnableUseRes) ||
				Ins.ABUpdater.IsStage(AssetBundleUpdater.UpdaterStage.NotNeedUpdater) ||
				Ins.ABUpdater.IsStage(AssetBundleUpdater.UpdaterStage.ResIsUpToDate) ||
				Ins.ABUpdater.IsStage(AssetBundleUpdater.UpdaterStage.Download_Over) ||
				Ins.ABUpdater.IsStage(AssetBundleUpdater.UpdaterStage.EnableUseRes) );
	}
	
	private static List<Tuple<string, Action<UnityEngine.Object>, bool>> tempNeedLoadList = new List<Tuple<string, Action<UnityEngine.Object>, bool>>();
	public static void PushResToNeedLoad_ResourcesPath(string path, Action<UnityEngine.Object> callback, bool bInstantiate = true)
	{
		PushResToNeedLoad(Path.Combine(@"Assets\Resources",path) , callback, bInstantiate);
	}
	public static void PushResToNeedLoad(string path, Action<UnityEngine.Object> callback, bool bInstantiate = true)
	{
		if(EnableUseRes())
		{
			tempNeedLoadList.Add(new Tuple<string, Action<UnityEngine.Object>, bool>(path, callback, bInstantiate));
		}
		else
		{
			if(Ins.ABSettingHelper.IsAsynLoadRes)
			{
				Ins.PushResToNeedLoad_Real(path, callback, bInstantiate);
			}
		}
	}
	#region 同步加载
	public static T LoadResource_Sync<T>(string assetName) where T : UnityEngine.Object
	{
		if(!EnableUseRes())
		{
			return null;
		}
		return Ins.LoadResource_Sync_Real<T>(assetName);
	}
	public static GameObject LoadResource_Sync_ResourcesPath_GO(string assetName)
	{
		return LoadResource_Sync_ResourcesPath<GameObject>(assetName);
	}
	public static T LoadResource_Sync_ResourcesPath<T>(string assetName) where T : UnityEngine.Object
	{
		return LoadResource_Sync<T>(Path.Combine(@"Assets\Resources", assetName));
	}

	private T LoadResource_Sync_Real<T>(string assetName) where T : UnityEngine.Object
	{
		if(ABSettingHelper.UseAssetBundle)
		{
			if (manifest == null)
			{
				throw new NullReferenceException("manifest == null");
			}
			var abpi = ABSettingHelper.GetCurPlatformABPath(assetName);
			string[] dps = manifest.GetAllDependencies(abpi.DependencyName);
			for (int i = 0; i < dps.Length; i++)
			{
				ABPInfo abpiTT = ABSettingHelper.GetCurPlatformABPath(dps[i]);
				{
					if (!AssetBundleDic.ContainsKey(abpiTT.URI))
					{
						var ab = AssetBundle.LoadFromFile(abpiTT.FullName);
						if(ab != null)
						{
							AssetBundleDic.Add(abpiTT.URI, ab, -1);
						}
						else
						{
							Debug.LogError("Not Found File:" + abpiTT.FullName);
						}
					}
				}
			}
			if (!AssetBundleDic.ContainsKey(abpi.URI))
			{
				var ab = AssetBundle.LoadFromFile(abpi.FullName);
				if (ab != null)
				{
					AssetBundleDic.Add(abpi.URI, ab, -1);
				}
				else
				{
					Debug.LogError("Not Found File:" + abpi.FullName);
				}
			}
			if (!AssetBundleDic.ContainsKey(abpi.URI, false))
			{
				Debug.LogError("!AssetBundleDic.ContainsKey(name) :" + abpi.URI);
				return null;
			}
			var obj = AssetBundleDic.GetAssetBundle(abpi.URI).LoadAsset<T>(abpi.AssetName);
			return obj;
		}
		else
		{
#if UNITY_EDITOR
			var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetName);
			if (obj == null)
			{
				Debug.LogError("Asset not load:" + assetName);
			}
			return obj;
#else
			throw new Exception("What!!!!!!!!!!!! look here!!!!!!!!");
#endif
		}
		//return null;
	}
#endregion
	private void BeginLoadRes()
	{
		if (!bLoadingRes)
		{
			bLoadingRes = true;
			//Debug.LogError(Thread.CurrentContext.ContextID + ":" + "LoadResourceAsyn()" + " :" + Time.realtimeSinceStartup);
			LoadResCor = StartCoroutine(LoadResourceAsyn());
			if(LoadResCor == null)
			{

			}
		}
	}
	private void PushResToNeedLoad_Real(string path, Action<UnityEngine.Object> callback, bool bInstantiate)
	{
		//path = Path.Combine(Ins.ABSettingHelper.NeedBuildABPath, path).ToLower();
		path = path.ToLower();
		//Debug.Log(path);
		//lock(NeedLoadQueue)
		// 		{
		// 			NeedLoadQueue.Enqueue(new KeyValuePair<string, KeyValuePair<bool, Action<UnityEngine.Object>>>(path, new KeyValuePair<bool, Action<UnityEngine.Object>>(bInstantiate, callback)));
		// 		}
		ABPInfo abpi = ABSettingHelper.GetCurPlatformABPath(path);
		NeedLoadQueue.Enqueue(new Tuple<ABPInfo, bool, Action<UnityEngine.Object>>(abpi, bInstantiate, callback));
		BeginLoadRes();
	}
	void OnDestroy()
	{
		AssetBundleDic.Clear();
	}

	/// <summary>
	/// 会被打成assetbundle所有资源的加载都通过这里实现，没有同步加载方式，只有异步
	/// 没有resources.load在加载方式，编辑器中可根据设置使用AssetDatabase加载(虽然这时是可以同步加载，但是保证一致性，还是要表现成异步)
	/// </summary>
	/// <param name="loadResourceName">要加载的资源的名字</param>
	/// <param name="callback">资源加载成功后的回调</param>
	/// <returns></returns>
	private IEnumerator LoadResourceAsyn()
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//这里可能没有修改正确，启用异步加载前请检查并检验代码
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		if (!Ins.ABSettingHelper.IsAsynLoadRes)
		{
			yield break;
		}
			//string streamingAssetsPath = (AssetBundleSettingHelper.PathToPlatformFormat(AssetBundleSettingHelper.GetStreamingAssetsPath()));
			//string platformBundlePath = (AssetBundleSettingHelper.Combine(streamingAssetsPath, ABSettingHelper.GetPlatformPathName()));
			//var abpi = ABSettingHelper.GetCurPlatformBundlePath();
		while (CurNeedPreloadAB.Count > 0 || NeedLoadQueue.Count > 0)
		{
			//先加载需要加载的资源，没有的时候再加载预加载资源
			while (NeedLoadQueue.Count > 0)
			{
				List<WWW> listWWWs = new List<WWW>();
				Dictionary<string, string> listDPs = new Dictionary<string, string>();

				var needLoad = NeedLoadQueue.Dequeue();
				
				var loadResourceABPI = needLoad.t1;
				var bInstantiate = needLoad.t2;
				var callback = needLoad.t3;
				UnityEngine.Object newObj = null;
				if (ABSettingHelper.UseAssetBundle)
				{
					listDPs.Clear();
					listWWWs.Clear();
					//通过bundle加载
					//ABPInfo abpiTmp = ABSettingHelper.GetCurPlatformABPath(loadResourceName);
					//如果这个时候还为空直接终止
					if (manifest == null)
					{
						throw new NullReferenceException("manifest == null");
						//yield break;
					}
					{
						string[] dps = manifest.GetAllDependencies(loadResourceABPI.DependencyName);
						//AssetBundle[] abs = new AssetBundle[dps.Length];
						for (int i = 0; i < dps.Length; i++)
						{
							ABPInfo abpiTT = ABSettingHelper.GetCurPlatformABPath(dps[i]);
							{
								if (!AssetBundleDic.ContainsKey(abpiTT.URI))
								{
									listWWWs.Add(new WWW(abpiTT.URI));

								}
								listDPs.Add(abpiTT.URI, dps[i]);
							}
						}
						{
							if (!AssetBundleDic.ContainsKey(loadResourceABPI.URI))
							{
								listWWWs.Add(new WWW(loadResourceABPI.URI));
							}
						}
						while (true && listWWWs.Count > 0)
						{
							yield return null;
							for (int i = 0; i < listWWWs.Count;)
							{
								var www = listWWWs[i];
								if (www.error != null)
								{
									listWWWs.RemoveAt(i);
									string info = "www.error != null :" + www.url + " " + www.error;
									www.Dispose();
									throw new NullReferenceException(info);
									//Debug.Assert(false, "wwwtar.error != null :" + www.url +" "+ www.error);
								}
								else if (www.isDone)
								{
									listWWWs.RemoveAt(i);
									var bundleTar = www.assetBundle;
									string key = www.url;
									int size = www.size;
									www.Dispose();
									if (bundleTar != null)
									{
										AssetBundleDic.Add(key, bundleTar, size);
									}
									else
									{
										//Debug.Assert(false, "bundleTar == null :" + www.url);
										throw new NullReferenceException("bundleTar == null :" + key);
									}
								}
								else
								{
									i++;
								}
							}
						}
						if (!AssetBundleDic.ContainsKey(loadResourceABPI.URI, false))
						{
							//Debug.LogError("!AssetBundleDic.ContainsKey(name) :" + name);
							throw new NullReferenceException("!AssetBundleDic.ContainsKey(loadResourceABPI.URI) :" + loadResourceABPI.URI);
						}
						{
							var obj = AssetBundleDic.GetAssetBundle(loadResourceABPI.URI).LoadAsset(loadResourceABPI.AssetName);
							if (obj != null)
							{
								if (bInstantiate)
								{
									newObj = Instantiate(obj) as UnityEngine.Object;
								}
								else
								{
									newObj = obj;
								}
							}
							else
							{
// 								while (true)
// 								{
// 									Debug.LogError("name:"+ name+ " loadResourceName" + loadResourceName);
// 									yield return new WaitForSeconds(5.0f) ;
// 								}
								throw new NullReferenceException("obj == null ");
								
								//Debug.Assert(false, "obj == null ");
							}
							AssetBundleDic.UnloadABUseAnalysis(listDPs, loadResourceABPI.URI);
						}

					}
				}
				else
				{
#if UNITY_EDITOR
					yield return null;
					var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(loadResourceABPI.AssetName);
					if (obj != null)
					{
						if (bInstantiate)
						{
							newObj = Instantiate(obj) as UnityEngine.Object;
						}
						else
						{
							newObj = obj;
						}
					}
					else
					{
						throw new NullReferenceException("obj == null here " + loadResourceABPI.AssetName);
						//Debug.Assert(false, "obj == null here ");
					}
					Debug.Log("AssetDataBase.load:" + loadResourceABPI.AssetName);
#endif
				}

				if (newObj != null)
				{
					callback(newObj);
				}
				else
				{
					//Debug.Assert(false, "newObj == null ");
					throw new NullReferenceException("newObj == null ");
				}

			}
			if(CurNeedPreloadAB.Count > 0)
			{
				yield return null;
				var fAB = CurNeedPreloadAB[0];
				ABPInfo abpiTmp_Preload = ABSettingHelper.GetCurPlatformABPath(fAB);
				{
					if (!AssetBundleDic.ContainsKey(abpiTmp_Preload.Dir_Full))
					{
						using(WWW www = new WWW(abpiTmp_Preload.URI))
						{
							if(www == null)
							{
								// 								while (true)
								// 								{
								// 									Debug.LogError("www == null " + namePreload);
								// 									Thread.Sleep(3000);
								// 								}
								Debug.LogError(Thread.CurrentContext.ContextID + ":www == null" + abpiTmp_Preload.Dir_Full + ":"+ Time.realtimeSinceStartup);
							}
							//else
							//{
							//Debug.LogError(Thread.CurrentContext.ContextID+":"+www.url + "----------" + www.assetBundle + " isDone:" + www.isDone + " error:" + www.error + " :" + Time.realtimeSinceStartup);
							//}
							while (!www.isDone && (www.error == null || www.error.Equals("")))
							{
								yield return null;
							}
							if (www.error != null)
							{
								string info = "www.error != null :" + www.url + " " + www.error;
								//www.Dispose();
								throw new NullReferenceException(info);
							}
							else if (www.isDone)
							{
								//try
								{
									AssetBundle bundleTar = null;
									//try
									//{
										bundleTar = www.assetBundle;
									//}
									//catch
									//{
									//	Debug.LogError(www.url + "**1111*" + www + " :" + Time.realtimeSinceStartup);
									//}
									string key = www.url;
									if (bundleTar != null)
									{
										AssetBundleDic.Add(key, bundleTar, www.size);
									}
									else
									{
										Debug.LogError(Thread.CurrentContext.ContextID + ":" + www.url + "**2222*"+ www.assetBundle + " isDone:" + www.isDone + " error:"+ www.error + " :" + Time.realtimeSinceStartup);
// 										while (true)
// 										{
// 											Debug.LogError(Thread.CurrentContext.ContextID + ":" + "bundleTar != null " + namePreload);
// 											yield return new WaitForSeconds(5.0f);
// 										}
										throw new NullReferenceException(Thread.CurrentContext.ContextID + ":" + "bundleTar == null :" + key);
									}
									//www.Dispose();
								}
								//catch
								{

								}
							}
						}
					}
				}
				CurNeedPreloadAB.RemoveAt(0);
			}
			//AssetBundleRecord.msgTmp.Clear();
		}
		LoadResCor = null;
		bLoadingRes = false;
		Debug.Log(Thread.CurrentContext.ContextID + ":Load End" + " :" + Time.realtimeSinceStartup);
	}


	public void LoadManifestAndPreloadInfo()
	{
		if(ABSettingHelper.UseAssetBundle)
		{
			//manifest
			LoadManifest();
			//PreloadInfo
			TryLoadPreloadInfo();
		}
	}
	private void LoadManifest()
	{
		if (manifest == null)
		{
			var abpi = ABSettingHelper.GetCurPlatformManifestPath();
			var ab = AssetBundle.LoadFromFile(abpi.FullName);

			if(ab != null)
			{
				manifest = (AssetBundleManifest)ab.LoadAsset("AssetBundleManifest");
				if (manifest != null)
				{
				}
				else
				{
					throw new NullReferenceException("mainfest == null");
				}
				ab.Unload(false);
			}
			else
			{
				throw new NullReferenceException("if(ab != null):" + abpi.FullName);
			}
// 			using (WWW www = new WWW(abpi.URI))
// 			{
// 				if (www.error != null)
// 				{
// 					Debug.Assert(false, "www.error != null " + www.error);
// 				}
// 				else
// 				{
// 					var bundle = www.assetBundle;
// 					if (bundle != null)
// 					{
// 						manifest = (AssetBundleManifest)bundle.LoadAsset("AssetBundleManifest");
// 						if (manifest != null)
// 						{
// 						}
// 						else
// 						{
// 							throw new NullReferenceException("mainfest == null");
// 						}
// 						bundle.Unload(false);
// 					}
// 					else
// 					{
// 						throw new NullReferenceException("var bundle = www.assetBundle; bundle == null:"+ abpi.URI);
// 					}
// 				}
// 			}
		}
	}
	/// <summary>
	/// 读取预加载信息，这个信息可以读取不到。
	/// </summary>
	private void TryLoadPreloadInfo()
	{
		//这不是一个必须读取成功的文件
		try
		{
			var abpi = ABSettingHelper.GetDataAnalysisXmlMoveTargetABPI();
			var ab = AssetBundle.LoadFromFile(abpi.FullName);

			if (ab != null)
			{
				var asset = ab.LoadAsset<TextAsset>(abpi.NameWithExt);
				if (asset != null)
				{
					XmlDocument doc = new XmlDocument();
					doc.LoadXml(asset.text);
					AssetBundleUseAnalysis.LoadPreloadInfo(doc);
				}
				else
				{
					Debug.LogWarning(@"if (asset != null) : " + abpi.FullName + " Res: " + abpi.NameWithExt);
				}
				ab.Unload(false);
			}
			else
			{
				Debug.LogWarning(@"if (ab != null) : " + abpi.FullName);
				throw new NullReferenceException("if(ab != null):" + abpi.FullName);
			}


// 			using (WWW www = new WWW(abpi.URI))
// 			{
// 				if (www.error != null)
// 				{
// 					//Debug.Assert(false, "www.error != null " + www.error);
// 					Debug.LogWarning(@"www.error != null " + www.error);
// 				}
// 				else
// 				{
// 					if (www.assetBundle != null)
// 					{
// 						var asset = www.assetBundle.LoadAsset<TextAsset>(abpi.NameWithExt);
// 						if (asset != null)
// 						{
// 							XmlDocument doc = new XmlDocument();
// 							doc.LoadXml(asset.text);
// 							AssetBundleUseAnalysis.LoadPreloadInfo(doc);
// 						}
// 						else
// 						{
// 							Debug.LogWarning(@"if (asset != null) : " + abpi.FullName + " Res: " + abpi.NameWithExt);
// 						}
// 						www.assetBundle.Unload(false);
// 					}
// 					else
// 					{
// 						Debug.LogWarning(@"if (www.assetBundle != null) : " + abpi.FullName);
// 					}
// 				}
// 			}
		}
		catch (Exception e)
		{
			Debug.LogWarning(e);
		}
	}

	public static void TestLog(string log)
	{
// 		if(text2 != null)
// 		{
// 			text2.text += "\n" + log;
// 		}
	}
}

public class AssetBundleRecord
{
	struct ABInfo
	{
		public ABInfo(AssetBundle a, int s)
		{
			ab = a;
			size = s;
			//assets = new List<UnityEngine.Object>();
		}
		public AssetBundle ab;
		//public List<UnityEngine.Object> assets;
		public int size;

		public List<UnityEngine.Object> GetALlAssets()
		{
			// 			if(assets.Count <= 0)
			// 			{
			// 				assets.AddRange(ab.LoadAllAssets());
			// 			}
			// 			return assets;
			return ab.LoadAllAssets().ToList();
		}
		public void UnloadAll()
		{
// 			foreach(var asset in assets)
// 			{
// 				//UnityEngine.Object.Destroy(asset);
// 			}
// 			assets.Clear();
			ab.Unload(false);
			ab = null;
		}
	}
	public static bool bABUseAnalysis = true;
	void OutputXml()
	{
		AssetBundleUseAnalysis.OutputABUseXml();
	}
	private Dictionary<string, ABInfo> assetBundleDic = new Dictionary<string, ABInfo>();
	//private List<UnityEngine.Object> assets = new List<UnityEngine.Object>();

	//public static List<string> msgTmp = new List<string>();
	public bool ContainsKey(string key, bool bAnalysis = true)
	{
		if(AssetBundleHelper.Ins.ABSettingHelper.IsABUseAnalysis && bAnalysis)
		{
			AssetBundleUseAnalysis.AddABUse(key.ToLower(), ABDetailRecord.RecordType.ContainsKey);
		}
		PrintLog();
		var keyL = key.ToLower();
		var ret = assetBundleDic.ContainsKey(keyL);
// 		if (!ret)
// 		{
// 			msgTmp.Add(key + ":" + keyL);
// 		}
		return ret;
	}

	public AssetBundle GetAssetBundle(string key)
	{
		if (AssetBundleHelper.Ins.ABSettingHelper.IsABUseAnalysis)
		{
			AssetBundleUseAnalysis.AddABUse(key.ToLower(), ABDetailRecord.RecordType.Get);
		}
		PrintLog();
		return assetBundleDic[key.ToLower()].ab;
	}
	public void Add(string key, AssetBundle bundle, int abSize)
	{
		if (AssetBundleHelper.Ins.ABSettingHelper.IsABUseAnalysis)
		{
			AssetBundleUseAnalysis.AddABUse(key.ToLower(), ABDetailRecord.RecordType.Add);
		}
		assetBundleDic.Add(key.ToLower(), new ABInfo(bundle, abSize));
		PrintLog();
	}
	//static int mm = 0;
	public List<UnityEngine.Object> loadAllDPs(Dictionary<string, string> keys)
	{
		string key = "";
		List<UnityEngine.Object> ret = new List<UnityEngine.Object>();
		foreach (var k in keys)
		{
			key = k.Key.ToLower();
			if (assetBundleDic.ContainsKey(key))
			{
				ret.AddRange(assetBundleDic[key].GetALlAssets());
				//assets.AddRange(ret);
				//var ls = assetBundleDic[key].LoadAllAssets();
				// 				foreach (var l in ls)
				// 				{
				// 					Debug.LogError("loadAll:"+ l.name);
				// 				}
			}
		}
		return ret;
	}
	public void UnloadABUseAnalysis(Dictionary<string, string> keys, string TarAB = "")
	{
// 		if (AssetBundleHelper.Ins.ABSettingHelper.IsABUseAnalysis)
// 		{
// 			Unload(keys.Select(k=>k.Key));
// 		}
	}
	public void OutputCurABInfos()
	{
// 		int s = 0;
// 		foreach (var ab in assetBundleDic)
// 		{
// 			s += ab.Value.size;
// 			
// 		}
// 		Debug.Log("AllABSize:"+ s/1024 + "KB ," + s/(1024*1024) + "MB");
	}
	public void Unload(IEnumerable<string> keys, string TarAB = "")
	{
		if(keys == null)
		{
			return;
		}
		string key = "";
		if(!TarAB.Equals(""))
		{
			key = TarAB.ToLower();
			if (assetBundleDic.ContainsKey(key))
			{
				//Debug.LogWarning("Unload:" + mm + ":" + assetBundleDic[key].name);
				assetBundleDic[key].UnloadAll();
				assetBundleDic.Remove(key);
			}
		}
		foreach (var k in keys)
		{
			key = k.ToLower();
			if (assetBundleDic.ContainsKey(key))
			{
				//Debug.LogWarning("Unload:" + mm + ":"+ assetBundleDic[key].name);
				
				assetBundleDic[key].UnloadAll();
				assetBundleDic.Remove(key);
			}
		}
	}

	private void PrintLog()
	{
		// 		foreach (var msg in assetBundleUseTime[assetBundleUseTime.Count-1])
		// 		{
		// 			Debug.Log(msg.Key + ": " + msg.Value);
		// 		}
// 		var msg = assetBundleUseTime[assetBundleUseTime.Count - 1];
// 		Debug.Log(msg.Key + ": " + msg.Value);
	}
	public void Clear()
	{
		OutputXml();
		foreach (var ab in assetBundleDic)
		{
			ab.Value.UnloadAll();
		}
		assetBundleDic.Clear();
	}
}

public class Tuple<T1, T2, T3>
{
	public T1 t1;
	public T2 t2;
	public T3 t3;
	public Tuple(T1 tt1, T2 tt2, T3 tt3)
	{
		t1 = tt1;
		t2 = tt2;
		t3 = tt3;
	}
}
