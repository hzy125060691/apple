#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 这是个可以设置AssetBundle相关参数的类
/// 编辑器窗口
/// 这里存放的基本都是和编辑器选项、显示有关的
/// </summary>
public class AssetBundleEditorManager : EditorWindow
{

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

	private static AssetBundleSettingHelper ABSH = null;
	private static ABPInfo ABPHInfo = null;
	private bool bDirty = false;
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

		ABSHInitAndSelect();
		bDirty = false;
	}
	public static void ABSHInitAndSelect()
	{
		if (ABSH == null)
		{
			ABSH = AssetBundleSettingHelperEditor.GetABSH(out ABPHInfo);

		}
		Selection.activeObject = ABSH;
	}

	public static AssetBundleSettingHelper GetABSH()
	{
		if (ABSH == null)
		{
			ABSHInitAndSelect();
		}
		return ABSH;
	}
	void OnDestroy()
	{
		if(bDirty)
		{
			AssetBundleSettingHelperEditor.SaveABSH(ABSH, ABPHInfo);
		}
// 		ABSH = null;
// 		ABPHInfo = null;
		
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
				if (ABSH.UseAssetBundle)
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
					ABSH.UseAssetBundle = !ABSH.UseAssetBundle;
					bDirty = true;
				}
				//AssetBundleHelper.UseAssetBundle = GUILayout.Toggle(AssetBundleHelper.UseAssetBundle, "", ToggleSytle, GUILayout.ExpandWidth(false));
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndArea();
		//**************************************************************************
		GUILayout.BeginArea(new Rect(0, LayoutOneHeight, position.width, position.height));
		GUILayout.Label("Build AssetBundle的时候:", titleSytle);
		GUILayout.Label("本打包脚本会将制定目录下所有的资源单独打包，被引用的资源如果被共享则单独打包，未共享的被一同打包:", titleSytle);

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
				var EditString = (GUILayout.TextField(ABSH.ABOutputPath, TextFieldSytle, GUILayout.Width(700)));
				if(!EditString.Equals(ABSH.ABOutputPath))//BuildBundleManager.AssetBundlePath
				{
					ABSH.ABOutputPath = EditString;
					bDirty = true;
				}
				//防止不小心修改，可以重置为默认值
				if (GUILayout.Button("重置为默认", ResetButtonSytle, GUILayout.Width(100)))
				{
					//BuildBundleManager.AssetBundlePath = AssetBundleHelper.Ins.ABSettingHelper.ABOutputPath;
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
				var EditString = (GUILayout.TextField(ABSH.NeedBuildABPath, TextFieldSytle, GUILayout.Width(700)));//BuildBundleManager.NeedBuildAssetBundlePath
				if (!EditString.Equals(ABSH.NeedBuildABPath))
				{
					ABSH.NeedBuildABPath = EditString;
					bDirty = true;
				}
				//防止不小心修改，可以重置为默认值
				if (GUILayout.Button("重置为默认", ResetButtonSytle, GUILayout.Width(100)))
				{
					//BuildBundleManager.NeedBuildAssetBundlePath = BuildBundleManager.needBuildAssetBundlePath;
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
					var abpi = ABSH.GetABInfoXmlPath(ABSH.RuntimePlatformToSimplifyName(RuntimePlatform.OSXEditor));
					GUILayout.Label(abpi.FullName_RelativePath, TextFieldSytle, GUILayout.Width(700));
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
					var abpi = ABSH.GetABInfoXmlPath(ABSH.RuntimePlatformToSimplifyName(RuntimePlatform.Android));
					GUILayout.Label(abpi.FullName_RelativePath, TextFieldSytle, GUILayout.Width(700));
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
					var abpi = ABSH.GetABInfoXmlPath(ABSH.RuntimePlatformToSimplifyName(RuntimePlatform.WindowsEditor));
					GUILayout.Label(abpi.FullName_RelativePath, TextFieldSytle, GUILayout.Width(700));
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
					var abpi = ABSH.GetDifferXmlPath(ABSH.RuntimePlatformToSimplifyName(RuntimePlatform.OSXEditor));
					GUILayout.Label(abpi.FullName_RelativePath, TextFieldSytle, GUILayout.Width(700));
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
					var abpi = ABSH.GetDifferXmlPath(ABSH.RuntimePlatformToSimplifyName(RuntimePlatform.Android));
					GUILayout.Label(abpi.FullName_RelativePath, TextFieldSytle, GUILayout.Width(700));
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
					var abpi = ABSH.GetDifferXmlPath(ABSH.RuntimePlatformToSimplifyName(RuntimePlatform.WindowsEditor));
					GUILayout.Label(abpi.FullName_RelativePath, TextFieldSytle, GUILayout.Width(700));
					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndArea();
		//*****************************************************************************
	}

	[MenuItem("AssetBundle/1.设置编辑器下AssetBundle的一些信息")]
	public static void AddWindow()
	{
		//创建窗口
		Rect wr = new Rect(0, 0, 1000, 800);
		AssetBundleEditorManager window = (AssetBundleEditorManager)EditorWindow.GetWindowWithRect(typeof(AssetBundleEditorManager), wr, true, "设置编辑器下AssetBundle的一些信息");
		window.Show();

	}

	[MenuItem("AssetBundle/2.Build/BuildAB_Win64")]
	public static void BuildWin64()
	{
		BuildBundleManager.BuildAssetBundle(BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
	}
	[MenuItem("AssetBundle/3.Move/moveAB_win64")]
	public static void MoveTest()
	{
		BuildBundleManager.MoveAssetBundleToStreamingAssets(BuildTarget.StandaloneWindows64);
	}

	[MenuItem("AssetBundle/2.Build/BuildAB_IOS")]
	public static void BuildIOS()
	{
		BuildBundleManager.BuildAssetBundle(BuildAssetBundleOptions.None, BuildTarget.iOS);
	}
	[MenuItem("AssetBundle/3.Move/moveAB_IOS")]
	public static void MoveIOS()
	{
		BuildBundleManager.MoveAssetBundleToStreamingAssets(BuildTarget.iOS);
	}
	[MenuItem("AssetBundle/2.Build/BuildAB_Android")]
	public static void BuildAndroid()
	{
		BuildBundleManager.BuildAssetBundle(BuildAssetBundleOptions.None, BuildTarget.Android);
	}
	[MenuItem("AssetBundle/3.Move/moveAB_Android")]
	public static void MoveAndroid()
	{
		BuildBundleManager.MoveAssetBundleToStreamingAssets(BuildTarget.Android);
	}
	[MenuItem("AssetBundle/4.BuildAB_CurrentBuildSettingPlatform")]
	public static void Build()
	{
		BuildBundleManager.BuildAssetBundle(BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
	}
	[MenuItem("AssetBundle/5.moveAB_CurrentBuildSettingPlatform")]
	public static void Move()
	{
		BuildBundleManager.MoveAssetBundleToStreamingAssets(EditorUserBuildSettings.activeBuildTarget);
	}

	[MenuItem("AssetBundle/6.ABUseAnalysis(AssetBundleEditorManager.cs)")]
	public static void InputAllXml()
	{
		AssetBundleUseAnalysisEditor.InputAllXml();
	}

	[MenuItem("AssetBundle/7.Move_CurrentPFResultXml(AssetBundleEditorManager.cs)")]
	public static void MoveOutputXml()
	{
		AssetBundleUseAnalysisEditor.MovePFResultXmlToStreamingAssets(EditorUserBuildSettings.activeBuildTarget);
	}

	[MenuItem("AssetBundle/8.Open_CurPlatformResFolder(AssetBundleEditorManager.cs)")]
	public static void OpenCurPDFolder()
	{
		var absh = GetABSH();
		var manifest = absh.GetCurPlatformManifestPath();
		if(Directory.Exists(manifest.Dir_Full))
		{
			EditorUtility.RevealInFinder(manifest.FullName);
			return;
		}

		//如果这个目录不存在，那么只能打开上层目录，如果还不存在，打个错误提示窗口吧
		string lastFolder = manifest.Dir_Full.Substring(0, manifest.Dir_Full.LastIndexOf(AssetBundleSettingHelper.GetSlash()));
		string lastName = Path.Combine(lastFolder, "sdfsdfsd");
		if (Directory.Exists(lastFolder))
		{
			EditorUtility.RevealInFinder(lastFolder);
			return;
		}
		Debug.LogError("dddddddddddddddddddddd");
	}
}
#endif