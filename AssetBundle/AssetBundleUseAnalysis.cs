using System.Collections.Generic;
using System.IO;
using System.Xml;
using System;
using UnityEngine;
using System.Linq;
/// <summary>
/// 用来分析AB的使用情况
/// 这里主要是非编辑器功能
/// </summary>
public partial class AssetBundleUseAnalysis
{

	private static List<ABDetailRecord> AssetBundleUseTime_RunTime = new List<ABDetailRecord>();

	public static Dictionary<string, RuntimePhaseABPreLoadInfo> PhaseABPreloadInfos_RT = new Dictionary<string, RuntimePhaseABPreLoadInfo>();
	public static void AddABUse(string name, ABDetailRecord.RecordType type)
	{
		if(AssetBundleHelper.Ins != null)
		{
			AssetBundleUseTime_RunTime.Add(new ABDetailRecord(name, type, DateTime.Now, AssetBundleHelper.Stage_C));
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
	
	public static void LoadPreloadInfo(XmlDocument xmlDoc)
	{
		PhaseABPreloadInfos_RT.Clear();
		XmlNodeList nodeList = xmlDoc.SelectSingleNode(AssetBundleSettingHelper.xmlNode_PhaseRoot).ChildNodes;
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
		var abpi = AssetBundleHelper.Ins.ABSettingHelper.GetABUsesABPI();
		if (!Directory.Exists(abpi.Dir_Relative))
		{
			Directory.CreateDirectory(abpi.Dir_Relative);
		}
		{
			//创建XML文档实例  
			XmlDocument xmlDoc = new XmlDocument();
			XmlElement AllRoot = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_Title);
			xmlDoc.AppendChild(AllRoot);
			foreach (var abu in AssetBundleUseTime_RunTime)
			{
				XmlElement ABD = xmlDoc.CreateElement(AssetBundleSettingHelper.xmlNode_Title_2);
				abu.OutputXmlNode(ABD);
				AllRoot.AppendChild(ABD);
			}

			xmlDoc.Save(abpi.FullName_RelativePath);
		}
#endif
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
		XmlElement node = doc.CreateElement(AssetBundleSettingHelper.xmlNode_Phase);
		node.SetAttribute(AssetBundleSettingHelper.xmlNode_Name, phaseName);
		foreach (var k in preloadWithPriority)
		{
			XmlElement e = doc.CreateElement(AssetBundleSettingHelper.xmlNode_AB);
			e.SetAttribute(AssetBundleSettingHelper.xmlNode_Name, k.Key);
			e.SetAttribute(AssetBundleSettingHelper.xmlNode_ABPri, k.Value.ToString());
			node.AppendChild(e);
		}
		root.AppendChild(node);
	}
#endif
	public void InputXMlNode(XmlDocument doc, XmlElement node)
	{
		phaseName = node.GetAttribute(AssetBundleSettingHelper.xmlNode_Name);
		XmlNodeList nodeList = node.ChildNodes;
		foreach (XmlElement e in nodeList)
		{
			string key = e.GetAttribute(AssetBundleSettingHelper.xmlNode_Name);
			float pri = float.Parse(e.GetAttribute(AssetBundleSettingHelper.xmlNode_ABPri));
			if (!preloadWithPriority.ContainsKey(key))
			{
				preloadWithPriority.Add(key, pri);
			}
			else
			{
				Debug.LogError("if (!preloadWithPriority.ContainsKey(key)) :" + key);
			}
		}
		foreach (var p in preloadWithPriority.OrderByDescending(p => p.Value).Select(p => p.Key))
		{
			preloadList_RT.Add(p);
		}

	}
}

public class ABDetailRecord
{
	public const string DateFormat = @"yyyy-MM-dd HH:mm:ss.ffffff";

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
		nameKey = node.GetAttribute(AssetBundleSettingHelper.xmlNode_Name);
		recordType = (RecordType)Enum.Parse(typeof(RecordType), node.GetAttribute(AssetBundleSettingHelper.xmlNode_Type));
		long ticks = long.Parse(node.GetAttribute(AssetBundleSettingHelper.xmlNode_Time_Ticks));
		time = new DateTime(ticks);
		ticks = long.Parse(node.GetAttribute(AssetBundleSettingHelper.xmlNode_StageBeginTime_Ticks));
		ABStage = new AssetBundleHelper.StageInfo(node.GetAttribute(AssetBundleSettingHelper.xmlNode_Stage), new DateTime(ticks));
	}
	public void OutputXmlNode(XmlElement node)
	{
		var abpi = AssetBundleHelper.Ins.ABSettingHelper.GetCurPlatformABPath();
		node.SetAttribute(AssetBundleSettingHelper.xmlNode_Name, nameKey.Substring(nameKey.LastIndexOf(abpi.Dir_Relative) + abpi.Dir_Relative.Length + 1));
		node.SetAttribute(AssetBundleSettingHelper.xmlNode_Type, Enum.GetName(typeof(RecordType), recordType));
		node.SetAttribute(AssetBundleSettingHelper.xmlNode_Time, time.ToString(DateFormat));
		node.SetAttribute(AssetBundleSettingHelper.xmlNode_Time_Ticks, time.Ticks.ToString());
		node.SetAttribute(AssetBundleSettingHelper.xmlNode_Stage, ABStage.name);
		node.SetAttribute(AssetBundleSettingHelper.xmlNode_StageBeginTime, ABStage.beginTime.ToString(DateFormat));
		node.SetAttribute(AssetBundleSettingHelper.xmlNode_StageBeginTime_Ticks, ABStage.beginTime.Ticks.ToString());

	}
}