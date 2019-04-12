using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using Game.Core;
using System;
using System.Linq;
using System.Reflection;
using UnityEditorInternal;
using System.Collections.Generic;

[CustomEditor(typeof(GameScrollRect_V2), true)]
[CanEditMultipleObjects]
class GameScrollRect_V2_Editor : ScrollRectEditor
{
	private GUIStyle textSytle = null;//大红色字体，醒目
	private GUIStyle warningTextSytle = null;//大红色字体，醒目
	private SerializedProperty EmptyRoot_SP = null;
	private SerializedProperty RealChildItem_SP = null;
	private SerializedProperty FirstEdge_SP = null;
	private SerializedProperty SecondEdge_SP = null;
	private SerializedProperty LayoutMode_SP = null;
	private SerializedProperty GridConstraint_SP = null;
	private SerializedProperty ConstraintCount_SP = null;
	private SerializedProperty CellSize_SP = null; 
	private SerializedProperty SpacingSize_SP = null;
	private SerializedProperty ViewSizeMinExt_SP = null;
	private SerializedProperty ViewSizeMaxExt_SP = null;

	private GameScrollRect_V2 SelfObj = null;
	private MonoScript Script= null;

	private Boolean ShowEditorProperty = false;
	//private ReorderableList EditorTestGOs;
	//private String ScriptPath = null;
	protected override void OnEnable()
	{
// 		var tarGSR = serializedObject.targetObject as GameScrollRect_V2;
// 		var t = typeof(GameScrollRect_V2);
// 		var tmp = t.GetField("DataAndPosProviders", BindingFlags.NonPublic | BindingFlags.Instance);
// 		var list = tmp.GetValue(tarGSR) as System.Collections.IList;
// 		EditorTestGOs = new ReorderableList(list, typeof(System.Object));
// 		EditorTestGOs.onAddCallback = (l) => { Debug.LogError("add"); list.Add(new GameScrollRect_V2.DataPos()); };
// 		EditorTestGOs.onRemoveCallback = (l) => { Debug.LogError("remove"); list.RemoveAt(list.Count -1); };


		if (textSytle == null)
		{
			//大红色字体，醒目
			textSytle = new GUIStyle(EditorStyles.label);
			textSytle.normal.textColor = Color.red;
			textSytle.fontSize = 15;
		}
		if(warningTextSytle == null)
		{
			warningTextSytle = new GUIStyle(EditorStyles.label);
			warningTextSytle.normal.textColor = Color.yellow;
			warningTextSytle.fontSize = 15;
		}
		if(Script == null)
		{
			//脚本对应的同名文件，必须同名……否则找不到
			var ss = AssetDatabase.FindAssets("t:MonoScript").Where(e=> AssetDatabase.GUIDToAssetPath(e).EndsWith(typeof(GameScrollRect_V2).Name + ".cs")).First();
			var path = AssetDatabase.GUIDToAssetPath(ss);
			Script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
		}
		SelfObj = target as GameScrollRect_V2;
		EmptyRoot_SP = serializedObject.FindProperty("EmptyRoot");
		RealChildItem_SP = serializedObject.FindProperty("ChildItem");
		FirstEdge_SP = serializedObject.FindProperty("FirstEdge");
		SecondEdge_SP = serializedObject.FindProperty("SecondEdge");
		LayoutMode_SP = serializedObject.FindProperty("LayoutMode");
		GridConstraint_SP = serializedObject.FindProperty("GridConstraint");
		ConstraintCount_SP = serializedObject.FindProperty("ConstraintCount");
		CellSize_SP = serializedObject.FindProperty("CellSize");
		SpacingSize_SP = serializedObject.FindProperty("SpacingSize");
		ViewSizeMinExt_SP = serializedObject.FindProperty("ViewSizeMinExt");
		ViewSizeMaxExt_SP = serializedObject.FindProperty("ViewSizeMaxExt");
		base.OnEnable();
	}
	#region 右键菜单，创建GameScrollRect_V2的操作
	[MenuItem("GameObject/游戏UI/GameScrollRect_V2", false, 0)]
	public static void Create_GameScrollRect_V2(MenuCommand menuCommand)
	{
		GameObject obj = new GameObject("ScrollView");
		GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
		Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
		Selection.activeObject = obj;
		obj.AddComponent<RectTransform>().sizeDelta = new Vector2(200.0f, 200.0f);

		GameObject viewPort = new GameObject("Viewport");
		GameObjectUtility.SetParentAndAlign(viewPort, obj);
		Undo.RegisterCreatedObjectUndo(viewPort, "Create " + viewPort.name);
		Selection.activeObject = viewPort;
		RectTransform viewrect = viewPort.AddComponent<RectTransform>();
		viewrect.sizeDelta = new Vector2(17.0f, 17.0f);
		viewPort.AddComponent<RectMask2D>();

		GameObject empty = new GameObject("EmptyRecycle");
		GameObjectUtility.SetParentAndAlign(empty, viewPort);
		Undo.RegisterCreatedObjectUndo(empty, "Create " + empty.name);
		Selection.activeObject = empty;
		RectTransform emptyrect = empty.AddComponent<RectTransform>();
		emptyrect.sizeDelta = new Vector2(0f, 0f);

		GameObject Content = new GameObject("Content");
		GameObjectUtility.SetParentAndAlign(Content, viewPort);
		Undo.RegisterCreatedObjectUndo(Content, "Create " + Content.name);
		Selection.activeObject = Content;
		RectTransform contentrect = Content.AddComponent<RectTransform>();
		//GridLayoutGroup contentgrid= Content.AddComponent<GridLayoutGroup>();
		contentrect.sizeDelta = new Vector2(0f, 300.0f);

		GameScrollRect_V2 scrollRect = obj.AddComponent<GameScrollRect_V2>();
		scrollRect.content = contentrect;
		scrollRect.viewport = viewrect;
		scrollRect.EmptyRoot = emptyrect;

	}
	#endregion
	public enum GSREditor_Edge_V
	{
		Top = 0,
		Bottom = 1
	}
	public enum GSREditor_Edge_H
	{
		Left = 0,
		Right = 1,
	}
	public override void OnInspectorGUI()
	{
		if (Script != null)
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.ObjectField("Script", Script, typeof(MonoScript), false);
			EditorGUI.EndDisabledGroup();
		}
		if (textSytle != null)
		{
			if(SelfObj.content != null && SelfObj.viewport != null && SelfObj.content.parent != SelfObj.viewport)
			{
				var str = "Content不是Viewport的子节点\n现在不支持这种用法";
				var content = new GUIContent(str);
				var height = textSytle.CalcHeight(content, 20);
				EditorGUILayout.LabelField(content, textSytle, GUILayout.Height(height));
			}
		}
		++EditorGUI.indentLevel;
		if (!ShowEditorProperty)
		{
			ShowEditorProperty = GUILayout.Button( "打开Editor测试选项" );
		}
		else
		{
			ShowEditorProperty = !GUILayout.Button("关闭Editor测试选项");
			EditorGUILayout.LabelField("随便显示点东西");
			//EditorTestGOs.DoLayoutList();
		}
		--EditorGUI.indentLevel;
		EditorGUILayout.LabelField("扩展属性如下：");
		++EditorGUI.indentLevel;
		serializedObject.UpdateIfRequiredOrScript();
		EditorGUILayout.PropertyField(EmptyRoot_SP);

		EditorGUILayout.PropertyField(RealChildItem_SP);
		EditorGUILayout.PropertyField(ViewSizeMinExt_SP);
		{
			ViewSizeMinExt_SP.vector2Value = new Vector2(Mathf.Max(ViewSizeMinExt_SP.vector2Value.x, 0), Mathf.Max(ViewSizeMinExt_SP.vector2Value.y, 0));
		}
		EditorGUILayout.PropertyField(ViewSizeMaxExt_SP);
		{
			ViewSizeMaxExt_SP.vector2Value = new Vector2(Mathf.Max(ViewSizeMaxExt_SP.vector2Value.x, 0), Mathf.Max(ViewSizeMaxExt_SP.vector2Value.y, 0));
		}
		//EditorGUILayout.PropertyField(ViewSizeExt_SP);
		{
			EditorGUILayout.PropertyField(LayoutMode_SP);
			var layoutMode = (GameScrollRect_V2.LayoutType)Enum.GetValues(typeof(GameScrollRect_V2.LayoutType)).GetValue(LayoutMode_SP.enumValueIndex);
			if (GameScrollRect_V2.LayoutType.Grid == layoutMode)
			{
				EditorGUILayout.PropertyField(GridConstraint_SP);
				if(GridLayoutGroup.Constraint.Flexible != (GridLayoutGroup.Constraint)Enum.GetValues(typeof(GridLayoutGroup.Constraint)).GetValue(GridConstraint_SP.enumValueIndex))
				{
					EditorGUILayout.PropertyField(ConstraintCount_SP);
				}
				EditorGUILayout.PropertyField(CellSize_SP);
				#region 排序方向
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PropertyField(FirstEdge_SP, new GUIContent("排序方向"));
					var edges = Enum.GetValues(typeof(RectTransform.Edge));
					var firstEdge = (RectTransform.Edge)edges.GetValue(FirstEdge_SP.enumValueIndex);
					var secondEdge = (RectTransform.Edge)edges.GetValue(SecondEdge_SP.enumValueIndex);
					if (firstEdge == RectTransform.Edge.Left || firstEdge == RectTransform.Edge.Right)
					{
						if (secondEdge == RectTransform.Edge.Left || secondEdge == RectTransform.Edge.Right)
						{
							for (var i = 0; i < edges.Length; i++)
							{
								if ((RectTransform.Edge)edges.GetValue(i) == RectTransform.Edge.Top && SecondEdge_SP.enumValueIndex != i)
								{
									SecondEdge_SP.enumValueIndex = i;
									break;
								}
							}
						}
						var eV = secondEdge == RectTransform.Edge.Bottom ? GSREditor_Edge_V.Bottom : GSREditor_Edge_V.Top;
						eV = (GSREditor_Edge_V)EditorGUILayout.EnumPopup(eV);
						var tar = eV == GSREditor_Edge_V.Top ? RectTransform.Edge.Top : RectTransform.Edge.Bottom;
						for (var i = 0; i < edges.Length; i++)
						{
							if ((RectTransform.Edge)edges.GetValue(i) == tar && SecondEdge_SP.enumValueIndex != i)
							{
								SecondEdge_SP.enumValueIndex = i;
								break;
							}
						}
					}
					else
					{
						if (secondEdge == RectTransform.Edge.Top || secondEdge == RectTransform.Edge.Bottom)
						{
							for (var i = 0; i < edges.Length; i++)
							{
								if ((RectTransform.Edge)edges.GetValue(i) == RectTransform.Edge.Left && SecondEdge_SP.enumValueIndex != i)
								{
									SecondEdge_SP.enumValueIndex = i;
									break;
								}
							}
						}
						var eH = secondEdge == RectTransform.Edge.Left ? GSREditor_Edge_H.Left : GSREditor_Edge_H.Right;
						eH = (GSREditor_Edge_H)EditorGUILayout.EnumPopup(eH);
						var tar = eH == GSREditor_Edge_H.Left ? RectTransform.Edge.Left : RectTransform.Edge.Right;
						for (var i = 0; i < edges.Length; i++)
						{
							if ((RectTransform.Edge)edges.GetValue(i) == tar && SecondEdge_SP.enumValueIndex != i)
							{
								SecondEdge_SP.enumValueIndex = i;
								break;
							}
						}
					}
					EditorGUILayout.EndHorizontal();
				}
				#endregion
			}
			else if(layoutMode == GameScrollRect_V2.LayoutType.Vertical)
			{
				#region 排序方向
				var edges = Enum.GetValues(typeof(RectTransform.Edge));
				var firstEdge = (RectTransform.Edge)edges.GetValue(FirstEdge_SP.enumValueIndex);
				if(firstEdge != RectTransform.Edge.Top && firstEdge != RectTransform.Edge.Bottom)
				{
					for (var i = 0; i < edges.Length; i++)
					{
						if ((RectTransform.Edge)edges.GetValue(i) == RectTransform.Edge.Top && FirstEdge_SP.enumValueIndex != i)
						{
							FirstEdge_SP.enumValueIndex = i;
							break;
						}
					}
				}
				var eV = firstEdge == RectTransform.Edge.Bottom ? GSREditor_Edge_V.Bottom : GSREditor_Edge_V.Top;
				eV = (GSREditor_Edge_V)EditorGUILayout.EnumPopup(new GUIContent("排序方向"), eV);
				var tar = eV == GSREditor_Edge_V.Top ? RectTransform.Edge.Top : RectTransform.Edge.Bottom;
				for (var i = 0; i < edges.Length; i++)
				{
					if ((RectTransform.Edge)edges.GetValue(i) == tar && FirstEdge_SP.enumValueIndex != i)
					{
						FirstEdge_SP.enumValueIndex = i;
						break;
					}
				}
				#endregion
			}
			else if (layoutMode == GameScrollRect_V2.LayoutType.Horizontal)
			{
				#region 排序方向
				var edges = Enum.GetValues(typeof(RectTransform.Edge));
				var firstEdge = (RectTransform.Edge)edges.GetValue(FirstEdge_SP.enumValueIndex);
				if (firstEdge != RectTransform.Edge.Left && firstEdge != RectTransform.Edge.Right)
				{
					for (var i = 0; i < edges.Length; i++)
					{
						if ((RectTransform.Edge)edges.GetValue(i) == RectTransform.Edge.Right && FirstEdge_SP.enumValueIndex != i)
						{
							FirstEdge_SP.enumValueIndex = i;
							break;
						}
					}
				}
				var eH = firstEdge == RectTransform.Edge.Right ? GSREditor_Edge_H.Right : GSREditor_Edge_H.Left;
				eH = (GSREditor_Edge_H)EditorGUILayout.EnumPopup(new GUIContent("排序方向"), eH);
				var tar = eH == GSREditor_Edge_H.Right ? RectTransform.Edge.Right : RectTransform.Edge.Left;
				for (var i = 0; i < edges.Length; i++)
				{
					if ((RectTransform.Edge)edges.GetValue(i) == tar && FirstEdge_SP.enumValueIndex != i)
					{
						FirstEdge_SP.enumValueIndex = i;
						break;
					}
				}
				#endregion
			}
			EditorGUILayout.PropertyField(SpacingSize_SP);
		}
		if (serializedObject.ApplyModifiedProperties())
		{
			(serializedObject.targetObject as GameScrollRect_V2).ForceRefresh();
		}
		--EditorGUI.indentLevel;

		//SerializedProperty childItem = serializedObject.FindProperty("ChildItem");
		// 尝试刷新serializedObject
		serializedObject.UpdateIfRequiredOrScript();
		++EditorGUI.indentLevel;
		//EditorGUILayout.LabelField("滑动列表子对象：");
		//EditorGUILayout.PropertyField(childItem);
		serializedObject.ApplyModifiedProperties();
		--EditorGUI.indentLevel;
		EditorGUILayout.LabelField("父类属性如下：");
		++EditorGUI.indentLevel;
		base.OnInspectorGUI();
		--EditorGUI.indentLevel;
	}
}