﻿#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Xml;
using System;
using System.Linq;
/// <summary>
/// 这个类只能在编辑器里使用，用来创建assetbundle和在打包前将assetbundle移动到StreamingAssets下指定位置。
/// </summary>
public class BuildBundleManager
{
	public static string needBuildAssetBundlePath                                   //将要打包的目录，这个可以随意更改
	{
		get
		{
			return AssetBundleHelper.AssetBundlePrefixPath;
		}
	}
	public const string assetBundlePath = @"AssetBundles";                          //打包输出目录，这个目录目前看来并不需要修改

	#region 默认的某些属性
	public const string AssetBundlePathKey = @"AssetBundlePath";
	public static string AssetBundlePath
	{
		get
		{
			var tar = EditorPrefs.GetString(AssetBundlePathKey);
			if (tar == null || tar.Equals(""))
			{
				return assetBundlePath;
			}
			return tar;
		}
		set
		{
			EditorPrefs.SetString(AssetBundlePathKey, value);
		}
	}
	public const string NeedBuildAssetBundlePathKey = @"NeedBuildAssetBundlePath";
	public static string NeedBuildAssetBundlePath
	{
		get
		{
			var tar = EditorPrefs.GetString(NeedBuildAssetBundlePathKey);
			if (tar == null || tar.Equals(""))
			{
				return needBuildAssetBundlePath;
			}
			return tar;
		}
		set
		{
			EditorPrefs.SetString(NeedBuildAssetBundlePathKey, value);
		}
	}
	#endregion

	#region 这一部分是生成的XML相关的，可以改也可以不改，人能看懂就行
	public const string ABVersionInfoName = @"ABVersionInfo.xml";                           //输出所有ab信息的文件名
	public const string ABDifferInfoName = @"ABDifferInfo.xml";                             //对比两次打包不同AB信息的文件名
	public const string ABDependenciesInfoName = @"ABDependenciesInfo.xml";                 //输出所有打包资源依赖项信息文件
	private const string xmlNode_AssetBundlesName = @"AssetBundles";                        //输出信息中XML node的名字
	private const string xmlNode_NewAssetBundlesName = @"NewAssetBundles";                  //输出信息中XML node的名字
	private const string xmlNode_DelAssetBundlesName = @"DelAssetBundles";                  //输出信息中XML node的名字
	private const string xmlNode_ChangedAssetBundlesName = @"ChangedAssetBundles";          //输出信息中XML node的名字
	private const string xmlAttribute_CreateTime = @"Time";                                 //输出信息中XML Attribute的名字
	private const string xmlNode_ABName = @"AssetBundle";                                   //输出信息中XML node的名字
	private const string xmlNode_NameName = @"name";                                        //输出信息中XML node的名字
	private const string xmlNode_HashName = @"hash";                                        //输出信息中XML node的名字
	private const string xmlNode_OldHashName = @"OldHash";                                  //输出信息中XML node的名字
	private const string xmlNode_NewHasHName = @"NewHash";                                  //输出信息中XML node的名字
	private const string xmlNode_ABDependenciesName = @"Dependencies";                      //输出信息中XML node的名字
	private const string xmlNode_ABDependencies_P_Name = @"Dependencies_P";                 //输出信息中XML node的名字
	private const string xmlNode_ABDependency_P_Name = @"Dependency_P";                     //输出信息中XML node的名字
	private const string xmlNode_ABDependencyName = @"Dependency";                          //输出信息中XML node的名字
	private const string xmlNode_ABDependencies_R_Name = @"Dependencies_R";                 //输出信息中XML node的名字
	private const string xmlNode_ABDependency_R_Name = @"Dependency_R";                     //输出信息中XML node的名字
	private const string xmlNode_ABDependency_Count_Name = @"Count";                        //输出信息中XML node的名字
	private const string xmlNode_ABSizeName = @"Size";                                      //输出信息中XML node的名字
	private const string xmlNode_ABSizeXBName = @"SizeXB";                                  //输出信息中XML node的名字

	#endregion

	#region 依赖信息统计
	private static MyDic ABDependenciesReverse = new MyDic();
	private static MyDic ABDependenciesPositive = new MyDic();
	class MyDic
	{
		private static Dictionary<string, DependenciesRefAndCount> allDic = new Dictionary<string, DependenciesRefAndCount>();
		public static void ClearAllDic()
		{
			allDic.Clear();
		}
		public DependenciesRefAndCount GetDRAC(string key, bool dontAdd = false)
		{
			key = AssetBundleHelper.PathToPlatformFormat(key).ToLower();
			if (!allDic.ContainsKey(key))
			{
				if (dontAdd)
				{
					return null;
				}
				var d = DependenciesRefAndCount.GetNew(key);
				allDic.Add(key, d);
			}
			return allDic[key];
			
		}
		private Dictionary<string, DependenciesRefAndCount> dic = new Dictionary<string, DependenciesRefAndCount>();
		public void Add(string key, DependenciesRefAndCount value)
		{
			key = AssetBundleHelper.PathToPlatformFormat(key).ToLower();
			dic.Add(key.ToLower(), value);
		}
		public bool ContainsKey(string key)
		{
			key = AssetBundleHelper.PathToPlatformFormat(key).ToLower();
			return dic.ContainsKey(key);
		}
		public DependenciesRefAndCount this[string key]
		{
			get
			{
				key = AssetBundleHelper.PathToPlatformFormat(key).ToLower();
				return dic[key];
			}
			set
			{
				key = AssetBundleHelper.PathToPlatformFormat(key).ToLower();
				dic[key] = value;
			}
		}
		public Dictionary<string, DependenciesRefAndCount> GetDic()
		{
			return dic;
		}
		public void Clear()
		{
			dic.Clear();
		}
	}
	class DependenciesRefAndCount
	{
		private string key;
		public string Key
		{
			get { return key; }
		}
		private int count;
		public int Count
		{
			get { return count; }
		}
		private List<string> refs;
		public List<string> Refs
		{
			get { return refs; }
		}
		private long finalSize;
		public long FinalSize
		{
			get
			{
				return finalSize;
			}
			set
			{
				finalSize = value;
			}
		}
		public static DependenciesRefAndCount  GetNew(string sKey)
		{
			sKey = AssetBundleHelper.PathToPlatformFormat(sKey).ToLower();
			return new DependenciesRefAndCount(sKey);
		}
		DependenciesRefAndCount(string sKey)
		{
			sKey = AssetBundleHelper.PathToPlatformFormat(sKey).ToLower();
			key = sKey;
			count = 0;
			finalSize = 0;
			refs = new List<string>();
		}
		public void AddRef(string sRef)
		{
			sRef = AssetBundleHelper.PathToPlatformFormat(sRef).ToLower();
			count++;
			refs.Add(sRef);
		}
	}

	#endregion
	/// <summary>
	/// 生成资源依赖信息
	/// </summary>
	/// <param name="versionName">一般是平台名字，也可以加其他文件夹在</param>
	private static void GenABDependenciesInfo(string versionName)
	{
		string filepath = (GetABDependenciesXmlPath(versionName));
		XmlDocument xmlDoc = null;
		//创建XML文档实例  
		xmlDoc = new XmlDocument();
		XmlElement AllRoot = xmlDoc.CreateElement(xmlNode_ABDependenciesName);
		//创建个时间属性，可以更直观的对比不同版本的
		AllRoot.SetAttribute(xmlAttribute_CreateTime, DateTimeString);
		xmlDoc.AppendChild(AllRoot);
		//正向依赖
		XmlElement ABDs_P = xmlDoc.CreateElement(xmlNode_ABDependencies_P_Name);
		foreach (var abd in ABDependenciesPositive.GetDic())
		{
			XmlElement ABD_P = xmlDoc.CreateElement(xmlNode_ABDependency_P_Name);
			ABD_P.SetAttribute(xmlNode_ABName, abd.Key);
			ABD_P.SetAttribute(xmlNode_ABDependency_Count_Name, abd.Value.Count.ToString());
			if (abd.Value.FinalSize > 0 && abd.Value.FinalSize < 1024)
			{
				ABD_P.SetAttribute(xmlNode_ABSizeXBName, ((float)abd.Value.FinalSize).ToString() + " B");
			}
			else if (abd.Value.FinalSize >= 1024 && abd.Value.FinalSize < 1024 * 1024)
			{
				ABD_P.SetAttribute(xmlNode_ABSizeXBName, ((float)abd.Value.FinalSize / 1024).ToString("f3") + " KB");
			}
			else if (abd.Value.FinalSize >= 1024 * 1024)
			{
				ABD_P.SetAttribute(xmlNode_ABSizeXBName, ((float)abd.Value.FinalSize / (1024 * 1024)).ToString("f3") + " MB");
			}
			foreach (var d in abd.Value.Refs)
			{
				XmlElement d_P = xmlDoc.CreateElement(xmlNode_ABDependencyName);
				d_P.SetAttribute(xmlNode_NameName, d);
				var size = ABDependenciesPositive.GetDRAC(d, true).FinalSize;
				if (size > 0  && size < 1024)
				{
					d_P.SetAttribute(xmlNode_ABSizeXBName, ((float)size).ToString() + " B");
				}
				else if (size >= 1024 && size < 1024 * 1024)
				{
					d_P.SetAttribute(xmlNode_ABSizeXBName, ((float)size / 1024).ToString("f3") + " KB");
				}
				else if (size >= 1024 * 1024)
				{
					d_P.SetAttribute(xmlNode_ABSizeXBName, ((float)size / (1024 * 1024)).ToString("f3") + " MB");
				}
				ABD_P.AppendChild(d_P);
			}
			ABDs_P.AppendChild(ABD_P);
		}
		AllRoot.AppendChild(ABDs_P);
		//反向依赖
		XmlElement ABDs_R = xmlDoc.CreateElement(xmlNode_ABDependencies_R_Name);
		foreach (var abd in ABDependenciesReverse.GetDic())
		{
			XmlElement ABD_R = xmlDoc.CreateElement(xmlNode_ABDependency_R_Name);
			ABD_R.SetAttribute(xmlNode_ABName, abd.Key);
			ABD_R.SetAttribute(xmlNode_ABDependency_Count_Name, abd.Value.Count.ToString());
			if (abd.Value.FinalSize > 0 && abd.Value.FinalSize < 1024)
			{
				ABD_R.SetAttribute(xmlNode_ABSizeXBName, ((float)abd.Value.FinalSize).ToString() + " B");
			}
			else if (abd.Value.FinalSize >= 1024 && abd.Value.FinalSize < 1024 * 1024)
			{
				ABD_R.SetAttribute(xmlNode_ABSizeXBName, ((float)abd.Value.FinalSize / 1024).ToString("f3") + " KB");
			}
			else if (abd.Value.FinalSize >= 1024 * 1024)
			{
				ABD_R.SetAttribute(xmlNode_ABSizeXBName, ((float)abd.Value.FinalSize / (1024 * 1024)).ToString("f3") + " MB");
			}
			foreach (var d in abd.Value.Refs)
			{
				XmlElement d_R = xmlDoc.CreateElement(xmlNode_ABDependencyName);
				d_R.SetAttribute(xmlNode_NameName, d);
				var size = ABDependenciesPositive.GetDRAC(d, true).FinalSize;
				if (size > 0 && size < 1024)
				{
					d_R.SetAttribute(xmlNode_ABSizeXBName, ((float)size).ToString() + " B");
				}
				else if (size >= 1024 && size < 1024 * 1024)
				{
					d_R.SetAttribute(xmlNode_ABSizeXBName, ((float)size / 1024).ToString("f3") + " KB");
				}
				else if (size >= 1024 * 1024)
				{
					d_R.SetAttribute(xmlNode_ABSizeXBName, ((float)size / (1024 * 1024)).ToString("f3") + " MB");
				}
				ABD_R.AppendChild(d_R);
			}
			ABDs_R.AppendChild(ABD_R);
		}
		AllRoot.AppendChild(ABDs_R);

		//同名文件直接覆盖
		xmlDoc.Save(filepath);
	}
	public const string DateFormat = @"yyyy-MM-dd HH:mm:ss.fff";
	public static string DateTimeString = @"";

	/// <summary>
	/// 根据build assetbundle后返回的AssetBundleManifest 生成一个本次AssetBundle的信息文件，包含所有的AssetBundle文件名与其hash值。
	/// </summary>
	/// <param name="mf">build assetbundle后返回的manifest对象</param>
	/// <param name="versionName">一般是平台名字，也可以加其他文件夹在</param>
	private static void GenABVersionInfo(AssetBundleManifest mf, string versionName)
	{
		//string ABPath = AssetBundleHelper.Combine(Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets")), GetAssetBundlePath(versionName));
		string ABPath = GetAssetBundlePath(versionName);
		string filepath = (GetABInfoXmlPath(versionName));
		XmlDocument xmlDoc = null;
		//创建XML文档实例  
		xmlDoc = new XmlDocument();
		XmlElement AllRoot = xmlDoc.CreateElement(xmlNode_AssetBundlesName);
		//创建个时间属性，可以更直观的对比不同版本的
		AllRoot.SetAttribute(xmlAttribute_CreateTime, DateTimeString);
		xmlDoc.AppendChild(AllRoot);

		//输出结果按名字排序，以后要对比两个文件也方面一些
		var abNames = mf.GetAllAssetBundles().OrderBy(n => n).Select(key=>AssetBundleHelper.PathToPlatformFormat(key).ToLower()) ;
		List<KeyValuePair<long, XmlElement>> allE = new List<KeyValuePair<long, XmlElement>>();
		foreach (var abName in abNames)
		{
			//bundle大小也输出一下
			FileInfo fi = new FileInfo(AssetBundleHelper.Combine(ABPath, abName));
			{
				//把bundle大小也写到AB依赖中
				string name = abName.Substring(0, abName.IndexOf(AssetBundleHelper.AssetBundelExtName));
				var drac = ABDependenciesPositive.GetDRAC(name, true);
				if (drac != null)
				{
					drac.FinalSize = fi.Length;
				}
			}
			var hash = mf.GetAssetBundleHash(abName);
			XmlElement node = xmlDoc.CreateElement(xmlNode_ABName);
			node.SetAttribute(xmlNode_NameName, abName);
			node.SetAttribute(xmlNode_HashName, hash.ToString());
			node.SetAttribute(xmlNode_ABSizeName, fi.Length.ToString());
			if(fi.Length < 1024)
			{
				node.SetAttribute(xmlNode_ABSizeXBName, ((float)fi.Length ).ToString()+" B");
			}
			else if(fi.Length >= 1024 && fi.Length < 1024 * 1024)
			{
				node.SetAttribute(xmlNode_ABSizeXBName, ((float)fi.Length / 1024).ToString("f3")+" KB");
			}
			else
			{
				node.SetAttribute(xmlNode_ABSizeXBName, ((float)fi.Length / (1024 * 1024)).ToString("f3") + " MB");
			}
			allE.Add(new KeyValuePair<long, XmlElement>(fi.Length, node));
		}
		foreach (var node in allE.OrderByDescending(k => k.Key).ThenBy(k => k.Value.GetAttribute(xmlNode_NameName)))
		{
			AllRoot.AppendChild(node.Value);
		}
		//同名文件直接覆盖
		xmlDoc.Save(filepath);
	}
	/// <summary>
	/// 获得上一次的的打包结果信息，也就是上一次打包时GenABVersionInfo生成的文件
	/// </summary>
	/// <param name="versionName">一般是平台名字，也可以加其他文件夹在</param>
	/// <returns>返回AssetBundle信息的name和hash值</returns>
	private static Dictionary<string, string> GetABVersionInfo(string versionName)
	{
		var list = new Dictionary<string, string>();
		string filepath = (GetABInfoXmlPath(versionName));
		XmlDocument xmlDoc = new XmlDocument();
		if (File.Exists(filepath))
		{
			xmlDoc.Load(filepath);
			XmlNodeList nodeList = xmlDoc.SelectSingleNode(xmlNode_AssetBundlesName).ChildNodes;
			foreach (XmlElement node in nodeList)
			{
				list.Add(node.GetAttribute(xmlNode_NameName), node.GetAttribute(xmlNode_HashName));
			}
		}
		return list;
	}
	/// <summary>
	/// 对比上次打包结果，输出新增，删除和改变的部分。
	/// </summary>
	/// <param name="oldABVersionInfo">GetABVersionInfo返回的值，也就是上一次打包的结果信息</param>
	/// <param name="newMf">build assetbundle后返回的manifest对象</param>
	/// <param name="versionName">一般是平台名字，也可以加其他文件夹在</param>
	private static void CompareOldInfoAndNewManifest(Dictionary<string, string> oldABVersionInfo, AssetBundleManifest newMf, string versionName,
		out Dictionary<string, string> newAB, out Dictionary<string, string> delAB, out Dictionary<string, KeyValuePair<string, string>> changedAB)
	{
		//临时记录新增的AssetBundle
		newAB = new Dictionary<string, string>();
		//临时记录删除的AssetBundle，这里返回后可以查找已经不需要存在的assetbundle，可以手动删除
		delAB = new Dictionary<string, string>();
		//临时记录改变的AssetBundle
		changedAB = new Dictionary<string, KeyValuePair<string, string>>();
		var abNames = newMf.GetAllAssetBundles().Select(key => AssetBundleHelper.PathToPlatformFormat(key).ToLower());
		foreach (var name in abNames)
		{
			var newHash = newMf.GetAssetBundleHash(name).ToString();
			if (oldABVersionInfo.ContainsKey(name))
			{
				if (!oldABVersionInfo[name].Equals(newHash))
				{
					//changedAB
					changedAB.Add(name, new KeyValuePair<string, string>(oldABVersionInfo[name], newHash));
				}
				else
				{
					//not changed
				}
				oldABVersionInfo.Remove(name);
			}
			else
			{
				//newAB
				newAB.Add(name, newHash);
			}
		}
		foreach (var name in oldABVersionInfo)
		{
			delAB.Add(name.Key, name.Value);
		}
		//对结果进行排序
		newAB.OrderBy(n => n);
		delAB.OrderBy(n => n);
		changedAB.OrderBy(n => n);

		//创建XML文档实例  
		string filepath = GetDifferXmlPath(versionName);
		XmlDocument xmlDoc = new XmlDocument();
		XmlElement AllRoot = xmlDoc.CreateElement(xmlNode_AssetBundlesName);
		xmlDoc.AppendChild(AllRoot);

		//创建个时间属性，可以更直观的对比不同版本的
		AllRoot.SetAttribute(xmlAttribute_CreateTime, DateTimeString);

		XmlElement NewRoot = xmlDoc.CreateElement(xmlNode_NewAssetBundlesName);
		AllRoot.AppendChild(NewRoot);
		foreach (var nAB in newAB)
		{
			XmlElement node = xmlDoc.CreateElement(xmlNode_ABName);
			node.SetAttribute(xmlNode_NameName, nAB.Key);
			node.SetAttribute(xmlNode_HashName, nAB.Value);
			NewRoot.AppendChild(node);
		}
		XmlElement ChangedRoot = xmlDoc.CreateElement(xmlNode_ChangedAssetBundlesName);
		AllRoot.AppendChild(ChangedRoot);
		foreach (var cAB in changedAB)
		{
			XmlElement node = xmlDoc.CreateElement(xmlNode_ABName);
			node.SetAttribute(xmlNode_NameName, cAB.Key);
			node.SetAttribute(xmlNode_OldHashName, cAB.Value.Key);
			node.SetAttribute(xmlNode_NewHasHName, cAB.Value.Value);
			ChangedRoot.AppendChild(node);
		}
		XmlElement DelRoot = xmlDoc.CreateElement(xmlNode_DelAssetBundlesName);
		AllRoot.AppendChild(DelRoot);
		foreach (var dAB in delAB)
		{
			XmlElement node = xmlDoc.CreateElement(xmlNode_ABName);
			node.SetAttribute(xmlNode_NameName, dAB.Key);
			node.SetAttribute(xmlNode_HashName, dAB.Value);
			DelRoot.AppendChild(node);
		}
		//直接覆盖上次结果
		xmlDoc.Save(filepath);
	}
	/// <summary>
	/// 根据versionName获得输出AB差异文件的路径
	/// </summary>
	/// <param name="versionName">一般是平台名字，也可以加其他文件夹在</param>
	/// <returns>输出路径</returns>
	public static string GetDifferXmlPath(string versionName)
	{
		return (AssetBundleHelper.Combine(GetAssetBundlePath(versionName), ABDifferInfoName));
	}
	/// <summary>
	/// 根据versionName获得输出AB信息文件的路径
	/// </summary>
	/// <param name="versionName">一般是平台名字，也可以加其他文件夹在</param>
	/// <returns>输出路径</returns>
	public static string GetABInfoXmlPath(string versionName)
	{
		return AssetBundleHelper.Combine(AssetBundlePath, versionName, ABVersionInfoName);
	}
	/// <summary>
	/// 根据versionName获得输出AB依赖信息文件的路径
	/// </summary>
	/// <param name="versionName">一般是平台名字，也可以加其他文件夹在</param>
	/// <returns>输出路径</returns>
	public static string GetABDependenciesXmlPath(string versionName)
	{
		return AssetBundleHelper.Combine(AssetBundlePath, versionName, ABDependenciesInfoName);
	}

	/// <summary>
	/// 获得路径下所有非meta文件，递归搜索文件夹
	/// </summary>
	/// <param name="path">搜索的目标文件夹</param>
	/// <returns>返回一个序列，包含每一个文件的信息和相对于assets文件夹的路径</returns>
	private static List<KeyValuePair<string, FileInfo>> GetAllFilesWithoutMeta(string path)
	{
		var list = new List<KeyValuePair<string, FileInfo>>();
		if (Directory.Exists(path))
		{
			DirectoryInfo fileDir = new DirectoryInfo(path);
			var files = fileDir.GetFiles();

			list.AddRange(files.Where(f => !f.Name.Contains(".meta")).Select(
				//这里文件名进行了转换，windows下都变成\\，OSX下都是/
				f => new KeyValuePair<string, FileInfo>(f.FullName.Substring(f.FullName.IndexOf(AssetBundleHelper.PathToPlatformFormat(NeedBuildAssetBundlePath))).ToLower(), f)
				));
			var dirs = fileDir.GetDirectories();
			foreach (var dir in dirs)
			{
				//递归
				var tmpList = GetAllFilesWithoutMeta(dir.FullName);
				list.AddRange(tmpList);
			}
		}
		return list;
	}
	/// <summary>
	/// 根据versionName获取需要打包的所有资源并返回AssetBundleBuild数组
	/// </summary>
	/// <param name="versionName">一般是平台名字，也可以加其他文件夹在</param>
	/// <returns>返回一个数组，可以直接用于BuildPipeline.BuildAssetBundles</returns>
	private static AssetBundleBuild[] GetBuildMapByVersion(string versionName)
	{
		List<AssetBundleBuild> list = new List<AssetBundleBuild>();

		if (!Directory.Exists(NeedBuildAssetBundlePath))
		{
			return null;
		}
		var NeedBuildABFileList = GetAllFilesWithoutMeta(NeedBuildAssetBundlePath);
		var dicDependencies = new Dictionary<string, string>();
		//遍历这些文件去找到所有的依赖项
		foreach (var file in NeedBuildABFileList)
		{
			//添加正向依赖，这个不可能会重复，所以不用判断ContainsKey
			ABDependenciesPositive.Add(file.Key, ABDependenciesPositive.GetDRAC(file.Key));
			//var relativePath = Path.Combine();
			var dps = AssetDatabase.GetDependencies(file.Key);
			//                                                                         这里文件名进行了转换，windows下都变成\\，OSX下都是/
			//也转换了一下小写
			foreach (var dp in dps.Where(d => !d.EndsWith(".cs")).Select(d=> AssetBundleHelper.PathToPlatformFormat(d.ToLower())))//脚本文件排除
			{
				{
					//增加反向依赖，这个会统计数量
					if (!file.Key.Equals(dp))
					{
						if (!ABDependenciesReverse.ContainsKey(dp))
						{
							ABDependenciesReverse.Add(dp, ABDependenciesReverse.GetDRAC(dp));
						}
						ABDependenciesReverse[dp].AddRef(file.Key);

						//正向依赖
						ABDependenciesPositive[file.Key].AddRef(dp);
					}
				}
				if (dicDependencies.ContainsKey(dp))
				{
					continue;
				}
				else
				{
					dicDependencies.Add(dp, dp);
				}
			}
		}
		//这里根据依赖关系，把只被一个单独AB依赖的资源不设置单独打包
		foreach (var abd in ABDependenciesReverse.GetDic().Where(dr=>dr.Value.Count==1))
		{
			if(dicDependencies.ContainsKey(abd.Key))
			{
				dicDependencies.Remove(abd.Key);
			}
		}
		//这里已经获得了所有的资源名称，可以直接生成AssetBundleBuild了
		foreach (var file in dicDependencies)
		{
			//目前先把所有资源单独打包，不做任何合并处理
			var tmp = new AssetBundleBuild();
			{
				tmp.assetBundleName = AssetBundleHelper.ResourceNameToBundleName(file.Key);
				string[] assets = new string[1] { file.Key };
				tmp.assetNames = assets;
			}
			list.Add(tmp);
		}

		return list.ToArray();
	}
	/// <summary>
	/// 删除指定名称的文件
	/// 会处理相关的相对路径
	/// </summary>
	/// <param name="path">搜索的目录</param>
	/// <param name="versionName">一般是平台名字，也可以加其他文件夹在</param>
	/// <param name="fileNames">需要删除的文件名字，相对路径，斜杠为/</param>
	private static void DeleteAssetBundleFiles(string path, string versionName, Dictionary<string, string> fileNames)
	{
		DirectoryInfo fileDir = new DirectoryInfo(path);
		FileInfo[] files = fileDir.GetFiles();
		foreach (var file in files)
		{
			//绝对路径转换为相对路径，并且转换为UNIX的斜杠‘/’
			string pathPrefix = (GetAssetBundlePath(versionName));
			//路径里可千万别有相同的目录结构啊……比如 Assets/AssetBundles/Windows64/XXXXXX/Assets/AssetBundles/Windows64/
			int index = file.FullName.LastIndexOf(pathPrefix) + pathPrefix.Length + 1;
			if (index >= 0)
			{
				if (index < file.FullName.Length)
				{
					var relativePath = AssetBundleHelper.PathToPlatformFormat(file.FullName.Substring(index));
					//Debug.Log(relativePath);
					if (fileNames.ContainsKey(relativePath.ToLower()))
					{
						DeleteFileAndManifest(file.FullName);
					}
				}
				else
				{
					Debug.LogError("DeleteAssetBundleFiles Error:" + file.FullName + " :pathPrefix:" + pathPrefix);
				}
			}
		}
		var dirs = fileDir.GetDirectories();
		foreach (var dir in dirs)
		{
			//递归调用
			DeleteAssetBundleFiles(dir.FullName, versionName, fileNames);
		}
	}
	/// <summary>
	/// 递归删除空文件夹
	/// </summary>
	/// <param name="path">搜索的目录</param>
	private static void DeleteEmptyFolders(string path)
	{
		DirectoryInfo fileDir = new DirectoryInfo(path);
		var dirs = fileDir.GetDirectories();
		foreach (var dir in dirs)
		{
			DeleteEmptyFolders(dir.FullName);
		}

		var files = fileDir.GetFiles();
		var afterPostDirs = fileDir.GetDirectories();
		if (files.Length == 0 && afterPostDirs.Length == 0)
		{
			DeleteFolder(path);
		}
	}
	/// <summary>
	/// 删除目标文件夹和meta文件
	/// </summary>
	/// <param name="path">目标文件夹</param>
	private static void DeleteFolder(string path)
	{
		Directory.Delete(path);
		File.Delete(path + ".meta");
	}
	/// <summary>
	/// 删除目标文件和meta文件
	/// </summary>
	/// <param name="path">目标文件</param>
	private static void DeleteFile(string path)
	{
		File.Delete(path);
		File.Delete(path + ".meta");
	}
	/// <summary>
	/// 删除目标文件，.META文件和manifest文件和manifest文件的meta文件
	/// </summary>
	/// <param name="path">目标文件</param>
	private static void DeleteFileAndManifest(string path)
	{
		DeleteFile(path);
		DeleteFile(path + ".manifest");
	}
	/// <summary>
	/// 删除老旧的assetbundle文件，并清理空文件夹
	/// </summary>
	/// <param name="versionName">一般是平台名字，也可以加其他文件夹在</param>
	/// <param name="fileNames">需要删除的文件列表</param>
	private static void DeleteAssetBundleFilesAndEmptyFolders(string versionName, Dictionary<string, string> fileNames)
	{
		string path = GetAssetBundlePath(versionName);
		DeleteAssetBundleFiles(path, versionName, fileNames);
		//这个时候改删的文件都删完了，把空文件夹也删了
		DeleteEmptyFolders(path);
	}
	/// <summary>
	/// 把AB目录和版本信息组合成一个运行时能找到的目录
	/// </summary>
	/// <param name="versionName">版本信息，这是一个能区分各大主流平台的名字</param>
	/// <returns></returns>
	private static string GetAssetBundlePath(string versionName)
	{
		return AssetBundleHelper.Combine(AssetBundlePath, versionName);
	}
	/// <summary>
	/// 把制定打包的AB文件移动到streamingAssets文件夹中适当的位置，要先清空文件夹后移动
	/// </summary>
	/// <param name="bt">BuildTarget的类型，一般都是打目标版本的类型</param>
	private static void MoveAssetBundleToStreamingAssets(BuildTarget bt)
	{
		RuntimePlatform runtimePlatform = BuildTargetToRuntimePlatform(bt);
		string versionName = AssetBundleHelper.RuntimePlatformToSimplifyName(runtimePlatform);
		//string streamingAssetsPath = AssetBundleHelper.GetStreamingAssetsPath();
		//清理三个文件夹
		string[] deletePaths = new string[3];
		deletePaths[0] = AssetBundleHelper.Combine(AssetBundleHelper.GetStreamingAssetsPath(), AssetBundleHelper.RuntimePlatformToSimplifyName(RuntimePlatform.WindowsEditor));
		deletePaths[1] = AssetBundleHelper.Combine(AssetBundleHelper.GetStreamingAssetsPath(), AssetBundleHelper.RuntimePlatformToSimplifyName(RuntimePlatform.OSXEditor));
		deletePaths[2] = AssetBundleHelper.Combine(AssetBundleHelper.GetStreamingAssetsPath(), AssetBundleHelper.RuntimePlatformToSimplifyName(RuntimePlatform.Android));
		foreach (var path in deletePaths)
		{
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}
		}
		string tarPath = AssetBundleHelper.Combine(AssetBundleHelper.GetStreamingAssetsPath(), versionName);
		Directory.CreateDirectory(tarPath);
		CopyFolder(GetAssetBundlePath(versionName), tarPath);

		AssetDatabase.Refresh();
	}
	/// <summary>
	/// 整个文件夹拷贝功能
	/// </summary>
	/// <param name="srcPath">源文件夹</param>
	/// <param name="tarPath">目标文件夹</param>
	private static void CopyFolder(string srcPath, string tarPath)
	{
		if (!Directory.Exists(srcPath))
		{
			return;
		}
		if (!Directory.Exists(tarPath))
		{
			Directory.CreateDirectory(tarPath);
		}
		CopyFile(srcPath, tarPath);
		string[] directionName = Directory.GetDirectories(srcPath);
		foreach (string dirPath in directionName)
		{
			string directionPathTemp = AssetBundleHelper.Combine(tarPath, dirPath.Substring(srcPath.Length + 1));
			CopyFolder(dirPath, directionPathTemp);
		}
	}
	/// <summary>
	/// 文件夹下具体文件的拷贝，.meta .manifest和xml文件目前不拷贝
	/// </summary>
	/// <param name="srcPath">源文件夹</param>
	/// <param name="tarPath">目标文件夹</param>
	private static void CopyFile(string srcPath, string tarPath)
	{
		string[] filesList = Directory.GetFiles(srcPath);
		//.meta文件会自动生成，不需要拷贝
		//.manifest对于实际加载AB没有意义，不需要拷贝
		//.xml目前是用来记录AB信息的，不需要运行时加载，不需要拷贝
		foreach (string f in filesList.Where(f => !f.EndsWith(".meta") && !f.EndsWith(".manifest") && !f.EndsWith(".xml")))
		{
			string fTarPath = AssetBundleHelper.Combine(tarPath, f.Substring(srcPath.Length + 1));
			if (File.Exists(fTarPath))
			{
				File.Copy(f, fTarPath, true);
			}
			else
			{
				File.Copy(f, fTarPath);
			}
		}
	}
	/// <summary>
	/// 根据打包目标平台，返回一个目标平台可能的情况
	/// 这个并不用十分详细精准，本打包方式只区分了微软 苹果 和谷歌三家公司的系统，区分也是用公司名，三家公司的下的平台加载时使用的路径都是一个名字
	/// </summary>
	/// <param name="bt">目标类型</param>
	/// <returns>一个假想的运行时平台</returns>
	private static RuntimePlatform BuildTargetToRuntimePlatform(BuildTarget bt)
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
	/// <summary>
	/// 打AB包
	/// </summary>
	/// <param name="options">打包选项</param>
	/// <param name="bt">打包类型</param>
	/// <param name="bIncrementalUpdate">是否增量打包，现在还没有什么用</param>
	private static void BuildAssetBundle(BuildAssetBundleOptions options, BuildTarget bt, bool bIncrementalUpdate = true)
	{
		DateTimeString = DateTime.Now.ToString(DateFormat);
		MyDic.ClearAllDic();
		ABDependenciesPositive.Clear();
		ABDependenciesReverse.Clear();
		RuntimePlatform runtimePlatform = BuildTargetToRuntimePlatform(bt);
		string versionName = AssetBundleHelper.RuntimePlatformToSimplifyName(runtimePlatform);
		string path = GetAssetBundlePath(versionName);
		if (!bIncrementalUpdate)
		{
			//不进行增量更新，删除全部文件重新生成
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}
		}
		else
		{
			//增量更新
			//这里并不需要做什么
		}
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		//在VersionInfo被覆盖前获得用来在下边做对比使用
		var lastVersionInfo = GetABVersionInfo(versionName);
		//获取所有需要打包的资源
		var buildMap = GetBuildMapByVersion(versionName);
		var manifest = BuildPipeline.BuildAssetBundles(GetAssetBundlePath(versionName), buildMap, options, bt);
		//生成新的versioninfo
		GenABVersionInfo(manifest, versionName);
		if (lastVersionInfo != null)
		{
			//其实这里的newAB和changedAB并不准备用来干什么，真正有用的只有delAB，这个需要把多余的AssetBundle文件和文件夹删除
			Dictionary<string, string> newAB;
			Dictionary<string, string> delAB;
			Dictionary<string, KeyValuePair<string, string>> changedAB;
			CompareOldInfoAndNewManifest(lastVersionInfo, manifest, versionName, out newAB, out delAB, out changedAB);
			if (delAB != null && delAB.Count > 0)
			{
				DeleteAssetBundleFilesAndEmptyFolders(versionName, delAB);
			}
		}
		//生成依赖信息
		GenABDependenciesInfo(versionName);
		//最后把一些空的文件夹和新版本不需要的assetbundle
		AssetDatabase.Refresh();
	}


	[MenuItem("AssetBundle/BuildAB_Win64")]
	public static void BuildWin64()
	{
		BuildAssetBundle(BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
	}
	[MenuItem("AssetBundle/moveAB_win64")]
	public static void MoveTest()
	{
		MoveAssetBundleToStreamingAssets(BuildTarget.StandaloneWindows64);
	}

	[MenuItem("AssetBundle/BuildAB_IOS")]
	public static void BuildIOS()
	{
		BuildAssetBundle(BuildAssetBundleOptions.None, BuildTarget.iOS);
	}
	[MenuItem("AssetBundle/moveAB_IOS")]
	public static void MoveIOS()
	{
		MoveAssetBundleToStreamingAssets(BuildTarget.iOS);
	}
	[MenuItem("AssetBundle/BuildAB_Android")]
	public static void BuildAndroid()
	{
		BuildAssetBundle(BuildAssetBundleOptions.None, BuildTarget.Android);
	}
	[MenuItem("AssetBundle/moveAB_Android")]
	public static void MoveAndroid()
	{
		MoveAssetBundleToStreamingAssets(BuildTarget.Android);
	}
}
/// <summary>
/// 这是个可以方便设置编辑下是否使用AssetBundle的类
/// </summary>
public class MyEditor : EditorWindow
{
	[MenuItem("AssetBundle/设置编辑器下AssetBundle的一些信息")]
	static void AddWindow()
	{
		//创建窗口
		Rect wr = new Rect(0, 0, 1000, 800);
		MyEditor window = (MyEditor)EditorWindow.GetWindowWithRect(typeof(MyEditor), wr, true, "设置编辑器下AssetBundle的一些信息");
		window.Show();

	}
	//输入文字的内容
	private GUIStyle titleSytle = null;
	private GUIStyle NormalSytle = null;
	private GUIStyle RedNormalSytle = null;
	private GUIStyle TextFieldSytle = null;
	private GUIStyle RedTextFieldSytle = null;
	private GUIStyle ResetButtonSytle = null;
	//private GUIStyle ToggleSytle = null;

	private const int TabSize = 70;
	private const string TabStr = "	";
	private const int LayoutOneHeight = 60;
	void Awake()
	{
		titleSytle = new GUIStyle(EditorStyles.boldLabel);
		titleSytle.fontSize = 20;

		NormalSytle = new GUIStyle(EditorStyles.boldLabel);
		NormalSytle.fontSize = 15;

		RedNormalSytle = new GUIStyle(EditorStyles.boldLabel);
		RedNormalSytle.fontSize = 15;
		RedNormalSytle.normal.textColor = Color.red;

		TextFieldSytle = new GUIStyle(EditorStyles.textField);
		TextFieldSytle.fontSize = 15;

		RedTextFieldSytle = new GUIStyle(EditorStyles.textField);
		RedTextFieldSytle.fontSize = 15;
		RedTextFieldSytle.normal.textColor = Color.red;

		ResetButtonSytle = new GUIStyle(EditorStyles.miniButton);
		ResetButtonSytle.fontSize = 15;

		//ToggleSytle = new GUIStyle(EditorStyles.toggle);
		//ToggleSytle.fontSize = 15;

	}
	void SetTab(int count)
	{
		GUILayout.Label(TabStr, NormalSytle, GUILayout.Width(TabSize * count));
	}
	//绘制窗口时调用
	void OnGUI()
	{
		//***************************************************************************
		GUILayout.BeginArea(new Rect(0, 0, position.width, LayoutOneHeight));
		GUILayout.Label("编辑器模式运行的过程中:", titleSytle);
		//是否使用bundle
		{
			GUILayout.BeginHorizontal();
			{
				SetTab(2);
				GUILayout.Label("编辑器模式下是否使用AssetBundle:", NormalSytle, GUILayout.Width(260));
				if (AssetBundleHelper.UseAssetBundle)
				{
					//GUI.enabled = false;
					GUILayout.Label("使用AB", RedTextFieldSytle, GUILayout.Width(100));
					//GUI.enabled = true;
				}
				else
				{
					GUI.enabled = false;
					GUILayout.Label("不使用AB", RedTextFieldSytle, GUILayout.Width(100));
					GUI.enabled = true;
				}
				if (GUILayout.Button("更改", ResetButtonSytle, GUILayout.Width(100)))
				{
					AssetBundleHelper.UseAssetBundle = !AssetBundleHelper.UseAssetBundle;
				}
				//AssetBundleHelper.UseAssetBundle = GUILayout.Toggle(AssetBundleHelper.UseAssetBundle, "", ToggleSytle, GUILayout.ExpandWidth(false));
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndArea();
		//**************************************************************************
		GUILayout.BeginArea(new Rect(0, LayoutOneHeight, position.width, position.height));
		GUILayout.Label("Build AssetBundle的时候:", titleSytle);
		//输出目录
		{
			GUILayout.BeginHorizontal();
			{
				SetTab(1);
				GUILayout.Label("AssetBundle输出的目录（相对与Assets目录上一层的相对目录,修改前请思考一下）:", NormalSytle);
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				SetTab(2);
				BuildBundleManager.AssetBundlePath = (GUILayout.TextField(BuildBundleManager.AssetBundlePath, TextFieldSytle, GUILayout.Width(700)));
				//防止不小心修改，可以重置为默认值
				if (GUILayout.Button("重置为默认", ResetButtonSytle, GUILayout.Width(100)))
				{
					BuildBundleManager.AssetBundlePath = BuildBundleManager.assetBundlePath;
				}
			}
			GUILayout.EndHorizontal();
		}
		//打包目录
		{
			GUILayout.BeginHorizontal();
			{
				SetTab(1);
				GUILayout.Label("需要build assetbundle的目录（相对与Assets目录上一层的相对目录,修改前请思考一下）:", NormalSytle);
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				SetTab(1);
				GUILayout.Label("如果真的要改这个，请记得把AssetBundleHelper.AssetBundlePrefixPath一同修改，这样游戏内才能正确找到bundle位置:", RedNormalSytle);
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				SetTab(2);
				BuildBundleManager.NeedBuildAssetBundlePath = (GUILayout.TextField(BuildBundleManager.NeedBuildAssetBundlePath, TextFieldSytle, GUILayout.Width(700)));
				//防止不小心修改，可以重置为默认值
				if (GUILayout.Button("重置为默认", ResetButtonSytle, GUILayout.Width(100)))
				{
					BuildBundleManager.NeedBuildAssetBundlePath = BuildBundleManager.needBuildAssetBundlePath;
				}
			}
			GUILayout.EndHorizontal();
		}
		//xml信息
		{
			//AB信息
			{
				GUILayout.BeginHorizontal();
				{
					SetTab(1);
					GUILayout.Label("ABInfo 的xml文件路径（相对与Assets目录上一层的相对目录,这个并不准备让你修改，但是让你看一下在哪）:", NormalSytle);
				}
				GUILayout.EndHorizontal();
				//ios系列
				GUILayout.BeginHorizontal();
				{
					SetTab(2);
					GUILayout.Label("iOS系列：", NormalSytle);
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				{
					SetTab(2);
					GUI.enabled = false;
					GUILayout.Label(BuildBundleManager.GetABInfoXmlPath(AssetBundleHelper.RuntimePlatformToSimplifyName(RuntimePlatform.OSXEditor)), TextFieldSytle, GUILayout.Width(700));
					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();
				//android系列
				GUILayout.BeginHorizontal();
				{
					SetTab(2);
					GUILayout.Label("android系列：", NormalSytle);
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				{
					SetTab(2);
					GUI.enabled = false;
					GUILayout.Label(BuildBundleManager.GetABInfoXmlPath(AssetBundleHelper.RuntimePlatformToSimplifyName(RuntimePlatform.Android)), TextFieldSytle, GUILayout.Width(700));
					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();
				//Windows系列
				GUILayout.BeginHorizontal();
				{
					SetTab(2);
					GUILayout.Label("Windows系列：", NormalSytle);
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				{
					SetTab(2);
					GUI.enabled = false;
					GUILayout.Label(BuildBundleManager.GetABInfoXmlPath(AssetBundleHelper.RuntimePlatformToSimplifyName(RuntimePlatform.WindowsEditor)), TextFieldSytle, GUILayout.Width(700));
					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();
			}
			//ABDiffer信息
			{
				GUILayout.BeginHorizontal();
				{
					SetTab(1);
					GUILayout.Label("ABInfo 的xml文件路径（相对与Assets目录上一层的相对目录,这个并不准备让你修改，但是让你看一下在哪）:", NormalSytle);
				}
				GUILayout.EndHorizontal();
				//ios系列
				GUILayout.BeginHorizontal();
				{
					SetTab(2);
					GUILayout.Label("iOS系列：", NormalSytle);
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				{
					SetTab(2);
					GUI.enabled = false;
					GUILayout.Label(BuildBundleManager.GetDifferXmlPath(AssetBundleHelper.RuntimePlatformToSimplifyName(RuntimePlatform.OSXEditor)), TextFieldSytle, GUILayout.Width(700));
					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();
				//android系列
				GUILayout.BeginHorizontal();
				{
					SetTab(2);
					GUILayout.Label("android系列：", NormalSytle);
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				{
					SetTab(2);
					GUI.enabled = false;
					GUILayout.Label(BuildBundleManager.GetDifferXmlPath(AssetBundleHelper.RuntimePlatformToSimplifyName(RuntimePlatform.Android)), TextFieldSytle, GUILayout.Width(700));
					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();
				//Windows系列
				GUILayout.BeginHorizontal();
				{
					SetTab(2);
					GUILayout.Label("Windows系列：", NormalSytle);
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				{
					SetTab(2);
					GUI.enabled = false;
					GUILayout.Label(BuildBundleManager.GetDifferXmlPath(AssetBundleHelper.RuntimePlatformToSimplifyName(RuntimePlatform.WindowsEditor)), TextFieldSytle, GUILayout.Width(700));
					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndArea();
		//*****************************************************************************
	}
}
#endif
