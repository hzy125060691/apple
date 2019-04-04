using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using Game.Core;
using System;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(GameScrollRect_V2), true)]
[CanEditMultipleObjects]
class GameScrollRect_V2_Editor : ScrollRectEditor
{
	private GUIStyle textSytle = null;//大红色字体，醒目
	private GUIStyle warningTextSytle = null;//大红色字体，醒目
	private SerializedProperty EmptyRoot_SP = null;
	private SerializedProperty RealChildItem_SP = null;
	private SerializedProperty LayoutMode_SP = null;
	private SerializedProperty GridConstraint_SP = null;
	private SerializedProperty ConstraintCount_SP = null;
	private SerializedProperty CellSize_SP = null; 
	private SerializedProperty SpacingSize_SP = null;
	private SerializedProperty ViewSizeMinExt_SP = null;
	private SerializedProperty ViewSizeMaxExt_SP = null;

	private GameScrollRect_V2 SelfObj = null;
	private MonoScript Script= null;
	//private String ScriptPath = null;
	protected override void OnEnable()
	{
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
		EditorGUILayout.LabelField("扩展属性如下：");
		++EditorGUI.indentLevel;
		serializedObject.UpdateIfRequiredOrScript();
		EditorGUILayout.PropertyField(EmptyRoot_SP);

		EditorGUILayout.PropertyField(RealChildItem_SP);
		EditorGUILayout.PropertyField(ViewSizeMinExt_SP);
		EditorGUILayout.PropertyField(ViewSizeMaxExt_SP);
		//EditorGUILayout.PropertyField(ViewSizeExt_SP);
		{
			EditorGUILayout.PropertyField(LayoutMode_SP);
			if(GameScrollRect_V2.LayoutType.Grid== (GameScrollRect_V2.LayoutType)Enum.GetValues(typeof(GameScrollRect_V2.LayoutType)).GetValue(LayoutMode_SP.enumValueIndex))
			{
				EditorGUILayout.PropertyField(GridConstraint_SP);
				EditorGUILayout.PropertyField(ConstraintCount_SP);
				EditorGUILayout.PropertyField(CellSize_SP);
			}
			EditorGUILayout.PropertyField(SpacingSize_SP);
		}

		serializedObject.ApplyModifiedProperties();
		--EditorGUI.indentLevel;

		//SerializedProperty childItem = serializedObject.FindProperty("ChildItem");
		// 尝试刷新serializedObject
		serializedObject.UpdateIfRequiredOrScript();
		++EditorGUI.indentLevel;
		//EditorGUILayout.LabelField("滑动列表子对象：");
		//EditorGUILayout.PropertyField(childItem);
		serializedObject.ApplyModifiedProperties();
		--EditorGUI.indentLevel;
		EditorGUILayout.LabelField("默认属性如下：");
		++EditorGUI.indentLevel;
		base.OnInspectorGUI();
		--EditorGUI.indentLevel;
	}
}