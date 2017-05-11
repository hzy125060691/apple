using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor;

public static partial class AssetBundleSettingHelperEditor
{
	/// <summary>
	/// 根据打包目标平台，返回一个目标平台可能的情况
	/// 这个并不用十分详细精准，本打包方式只区分了微软 苹果 和谷歌三家公司的系统，区分也是用公司名，三家公司的下的平台加载时使用的路径都是一个名字
	/// </summary>
	/// <param name="bt">目标类型</param>
	/// <returns>一个假想的运行时平台</returns>
	public static RuntimePlatform BuildTargetToRuntimePlatform(BuildTarget bt)
	{
		RuntimePlatform platform;
		switch (bt)
		{
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
				platform = RuntimePlatform.WindowsEditor;
				break;
			case BuildTarget.iOS:
			case BuildTarget.StandaloneOSXIntel:
			case BuildTarget.StandaloneOSXUniversal:
			case BuildTarget.StandaloneOSXIntel64:
				platform = RuntimePlatform.OSXEditor;
				break;
			case BuildTarget.Android:
				platform = RuntimePlatform.Android;
				break;
			default:
				platform = RuntimePlatform.XboxOne;
				break;
		}
		return platform;
	}
	public static void SaveABSH(AssetBundleSettingHelper absh, ABPInfo abpi)
	{
		EditorUtility.SetDirty(absh);
		AssetDatabase.SaveAssets();
		// 		EditorApplication
		// 			Application
		// 			Editor.
		//absh.SetDirty();
	}
	public static void CreateABSH(AssetBundleSettingHelper absh, ABPInfo abpi)
	{
		AssetDatabase.CreateAsset(absh, abpi.FullName_RelativePath);
	}

	public static AssetBundleSettingHelper GetABSH(out ABPInfo abpInfo)
	{
		AssetBundleSettingHelper absh = null;

		abpInfo = GetABPInfo();
		var fi = new FileInfo(abpInfo.FullName);
		if (fi.Exists)
		{
			absh = AssetDatabase.LoadAssetAtPath<AssetBundleSettingHelper>(abpInfo.FullName_RelativePath);
			if (absh == null)
			{
				Debug.LogError("Not found :" + abpInfo.FullName + " then create");
			}
			else
			{
				return absh;
			}
		}

		absh = ScriptableObject.CreateInstance<AssetBundleSettingHelper>();
		CreateABSH(absh, abpInfo);
		return absh;
	}
	private static ABPInfo GetABPInfo()
	{
		var tt = typeof(AssetBundleSettingHelper);
		var abp = new ABPInfo(tt.Name, ".cs");
		var filesDir = SearchFolders(Application.dataPath, abp.NameWithExt);
		if (filesDir.Count <= 0)
		{
			//没有找到本文件路径，把Application.dataPath目录放进去吧
			filesDir.Add(Application.dataPath);
		}
		else if (filesDir.Count > 1)
		{
			//找到多个同名文件，这个需要打个日志
			foreach (var f in filesDir)
			{
				Debug.LogError("same file ？ " + f + abp.NameWithExt);
			}
		}
		var abpInfo = new ABPInfo(AssetBundleSettingHelper.ABSHName, AssetBundleSettingHelper.ABSHExtName);
		foreach (var f in filesDir)
		{
			abpInfo.SetPath(f);
			var fi = new FileInfo(Path.Combine(f, abpInfo.NameWithExt));
			if (fi.Exists)
			{
				return abpInfo;
			}
		}

		return abpInfo;
	}

	/// <summary>
	/// 递归查找目标文件所在位置
	/// </summary>
	/// <param name="path">搜索的目录</param>
	/// <param name="name">搜索的目标</param>
	/// <returns>当前目录中匹配的文件</returns>
	private static List<string> SearchFolders(string path, string name)
	{
		List<string> ret = new List<string>();
		DirectoryInfo fileDir = new DirectoryInfo(path);
		var dirs = fileDir.GetDirectories();
		foreach (var dir in dirs)
		{
			ret.AddRange(SearchFolders(dir.FullName, name));
		}

		var files = fileDir.GetFiles();
		foreach (var f in files)
		{
			if (f.Name.ToLower().Equals(name.ToLower()))
			{
				ret.Add(f.DirectoryName);
			}
		}
		return ret;
	}

	public static ABPInfo GetABOutputABPIByVersion(this AssetBundleSettingHelper absh, string versionName, string name, string extName = "")
	{
		if (extName.Equals(""))
		{
			extName = absh.AssetBundelExtName;
		}
		string path = Path.Combine(absh.ABOutputPath, versionName);
		ABPInfo abpi = new ABPInfo(name, extName, relativePath: path);
		return abpi;
	}
	/// <summary>
	/// 根据versionName获得输出AB依赖信息文件的路径
	/// </summary>
	/// <param name="versionName">一般是平台名字，也可以加其他文件夹在</param>
	/// <returns>输出路径</returns>
	public static ABPInfo GetABDependenciesXmlPath(this AssetBundleSettingHelper absh, string versionName)
	{
		return absh.GetABOutputABPIByVersion(versionName, AssetBundleSettingHelper.ABDependenciesInfoName, AssetBundleSettingHelper.xmlExtName);
	}
	/// <summary>
	/// 根据versionName获得输出AB差异文件的路径
	/// </summary>
	/// <param name="versionName">一般是平台名字，也可以加其他文件夹在</param>
	/// <returns>输出路径</returns>
	public static ABPInfo GetDifferXmlPath(this AssetBundleSettingHelper absh, string versionName)
	{
		return absh.GetABOutputABPIByVersion(versionName, AssetBundleSettingHelper.ABDifferInfoName, AssetBundleSettingHelper.xmlExtName);
	}
	/// <summary>
	/// 根据versionName获得输出AB信息文件的路径
	/// </summary>
	/// <param name="versionName">一般是平台名字，也可以加其他文件夹在</param>
	/// <returns>输出路径</returns>
	public static ABPInfo GetABInfoXmlPath(this AssetBundleSettingHelper absh, string versionName)
	{
		return absh.GetABOutputABPIByVersion(versionName, AssetBundleSettingHelper.ABVersionInfoName, AssetBundleSettingHelper.xmlExtName);
	}
	public static ABPInfo GetABXmlPath(this AssetBundleSettingHelper absh, string versionName, string name)
	{
		return absh.GetABOutputABPIByVersion(versionName, name, AssetBundleSettingHelper.xmlExtName);
	}
	/// <summary>
	/// 把AB目录和版本信息组合成一个运行时能找到的目录
	/// </summary>
	/// <param name="versionName">版本信息，这是一个能区分各大主流平台的名字</param>
	/// <returns></returns>
	public static ABPInfo GetAssetBundleOutputPath(this AssetBundleSettingHelper absh, string versionName)
	{
		return absh.GetABOutputABPIByVersion(versionName, "");
	}

	[MenuItem("AssetBundle/test")]
	private static void test()
	{
		Debug.Log("Application.dataPath:" + Application.dataPath);
		Debug.Log("Application.absoluteURL:" + Application.absoluteURL);
		Debug.Log("Application.persistentDataPath:" + Application.persistentDataPath);
		Debug.Log("Application.streamingAssetsPath:" + Application.streamingAssetsPath);
		Debug.Log("Application.temporaryCachePath:" + Application.temporaryCachePath);

		Debug.LogError("UnityEditor.EditorUserBuildSettings.activeBuildTarget:" + UnityEditor.EditorUserBuildSettings.activeBuildTarget);
		Debug.LogError("UnityEditor.EditorUserBuildSettings.selectedStandaloneTarget:" + UnityEditor.EditorUserBuildSettings.selectedStandaloneTarget);
	}
}
