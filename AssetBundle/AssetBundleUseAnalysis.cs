using System.Collections.Generic;
using System.IO;
using System.Xml;
using System;
using System.Globalization;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// 用来分析AB的使用情况
/// </summary>
public class AssetBundleUseAnalysis
{

	public const string xmlExtName = @".xml";
	public const string xmlDirRoot = @"ABAnalysis";
	public const string xmlUsesDirName = @"ABUses";
	public const string xmlUsesDirName_Outdate = @"ABUses_OutDate";
	public const string xmlDataAnalysisDirName = @"DataAnalysis";
	public const string xmlNode_Title = @"ABUse";
	public const string xmlNode_Title_2 = @"Detail";
	public const string xmlNode_Name = @"Name";
	public const string xmlNode_Type = @"Type";
	public const string xmlNode_Time = @"Time";
	public const string xmlNode_Stage = @"Stage";
	public const string xmlNode_StageBeginTime = @"StageBeginTime";
	public const string xmlNode_StageBeginTime_Ticks = @"StageBeginTicks";
	public const string xmlNode_Time_Ticks = @"Ticks";
	public const string xmlNode_PhaseRoot = @"PhaseRoot";
	public const string xmlNode_Phase = @"Phase";
	public const string xmlNode_PhaseName = @"PhaseName";
	public const string xmlNode_AB = @"AssetBundle";
	public const string xmlNode_ABKey = @"ABName";
	public const string xmlNode_ABPri = @"Priority";

	public const string DateFormat = @"yyyy-MM-dd HH:mm:ss.ffffff";

	public static readonly string PlatformBundlePath = AssetBundleHelper.Combine(AssetBundleHelper.PathToPlatformFormat(AssetBundleHelper.GetStreamingAssetsPath()), AssetBundleHelper.GetPlatformPathName()).ToLower();

	private static List<ABDetailRecord> AssetBundleUseTime_RunTime = new List<ABDetailRecord>();

	public static Dictionary<string, RuntimePhaseABPreLoadInfo> PhaseABPreloadInfos_RT = new Dictionary<string, RuntimePhaseABPreLoadInfo>();
	public static void AddABUse(string name, ABDetailRecord.RecordType type)
	{
		if(AssetBundleHelper.Ins != null)
		{
			AssetBundleUseTime_RunTime.Add(new ABDetailRecord(name, type, DateTime.Now, AssetBundleHelper.Stage_C));
		}
	}
	
	public class RuntimePhaseABPreLoadInfo
	{
		public string phaseName;
		public Dictionary<string, float> preloadWithPriority = new Dictionary<string, float>();
		public List<string> preloadList_RT = new List<string>();

		//差集
		public List<string> Except(RuntimePhaseABPreLoadInfo other)
		{
			return preloadList_RT.Except(other.preloadList_RT).ToList();
		}
		public RuntimePhaseABPreLoadInfo()
		{
		}
		public RuntimePhaseABPreLoadInfo(string n)
		{
			phaseName = n;
		}

#if UNITY_EDITOR
		public void OutputXMlNode(XmlDocument doc, XmlElement root)
		{
			XmlElement node = doc.CreateElement(xmlNode_Phase);
			node.SetAttribute(xmlNode_PhaseName, phaseName);
			foreach (var k in preloadWithPriority)
			{
				XmlElement e = doc.CreateElement(xmlNode_AB);
				e.SetAttribute(xmlNode_ABKey, k.Key);
				e.SetAttribute(xmlNode_ABPri, k.Value.ToString());
				node.AppendChild(e);
			}
			root.AppendChild(node);
		}
#endif
		public void InputXMlNode(XmlDocument doc, XmlElement node)
		{
			phaseName = node.GetAttribute(xmlNode_PhaseName);
			XmlNodeList nodeList = node.ChildNodes;
			foreach (XmlElement e in nodeList)
			{
				string key = e.GetAttribute(xmlNode_ABKey);
				float pri = float.Parse(e.GetAttribute(xmlNode_ABPri));
				if (!preloadWithPriority.ContainsKey(key))
				{
					preloadWithPriority.Add(key, pri);
				}
				else
				{
					Debug.LogError("if (!preloadWithPriority.ContainsKey(key)) :" + key);
				}
			}
			foreach(var p in preloadWithPriority.OrderByDescending(p => p.Value).Select(p => p.Key))
			{
				preloadList_RT.Add(p);
			}

		}
	}
	public static List<string> StagePreload(string tar)
	{
		if (PhaseABPreloadInfos_RT.ContainsKey(tar))
		{
			return PhaseABPreloadInfos_RT[tar].preloadList_RT.ToList();
		}
		return null;
	}
	public static List<string> StageDiff(string from, string to)
	{
		if(PhaseABPreloadInfos_RT.ContainsKey(from) && PhaseABPreloadInfos_RT.ContainsKey(to))
		{
			return PhaseABPreloadInfos_RT[from].Except(PhaseABPreloadInfos_RT[to]);
		}
		return null;
	}
	public class ABDetailRecord
	{
		public enum RecordType
		{
			None,
			Get,
			ContainsKey,
			Add,
		}

		public string nameKey;
		public RecordType recordType;
		public DateTime time;
		public AssetBundleHelper.StageInfo ABStage;
		public ABDetailRecord()
		{
		}
		public ABDetailRecord(string n, RecordType t, DateTime d, AssetBundleHelper.StageInfo s)
		{
			nameKey = n;
			recordType = t;
			time = d;
			ABStage = s;
		}
		public void InputXmlNode(XmlElement node)
		{
			//IFormatProvider ifp = new CultureInfo("zh-CN", true);
			nameKey = node.GetAttribute(xmlNode_Name);
			recordType = (RecordType)Enum.Parse(typeof(RecordType), node.GetAttribute(xmlNode_Type));
			long ticks = long.Parse(node.GetAttribute(xmlNode_Time_Ticks));
			time = new DateTime(ticks);
			ticks = long.Parse(node.GetAttribute(xmlNode_StageBeginTime_Ticks));
			ABStage = new AssetBundleHelper.StageInfo(node.GetAttribute(xmlNode_Stage), new DateTime(ticks));
		}
		public void OutputXmlNode(XmlElement node)
		{
			node.SetAttribute(xmlNode_Name, nameKey.Substring(nameKey.LastIndexOf(PlatformBundlePath) + PlatformBundlePath.Length + 1));
			node.SetAttribute(xmlNode_Type, Enum.GetName(typeof(RecordType), recordType));
			node.SetAttribute(xmlNode_Time, time.ToString(DateFormat));
			node.SetAttribute(xmlNode_Time_Ticks, time.Ticks.ToString());
			node.SetAttribute(xmlNode_Stage, ABStage.name);
			node.SetAttribute(xmlNode_StageBeginTime, ABStage.beginTime.ToString(DateFormat));
			node.SetAttribute(xmlNode_StageBeginTime_Ticks, ABStage.beginTime.Ticks.ToString());

		}
	}
	public static void LoadPreloadInfo(XmlDocument xmlDoc)
	{
		PhaseABPreloadInfos_RT.Clear();
		XmlNodeList nodeList = xmlDoc.SelectSingleNode(xmlNode_PhaseRoot).ChildNodes;
		foreach (XmlElement node in nodeList)
		{
			var phase = new RuntimePhaseABPreLoadInfo();
			phase.InputXMlNode(xmlDoc, node);
			PhaseABPreloadInfos_RT.Add(phase.phaseName, phase);
		}
	}
	public static void OutputABUseXml()
	{
#if UNITY_EDITOR
		if (!Directory.Exists(XmlUsesDirName))
		{
			Directory.CreateDirectory(XmlUsesDirName);
		}
		{
			//创建XML文档实例  
			XmlDocument xmlDoc = new XmlDocument();
			XmlElement AllRoot = xmlDoc.CreateElement(xmlNode_Title);
			xmlDoc.AppendChild(AllRoot);
			foreach (var abu in AssetBundleUseTime_RunTime)
			{
				XmlElement ABD = xmlDoc.CreateElement(xmlNode_Title_2);
				abu.OutputXmlNode(ABD);
				AllRoot.AppendChild(ABD);
			}

			xmlDoc.Save(Path.Combine(XmlUsesDirName, xmlUsesDirName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss") + xmlExtName));
		}
#endif
	}
	public static string GetXmlNameWithExt()
	{
		return xmlDataAnalysisDirName + xmlExtName;
	}



#if UNITY_EDITOR
	public static string XmlUsesDirName { get { return Path.Combine(xmlDirRoot, xmlUsesDirName); } }
	public static string XmlUsesDirName_Outdate { get { return Path.Combine(xmlDirRoot, xmlUsesDirName_Outdate); } }
	public static string XmlDataAnalysisDirName { get { return Path.Combine(xmlDirRoot, xmlDataAnalysisDirName); } }

	public static Dictionary<string, RuntimePhaseABPreLoadInfo> PhaseABPreloadInfos_EditorAnalysis = new Dictionary<string, RuntimePhaseABPreLoadInfo>();
	public class ABUseInfo
	{
		public static Dictionary<string, List<TimeSpan>> ABLoadTime = new Dictionary<string, List<TimeSpan>>();
		public static Dictionary<string, DateTime> ABFirstbeginTime = new Dictionary<string, DateTime>();
		public static Dictionary<string, TimeSpan> ABLoadTimeAvg = new Dictionary<string, TimeSpan>();
		public static void CalcABLoadTimeAvg()
		{
			foreach (var ab in ABLoadTime)
			{
				long tick = 0;
				foreach(var ts in ab.Value)
				{
					tick += ts.Ticks;
				}
				tick /= ab.Value.Count;
				ABLoadTimeAvg.Add(ab.Key, new TimeSpan(tick));
			}
		}
		public static void ClearSomething()
		{
			ABLoadTime.Clear();
			ABFirstbeginTime.Clear();
			ABLoadTimeAvg.Clear();
		}
		public string nameKey;
		public int getCount = 0;
		public int containCount = 0;
		public TimeSpan loadTime;
		public DateTime firstbeginLoadTime;
		public List<ABDetailRecord> listRecords = new List<ABDetailRecord>();
		public ABUseInfo(string n)
		{
			nameKey = n;
			getCount = 0;
			containCount = 0;
			listRecords.Clear();
			loadTime = TimeSpan.Zero;
			firstbeginLoadTime = DateTime.MinValue;
		}
		public void FixTime()
		{
			if (ABLoadTimeAvg.ContainsKey(nameKey))
			{
				loadTime = ABLoadTimeAvg[nameKey];
			}
			else
			{
				loadTime = TimeSpan.Zero;
			}
			if (ABFirstbeginTime.ContainsKey(nameKey))
			{
				firstbeginLoadTime = ABFirstbeginTime[nameKey];
			}
			else
			{
				firstbeginLoadTime = DateTime.MinValue;
			}
		}
		public void Add(ABDetailRecord add)
		{
			listRecords.Add(add);
		}
		public void Sort(Comparison<ABDetailRecord> comparison)
		{
			listRecords.Sort(comparison);
			ABDetailRecord last = null;
			foreach (var l in listRecords)
			{
				if(l.recordType == ABDetailRecord.RecordType.Get)
				{
					getCount++;
				}
				else if(l.recordType == ABDetailRecord.RecordType.ContainsKey)
				{
					containCount++;
				}
				else if(l.recordType == ABDetailRecord.RecordType.Add)
				{
// 					if (loadTime == TimeSpan.Zero)
// 					{
// 						loadTime = l.time - last.time;
// 					}
// 					else
// 					{
// 						loadTime = new TimeSpan((loadTime.Ticks + (l.time - last.time).Ticks) / 2);
// 					}
					// 					if(firstbeginLoadTime == DateTime.MinValue)
					// 					{
					// 						firstbeginLoadTime = last.time;
					// 					}
					if (!ABLoadTime.ContainsKey(nameKey))
					{
						ABLoadTime.Add(nameKey, new List<TimeSpan>());
					}
					
					{
						ABLoadTime[nameKey].Add((l.time - last.time));
					}
					if (!ABFirstbeginTime.ContainsKey(nameKey))
					{
						ABFirstbeginTime.Add(nameKey, firstbeginLoadTime);
					}
				}
				last = l;
			}
		}
	}
	public class PhaseABInfo
	{
		public string phaseName;
		public Dictionary<string, ABUseInfo> phaseAllABDic = new Dictionary<string, ABUseInfo>();
		public Dictionary<string, ABUseInfo> phaseOnlyOnceUsedABDic = new Dictionary<string, ABUseInfo>();
		
		public PhaseABInfo(string n)
		{
			phaseName = n;
		}
		//交集
		public static List<string> Intersection(List<string> a, List<string> b)
		{
			if(a == null || b == null || a.Count == 0 || b.Count == 0)
			{
				return new List<string>();
			}
			return a.Intersect(b).ToList();
		}
		public List<string> AllABDicToList()
		{
			return phaseAllABDic.Select(p => p.Key).ToList();
		}
// 		public List<string> Intersection(PhaseABInfo other)
// 		{
// 			return Intersection(phaseAllABDic.Select(p => p.Key).ToList(), other.phaseAllABDic.Select(p => p.Key).ToList());
// 		}
		//差集
		public static List<string> Diffenrence(List<string> a, List<string> b)
		{
			bool aE = (a == null || a.Count == 0);
			bool bE = (b == null || b.Count == 0);
			if (aE)
			{
				return new List<string>();
			}
			else if (bE)
			{
				return a.ToList();
			}
			return a.Except(b).ToList();
		}
		//并集
		public static List<string> Union(List<string> a, List<string> b)
		{
			bool aE = (a == null || a.Count == 0);
			bool bE = (b == null || b.Count == 0);
			if ( aE && bE)
			{
				return new List<string>();
			}
			else if(aE)
			{
				return b.ToList();
			}
			else if (bE)
			{
				return a.ToList();
			}
			return a.Union(b).ToList();
		}

	}

	public static Dictionary<string, List<KeyValuePair<string, Dictionary<string, ABUseInfo>>>> sourceDatasDic = new Dictionary<string, List<KeyValuePair<string, Dictionary<string, ABUseInfo>>>>();

	private static void SourceFilesProcess(Dictionary<string, List<ABDetailRecord>> datas)
	{
		foreach (var rs in datas)
		{
			sourceDatasDic.Add(rs.Key, SingleSourceFileProcess(rs.Key, rs.Value));
		}
	}
	private static List<KeyValuePair<string, Dictionary<string, ABUseInfo>>> SingleSourceFileProcess(string name, List<ABDetailRecord> rs)
	{
		Comparison<ABDetailRecord> comparison = (a, b) => DateTime.Compare(a.ABStage.beginTime, b.ABStage.beginTime);
		rs.Sort(comparison);
		List<KeyValuePair<string, Dictionary<string, ABUseInfo>>> tarL = new List<KeyValuePair<string, Dictionary<string, ABUseInfo>>>();
		string lastKey = "";
		foreach (var r in rs)
		{
			if (!r.ABStage.name.Equals(lastKey))
			{
				tarL.Add(new KeyValuePair<string, Dictionary<string, ABUseInfo>>(r.ABStage.name, new Dictionary<string, ABUseInfo>()));
				lastKey = r.ABStage.name;
			}
			if (!tarL[tarL.Count - 1].Value.ContainsKey(r.nameKey))
			{
				tarL[tarL.Count - 1].Value.Add(r.nameKey, new ABUseInfo(r.nameKey));
			}
			tarL[tarL.Count - 1].Value[r.nameKey].Add(r);
		}
		ABUseInfo.ClearSomething();
		//最后针对每个单独的按时间排序，然后可以开始处理了
		Comparison<ABDetailRecord> comparison2 = (a, b) => DateTime.Compare(a.time, b.time);
		foreach (var t in tarL)
		{
			foreach(var tt in t.Value)
			{
				tt.Value.Sort(comparison2);
			}
		}
		ABUseInfo.CalcABLoadTimeAvg();
		foreach (var t in tarL)
		{
			foreach (var tt in t.Value)
			{
				tt.Value.FixTime();
			}
		}

		return tarL;
	}

	public class PhaseABInfoToAnother
	{
		public string fromPhase;
		public string toPhase;
		public PhaseABInfo from;
		public PhaseABInfo to;

		public PhaseABInfoToAnother(string fp, string tp, PhaseABInfo f, PhaseABInfo t)
		{
			fromPhase = fp;
			toPhase = tp;
			from = f;
			to = t;
		}
	}
	public class PhaseABInfoTreeLinkInfo
	{
		public string fromPhase;
		public string toPhase;
		public PhaseABInfoTreeNode node;
		public List<PhaseABInfoToAnother> linkList = new List<PhaseABInfoToAnother>();
		public PhaseABInfoTreeLinkInfo(string f, string t, PhaseABInfoTreeNode r)
		{
			fromPhase = f;
			toPhase = t;
			node = r;
		}
		public void Add(PhaseABInfoToAnother pA)
		{
			linkList.Add(pA);
		}
	}
	public class PhaseABInfoTreeNode
	{
		public string phaseName;
		public Dictionary<string, PhaseABInfoTreeLinkInfo> Lasts = new Dictionary<string, PhaseABInfoTreeLinkInfo>();
		public Dictionary<string, PhaseABInfoTreeLinkInfo> Nexts = new Dictionary<string, PhaseABInfoTreeLinkInfo>();
		public Dictionary<string, int> priority = new Dictionary<string, int>();

		public PhaseABInfoTreeNode(string n)
		{
			phaseName = n;
		}

		public void AddLast(PhaseABInfoToAnother pA)
		{
			if(!Lasts.ContainsKey(pA.fromPhase))
			{
				Lasts.Add(pA.fromPhase, new PhaseABInfoTreeLinkInfo(pA.fromPhase, pA.toPhase, this));
			}
			Lasts[pA.fromPhase].Add(pA);
		}
		public void AddNext(PhaseABInfoToAnother pA)
		{
			if (!Nexts.ContainsKey(pA.toPhase))
			{
				Nexts.Add(pA.toPhase, new PhaseABInfoTreeLinkInfo(pA.fromPhase, pA.toPhase, this));
			}
			Nexts[pA.toPhase].Add(pA);
		}
		//以下是分析结果
		public Dictionary<string, float> NeedLoadKeys = new Dictionary<string, float>();//进入本阶段共同的资源
		public Dictionary<string, float> CanLoadKeys = new Dictionary<string, float>(); //进入本阶段可能需要的资源

		//以下是比较有实际意义的结果
		public Dictionary<string, float> PreLoadKeys_Priority;//所有可能加载资源设置权重

	}
	private const int OnlyOnceUsedCount = 1;
	
	private static void  DataAnalysis( Dictionary<string, List<KeyValuePair<string, Dictionary<string, ABUseInfo>>>> srcDatas)
	//private static void SingleDataAnalysis(string name, List<KeyValuePair<string, Dictionary<string, ABUseInfo>>> data)
	{
		//单独阶段只引用一次和多次的，输出结果备用。
		List<List<PhaseABInfo>> phasesList = new List<List<PhaseABInfo>>();
		Dictionary<string ,List<PhaseABInfo>> phasesDic = new Dictionary<string, List<PhaseABInfo>>();
		foreach(var sd in srcDatas)
		{
			var tmpList = new List<PhaseABInfo>();
			foreach (var phase in sd.Value)
			{
				var phaseInfo = new PhaseABInfo(phase.Key);
				//phase.Key 阶段名称（login，selectchar，类似这种）

				foreach (var ab in phase.Value)
				{
					//ab.Key AB名字
					phaseInfo.phaseAllABDic.Add(ab.Key, ab.Value);
					if (ab.Value.getCount == OnlyOnceUsedCount || ab.Value.containCount == OnlyOnceUsedCount)
					{
						phaseInfo.phaseOnlyOnceUsedABDic.Add(ab.Key, ab.Value);
					}
				}
				tmpList.Add(phaseInfo);
				{
					if (!phasesDic.ContainsKey(phase.Key))
					{
						phasesDic.Add(phase.Key, new List<PhaseABInfo>());
					}
					phasesDic[phase.Key].Add(phaseInfo);
				}
			}
			phasesList.Add(tmpList);
		}
		//先根据阶段变化顺序理清可能发生的阶段变化
		Dictionary<string, PhaseABInfoTreeNode> phaseTree = new Dictionary<string, PhaseABInfoTreeNode>();
		foreach (var phases in phasesList)
		{
			PhaseABInfo last = null;
			foreach (var phase in phases)
			{
				if (last != null)
				{
					if (!phaseTree.ContainsKey(last.phaseName))
					{
						phaseTree.Add(last.phaseName, new PhaseABInfoTreeNode(last.phaseName));
					}
					if (!phaseTree.ContainsKey(phase.phaseName))
					{
						phaseTree.Add(phase.phaseName, new PhaseABInfoTreeNode(phase.phaseName));
					}
					var pA = new PhaseABInfoToAnother(last.phaseName, phase.phaseName, last, phase);
					phaseTree[last.phaseName].AddNext(pA);
					phaseTree[phase.phaseName].AddLast(pA);
				}
				last = phase;
			}
		}
		//求交集
		foreach (var phase in phaseTree)
		{
			if(phasesDic.ContainsKey(phase.Key))
			{

				Dictionary<string, int> priority = new Dictionary<string, int>();
				List<string> tmp = null;
				List<string> tmpI = null;
				List<string> tmpU = null;
				foreach (var pk in phase.Value.Nexts)
				{
					phase.Value.priority.Add(pk.Key, pk.Value.linkList.Count);//phasesDic[pk.Key].Count();
					var ps = phasesDic[pk.Key];
					foreach(var p in ps)
					{
						if (tmp != null)
						{
							tmpI = PhaseABInfo.Intersection(tmp, tmpI);
							tmpU = PhaseABInfo.Union(tmp, tmpU);
						}
						else
						{
							tmpI = p.AllABDicToList();
							tmpU = p.AllABDicToList();
						}
 						tmp = p.AllABDicToList();
						foreach (var k in tmp)
						{
							if (!priority.ContainsKey(k))
							{
								priority.Add(k, 0);
							}
							priority[k]++;
						}
					}
				}
				if (tmpI != null)
				{
					foreach (var i in tmpI)
					{
						phase.Value.NeedLoadKeys.Add(i, (float)priority[i] / phasesDic[phase.Key].Count);
					}
					foreach (var d in PhaseABInfo.Diffenrence(tmpU, tmpI))
					{
						phase.Value.CanLoadKeys.Add(d, (float)priority[d] / phasesDic[phase.Key].Count);
					}
				}
				else
				{
					Debug.LogError("if(tmpI != null) :" + phase.Key);
				}

			}
		}
		//每个阶段的必加载项和可选加载项已经设置好了，下边要设置从当前阶段往其他阶段转换时可能需要的项
// 		foreach(var phase in phaseTree)
// 		{
// 			Dictionary<string, float> preloadWithPriority = new Dictionary<string, float>();
// 			foreach (var next in phase.Value.Nexts)
// 			{
// 				//next.Value.node.priority *
// 				foreach (var n in next.Value.node.NeedLoadKeys)
// 				{
// 					if(!preloadWithPriority.ContainsKey(n.Key))
// 					{
// 						preloadWithPriority.Add(n.Key, 0);
// 					}
// 					preloadWithPriority[n.Key] += next.Value.node.priority[n.Key] * n.Value;
// 				}
// 				foreach (var c in next.Value.node.CanLoadKeys)
// 				{
// 					if (!preloadWithPriority.ContainsKey(c.Key))
// 					{
// 						preloadWithPriority.Add(c.Key, 0);
// 					}
// 					preloadWithPriority[c.Key] += next.Value.node.priority[c.Key] * c.Value;
// 				}
// 			}
// 			phase.Value.PreLoadKeys_Priority = preloadWithPriority;
// 		}
		//填充到XML里
		PhaseABPreloadInfos_EditorAnalysis.Clear();
		foreach (var phase in phaseTree)
		{
			RuntimePhaseABPreLoadInfo rtPAB = new RuntimePhaseABPreLoadInfo(phase.Key);
			foreach (var n in phase.Value.NeedLoadKeys)
			{
				rtPAB.preloadWithPriority.Add(n.Key, n.Value);
			}
			foreach (var c in phase.Value.CanLoadKeys)
			{
				rtPAB.preloadWithPriority.Add(c.Key, c.Value);

			}
			PhaseABPreloadInfos_EditorAnalysis.Add(rtPAB.phaseName, rtPAB);
		}
		phasesList.Clear();


		return;
	}

	public static void OutputXml()
	{
		if (!Directory.Exists(XmlDataAnalysisDirName))
		{
			Directory.CreateDirectory(XmlDataAnalysisDirName);
		}
		XmlDocument doc = new XmlDocument();
		XmlElement AllRoot = doc.CreateElement(xmlNode_PhaseRoot);
		doc.AppendChild(AllRoot);
		foreach (var p in PhaseABPreloadInfos_EditorAnalysis)
		{
			p.Value.OutputXMlNode(doc, AllRoot);
		}

		doc.Save(Path.Combine(XmlDataAnalysisDirName, GetXmlNameWithExt()));
	}
	private static void DatasAnalysis()
	{
		DataAnalysis(sourceDatasDic);
	}
	private static void Analysis(Dictionary<string, List<ABDetailRecord>> datas)
	{
		sourceDatasDic.Clear();
		SourceFilesProcess(datas);
		DatasAnalysis();
	}
	private static void MovePFResultXmlToStreamingAssets(BuildTarget bt)
	{
		//RuntimePlatform runtimePlatform = BuildBundleManager.BuildTargetToRuntimePlatform(bt);
		//string versionName = AssetBundleHelper.RuntimePlatformToSimplifyName(runtimePlatform);

		string tarPath = BuildBundleManager.NeedBuildAssetBundlePath;
		if (!Directory.Exists(tarPath))
			Directory.CreateDirectory(tarPath);

		string fileName = GetXmlNameWithExt();
		string f = Path.Combine(XmlDataAnalysisDirName, fileName);
		string fTarPath = AssetBundleHelper.Combine(tarPath, fileName);
		if (File.Exists(fTarPath))
		{
			File.Copy(f, fTarPath, true);
		}
		else
		{
			File.Copy(f, fTarPath);
		}
		AssetDatabase.Refresh();
	}
	[MenuItem("AssetBundle/6.ABUseAnalysis(AssetBundleUseAnalysis.cs)")]
	public static void InputAllXml()
	{
		if (!Directory.Exists(XmlUsesDirName))
		{
			Debug.Log(XmlUsesDirName + "文件夹不存在");
			return;
		}
		Dictionary<string ,List<ABDetailRecord>> tmpRecords = new Dictionary<string, List<ABDetailRecord>>();
		var files = Directory.GetFiles(XmlUsesDirName);
		if(files == null || files.Length == 0)
		{
			Debug.Log(XmlUsesDirName + "文件夹里没有文件，无法分析");
			return;
		}
		if (!Directory.Exists(XmlUsesDirName_Outdate))
		{
			Directory.CreateDirectory(XmlUsesDirName_Outdate);
		}
		foreach (var f in files.Where(f=>f.EndsWith(xmlExtName)))
		{
			List<ABDetailRecord> l = new List<ABDetailRecord>();
			tmpRecords.Add(f, l);
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(f);
			XmlNodeList nodeList = xmlDoc.SelectSingleNode(xmlNode_Title).ChildNodes;
			foreach (XmlElement node in nodeList)
			{
				var abr = new ABDetailRecord();
				abr.InputXmlNode(node);
				tmpRecords[f].Add(abr);
			}
			//File.Move(f, Path.Combine(xmlDir_OutDate_Name, f.Substring(f.LastIndexOf(xmlDir_Name) + xmlDir_Name.Length + 1)));
		}
		Analysis(tmpRecords);

		OutputXml();
	}

	[MenuItem("AssetBundle/7.Move_CurrentPFResultXml(AssetBundleUseAnalysis.cs)")]
	public static void MoveOutputXml()
	{
		MovePFResultXmlToStreamingAssets(EditorUserBuildSettings.activeBuildTarget);
	}
#endif
}
