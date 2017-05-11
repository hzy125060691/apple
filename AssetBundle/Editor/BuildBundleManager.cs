#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Xml;
using System;
using System.Linq;
/// <summary>
/// 这个类只能在编辑器里使用，用来创建assetbundle和在打包前将assetbundle移动到StreamingAssets下指定位置。
/// 这里主要是打assetbundle相关的主要逻辑，也只是编辑器模式下的功能
/// </summary>
public class BuildBundleManager
{
	private static AssetBundleSettingHelper ABSH = null;
	private static ABPInfo ABPHInfo = null;

	public static void ABSHInitAndSelect()
	{
		if (ABSH == null)
		{
			ABSH = AssetBundleSettingHelperEditor.GetABSH(out ABPHInfo);
			if(ABPHInfo == null)
			{

			}
		}
		Selection.activeObject = ABSH;
	}

	public static AssetBundleSettingHelper GetABSH()
	{
		if(ABSH == null)
		{
			ABSHInitAndSelect();
		}
		return ABSH;
	}
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
			key = AssetBundleSettingHelper.PathToPlatformFormat(key).ToLower();
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
			key = AssetBundleSettingHelper.PathToPlatformFormat(key).ToLower();
			dic.Add(key.ToLower(), value);
		}
		public bool ContainsKey(string key)
		{
			key = AssetBundleSettingHelper.PathToPlatformFormat(key).ToLower();
			return dic.ContainsKey(key);
		}
		public DependenciesRefAndCount this[string key]
		{
			get
			{
				key = AssetBundleSettingHelper.PathToPlatformFormat(key).ToLower();
				return dic[key];
			}
			set
			{
				key = AssetBundleSettingHelper.PathToPlatformFormat(key).ToLower();
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
			sKey = AssetBundleSettingHelper.PathToPlatformFormat(sKey).ToLower();
			return new DependenciesRefAndCount(sKey);
		}
		DependenciesRefAndCount(string sKey)
		{
			sKey = AssetBundleSettingHelper.PathToPlatformFormat(sKey).ToLower();
			key = sKey;
			count = 0;
			finalSize = 0;
			refs = new List<string>();
		}
		public void AddRef(string sRef)
		{
			sRef = AssetBundleSettingHelper.PathToPlatformFormat(sRef).ToLower();
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
		var abpi = ABSH.GetABDependenciesXmlPath(versionName);
		XmlDocument xmlDoc = null;
		//创建XML文档实例  
		xmlDoc = new XmlDocument();
		XmlElement AllRoot = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_ABDependencies);
		//创建个时间属性，可以更直观的对比不同版本的
		AllRoot.SetAttribute(AssetBundleSettingHelper.xmlAttribute_CreateTime, VersionInfo.GetVersionString());
		xmlDoc.AppendChild(AllRoot);
		//正向依赖
		XmlElement ABDs_P = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_ABDependencies_P);
		foreach (var abd in ABDependenciesPositive.GetDic())
		{
			XmlElement ABD_P = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_ABDependency_P);
			ABD_P.SetAttribute(AssetBundleSettingHelper.xmlNode_AB, abd.Key);
			ABD_P.SetAttribute(AssetBundleSettingHelper.xmlNode_ABDependency_Count, abd.Value.Count.ToString());
			if (abd.Value.FinalSize > 0 && abd.Value.FinalSize < 1024)
			{
				ABD_P.SetAttribute(AssetBundleSettingHelper.xmlNode_ABSizeXB, ((float)abd.Value.FinalSize).ToString() + " B");
			}
			else if (abd.Value.FinalSize >= 1024 && abd.Value.FinalSize < 1024 * 1024)
			{
				ABD_P.SetAttribute(AssetBundleSettingHelper.xmlNode_ABSizeXB, ((float)abd.Value.FinalSize / 1024).ToString("f3") + " KB");
			}
			else if (abd.Value.FinalSize >= 1024 * 1024)
			{
				ABD_P.SetAttribute(AssetBundleSettingHelper.xmlNode_ABSizeXB, ((float)abd.Value.FinalSize / (1024 * 1024)).ToString("f3") + " MB");
			}
			foreach (var d in abd.Value.Refs)
			{
				XmlElement d_P = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_ABDependency);
				d_P.SetAttribute(AssetBundleSettingHelper.xmlNode_Name, d);
				var size = ABDependenciesPositive.GetDRAC(d, true).FinalSize;
				if (size > 0  && size < 1024)
				{
					d_P.SetAttribute(AssetBundleSettingHelper.xmlNode_ABSizeXB, ((float)size).ToString() + " B");
				}
				else if (size >= 1024 && size < 1024 * 1024)
				{
					d_P.SetAttribute(AssetBundleSettingHelper.xmlNode_ABSizeXB, ((float)size / 1024).ToString("f3") + " KB");
				}
				else if (size >= 1024 * 1024)
				{
					d_P.SetAttribute(AssetBundleSettingHelper.xmlNode_ABSizeXB, ((float)size / (1024 * 1024)).ToString("f3") + " MB");
				}
				ABD_P.AppendChild(d_P);
			}
			ABDs_P.AppendChild(ABD_P);
		}
		AllRoot.AppendChild(ABDs_P);
		//反向依赖
		XmlElement ABDs_R = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_ABDependencies_R);
		foreach (var abd in ABDependenciesReverse.GetDic())
		{
			XmlElement ABD_R = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_ABDependency_R);
			ABD_R.SetAttribute(AssetBundleSettingHelper.xmlNode_AB, abd.Key);
			ABD_R.SetAttribute(AssetBundleSettingHelper.xmlNode_ABDependency_Count, abd.Value.Count.ToString());
			if (abd.Value.FinalSize > 0 && abd.Value.FinalSize < 1024)
			{
				ABD_R.SetAttribute(AssetBundleSettingHelper.xmlNode_ABSizeXB, ((float)abd.Value.FinalSize).ToString() + " B");
			}
			else if (abd.Value.FinalSize >= 1024 && abd.Value.FinalSize < 1024 * 1024)
			{
				ABD_R.SetAttribute(AssetBundleSettingHelper.xmlNode_ABSizeXB, ((float)abd.Value.FinalSize / 1024).ToString("f3") + " KB");
			}
			else if (abd.Value.FinalSize >= 1024 * 1024)
			{
				ABD_R.SetAttribute(AssetBundleSettingHelper.xmlNode_ABSizeXB, ((float)abd.Value.FinalSize / (1024 * 1024)).ToString("f3") + " MB");
			}
			foreach (var d in abd.Value.Refs)
			{
				XmlElement d_R = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_ABDependency);
				d_R.SetAttribute(AssetBundleSettingHelper.xmlNode_Name, d);
				var size = ABDependenciesPositive.GetDRAC(d, true).FinalSize;
				if (size > 0 && size < 1024)
				{
					d_R.SetAttribute(AssetBundleSettingHelper.xmlNode_ABSizeXB, ((float)size).ToString() + " B");
				}
				else if (size >= 1024 && size < 1024 * 1024)
				{
					d_R.SetAttribute(AssetBundleSettingHelper.xmlNode_ABSizeXB, ((float)size / 1024).ToString("f3") + " KB");
				}
				else if (size >= 1024 * 1024)
				{
					d_R.SetAttribute(AssetBundleSettingHelper.xmlNode_ABSizeXB, ((float)size / (1024 * 1024)).ToString("f3") + " MB");
				}
				ABD_R.AppendChild(d_R);
			}
			ABDs_R.AppendChild(ABD_R);
		}
		AllRoot.AppendChild(ABDs_R);

		//同名文件直接覆盖
		xmlDoc.Save(abpi.FullName);
	}
	public static VersionXmlInfo VersionInfo = new VersionXmlInfo();
	/// <summary>
	/// 根据build assetbundle后返回的AssetBundleManifest 生成一个本次AssetBundle的信息文件，包含所有的AssetBundle文件名与其hash值。
	/// </summary>
	/// <param name="mf">build assetbundle后返回的manifest对象</param>
	/// <param name="versionName">一般是平台名字，也可以加其他文件夹在</param>
	private static void GenABVersionInfo(AssetBundleManifest mf, string versionName)
	{
		var abpi = ABSH.GetABInfoXmlPath(versionName);
		XmlDocument xmlDoc = null;
		//创建XML文档实例  
		xmlDoc = new XmlDocument();
		XmlElement AllRoot = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_AssetBundles);
		//创建个时间属性，可以更直观的对比不同版本的
		AllRoot.SetAttribute(AssetBundleSettingHelper.xmlAttribute_CreateTime, VersionInfo.GetVersionString());
		xmlDoc.AppendChild(AllRoot);

		//输出结果按名字排序，以后要对比两个文件也方面一些
		var abNames = mf.GetAllAssetBundles().OrderBy(n => n).Select(key=> AssetBundleSettingHelper.PathToPlatformFormat(key).ToLower()) ;
		List<KeyValuePair<long, XmlElement>> allE = new List<KeyValuePair<long, XmlElement>>();
		foreach (var abName in abNames)
		{
			//bundle大小也输出一下
			FileInfo fi = new FileInfo(AssetBundleSettingHelper.Combine(abpi.Dir_Relative, abName));
			{
				//把bundle大小也写到AB依赖中
				string name = abName.Substring(0, abName.IndexOf(ABSH.AssetBundelExtName));
				var drac = ABDependenciesPositive.GetDRAC(name, true);
				if (drac != null)
				{
					drac.FinalSize = fi.Length;
				}
			}
			var hash = mf.GetAssetBundleHash(abName);
			XmlElement node = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_AB);
			node.SetAttribute(AssetBundleSettingHelper.xmlNode_Name, abName);
			node.SetAttribute(AssetBundleSettingHelper.xmlNode_Hash, hash.ToString());
			node.SetAttribute(AssetBundleSettingHelper.xmlNode_ABSize, fi.Length.ToString());
			if(fi.Length < 1024)
			{
				node.SetAttribute(AssetBundleSettingHelper.xmlNode_ABSizeXB, ((float)fi.Length ).ToString()+" B");
			}
			else if(fi.Length >= 1024 && fi.Length < 1024 * 1024)
			{
				node.SetAttribute(AssetBundleSettingHelper.xmlNode_ABSizeXB, ((float)fi.Length / 1024).ToString("f3")+" KB");
			}
			else
			{
				node.SetAttribute(AssetBundleSettingHelper.xmlNode_ABSizeXB, ((float)fi.Length / (1024 * 1024)).ToString("f3") + " MB");
			}
			allE.Add(new KeyValuePair<long, XmlElement>(fi.Length, node));
		}
		foreach (var node in allE.OrderByDescending(k => k.Key).ThenBy(k => k.Value.GetAttribute(AssetBundleSettingHelper.xmlNode_Name)))
		{
			AllRoot.AppendChild(node.Value);
		}
		//同名文件直接覆盖
		xmlDoc.Save(abpi.FullName);

		xmlDoc = VersionInfo.OutputXmlDoc();
		abpi = ABSH.GetABXmlPath(versionName, VersionXmlInfo.FileName);
		xmlDoc.Save(abpi.FullName);

	}
	/// <summary>
	/// 获得上一次的的打包结果信息，也就是上一次打包时GenABVersionInfo生成的文件
	/// </summary>
	/// <param name="versionName">一般是平台名字，也可以加其他文件夹在</param>
	/// <returns>返回AssetBundle信息的name和hash值</returns>
	private static Dictionary<string, string> GetABVersionInfo(string versionName)
	{
		var list = new Dictionary<string, string>();
		var abpi = ABSH.GetABInfoXmlPath(versionName);
		XmlDocument xmlDoc = new XmlDocument();
		if (File.Exists(abpi.FullName))
		{
			xmlDoc.Load(abpi.FullName);
			XmlNodeList nodeList = xmlDoc.SelectSingleNode(AssetBundleSettingHelper.xmlNode_AssetBundles).ChildNodes;
			foreach (XmlElement node in nodeList)
			{
				list.Add(node.GetAttribute(AssetBundleSettingHelper.xmlNode_Name), node.GetAttribute(AssetBundleSettingHelper.xmlNode_Hash));
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
		var abNames = newMf.GetAllAssetBundles().Select(key => AssetBundleSettingHelper.PathToPlatformFormat(key).ToLower());
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
		var abpi = ABSH.GetDifferXmlPath(versionName);
		//string filepath = ABSH.GetDifferXmlPath(versionName);
		XmlDocument xmlDoc = new XmlDocument();
		XmlElement AllRoot = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_AssetBundles);
		xmlDoc.AppendChild(AllRoot);

		//创建个时间属性，可以更直观的对比不同版本的
		AllRoot.SetAttribute(AssetBundleSettingHelper.xmlAttribute_CreateTime, VersionInfo.GetVersionString());

		XmlElement NewRoot = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_NewAssetBundles);
		AllRoot.AppendChild(NewRoot);
		foreach (var nAB in newAB)
		{
			XmlElement node = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_AB);
			node.SetAttribute(AssetBundleSettingHelper.xmlNode_Name, nAB.Key);
			node.SetAttribute(AssetBundleSettingHelper.xmlNode_Hash, nAB.Value);
			NewRoot.AppendChild(node);
		}
		XmlElement ChangedRoot = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_ChangedAssetBundles);
		AllRoot.AppendChild(ChangedRoot);
		foreach (var cAB in changedAB)
		{
			XmlElement node = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_AB);
			node.SetAttribute(AssetBundleSettingHelper.xmlNode_Name, cAB.Key);
			node.SetAttribute(AssetBundleSettingHelper.xmlNode_OldHash, cAB.Value.Key);
			node.SetAttribute(AssetBundleSettingHelper.xmlNode_NewHash, cAB.Value.Value);
			ChangedRoot.AppendChild(node);
		}
		XmlElement DelRoot = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_DelAssetBundles);
		AllRoot.AppendChild(DelRoot);
		foreach (var dAB in delAB)
		{
			XmlElement node = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_AB);
			node.SetAttribute(AssetBundleSettingHelper.xmlNode_Name, dAB.Key);
			node.SetAttribute(AssetBundleSettingHelper.xmlNode_Hash, dAB.Value);
			DelRoot.AppendChild(node);
		}
		//直接覆盖上次结果
		xmlDoc.Save(abpi.FullName);
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
				f => new KeyValuePair<string, FileInfo>(f.FullName.Substring(f.FullName.IndexOf(AssetBundleSettingHelper.PathToPlatformFormat(ABSH.NeedBuildABPath))).ToLower(), f)
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

		if (!Directory.Exists(ABSH.NeedBuildABPath))
		{
			return null;
		}
		var NeedBuildABFileList = GetAllFilesWithoutMeta(ABSH.NeedBuildABPath);
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
			foreach (var dp in dps.Where(d => !d.EndsWith(".cs")).Select(d=> AssetBundleSettingHelper.PathToPlatformFormat(d.ToLower())))//脚本文件排除
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
				tmp.assetBundleName = ABSH.ResourceNameToBundleName(file.Key);
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
			var abpi = ABSH.GetAssetBundleOutputPath(versionName);
			//路径里可千万别有相同的目录结构啊……比如 Assets/AssetBundles/Windows64/XXXXXX/Assets/AssetBundles/Windows64/
			int index = file.FullName.LastIndexOf(abpi.Dir_Relative) + abpi.Dir_Relative.Length + 1;
			if (index >= 0)
			{
				if (index < file.FullName.Length)
				{
					var relativePath = AssetBundleSettingHelper.PathToPlatformFormat(file.FullName.Substring(index));
					//Debug.Log(relativePath);
					if (fileNames.ContainsKey(relativePath.ToLower()))
					{
						DeleteFileAndManifest(file.FullName);
					}
				}
				else
				{
					Debug.LogError("DeleteAssetBundleFiles Error:" + file.FullName + " :abpi.Dir_Relative:" + abpi.Dir_Relative);
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
		var abpi = ABSH.GetAssetBundleOutputPath(versionName);
		//string path = ABSH.GetAssetBundleOutputPath(versionName);
		DeleteAssetBundleFiles(abpi.Dir_Relative, versionName, fileNames);
		//这个时候改删的文件都删完了，把空文件夹也删了
		DeleteEmptyFolders(abpi.Dir_Relative);
	}
	
	/// <summary>
	/// 把制定打包的AB文件移动到streamingAssets文件夹中适当的位置，要先清空文件夹后移动
	/// </summary>
	/// <param name="bt">BuildTarget的类型，一般都是打目标版本的类型</param>
	public static void MoveAssetBundleToStreamingAssets(BuildTarget bt)
	{
		RuntimePlatform runtimePlatform = AssetBundleSettingHelperEditor.BuildTargetToRuntimePlatform(bt);
		string versionName = GetABSH().RuntimePlatformToSimplifyName(runtimePlatform);
		//string streamingAssetsPath = AssetBundleHelper.GetStreamingAssetsPath();
		//清理三个文件夹
		ABPInfo[] deleteABPIs = new ABPInfo[3];
		deleteABPIs[0] = GetABSH().GetStreamingAssetABPath(ABSH.RuntimePlatformToSimplifyName(RuntimePlatform.WindowsEditor));
		deleteABPIs[1] = GetABSH().GetStreamingAssetABPath(ABSH.RuntimePlatformToSimplifyName(RuntimePlatform.OSXEditor));
		deleteABPIs[2] = GetABSH().GetStreamingAssetABPath(ABSH.RuntimePlatformToSimplifyName(RuntimePlatform.Android));
		foreach (var abpiTmp in deleteABPIs)
		{
			if (Directory.Exists(abpiTmp.Dir_Relative))
			{
				Directory.Delete(abpiTmp.Dir_Relative, true);
			}
		}
		var abpiTar = GetABSH().GetStreamingAssetABPath(versionName);
		Directory.CreateDirectory(abpiTar.Dir_Relative);
		var abpi = GetABSH().GetAssetBundleOutputPath(versionName);
		CopyFolder(abpi.Dir_Relative, abpiTar.Dir_Relative);

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
			string directionPathTemp = AssetBundleSettingHelper.Combine(tarPath, dirPath.Substring(srcPath.Length + 1));
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
		foreach (string f in filesList.Where(f => !f.EndsWith(".meta") && !f.EndsWith(".manifest") && (!f.EndsWith(".xml") || f.EndsWith(VersionXmlInfo.FileName + AssetBundleSettingHelper.xmlExtName))))
		{
			string fTarPath = AssetBundleSettingHelper.Combine(tarPath, f.Substring(srcPath.Length + 1));
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
	/// 打AB包
	/// </summary>
	/// <param name="options">打包选项</param>
	/// <param name="bt">打包类型</param>
	/// <param name="bIncrementalUpdate">是否增量打包，现在还没有什么用</param>
	public static void BuildAssetBundle(BuildAssetBundleOptions options, BuildTarget bt, bool bIncrementalUpdate = true)
	{
		ABSHInitAndSelect();

		//DateTimeString = DateTime.Now.ToString(VersionXmlInfo.DateFormat);
		VersionInfo.version = DateTime.Now;
		MyDic.ClearAllDic();
		ABDependenciesPositive.Clear();
		ABDependenciesReverse.Clear();
		RuntimePlatform runtimePlatform = AssetBundleSettingHelperEditor.BuildTargetToRuntimePlatform(bt);
		string versionName = ABSH.RuntimePlatformToSimplifyName(runtimePlatform);
		var abpi = ABSH.GetAssetBundleOutputPath(versionName);
		if (!bIncrementalUpdate)
		{
			//不进行增量更新，删除全部文件重新生成
			if (Directory.Exists(abpi.Dir_Relative))
			{
				Directory.Delete(abpi.Dir_Relative, true);
			}
		}
		else
		{
			//增量更新
			//这里并不需要做什么
		}
		if (!Directory.Exists(abpi.Dir_Relative))
		{
			Directory.CreateDirectory(abpi.Dir_Relative);
		}
		//在VersionInfo被覆盖前获得用来在下边做对比使用
		var lastVersionInfo = GetABVersionInfo(versionName);
		//获取所有需要打包的资源
		var buildMap = GetBuildMapByVersion(versionName);
		var manifest = BuildPipeline.BuildAssetBundles(abpi.Dir_Relative, buildMap, options, bt);
		if(manifest != null)
		{
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
		
	}
}
#endif
