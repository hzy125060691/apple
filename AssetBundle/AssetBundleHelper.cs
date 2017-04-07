using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading;
using System.Linq;
using System.Xml;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// 加载bundle的类，编辑器模式下也可以通过AssetDataBase直接加载资源(需要设置一下UseAssetBundle，搜索一下，它最开始是在本文件中)
/// </summary>
public class AssetBundleHelper : MonoBehaviour
{
	public const string AssetBundelExtName = @".assetbundle";							//assetbundle的扩展名
	public const string WindowsSimplifyName = @"Microsoft";
	public const string AndroidSimplifyName = @"Google";
	public const string OSXSimplifyName = @"Apple";

	public const string AssetBundlePrefixPath = @"Assets/Resources";					//bundle路径的前缀

	private static Queue<KeyValuePair<string, KeyValuePair<bool,Action<UnityEngine.Object>>>> NeedLoadQueue = new Queue<KeyValuePair<string, KeyValuePair<bool, Action<UnityEngine.Object>>>>();

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
	private static StageInfo lastStage = null;
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
			lastStage = stage;
			stage = new StageInfo(value);
			//先卸载不需要的AB，后设置预加载新的AB
			var list = AssetBundleUseAnalysis.StageDiff(lastStage.name, stage.name);
			AssetBundleDic.Unload(list);

			var preload = AssetBundleUseAnalysis.StagePreload(stage.name);
			CurNeedPreloadAB.Clear();
			if(preload != null)
			{
				CurNeedPreloadAB.AddRange(preload);
				if (CurNeedPreloadAB.Count > 0 && Ins != null)
				{
					Ins.BeginLoadRes();
				}
			}
		}
	}

	private static AssetBundleRecord AssetBundleDic = new AssetBundleRecord();
	private static List<string> CurNeedPreloadAB = new List<string>();
	//private static Dictionary<string, AssetBundle> AssetBundleDic = new Dictionary<string, AssetBundle>();
	//private static Dictionary<string, AssetBundle> AssetBundleDic = new Dictionary<string, AssetBundle>();
	public static AssetBundleHelper Ins = null;
	private static Coroutine LoadResCor = null;
	//private Thread LoadThread = null;
	void Awake()
	{
		DontDestroyOnLoad(this);
		LoadManifestAndPreloadInfo();
		Ins = this;
		if(tempNeedLoadList.Count > 0)
		{
			foreach(var t in tempNeedLoadList)
			{
				Ins.PushResToNeedLoad_Real(t.t1, t.t2, t.t3);
			}
			tempNeedLoadList.Clear();
		}
	}
	// 	void Start()
	// 	{
	// 	}
	private static List<Tuple<string, Action<UnityEngine.Object>, bool>> tempNeedLoadList = new List<Tuple<string, Action<UnityEngine.Object>, bool>>();
	public static void PushResToNeedLoad(string path, Action<UnityEngine.Object> callback, bool bInstantiate = true)
	{
		if(Ins == null)
		{
			tempNeedLoadList.Add(new Tuple<string, Action<UnityEngine.Object>, bool>(path, callback, bInstantiate));
		}
		else
		{
			Ins.PushResToNeedLoad_Real(path, callback, bInstantiate);
		}
	}
	private void BeginLoadRes()
	{
		if (LoadResCor == null)
		{
			LoadResCor = StartCoroutine(LoadResourceAsyn());
		}
	}
	private void PushResToNeedLoad_Real(string path, Action<UnityEngine.Object> callback, bool bInstantiate)
	{
		path = Path.Combine(AssetBundlePrefixPath, path).ToLower();
		//Debug.Log(path);
		lock(NeedLoadQueue)
		{
			NeedLoadQueue.Enqueue(new KeyValuePair<string, KeyValuePair<bool, Action<UnityEngine.Object>>>(path, new KeyValuePair<bool, Action<UnityEngine.Object>>(bInstantiate, callback)));
		}
		BeginLoadRes();
// 		if (LoadThread==null)
// 		{
// 			LoadThread = new Thread(new ThreadStart(ThreadLoadRes));
// 			LoadThread.Start();
// 		}
	}
	void OnDestroy()
	{
		AssetBundleDic.Clear();
	}
	/// <summary>
	/// 获取StreamingAssets路径
	/// </summary>
	/// <returns>StreamingAssets路径</returns>
	public static string GetStreamingAssetsPath()
	{
		return Application.streamingAssetsPath;
	}
	/// <summary>
	/// File路径专程URL
	/// </summary>
	/// <param name="path">文件名(带路径)</param>
	/// <returns>url</returns>
	public static string PathToFileUri(string path)
	{
		switch (Application.platform)
		{
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.IPhonePlayer:
				path = "file://" + path;
				break;
			case RuntimePlatform.Android:
				//platformName = "Google";
				break;
			default:
				//platformName = "Default";
				break;
		}
		return path;
	}
	/// <summary>
	/// 根据制定的RuntimePlatform，获得一个简化的能区分大体平台的一个名字
	/// </summary>
	/// <param name="pf">制定的RuntimePlatform</param>
	/// <returns>针对windows OS Android有所区分的一个名字，区分不是很详细</returns>
	public static string RuntimePlatformToSimplifyName(RuntimePlatform pf)
	{
		string platformName = "";
		switch(pf)
		{
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.WindowsPlayer:
				platformName = WindowsSimplifyName;
				break;
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.IPhonePlayer:
				platformName = OSXSimplifyName;
				break;
			case RuntimePlatform.Android:
				platformName = AndroidSimplifyName;
				break;
			default:
				platformName = "Default";
				break;
		}
		return platformName;
	}
	/// <summary>
	/// 根据当前运行的平台，获得一个简化的能区分大体平台的一个名字
	/// </summary>
	/// <returns>针对windows OS Android有所区分的一个名字，区分不是很详细</returns>
	public static string GetPlatformPathName()
	{
#if UNITY_EDITOR
		return RuntimePlatformToSimplifyName(BuildBundleManager.BuildTargetToRuntimePlatform(EditorUserBuildSettings.activeBuildTarget));
#else
		return RuntimePlatformToSimplifyName(Application.platform);
#endif
	}
	/// <summary>
	/// 把名字加上AB扩展名
	/// </summary>
	/// <param name="name">资源名称</param>
	/// <returns>加上扩展名后的结果</returns>
	private static string GetAssetBundleNameWithExtName(string name)
	{
		return name + AssetBundelExtName;
	}
	/// <summary>
	/// 通过资源名获得AB名
	/// 这个可以自己制作规则，资源如何与AB对应
	/// </summary>
	/// <param name="resourceName">资源名称</param>
	/// <returns>资源对应的AB名称</returns>
	public static string ResourceNameToBundleName(string resourceName)
	{
		return GetAssetBundleNameWithExtName(resourceName);
	}
	/// <summary>
	/// 合成路径（两参数版本）
	/// </summary>
	/// <param name="path1">路径1</param>
	/// <param name="path2">路径2</param>
	/// <returns>结果根据平台有所不同，但是都是组合在一起的</returns>
	public static string Combine(string path1, string path2)
	{
		return PathToPlatformFormat(Path.Combine(path1, path2));
	}
	/// <summary>
	/// 合成路径（三参数版本）
	/// </summary>
	/// <param name="path1">路径1</param>
	/// <param name="path2">路径2</param>
	/// <param name="path3">路径3</param>
	/// <returns>结果根据平台有所不同，但是都是组合在一起的</returns>
	public static string Combine(string path1, string path2, string path3)
	{
		return PathToPlatformFormat(Path.Combine(Path.Combine(path1, path2), path3));
	}
	/// <summary>
	/// /和\\的转换，转换成当前运行系统下的模式
	/// 需要注意的是Url的协议中的//不要调用这个进行转换
	/// 需要注意的是Url的协议中的//不要调用这个进行转换
	/// 需要注意的是Url的协议中的//不要调用这个进行转换
	/// </summary>
	/// <param name="path">需要转换的路径</param>
	/// <returns>转换后的结果</returns>
	public static string PathToPlatformFormat(string path)
	{
		string formatPath = "";
		switch (Application.platform)
		{
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.WindowsPlayer:
				formatPath = path.Replace('/','\\');
				break;
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.IPhonePlayer:
				formatPath = path.Replace('\\', '/');
				break;
			case RuntimePlatform.Android:
				formatPath = path.Replace('\\', '/');
				//platformName = "Google";
				break;
			default:
				//platformName = "Default";
				break;
		}
		return formatPath;
	}
	/// <summary>
	/// 会被打成assetbundle所有资源的加载都通过这里实现，没有同步加载方式，只有异步
	/// 没有resources.load在加载方式，编辑器中可根据设置使用AssetDatabase加载
	/// </summary>
	/// <param name="loadResourceName">要加载的资源的名字</param>
	/// <param name="callback">资源加载成功后的回调</param>
	/// <returns></returns>
	private IEnumerator LoadResourceAsyn()
	{

		string streamingAssetsPath = (PathToPlatformFormat(GetStreamingAssetsPath()));
		string platformBundlePath = (Combine(streamingAssetsPath, GetPlatformPathName()));
		while (CurNeedPreloadAB.Count > 0 || NeedLoadQueue.Count > 0)
		{
			//先加载需要加载的资源，没有的时候再加载预加载资源
			while (NeedLoadQueue.Count > 0)
			{
				List<WWW> listWWWs = new List<WWW>();
				Dictionary<string, string> listDPs = new Dictionary<string, string>();
				KeyValuePair<string, KeyValuePair<bool, Action<UnityEngine.Object>>> needLoad;
				lock (NeedLoadQueue)
				{
					needLoad = NeedLoadQueue.Dequeue();
				}
				string loadResourceName = needLoad.Key;
				var bInstantiate = needLoad.Value.Key;
				var callback = needLoad.Value.Value;
				UnityEngine.Object newObj = null;
				if (UseAssetBundle)
				{
					listDPs.Clear();
					listWWWs.Clear();
					//通过bundle加载
					string LoadBundleName = ResourceNameToBundleName(loadResourceName);
					//string platformManifestPath = PathToFileUri(Combine(platformBundlePath, GetPlatformPathName()));
					//{
					//	if (manifest == null)
					//	{
					//		using (WWW www = new WWW(platformManifestPath))
					//		{
					//			yield return www;
					//			if (www.error != null)
					//			{
					//				Debug.Assert(false, "www.error != null " + www.error);
					//			}
					//			else
					//			{
					//				var bundle = www.assetBundle;
					//				if (bundle != null)
					//				{
					//					manifest = (AssetBundleManifest)bundle.LoadAsset("AssetBundleManifest");
					//					if (manifest != null)
					//					{
					//					}
					//					else
					//					{
					//						Debug.Assert(false, "mainfest == null");
					//					}
					//					bundle.Unload(false);
					//				}
					//				else
					//				{
					//					Debug.Assert(false, "bundle == null");
					//				}
					//			}

					//		}
					//	}
					//}
					//如果这个时候还为空直接终止
					if (manifest == null)
					{
						throw new NullReferenceException("manifest == null");
						//yield break;
					}
					{
						string[] dps = manifest.GetAllDependencies(LoadBundleName);
						//AssetBundle[] abs = new AssetBundle[dps.Length];
						for (int i = 0; i < dps.Length; i++)
						{
							string nameTmp = PathToFileUri(Combine(platformBundlePath, dps[i]));
							{
								if (!AssetBundleDic.ContainsKey(nameTmp))
								{
									listWWWs.Add(new WWW(nameTmp));

								}
								listDPs.Add(nameTmp, dps[i]);
							}
						}
						string name = PathToFileUri(Combine(platformBundlePath, LoadBundleName));
						{
							if (!AssetBundleDic.ContainsKey(name))
							{
								listWWWs.Add(new WWW(name));
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
									www.Dispose();
									if (bundleTar != null)
									{
										AssetBundleDic.Add(key, bundleTar);
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
						if (!AssetBundleDic.ContainsKey(name, false))
						{
							//Debug.LogError("!AssetBundleDic.ContainsKey(name) :" + name);
							throw new NullReferenceException("!AssetBundleDic.ContainsKey(name) :" + name);
							//continue;
							//yield break;
						}
						{
							AssetBundleDic.loadAllDPs(listDPs);
							var obj = AssetBundleDic.GetAssetBundle(name).LoadAsset(loadResourceName);
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
								throw new NullReferenceException("obj == null ");
								//Debug.Assert(false, "obj == null ");
							}
							AssetBundleDic.UnloadABUseAnalysis(listDPs, name);
						}

					}
				}
				else
				{
#if UNITY_EDITOR
					yield return null;
					var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(loadResourceName);
					if (obj != null)
					{
						newObj = Instantiate(obj) as UnityEngine.Object;
					}
					else
					{
						throw new NullReferenceException("obj == null here " + loadResourceName);
						//Debug.Assert(false, "obj == null here ");
					}
					Debug.Log("AssetDataBase.load:" + loadResourceName);
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
				//string ABName = ResourceNameToBundleName(fAB);
				string namePreload = PathToFileUri(Combine(platformBundlePath, fAB));
				{
					if (!AssetBundleDic.ContainsKey(namePreload))
					{
						using (WWW www = new WWW(namePreload))
						{
							while (!www.isDone && (www.error == null || www.error.Equals("")))
							{
								yield return null;
							}
							if (www.error != null)
							{
								string info = "www.error != null :" + www.url + " " + www.error;
								www.Dispose();
								throw new NullReferenceException(info);
							}
							else if (www.isDone)
							{
								var bundleTar = www.assetBundle;
								string key = www.url;
								www.Dispose();
								if (bundleTar != null)
								{
									AssetBundleDic.Add(key, bundleTar);
								}
								else
								{
									throw new NullReferenceException("bundleTar == null :" + key);
								}
							}
						}
					}
				}
				CurNeedPreloadAB.RemoveAt(0);
			}
		}
		LoadResCor = null;
	}
	/// <summary>
	/// UseAssetBundle用来设置编辑器下是否使用AB模式，非编辑器只能用AB模式
	/// </summary>
#if UNITY_EDITOR
	public static bool UseAssetBundle
	{
		get
		{
			return EditorPrefs.GetBool(AssetBundleHelper.UseAssetBundleKey);
		}
		set
		{
			EditorPrefs.SetBool(AssetBundleHelper.UseAssetBundleKey, value);
		}
	}
	public const string UseAssetBundleKey = "UseAssetBundle";
#else
	public const bool UseAssetBundle = true;
#endif

	public static void LoadManifestAndPreloadInfo()
	{
		//这里毫无疑问需要同步加载，但是发现使用了using (WWW www = new WWW(platformManifestPath))的地方好像确实是同步的，可能是using有自动释放机制所以它不得不同步的原因？
		string streamingAssetsPath = (PathToPlatformFormat(GetStreamingAssetsPath()));
		string platformBundlePath = (Combine(streamingAssetsPath, GetPlatformPathName()));
		//manifest
		if (manifest == null)
		{
			string platformManifestPath = PathToFileUri(Combine(platformBundlePath, GetPlatformPathName()));
			using (WWW www = new WWW(platformManifestPath))
			{
				if (www.error != null)
				{
					Debug.Assert(false, "www.error != null " + www.error);
				}
				else
				{
					var bundle = www.assetBundle;
					if (bundle != null)
					{
						manifest = (AssetBundleManifest)bundle.LoadAsset("AssetBundleManifest");
						if (manifest != null)
						{
						}
						else
						{
							throw new NullReferenceException("mainfest == null");
						}
						bundle.Unload(false);
					}
					else
					{
						throw new NullReferenceException("var bundle = www.assetBundle; bundle == null");
					}
				}
			}
		}
		//PreloadInfo
		//这不是一个必须读取成功的文件
		try
		{
			{
				string resName = AssetBundleUseAnalysis.GetXmlNameWithExt();
				string platformManifestPath = PathToFileUri(Combine(platformBundlePath, AssetBundlePrefixPath, ResourceNameToBundleName(resName)));
				using (WWW www = new WWW(platformManifestPath))
				{
					if (www.error != null)
					{
						//Debug.Assert(false, "www.error != null " + www.error);
						Debug.LogWarning(@"www.error != null " + www.error);
					}
					else
					{
						if (www.assetBundle != null)
						{
							var asset = www.assetBundle.LoadAsset<TextAsset>(resName);
							if (asset != null)
							{
								XmlDocument doc = new XmlDocument();
								doc.LoadXml(asset.text);
								AssetBundleUseAnalysis.LoadPreloadInfo(doc);
							}
							else
							{
								Debug.LogWarning(@"if (asset != null) : " + platformManifestPath + " Res: " + resName);
							}
							www.assetBundle.Unload(false);
						}
						else
						{
							Debug.LogWarning(@"if (www.assetBundle != null) : " + platformManifestPath);
						}
					}
				}
			}
		}
		catch(Exception e)
		{
			Debug.LogWarning(e);
		}

	}
}

public class AssetBundleRecord
{
	public static bool bABUseAnalysis = true;
	void OutputXml()
	{
		AssetBundleUseAnalysis.OutputABUseXml();
	}
	private Dictionary<string, AssetBundle> assetBundleDic = new Dictionary<string, AssetBundle>();

	public bool ContainsKey(string key, bool bAnalysis = true)
	{
		if(bABUseAnalysis && bAnalysis)
		{
			AssetBundleUseAnalysis.AddABUse(key.ToLower(), AssetBundleUseAnalysis.ABDetailRecord.RecordType.ContainsKey);
		}
		PrintLog();
		return assetBundleDic.ContainsKey(key.ToLower());
	}

	public AssetBundle GetAssetBundle(string key)
	{
		if (bABUseAnalysis)
		{
			AssetBundleUseAnalysis.AddABUse(key.ToLower(), AssetBundleUseAnalysis.ABDetailRecord.RecordType.Get);
		}
		PrintLog();
		return assetBundleDic[key.ToLower()];
	}
	public void Add(string key, AssetBundle bundle)
	{
		if (bABUseAnalysis)
		{
			AssetBundleUseAnalysis.AddABUse(key.ToLower(), AssetBundleUseAnalysis.ABDetailRecord.RecordType.Add);
		}
		assetBundleDic.Add(key.ToLower(), bundle);
		PrintLog();
	}
	static int mm = 0;
	public List<UnityEngine.Object> loadAllDPs(Dictionary<string, string> keys)
	{
		string key = "";
		List<UnityEngine.Object> ret = new List<UnityEngine.Object>();
		foreach (var k in keys)
		{
			key = k.Key.ToLower();
			if (assetBundleDic.ContainsKey(key))
			{
				ret.AddRange(assetBundleDic[key].LoadAllAssets());
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
		if (bABUseAnalysis)
		{
			Unload(keys.Select(k=>k.Key));
		}
	}
	public void Unload(IEnumerable<string> keys ,string TarAB = "")
	{
		//mm++;
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
				assetBundleDic[key].Unload(false);
				assetBundleDic.Remove(key);
			}
		}
		foreach (var k in keys)
		{
			key = k.ToLower();
			if (assetBundleDic.ContainsKey(key))
			{
				//Debug.LogWarning("Unload:" + mm + ":"+ assetBundleDic[key].name);
				
				assetBundleDic[key].Unload(false);
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
			ab.Value.Unload(false);
		}
		assetBundleDic.Clear();
	}
}

public class Tuple<T1,T2,T3>
{
	public T1 t1;
	public T2 t2;
	public T3 t3;
	public Tuple(T1 tt1, T2 tt2, T3 tt3)
	{
		t1 = tt1;
		t2 = tt2;
		t3= tt3;
	}
}
