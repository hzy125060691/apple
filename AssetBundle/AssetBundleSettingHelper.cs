using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;
using System.Globalization;

/// <summary>
/// 这里主要是个配置文件，配置各种参数，包括打包路径，输出路径这种，还处理了各种路径相关的东西。
/// </summary>
public partial class AssetBundleSettingHelper : ScriptableObject 
{
	public string AssetBundelExtName = @".assetbundle";//assetbundle的扩展名

	#region WinPC android iOS系列的简单命名,同一用公司名称代替,不区分具体平台
	public string WindowsSimplifyName = @"Microsoft";
	public string AndroidSimplifyName = @"Google";
	public string OSXSimplifyName = @"Apple";
	#endregion

	public string NeedBuildABPath = @"Assets/Resources";//需要打包的目录，相对Unity工程根目录
	public string ABOutputPath = @"AssetBundles";//打包输出目录，这个目录目前看来并不需要修改

	public bool EditorUseAB = false;//是否使用AB，这个在非编辑器下没有用处，但是也放在这吧

	public bool IsABUseAnalysis = true;

	public bool IsAsynLoadRes = false;

	public bool UseSteamingAssetRes = true;

	#region 这几个常量应该是不会改变的
	public const string ABSHName = "ABSH";
	public const string ABSHExtName = ".asset";
	public const string Asset_RelativePath = "Assets";
	#endregion

	/// <summary>
	/// UseAssetBundle用来设置编辑器下是否使用AB模式，非编辑器只能用AB模式
	/// </summary>
	public bool UseAssetBundle
	{
		get
		{
#if UNITY_EDITOR
			return EditorUseAB;
#else
			return true;
#endif
		}
		set
		{
			EditorUseAB = value;
		}
	}

	#region 这一部分是生成的XML相关的，可以改也可以不改，人能看懂就行
	public const string xmlExtName = @".xml";

	public const string ABVersionInfoName = @"ABVersionInfo";                           //输出所有ab信息的文件名
	public const string ABDifferInfoName = @"ABDifferInfo";                             //对比两次打包不同AB信息的文件名
	public const string ABDependenciesInfoName = @"ABDependenciesInfo";                 //输出所有打包资源依赖项信息文件
	public const string xmlDataAnalysisName = @"DataAnalysis";

	public const string xmlNode_AssetBundles = @"AssetBundles";                        //输出信息中XML node的名字
	public const string xmlNode_NewAssetBundles = @"NewAssetBundles";                  //输出信息中XML node的名字
	public const string xmlNode_DelAssetBundles = @"DelAssetBundles";                  //输出信息中XML node的名字
	public const string xmlNode_ChangedAssetBundles = @"ChangedAssetBundles";          //输出信息中XML node的名字
	public const string xmlAttribute_CreateTime = @"Time";                                 //输出信息中XML Attribute的名字
	public const string xmlNode_AB = @"AssetBundle";                                   //输出信息中XML node的名字
	public const string xmlNode_Name = @"Name";                                        //输出信息中XML node的名字
	public const string xmlNode_Hash = @"Hash";                                        //输出信息中XML node的名字
	public const string xmlNode_OldHash = @"OldHash";                                  //输出信息中XML node的名字
	public const string xmlNode_NewHash = @"NewHash";                                  //输出信息中XML node的名字
	public const string xmlNode_ABDependencies = @"Dependencies";                      //输出信息中XML node的名字
	public const string xmlNode_ABDependencies_P = @"Dependencies_P";                 //输出信息中XML node的名字
	public const string xmlNode_ABDependency_P = @"Dependency_P";                     //输出信息中XML node的名字
	public const string xmlNode_ABDependency = @"Dependency";                          //输出信息中XML node的名字
	public const string xmlNode_ABDependencies_R = @"Dependencies_R";                 //输出信息中XML node的名字
	public const string xmlNode_ABDependency_R = @"Dependency_R";                     //输出信息中XML node的名字
	public const string xmlNode_ABDependency_Count = @"Count";                        //输出信息中XML node的名字
	public const string xmlNode_ABSize = @"Size";                                      //输出信息中XML node的名字
	public const string xmlNode_ABSizeXB = @"SizeXB";                                  //输出信息中XML node的名字

	public const string xmlDataAnalysisDir = @"DataAnalysis";
	public const string xmlABAnalysisDir = @"ABAnalysis";
	public const string xmlABUsesDir = @"ABUses";
	public const string xmlABUses_OutdateDir = @"ABUses_OutDate";

	public const string xmlNode_Title = @"ABUse";
	public const string xmlNode_Title_2 = @"Detail";
	public const string xmlNode_Type = @"Type";
	public const string xmlNode_Time = @"Time";
	public const string xmlNode_Stage = @"Stage";
	public const string xmlNode_StageBeginTime = @"StageBeginTime";
	public const string xmlNode_StageBeginTime_Ticks = @"StageBeginTicks";
	public const string xmlNode_Time_Ticks = @"Ticks";
	public const string xmlNode_PhaseRoot = @"PhaseRoot";
	public const string xmlNode_Phase = @"Phase";
	public const string xmlNode_ABPri = @"Priority";
	#endregion

	public ABPInfo GetCurPlatformManifestPath()
	{
		string versionName = GetPlatformPathName();
		if(UseSteamingAssetRes)
		{
			return GetStreamingAssetABPIByVersion(versionName, versionName, "");
		}
		else
		{
			return GetPersistentDataABPIByVersion(versionName, versionName, "");
		}
	}
	public ABPInfo GetCurPlatformStreamingAssetManifestPath()
	{
		string versionName = GetPlatformPathName();
		return GetStreamingAssetABPIByVersion(versionName, versionName, "");
	}
	public ABPInfo GetCurPlatformABPath(string name = "")
	{
		if (UseSteamingAssetRes)
		{
			return GetStreamingAssetABPIByVersion(GetPlatformPathName(), name, AssetBundelExtName);
		}
		else
		{
			return GetPersistentDataABPIByVersion(GetPlatformPathName(), name, AssetBundelExtName);
		}
	}
	public ABPInfo GetCurPlatformStreamingABPath(string name = "")
	{
		return GetStreamingAssetABPIByVersion(GetPlatformPathName(), name, AssetBundelExtName);
	}
// 	public ABPInfo GetPersistentDataABPath(string name = "")
// 	{
// 		return GetPersistentDataABPIByVersion(GetPlatformPathName(), name, AssetBundelExtName);
// 	}
	public ABPInfo GetStreamingAssetABPath(string versionName)
	{
		return GetStreamingAssetABPIByVersion(versionName, "", AssetBundelExtName);
	}
	private ABPInfo GetStreamingAssetABPIByVersion(string versionName, string name, string extName)
	{
		string path = Path.Combine(GetStreamingAssetsPath(), versionName);
		ABPInfo abpi = new ABPInfo(name, extName, path: path);
		return abpi;
	}
	public ABPInfo GetCurABVersionXmlStreamingAssetPath()
	{
		return GetStreamingAssetABPIByVersion(GetPlatformPathName(), VersionXmlInfo.FileName, xmlExtName);
	}
	public ABPInfo GetCurABVersionXmlPersistentDataPath()
	{
		return GetPersistentDataABPIByVersion(GetPlatformPathName(), VersionXmlInfo.FileName, xmlExtName);
	}
	private ABPInfo GetPersistentDataABPIByVersion(string versionName, string name, string extName)
	{
		string path = Path.Combine(GetPersistentDataPath(), versionName);
		ABPInfo abpi = new ABPInfo(name, extName, path: path);
		return abpi;
	}
	public ABPInfo GetABUsesABPI()
	{
		ABPInfo abpi = new ABPInfo(xmlABUsesDir + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss"), xmlExtName, relativePath: Path.Combine(xmlABAnalysisDir, xmlABUsesDir));
		return abpi;
	}
	public ABPInfo GetABUses_OutdateABPI()
	{
		ABPInfo abpi = new ABPInfo("", xmlExtName, relativePath: Path.Combine(xmlABAnalysisDir, xmlABUses_OutdateDir));
		return abpi;
	}

	/// <summary>
	/// 这个获得的路径位置是分析结果xml在打成AB前应该存在的位置，也就是要打包的目录下
	/// </summary>
	/// <returns></returns>
	public ABPInfo GetDataAnalysisXmlMoveTargetABPI()
	{
		ABPInfo abpi = new ABPInfo(xmlDataAnalysisName, xmlExtName, relativePath: NeedBuildABPath);
		return abpi;
	}
	/// <summary>
	/// 这里获得是分析结果输出位置
	/// </summary>
	/// <returns></returns>
	public ABPInfo GetDataAnalysisXmlABPI()
	{
		ABPInfo abpi = new ABPInfo(xmlDataAnalysisName, xmlExtName, relativePath: Path.Combine(xmlABAnalysisDir, xmlDataAnalysisDir));
		return abpi;
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
	/// 通过资源名获得AB名
	/// 这个可以自己制作规则，资源如何与AB对应
	/// </summary>
	/// <param name="resourceName">资源名称</param>
	/// <returns>资源对应的AB名称</returns>
	public string ResourceNameToBundleName(string resourceName)
	{
		return resourceName + AssetBundelExtName;
	}

	/// <summary>
	/// 根据当前运行的平台，获得一个简化的能区分大体平台的一个名字
	/// </summary>
	/// <returns>针对windows OS Android有所区分的一个名字，区分不是很详细</returns>
	public string GetPlatformPathName()
	{
#if UNITY_EDITOR
		RuntimePlatform platform;
		switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
		{
			case UnityEditor.BuildTarget.StandaloneWindows:
			case UnityEditor.BuildTarget.StandaloneWindows64:
				platform = RuntimePlatform.WindowsEditor;
				break;
			case UnityEditor.BuildTarget.iOS:
			case UnityEditor.BuildTarget.StandaloneOSXIntel:
			case UnityEditor.BuildTarget.StandaloneOSXUniversal:
			case UnityEditor.BuildTarget.StandaloneOSXIntel64:
				platform = RuntimePlatform.OSXEditor;
				break;
			case UnityEditor.BuildTarget.Android:
				platform = RuntimePlatform.Android;
				break;
			default:
				platform = RuntimePlatform.XboxOne;
				break;
		}
		return RuntimePlatformToSimplifyName(platform);
#else
		return RuntimePlatformToSimplifyName(Application.platform);
#endif
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
				if(!path.StartsWith(JarFile))
				{
					path = JarFile + path;
				}
				//platformName = "Google";
				break;
			default:
				//platformName = "Default";
				break;
		}
		return path;
	}

	/// <summary>
	/// 获取StreamingAssets路径
	/// 这里把android下的jar:file:// 去掉,需要它的时候会有其他东西加上的
	/// </summary>
	/// <returns>StreamingAssets路径</returns>
	private const string JarFile = @"jar:file://";
	public static string GetStreamingAssetsPath()
	{
		
		string ret = Application.streamingAssetsPath;
		switch (Application.platform)
		{
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.IPhonePlayer:
				break;
			case RuntimePlatform.Android:
				//5.4后android平台，assetbundle。loadfromfile后边可以使用带“jar:file:// ”的路径访问了
				//ret = ret.Substring(ret.IndexOf(JarFile) + JarFile.Length);
				break;
			default:
				break;
		}
		return ret;
	}
	/// <summary>
	/// 获取persistentDataPath路径
	/// </summary>
	/// <returns>persistentDataPath路径</returns>
	private static string GetPersistentDataPath()
	{
		return Application.persistentDataPath;
	}

	/// <summary>
	/// 根据制定的RuntimePlatform，获得一个简化的能区分大体平台的一个名字
	/// </summary>
	/// <param name="pf">制定的RuntimePlatform</param>
	/// <returns>针对windows OS Android有所区分的一个名字，区分不是很详细</returns>
	public string RuntimePlatformToSimplifyName(RuntimePlatform pf)
	{
		string platformName = "";
		switch (pf)
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
	/// /和\\的转换，转换成当前运行系统下的模式
	/// 需要注意的是Url的协议中的//不要调用这个进行转换
	/// 需要注意的是Url的协议中的//不要调用这个进行转换
	/// 需要注意的是Url的协议中的//不要调用这个进行转换
	/// </summary>
	/// <param name="path">需要转换的路径</param>
	/// <returns>转换后的结果</returns>
	public static string PathToPlatformFormat(string path)
	{
		if(path.Equals(""))
		{
			return "";
		}
		string formatPath = "";
		char slash = GetSlash();
		switch (Application.platform)
		{
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.WindowsPlayer:
				formatPath = path.Replace('/', slash);
				break;
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.Android:
				formatPath = path.Replace('\\', slash);
				break;
			default:
				formatPath = path.Replace('\\', slash);
				break;
		}
		return formatPath;
	}

	public static char GetSlash()
	{
		char slash = '/';
		switch (Application.platform)
		{
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.WindowsPlayer:
				slash = '\\';
				break;
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.Android:
				slash = '/';
				break;
			default:
				slash = '/';
				break;
		}
		return slash;
	}
#if UNITY_EDITOR

	

#endif
}

/// <summary>
/// AssetBundlePath类，记录了某一个文件的各种路径。
/// </summary>
public class ABPInfo
{
	#region 这是跟文件名有关的
	public string Name;                     //普通的名字，例如"shellroot.prefab"
	public string NameWithExt;              //带扩展名的名字,例如"shellroot.prefab.assetbundle"
	public string FullName;                 //名字全路径,例如"c:\\hzy\\work\\svn_ft\\19_tank_ab\\client\\tankproject\\assets\\streamingassets\\apple\\assets\\resources\\prefabs\\shellroot.prefab.assetbundle"
	public string FullName_RelativePath;    //完整名字的相对路径,例如"assets\\streamingassets\\apple\\assets\\resources\\prefabs\\shellroot.prefab.assetbundle"
	public string FullNameWithoutExtName_Relative; //无扩展名名字的相对路径,例如"assets\\streamingassets\\apple\\assets\\resources\\prefabs\\shellroot.prefab"
	public string URI;                      //Uri,例如"file://c:\\hzy\\work\\svn_ft\\19_tank_ab\\client\\tankproject\\assets\\streamingassets\\apple\\assets\\resources\\prefabs\\shellroot.prefab.assetbundle"
	public string AssetName;                        //资源名称，例如"assets\\resources\\prefabs\\shellroot.prefab"
	public string DependencyName;           //打包时输出的名字，也是加载时求依赖的名字，例如"assets\\resources\\prefabs\\shellroot.prefab.assetbundle"
	#endregion
	public string ExtName;//扩展名，例如".assetbundle"
	#region 这是跟路径有关的
	public string Dir_Full;//全路径，例如"c:\\hzy\\work\\svn_ft\\19_tank_ab\\client\\tankproject\\assets\\streamingassets\\apple\\assets\\resources\\prefabs"
	public string Dir_Relative;//相对路径，例如"assets\\streamingassets\\apple\\assets\\resources\\prefabs"
	#endregion
	public ABPInfo(string name,string extName, string path = "", bool bFindPath = false,string relativePath = "", bool bFindRelativePath = false)
	{
		name = AssetBundleSettingHelper.PathToPlatformFormat(name.ToLower());
		path = AssetBundleSettingHelper.PathToPlatformFormat(path.ToLower());
		relativePath = AssetBundleSettingHelper.PathToPlatformFormat(relativePath.ToLower());

		ExtName = extName.ToLower(); ;
		string tmpRP = "";
		if (!name.Equals(""))
		{
			//name中可能自己就带有路径，所以要剥离出来
			char slash = AssetBundleSettingHelper.GetSlash();
			int idxLS = name.LastIndexOf(slash);
			if(idxLS > 0)
			{
				//名字里包含路径
				tmpRP = name.Substring(0, idxLS);
				Name = name.Substring(idxLS + 1);
			}
			else
			{
				Name = name;
			}
			//名字最末端如果正是扩展名，那么把它干掉，所以一定不能出现连续相同扩展名在结尾，如XXXXX.assetbundle.assetbundle
			if(!ExtName.Equals(""))
			{
				if (Name.EndsWith(ExtName))
				{
					Name = Name.Substring(0, Name.Length - ExtName.Length);
				}
			}
			NameWithExt = Name + ExtName;
		}

		AssetName = name;
		if (!AssetName.Equals(""))
		{
			if(AssetName.EndsWith(ExtName))
			{
				AssetName = AssetName.Substring(0, AssetName.Length - ExtName.Length);

			}
			DependencyName = AssetName + ExtName;
		}

		if (!path.Equals("") || bFindPath)
		{
			SetPath(Path.Combine(path, tmpRP));
		}
		else if(!relativePath.Equals("") || bFindRelativePath)
		{
			SetRelativePath(Path.Combine(relativePath, tmpRP));
		}



		CaseToLower();
	}
	private void CaseToLower()
	{
		Name = Name == null ? null : Name.ToLower();
		NameWithExt = NameWithExt == null ? null : NameWithExt.ToLower();
		FullName = FullName == null ? null : FullName.ToLower();
		FullName_RelativePath = FullName_RelativePath == null ? null : FullName_RelativePath.ToLower();
		FullNameWithoutExtName_Relative = FullNameWithoutExtName_Relative == null ? null : FullNameWithoutExtName_Relative.ToLower();
		URI = URI == null ? null : URI.ToLower();
		AssetName = AssetName == null ? null : AssetName.ToLower();
		DependencyName = DependencyName == null ? null : DependencyName.ToLower();
		ExtName = ExtName == null ? null : ExtName.ToLower();
		Dir_Full = Dir_Full == null ? null : Dir_Full.ToLower();
		Dir_Relative = Dir_Relative == null ? null : Dir_Relative.ToLower();
	}

	public void SetRelativePath(string rPath)
	{
		Dir_Relative = AssetBundleSettingHelper.PathToPlatformFormat(rPath);

		var rootPath = AssetBundleSettingHelper.PathToPlatformFormat(Application.dataPath);
		rootPath = rootPath.Substring(0, rootPath.Length - AssetBundleSettingHelper.Asset_RelativePath.Length);
		Dir_Full = Path.Combine(rootPath, Dir_Relative);
		if (NameWithExt != null)
		{
			FullName_RelativePath = (Path.Combine(Dir_Relative, NameWithExt));
			FullName = Path.Combine(rootPath, FullName_RelativePath);
			URI = AssetBundleSettingHelper.PathToFileUri(FullName);
			FullNameWithoutExtName_Relative = FullName_RelativePath.Substring(0, FullName_RelativePath.Length - ExtName.Length);
		}
	}
	public void SetPath(string path)
	{
		path = AssetBundleSettingHelper.PathToPlatformFormat(path); ;
		var rootPath = AssetBundleSettingHelper.PathToPlatformFormat(Application.dataPath);
		rootPath = rootPath.Substring(0, rootPath.Length - AssetBundleSettingHelper.Asset_RelativePath.Length);
		if (NameWithExt != null)
		{
			FullName = AssetBundleSettingHelper.PathToPlatformFormat(Path.Combine(path, NameWithExt));
			var index = FullName.IndexOf(rootPath) + rootPath.Length;
			if (index < FullName.Length)
			{
				FullName_RelativePath = FullName.Substring(index);
				FullNameWithoutExtName_Relative = FullName_RelativePath.Substring(0, FullName_RelativePath.Length - ExtName.Length);
			}
			else
			{
				Debug.LogError("ABPInfo::SetPath Error:" + FullName + " :pathPrefix:" + rootPath);
			}
			URI = AssetBundleSettingHelper.PathToFileUri(FullName);
		}
		Dir_Relative = path.Substring(path.IndexOf(rootPath) + rootPath.Length + 1);
		Dir_Full = path;

		
	}
}
public class VersionXmlInfo
{
	//这个文件比较特别，就都写自己这里了
	public const string FileName = @"version";
	public const string VersionTitle = @"version";
	public const string VersionName = @"version";

	public const string DateFormat = @"yyyy-MM-dd HH:mm:ss.fff";
	public DateTime version;
	private IFormatProvider culture;
	public VersionXmlInfo()
	{
		culture = new CultureInfo("zh-CN", true);
	}
	public string GetVersionString()
	{
		return version.ToString(DateFormat, culture);
	}

	public XmlDocument OutputXmlDoc()
	{
		var xmlDoc = new XmlDocument();
		XmlElement AllRoot = xmlDoc.CreateElement(VersionTitle);
		AllRoot.SetAttribute(VersionName, GetVersionString());
		xmlDoc.AppendChild(AllRoot);

		return xmlDoc;
	}
	public void InputXmlDoc(XmlDocument doc)
	{
		var ele = doc.SelectSingleNode(VersionTitle);
		if(ele != null && ele.GetType() == typeof(XmlElement))
		{
			XmlElement e = (XmlElement)ele;
			string versionStr = e.GetAttribute(VersionName);
			version = DateTime.ParseExact(versionStr, DateFormat, culture);
		}
		else
		{
			throw new TypeLoadException("if(ele.GetType() == typeof(XmlElement)) :" + ele.GetType());
		}
	}

	public bool IsUpTodate(VersionXmlInfo tar)
	{
		return version.CompareTo(tar.version)>=0;
	}
}

