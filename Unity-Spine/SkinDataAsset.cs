using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Object = System.Object;
namespace SpineEx
{
	public class SkinDataAsset : ScriptableObject
	{
		public List<SkinDataEx> AllSkins;

		#region 主要逻辑都是从SkeletonJson的代码中转换过来的，所以要升级Spine后需要对照着看看有没有改变再升级一下这段代码
		public void InputJson(String text, Boolean isOutputTxt = false, String outputName = "Assets/output.txt",Boolean debugMode =false)
		{
			var reader = new System.IO.StringReader(text);
			var ret = Spine.Json.Deserialize(reader);
			var root = ret as Dictionary<string, System.Object>;
			if (!root.ContainsKey("skins"))
			{
				Debug.LogWarning("root does not contain skins.");
				return;
			}
			if (isOutputTxt)
			{
				OutputDebugTxt((Dictionary<string, Object>)root["skins"], outputName, debugMode);
			}
			SkinDataEx sd;
			SlotDataEx ssd;
			AttachmentDataEx ad;
			if(AllSkins == null)
			{
				AllSkins = new List<SkinDataEx>();
			}
			else
			{
				AllSkins.Clear();
			}
			foreach (var skinMap in (Dictionary<string, Object>)root["skins"])
			{
				sd = new SkinDataEx();
				sd.AllSlots = new List<SlotDataEx>();
				sd.SkinName = skinMap.Key;
				foreach (var slotEntry in (Dictionary<string, Object>)skinMap.Value)
				{
					ssd = new SlotDataEx();
					ssd.AllAttachments = new List<AttachmentDataEx>();
					ssd.SlotName = slotEntry.Key;
					foreach (var entry in ((Dictionary<string, Object>)slotEntry.Value))
					{
						ad = new AttachmentDataEx();
						ad.AttachmentName = entry.Key;
						var map = entry.Value as Dictionary<string, Object>;
						//if (map.ContainsKey("name"))
						//{
						//	ad.Add(new KeyStringPair("name", GetString(map, "name", entry.Key)));
						//}
						var attName = GetString(map, "name", entry.Key);

						var typeName = GetString(map, "type", "region");
						if (typeName == "skinnedmesh") typeName = "weightedmesh";
						if (typeName == "weightedmesh") typeName = "mesh";
						if (typeName == "weightedlinkedmesh") typeName = "linkedmesh";
						ad.type = (AttachmentType)Enum.Parse(typeof(AttachmentType), typeName, true);


						if (map.ContainsKey("path"))
						{
							attName = GetString(map, "path", attName);
							//ad.Add(new KeyStringPair("path", attName));
						}
						ad.PathName = attName;

						switch (ad.type)
						{
							case AttachmentType.Region:
								ad.Add(new KeyFloatPair("x", GetFloat(map, "x", 0)));
								ad.Add(new KeyFloatPair("y", GetFloat(map, "y", 0)));
								ad.Add(new KeyFloatPair("scaleX", GetFloat(map, "scaleX", 1)));
								ad.Add(new KeyFloatPair("scaleY", GetFloat(map, "scaleY", 1)));
								ad.Add(new KeyFloatPair("rotation", GetFloat(map, "rotation", 0)));
								ad.Add(new KeyFloatPair("width", GetFloat(map, "width", 32)));
								ad.Add(new KeyFloatPair("height", GetFloat(map, "height", 32)));
								if (map.ContainsKey("color"))
								{
									ad.Add(new KeyColorPair("color", GetColor(map, "color", Color.white)));
								}
								break;
							case AttachmentType.Boundingbox:
								ad.Add(new KeyIntPair("vertexCount", GetInt(map, "vertexCount", 0)));
								ad.Add(new KeyFloatArrayPair("vertices", GetFloatList(map, "vertices")));
								break;
							case AttachmentType.Mesh:
							case AttachmentType.Linkedmesh:
								if (map.ContainsKey("color"))
								{
									ad.Add(new KeyColorPair("color", GetColor(map, "color", Color.white)));
								}
								ad.Add(new KeyFloatPair("width", GetFloat(map, "width", 0)));
								ad.Add(new KeyFloatPair("height", GetFloat(map, "height", 0)));
								if (map.ContainsKey("parent"))
								{
									ad.Add(new KeyStringPair("parent", GetString(map, "parent", null)));
									ad.Add(new KeyBooleanPair("deform", GetBoolean(map, "deform", true)));
									ad.Add(new KeyStringPair("skin", GetString(map, "skin", null)));//LinkedMesh
								}
								ad.Add(new KeyFloatArrayPair("uvs", GetFloatList(map, "uvs")));
								ad.Add(new KeyFloatArrayPair("vertices", GetFloatList(map, "vertices")));
								ad.Add(new KeyIntArrayPair("triangles", GetIntList(map, "triangles")));
								if (map.ContainsKey("hull"))
								{
									ad.Add(new KeyIntPair("hull", GetInt(map, "hull", 0)));
								}
								if (map.ContainsKey("edges"))
								{ 
									ad.Add(new KeyIntArrayPair("edges", GetIntList(map, "edges")));
								}
								break;
							case AttachmentType.Path:
								ad.Add(new KeyBooleanPair("closed", GetBoolean(map, "closed", false)));
								ad.Add(new KeyBooleanPair("constantSpeed", GetBoolean(map, "constantSpeed", true)));

								ad.Add(new KeyIntPair("vertexCount", GetInt(map, "vertexCount", 0)));
								ad.Add(new KeyFloatArrayPair("vertices", GetFloatList(map, "vertices")));
								ad.Add(new KeyFloatArrayPair("lengths", GetFloatList(map, "lengths")));
								break;
							case AttachmentType.Point:
								ad.Add(new KeyFloatPair("x", GetFloat(map, "x", 0)));
								ad.Add(new KeyFloatPair("y", GetFloat(map, "y", 0)));
								ad.Add(new KeyFloatPair("rotation", GetFloat(map, "rotation", 0)));
								break;
							case AttachmentType.Clipping:
								ad.Add(new KeyStringPair("end", GetString(map, "end", null)));
								ad.Add(new KeyIntPair("vertexCount", GetInt(map, "vertexCount", 0)));
								ad.Add(new KeyFloatArrayPair("vertices", GetFloatList(map, "vertices")));
								break;
							default:
								Debug.LogError("Need Update This CSharp File.:" + ad.type);
								break;

						}
						ssd.AllAttachments.Add(ad);
					}
					sd.AllSlots.Add(ssd);
				}
				AllSkins.Add(sd);
			}
		}
		#region 这部分是为了把数据输出到TXT方便查看而做
		private void OutputDebugTxt(Dictionary<string, Object> root,String outputName, Boolean debugMode)
		{
			var sb = new StringBuilder();
			foreach (var skinMap in root)
			{
				sb.Append('{');

				var skinTab = "	";
				if (debugMode)
				{
					sb.Append('\n');
					sb.Append(skinTab);
				}
				//var skin = new Skin(skinMap.Key);
				//皮肤的名字
				sb.Append('"');
				sb.Append(skinMap.Key);
				sb.Append("\": {");

				foreach (var slotEntry in (Dictionary<string, Object>)skinMap.Value)
				{
					var slotTab = "		";
					if (debugMode)
					{
						sb.Append('\n');
						sb.Append(slotTab);
					}
					//Slot的名字
					sb.Append('"');
					sb.Append(slotEntry.Key);
					sb.Append("\": {");

					//int slotIndex = skeletonData.FindSlotIndex(slotEntry.Key);
					foreach (var entry in ((Dictionary<string, Object>)slotEntry.Value))
					{
						var attachmentTab = "			";
						if (debugMode)
						{
							sb.Append('\n');
							sb.Append(attachmentTab);
						}
						//Attachment的名字
						//Slot的名字
						sb.Append('"');
						sb.Append(entry.Key);
						sb.Append("\": {");
						var props = entry.Value as Dictionary<string, Object>;
						var propertyTab = "				";

						var typeName = GetString(props, "type", "region");
						//var realTypeName = typeName;
						if (typeName == "skinnedmesh") typeName = "weightedmesh";
						if (typeName == "weightedmesh") typeName = "mesh";
						if (typeName == "weightedlinkedmesh") typeName = "linkedmesh";
						var type = (AttachmentType)Enum.Parse(typeof(AttachmentType), typeName, true);
						{
							if (debugMode)
							{
								sb.Append('\n');
							}
							WriteStringValue(props, "type", sb, true, debugMode, propertyTab);
						}
						switch (type)
						{
							case AttachmentType.Region:
								{
									WriteStringValue(props, "color", sb, true, debugMode, propertyTab);
									WriteSingleValue(props, "x", sb, true, debugMode, propertyTab);
									WriteSingleValue(props, "y", sb, true, debugMode, propertyTab);
									WriteSingleValue(props, "scaleX", sb, true, debugMode, propertyTab);
									WriteSingleValue(props, "scaleY", sb, true, debugMode, propertyTab);
									WriteSingleValue(props, "rotation", sb, true, debugMode, propertyTab);
									WriteSingleValue(props, "width", sb, true, debugMode, propertyTab);
									WriteSingleValue(props, "height", sb, false, debugMode, propertyTab);
								}
								break;
							case AttachmentType.Boundingbox:
								WriteSingleArrayValue(props, "vertices", sb, true, debugMode, propertyTab);
								WriteStringValue(props, "vertexCount", sb, false, debugMode, propertyTab);
								break;
							case AttachmentType.Mesh:
							case AttachmentType.Linkedmesh:
								WriteStringValue(props, "color", sb, true, debugMode, propertyTab);
								WriteStringValue(props, "parent", sb, true, debugMode, propertyTab);
								WriteBooleanValue(props, "deform", sb, true, debugMode, propertyTab);
								WriteStringValue(props, "skin", sb, true, debugMode, propertyTab);
								WriteSingleArrayValue(props, "uvs", sb, true, debugMode, propertyTab);
								WriteSingleArrayValue(props, "vertices", sb, true, debugMode, propertyTab);
								WriteInt32ArrayValue(props, "triangles", sb, true, debugMode, propertyTab);
								WriteInt32Value(props, "hull", sb, true, debugMode, propertyTab);
								WriteInt32ArrayValue(props, "edges", sb, true, debugMode, propertyTab);
								WriteSingleValue(props, "width", sb, true, debugMode, propertyTab);
								WriteSingleValue(props, "height", sb, false, debugMode, propertyTab);
								break;
							case AttachmentType.Path:
								WriteBooleanValue(props, "closed", sb, true, debugMode, propertyTab);
								WriteBooleanValue(props, "constantSpeed", sb, true, debugMode, propertyTab);
								WriteSingleArrayValue(props, "vertices", sb, true, debugMode, propertyTab);
								WriteSingleArrayValue(props, "lengths", sb, true, debugMode, propertyTab);
								WriteInt32Value(props, "vertexCount", sb, false, debugMode, propertyTab);
								break;
							case AttachmentType.Point:
								WriteSingleValue(props, "rotation", sb, true, debugMode, propertyTab);
								WriteSingleValue(props, "x", sb, true, debugMode, propertyTab);
								WriteSingleValue(props, "y", sb, false, debugMode, propertyTab);
								break;
							case AttachmentType.Clipping:
								WriteStringValue(props, "end", sb, true, debugMode, propertyTab);
								WriteSingleArrayValue(props, "vertices", sb, true, debugMode, propertyTab);
								WriteInt32Value(props, "vertexCount", sb, false, debugMode, propertyTab);
								break;
						}
						if (debugMode)
						{
							sb.Append(attachmentTab);
						}
						sb.Append('}');
						if (debugMode)
						{
							sb.Append('\n');
						}
					}
					if (debugMode)
					{
						sb.Append(slotTab);
					}
					sb.Append('}');
					if (debugMode)
					{
						sb.Append('\n');
					}
				}
				if (debugMode)
				{
					sb.Append(skinTab);
				}
				sb.Append('}');
				if (debugMode)
				{
					sb.Append('\n');
				}
				sb.Append('}');
			}

			var fs = File.Open(outputName, FileMode.OpenOrCreate);
			fs.Seek(0, SeekOrigin.Begin);
			fs.SetLength(0);
			var sw = new StreamWriter(fs);
			var chars = sb.ToString().ToCharArray();
			sw.Write(chars, 0, chars.Length);
			sw.Flush();
			sw.Close();
		}
		#region 这一段是为了输出到文件中带有tab和换行而做的

		void WriteSingleValue(Dictionary<string, Object> map, String name, StringBuilder sb, Boolean isLast, Boolean debugMode = false, String tab = "")
		{
			if (!map.ContainsKey(name))
			{
				return;
			}
			WritePropName(name, sb, debugMode, tab);
			sb.Append(GetFloat(map, name, 0));
			WritePropTail(isLast, sb, debugMode, tab);
		}
		void WriteBooleanValue(Dictionary<string, Object> map, String name, StringBuilder sb, Boolean isLast, Boolean debugMode = false, String tab = "")
		{
			if (!map.ContainsKey(name))
			{
				return;
			}
			WritePropName(name, sb, debugMode, tab);
			sb.Append(GetBoolean(map, name, false));
			WritePropTail(isLast, sb, debugMode, tab);
		}
		void WriteStringValue(Dictionary<string, Object> map, String name, StringBuilder sb, Boolean needComma, Boolean debugMode = false, String tab = "")
		{
			if (!map.ContainsKey(name))
			{
				return;
			}
			WritePropName(name, sb, debugMode, tab);
			sb.Append('"');
			sb.Append(GetString(map, name, name));
			sb.Append('"');
			WritePropTail(needComma, sb, debugMode, tab);
		}
		void WriteInt32Value(Dictionary<string, Object> map, String name, StringBuilder sb, Boolean isLast, Boolean debugMode = false, String tab = "")
		{
			if (!map.ContainsKey(name))
			{
				return;
			}
			WritePropName(name, sb, debugMode, tab);
			sb.Append('"');
			sb.Append(GetInt(map, name, 0));
			sb.Append('"');
			WritePropTail(isLast, sb, debugMode, tab);
		}
		void WriteInt32ArrayValue(Dictionary<string, Object> map, String name, StringBuilder sb, Boolean isLast, Boolean debugMode = false, String tab = "")
		{
			if (!map.ContainsKey(name))
			{
				return;
			}
			WritePropName(name, sb, debugMode, tab);
			sb.Append('[');
			var list = (List<Object>)map[name];
			for (var i = 0; i < list.Count; i++)
			{
				//var num = (Int32)list[i];
				sb.Append(list[i]);
				if (i != list.Count - 1)
				{
					sb.Append(',');
				}
			}
			sb.Append(']');
			WritePropTail(isLast, sb, debugMode, tab);
		}
		void WriteSingleArrayValue(Dictionary<string, Object> map, String name, StringBuilder sb, Boolean isLast, Boolean debugMode = false, String tab = "")
		{
			if (!map.ContainsKey(name))
			{
				return;
			}
			WritePropName(name, sb, debugMode, tab);
			sb.Append('[');
			var list = (List<Object>)map[name];
			for (var i = 0; i < list.Count; i++)
			{
				sb.Append(((Single)list[i]));
				if (i != list.Count - 1)
				{
					sb.Append(',');
				}
			}
			sb.Append(']');
			WritePropTail(isLast, sb, debugMode, tab);
		}
		void WritePropName(String name, StringBuilder sb, Boolean debugMode = false, String tab = "")
		{
			if (debugMode)
			{
				sb.Append(tab);
			}
			//Slot的名字
			sb.Append('"');
			sb.Append(name);
			sb.Append("\":");
		}
		void WritePropTail(Boolean needComma, StringBuilder sb, Boolean debugMode = false, String tab = "")
		{
			if (needComma)
			{
				sb.Append(',');
			}
			if (debugMode)
			{
				sb.Append('\n');
			}
		}
		#endregion
		#region copy 这段是完整的copy，没有修改过的那种
		/// <summary>
		/// 这里有点修改
		/// </summary>
		/// <param name="map"></param>
		/// <param name="name"></param>
		/// <param name="scale"></param>
		/// <returns></returns>
		static float[] GetFloatArray(Dictionary<string, Object> map, string name/*, float scale*/)
		{
			var list = (List<Object>)map[name];
			var values = new float[list.Count];
			//if (scale == 1)
			{
				for (int i = 0, n = list.Count; i < n; i++)
					values[i] = (float)list[i];
			}
// 			else
// 			{
// 				for (int i = 0, n = list.Count; i < n; i++)
// 					values[i] = (float)list[i] * scale;
// 			}
			return values;
		}

		static int[] GetIntArray(Dictionary<string, Object> map, string name)
		{
			var list = (List<Object>)map[name];
			var values = new int[list.Count];
			for (int i = 0, n = list.Count; i < n; i++)
				values[i] = (int)(float)list[i];
			return values;
		}

		static float GetFloat(Dictionary<string, Object> map, string name, float defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			return (float)map[name];
		}

		static int GetInt(Dictionary<string, Object> map, string name, int defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			return (int)(float)map[name];
		}

		static bool GetBoolean(Dictionary<string, Object> map, string name, bool defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			return (bool)map[name];
		}

		static string GetString(Dictionary<string, Object> map, string name, string defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			return (string)map[name];
		}
		static float ToColor(string hexString, int colorIndex, int expectedLength = 8)
		{
			if (hexString.Length != expectedLength)
				throw new ArgumentException("Color hexidecimal length must be " + expectedLength + ", recieved: " + hexString, "hexString");
			return Convert.ToInt32(hexString.Substring(colorIndex * 2, 2), 16) / (float)255;
		}
		#endregion
		static Color GetColor(Dictionary<string, Object> map, string name, Color defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			var color = (string)map["color"];
			return new Color(ToColor(color, 0), ToColor(color, 1), ToColor(color, 2), ToColor(color, 3));
		}
		private List<Single> GetFloatList(Dictionary<string, Object> map, String key)
		{
			float[] fs = GetFloatArray(map, key);
			var ret = new List<Single>();
			for(var i = 0;i < fs.Length; i++)
				ret.Add(fs[i]);
			return ret;
		}
		private List<Int32> GetIntList(Dictionary<string, Object> map, String key)
		{
			Int32[] i32s = GetIntArray(map, key);
			var ret = new List<Int32>();
			for (var i = 0; i < i32s.Length; i++)
				ret.Add(i32s[i]);
			return ret;
		}
		#endregion
		#endregion
	}
	[Serializable]
	public struct SkinDataEx
	{
		[SerializeField]
		public String SkinName;
		[SerializeField]
		public List<SlotDataEx> AllSlots;

	}
	[Serializable]
	public struct SlotDataEx
	{
		[SerializeField]
		public String SlotName;
		[SerializeField]
		public List<AttachmentDataEx> AllAttachments;
	}
	[Serializable]
	public struct AttachmentDataEx
	{
		[SerializeField]
		public String AttachmentName;
		[SerializeField]
		public String PathName;
		[SerializeField]
		public AttachmentType type;
		[SerializeField]
		public List<KeyFloatPair> FloatValues;
		[SerializeField]
		public List<KeyFloatArrayPair> FloatArrayValues;
		[SerializeField]
		public List<KeyBooleanPair> BoolValues;
		[SerializeField]
		public List<KeyIntPair> IntValues;
		[SerializeField]
		public List<KeyIntArrayPair> IntArrayValues;
		[SerializeField]
		public List<KeyStringPair> StringValues;
		[SerializeField]
		public List<KeyColorPair> ColorValues;

		public void Add(KeyFloatPair kv)
		{
			if(FloatValues == null)
			{
				FloatValues = new List<KeyFloatPair>();
			}
			FloatValues.Add(kv);
		}
		public void Add(KeyFloatArrayPair kv)
		{
			if (FloatArrayValues == null)
			{
				FloatArrayValues = new List<KeyFloatArrayPair>();
			}
			FloatArrayValues.Add(kv);
		}
		public void Add(KeyBooleanPair kv)
		{
			if (BoolValues == null)
			{
				BoolValues = new List<KeyBooleanPair>();
			}
			BoolValues.Add(kv);
		}
		public void Add(KeyIntPair kv)
		{
			if (IntValues == null)
			{
				IntValues = new List<KeyIntPair>();
			}
			IntValues.Add(kv);
		}
		public void Add(KeyIntArrayPair kv)
		{
			if (IntArrayValues == null)
			{
				IntArrayValues = new List<KeyIntArrayPair>();
			}
			IntArrayValues.Add(kv);
		}

		public void Add(KeyStringPair kv)
		{
			if (StringValues == null)
			{
				StringValues = new List<KeyStringPair>();
			}
			StringValues.Add(kv);
		}
		public void Add(KeyColorPair kv)
		{
			if (ColorValues == null)
			{
				ColorValues = new List<KeyColorPair>();
			}
			ColorValues.Add(kv);
		}
	}
	#region key value pair的定义
	[Serializable]
	public struct KeyFloatPair
	{
		public KeyFloatPair(String s, Single p)
		{
			Key = s;
			FloatValue = p;
		}
		[SerializeField]
		public String Key;
		[SerializeField]
		public Single FloatValue;
	}
	[Serializable]
	public struct KeyFloatArrayPair
	{
		public KeyFloatArrayPair(String s, List<Single> p)
		{
			Key = s;
			FloatArrayValue = p;
		}
		[SerializeField]
		public String Key;
		[SerializeField]
		public List<Single> FloatArrayValue;
	}
	[Serializable]
	public struct KeyIntPair
	{
		public KeyIntPair(String s, Int32 p)
		{
			Key = s;
			IntValue = p;
		}
		[SerializeField]
		public String Key;
		[SerializeField]
		public Int32 IntValue;
	}
	[Serializable]
	public struct KeyIntArrayPair
	{
		public KeyIntArrayPair(String s, List<Int32> p)
		{
			Key = s;
			IntArrayValue = p;
		}
		[SerializeField]
		public String Key;
		[SerializeField]
		public List<Int32> IntArrayValue;
	}
	[Serializable]
	public struct KeyBooleanPair
	{
		public KeyBooleanPair(String s, Boolean p)
		{
			Key = s;
			BooleanValue = p;
		}
		[SerializeField]
		public String Key;
		[SerializeField]
		public Boolean BooleanValue;
	}
	[Serializable]
	public struct KeyStringPair
	{
		public KeyStringPair(String s, String p)
		{
			Key = s;
			StringValue = p;
		}
		[SerializeField]
		public String Key;
		[SerializeField]
		public String StringValue;
	}
	[Serializable]
	public struct KeyColorPair
	{
		public KeyColorPair(String s, Color p)
		{
			Key = s;
			ColorValue = p;
		}
		[SerializeField]
		public String Key;
		[SerializeField]
		public Color ColorValue;
	}
	#endregion
}
