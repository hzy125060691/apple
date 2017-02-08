using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// 加载bundle的类，编辑器模式下也可以通过AssetDataBase直接加载资源(需要设置一下UseAssetBundle，搜索一下，它最开始是在本文件中)
/// </summary>
public class AssetBundleHelper : MonoBehaviour
{
	public const string AssetBundelExtName = @".assetbundle";           //assetbundle的扩展名
	public const string WindowsSimplifyName = @"Microsoft";
	public const string AndroidSimplifyName = @"Google";
	public const string OSXSimplifyName = @"Apple";

	private static Queue<KeyValuePair<string, Action<UnityEngine.Object>>> NeedLoadQueue = new Queue<KeyValuePair<string, Action<UnityEngine.Object>>>();

	private static AssetBundleManifest manifest = null;
	//private static object manifest_Sum_LockObj = new object();

	private static AssetBundleRecord AssetBundleDic = new AssetBundleRecord();
	//private static Dictionary<string, AssetBundle> AssetBundleDic = new Dictionary<string, AssetBundle>();
	//private static Dictionary<string, AssetBundle> AssetBundleDic = new Dictionary<string, AssetBundle>();
	public static AssetBundleHelper Ins = null;
	private Coroutine LoadResCor = null;
	void Awake()
	{
		DontDestroyOnLoad(this);
		Ins = this;
	}
// 	void Start()
// 	{
// 	}
	public void PushResToNeedLoad(string path, Action<UnityEngine.Object> callback)
	{
		NeedLoadQueue.Enqueue(new KeyValuePair<string, Action<UnityEngine.Object>>(path, callback));
		if (LoadResCor == null)
		{
			LoadResCor = StartCoroutine(LoadResourceAsyn());
		}
	}
	void Destory()
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
		return RuntimePlatformToSimplifyName(Application.platform);
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
		while(NeedLoadQueue.Count > 0)
		{
			var needLoad = NeedLoadQueue.Dequeue();
			string loadResourceName = needLoad.Key;
			var callback = needLoad.Value;
			UnityEngine.Object newObj = null;
			if (UseAssetBundle)
			{
				//通过bundle加载
				string LoadBundleName = ResourceNameToBundleName(loadResourceName);
				string streamingAssetsPath = (PathToPlatformFormat(GetStreamingAssetsPath()));
				string platformBundlePath = (Combine(streamingAssetsPath, GetPlatformPathName()));
				string platformManifestPath = PathToFileUri(Combine(platformBundlePath, GetPlatformPathName()));
				{
					if (manifest == null)
					{
						using (WWW www = new WWW(platformManifestPath))
						{
							yield return www;
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
										Debug.Assert(false, "mainfest == null");
									}
									bundle.Unload(false);
								}
								else
								{
									Debug.Assert(false, "bundle == null");
								}
							}

						}
					}
				}
				//如果这个时候还为空直接终止
				if (manifest == null)
				{
					yield break;
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
								using (WWW wwwTmp = new WWW(nameTmp))
								{
									yield return wwwTmp;
									if (wwwTmp.error != null)
									{
										Debug.Assert(false, "wwwTmp.error != null " + wwwTmp.error);
									}
									//abs[i] = wwwTmp.assetBundle;
									//Debug.Log(nameTmp + " : " + loadResourceName);
									AssetBundleDic.Add(nameTmp, wwwTmp.assetBundle);
								}
							}
						}
					}
					string name = PathToFileUri(Combine(platformBundlePath, LoadBundleName));
					{
						if (!AssetBundleDic.ContainsKey(name))
						{
							using (WWW wwwtar = new WWW(name))
							{
								yield return wwwtar;
								if (wwwtar.error != null)
								{
									Debug.Assert(false, "wwwtar.error != null " + wwwtar.error);
								}
								else
								{
									var bundleTar = wwwtar.assetBundle;
									if (bundleTar != null)
									{
										AssetBundleDic.Add(name, bundleTar);
									}
									else
									{
										Debug.Assert(false, "bundleTar == null ");
									}
								}
							}
						}
					}
					if (!AssetBundleDic.ContainsKey(name))
					{
						yield break;
					}
					var obj = AssetBundleDic.GetAssetBundle(name).LoadAsset(loadResourceName);
					if (obj != null)
					{
						newObj = Instantiate(obj) as UnityEngine.Object;
					}
					else
					{
						Debug.Assert(false, "obj == null ");
					}
					// 				for (int i = 0; i < abs.Length; i++)
					// 				{
					// 					if (abs[i] != null)
					// 					{
					// 						abs[i].Unload(false);
					// 						abs[i] = null;
					// 					}
					// 				}
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
					Debug.Assert(false, "obj == null here ");
				}
				Debug.Log("AssetDataBase.load");
#endif
			}

			if (newObj != null)
			{
				callback(newObj);
			}
			else
			{
				Debug.Assert(false, "newObj == null ");
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
}

public class AssetBundleRecord
{
	private Dictionary<string, AssetBundle> assetBundleDic = new Dictionary<string, AssetBundle>();
	private List<KeyValuePair<string, string>> assetBundleUseTime = new List<KeyValuePair<string, string>>();

	public bool ContainsKey(string key)
	{
		assetBundleUseTime.Add(new KeyValuePair<string, string>(key, DateTime.Now.ToString() + ":ContainsKey"));
		PrintLog();
		return assetBundleDic.ContainsKey(key);
	}

	public AssetBundle GetAssetBundle(string key)
	{
// 		if (!ContainsKey(key))
// 		{
// 			return null;
// 		}
		assetBundleUseTime.Add(new KeyValuePair<string, string>(key, DateTime.Now.ToString() + ":Get"));
		PrintLog();
		return assetBundleDic[key];
	}
	public void Add(string key, AssetBundle bundle)
	{
		assetBundleDic.Add(key, bundle);
		assetBundleUseTime.Add(new KeyValuePair<string, string>(key, DateTime.Now.ToString() + ":Add"));
		PrintLog();
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
		foreach (var ab in assetBundleDic)
		{
			ab.Value.Unload(false);
		}
		assetBundleDic.Clear();
	}
}

