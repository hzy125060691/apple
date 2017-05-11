using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System;
using System.Linq;
public class AssetBundleUpdater
{
	public string UrlPrefix = "http://10.12.21.216/AssetBundles";

	private UpdaterStageInfo StageInfo = new UpdaterStageInfo();

	private const int OtherResCount = 2;//manifest和version.xml这两个文件
	public UpdaterStage Stage()
	{
		return StageInfo.Stage;
	}
	public void GetStageParam(out int nuc, out int uc)
	{
		nuc = StageInfo.NeedUpdateCount;
		uc = StageInfo.UpdatedCount;
	}
	public bool IsStage(UpdaterStage stage)
	{
		return StageInfo.Stage == stage;
	}
	class UpdaterStageInfo
	{
		public UpdaterStage Stage = UpdaterStage.None;
		public int NeedUpdateCount = -1;
		public int UpdatedCount = -1;

		public bool IsStage(UpdaterStage stage)
		{
			return Stage == stage;
		}
		public void SetNUC(int nuc)
		{
			NeedUpdateCount = nuc + OtherResCount;
			UpdatedCount = 0;
		}
		public void AddUC()
		{
			UpdatedCount++;
			System.Threading.Thread.Sleep(1000);
		}
		private void ClearCount()
		{
			NeedUpdateCount = -1;
			UpdatedCount = -1;
		}

		public void NextStage()
		{
			Stage++;
			if(Stage == UpdaterStage.StageCount)
			{
				Stage = UpdaterStage.None;
			}
			ClearCount();
		}
		public void ToStage(UpdaterStage stage)
		{
			Stage = stage;
			ClearCount();
		}
	}

	public enum UpdaterStage
	{
		None = 0,
		NotNeedUpdater,
		CheckCopySAToPD,
		CopySAToPD,
		Copy_Over,
		DownloadRes,
		DownloadRes_Error,
		ResIsUpToDate,
		Download_Over,
		EnableUseRes,
		StageCount,
	}

	public IEnumerator CheckAndUpdateABToPD(AssetBundleSettingHelper ABSettingHelper, AssetBundleHelper abh, Action callback = null)
	{
		StageInfo.ToStage(UpdaterStage.CheckCopySAToPD);
		{
			//先大致写下思路，android下，streamingAssets目录是在jar包里的，所以不能直接操作文件和路径，得www或者assetbundle.loadfromfile去读取，但是其他地方可以直接读，所以为了照顾android，SA目录下都改用www和assetbundle.loadfromfile
			//但是有些地方需要搞复制这种事，所以WWW后可以直接写入到新文件中。所以可以发现很多地方用了www


			//如果使用AB，检查一下自己的私有目录下是否有assetbundle的拷贝，如果没有则把所有的AB拷过去
			//bool bCopySAToPDFinished = false;
			if (!ABSettingHelper.UseAssetBundle || ABSettingHelper.UseSteamingAssetRes)
			{
				StageInfo.ToStage(UpdaterStage.NotNeedUpdater);
				if (callback != null)
				{
					callback.Invoke();
				}
				yield break;
			}
			var abpiSrc = ABSettingHelper.GetCurABVersionXmlStreamingAssetPath();
			var abpiDst = ABSettingHelper.GetCurABVersionXmlPersistentDataPath();
			//首先检查目标文件是否存在
			if (!File.Exists(abpiDst.FullName))
			{
				//不存在，这个时候无脑删除文件夹，然后拷贝，防止文件夹里残留了奇怪的东西
				abh.StartCoroutine(CopyAllABToPD(ABSettingHelper,abh, () => StageInfo.ToStage(UpdaterStage.Copy_Over)));
				
				AssetBundleHelper.TestLog("!File.Exists(abpiDst.FullName):" + abpiDst.FullName);
			}
			else
			{
				//虽然存在，但是对比一下版本号，如果不如SA目录下的新，也是要覆盖过去的
				VersionXmlInfo verSrc = null;
				//AssetBundleHelper.TestLog("abpiSrc.URI:" + abpiSrc.URI);
				using (WWW www = new WWW(abpiSrc.URI))
				{
					bool bFind = false;
					while (true)
					{
						if (www.error != null)
						{
							Debug.LogError(www.error);
							AssetBundleHelper.TestLog(www.error);
							bFind = true;
							break;
						}
						else if (www.isDone)
						{
							break;
						}
						yield return null;
					}
					if (!bFind)
					{
						verSrc = GetVersion(www.bytes);
					}
				}
				if (verSrc != null)
				{
					//AssetBundleHelper.TestLog("abpiDst.URI:" + abpiDst.URI);
					VersionXmlInfo verDst = null;

					verDst = GetVersion(abpiDst.FullName);

					//AssetBundleHelper.TestLog("verDst.IsUpTodate(verSrc)");
					if (verDst.IsUpTodate(verSrc))
					{
						//目标版本比较新，所以什么都不用干
						//AssetBundleHelper.TestLog("Is Up To Date");
						StageInfo.ToStage(UpdaterStage.Copy_Over);
					}
					else
					{
						//无脑拷过去覆盖
						//CopyAllABToPD(ABSettingHelper);

						//AssetBundleHelper.TestLog("Copy all:");
						abh.StartCoroutine(CopyAllABToPD(ABSettingHelper, abh, () => StageInfo.ToStage(UpdaterStage.Copy_Over)));
					}
				}
				else
				{
					StageInfo.ToStage(UpdaterStage.Copy_Over);
				}
			}
			//AssetBundleHelper.TestLog("Wait bCopySAToPDFinished");
			while (!StageInfo.IsStage(UpdaterStage.Copy_Over))
			{
				yield return null;
			}
		}


		StageInfo.ToStage(UpdaterStage.DownloadRes);
		//然后检查网络上是否有更新的版本
		//尝试从网络上获得version的xml
		string VersionABPrefixUrl = Path.Combine(UrlPrefix, ABSettingHelper.GetPlatformPathName());
		string manifestUrl = Path.Combine(VersionABPrefixUrl, ABSettingHelper.GetPlatformPathName());
		var VersionUrl = Path.Combine(VersionABPrefixUrl, VersionXmlInfo.FileName + AssetBundleSettingHelper.xmlExtName);
		//AssetBundleHelper.TestLog(@"Begin WWW VersionUrl:"+ VersionUrl);
		using (WWW www = new WWW(VersionUrl))
		{
			while (true)
			{
				yield return null;
				//AssetBundleHelper.TestLog(@"Begin www Time:" + DateTime.Now.ToString());
				if (www.error != null)
				{
					//Debug.Assert(false, "www.error != null " + www.error);
					AssetBundleHelper.TestLog(@"www.error != null " + www.error);
					StageInfo.ToStage(UpdaterStage.DownloadRes_Error);
					if (callback != null)
					{
						callback.Invoke();
					}

					yield break;
				}
				else if (www.isDone)
				{
					break;
				}
			}
//			AssetBundleHelper.TestLog(@"Begin etVersion(www.bytes):");
			var versionOnline = GetVersion(www.bytes);
/*			AssetBundleHelper.TestLog(@"Begin e11111111111:"+ versionOnline.GetVersionString());*/
			var abpiDst = ABSettingHelper.GetCurABVersionXmlPersistentDataPath();
// 			AssetBundleHelper.TestLog(@"Begin 222222222222222222:"+ abpiDst.FullName);
// 			AssetBundleHelper.TestLog(@"Begin 333333333333:" + Application.persistentDataPath);
			VersionXmlInfo verDst = null;
			try
			{
				verDst = GetVersion(abpiDst.FullName);
			}
 			catch (Exception e)
			{
				AssetBundleHelper.TestLog(e.Message);
			}
			AssetBundleHelper.TestLog(@"Begin abpiDst.FullName:" + abpiDst.FullName);
			if (verDst != null && verDst.IsUpTodate(versionOnline))
			{
				AssetBundleHelper.TestLog(@"Up To Date Online:" + abpiDst.FullName);
				StageInfo.ToStage(UpdaterStage.ResIsUpToDate);
				//本地的是最新的，不需要更新
				if (callback != null)
				{
					callback.Invoke();
				}
				yield break;
			}

			//Debug.Log("需要更新");
			//先下载manifest，然后下载所有需要更新的AB
			AssetBundleHelper.TestLog(@"Begin WWW manifestUrl:" + manifestUrl);
			using (WWW wwwMF = new WWW(manifestUrl))
			{
				while (true)
				{
					yield return null;
					if (wwwMF.error != null)
					{
						AssetBundleHelper.TestLog(@"wwwMF.error != null " + wwwMF.error);
						StageInfo.ToStage(UpdaterStage.DownloadRes_Error);
						if (callback != null)
						{
							callback.Invoke();
						}
						yield break; ;
					}
					else if (wwwMF.isDone)
					{
						break;
					}
				}

				var newManifest = GetPathManifest(wwwMF.bytes);
				var localManifestABPI = ABSettingHelper.GetCurPlatformManifestPath();
				var localManifest = GetPathManifest(localManifestABPI.FullName);
				List<string> updateList = null;
				List<string> deleteList = null;
				GetNeedUpdateList(localManifest, newManifest, out updateList, out deleteList);
				int needUpdateCount = 0;
				if(updateList != null)
				{
					needUpdateCount += updateList.Count();
				}
				if (deleteList != null)
				{
					needUpdateCount += deleteList.Count();
				}
				{
					StageInfo.SetNUC(needUpdateCount);
					DeleteFile(deleteList, ABSettingHelper);

					if (updateList != null && updateList.Count > 0)
					{
						foreach (var f in updateList)
						{
							string dlUrl = Path.Combine(VersionABPrefixUrl, f);
							using (WWW wwwUPDATE = new WWW(dlUrl))
							{
								while (true)
								{
									yield return null;
									if (wwwUPDATE.error != null)
									{
										AssetBundleHelper.TestLog(@"wwwUPDATE.error != null " + wwwUPDATE.error);
										StageInfo.ToStage(UpdaterStage.DownloadRes_Error);
										if (callback != null)
										{
											callback.Invoke();
										}
										yield break; ;
									}
									else if (wwwUPDATE.isDone)
									{
										break;
									}
								}
								var abpi = ABSettingHelper.GetCurPlatformABPath(f);
								UpdateFile(abpi, wwwUPDATE.bytes, ABSettingHelper);

								if (wwwUPDATE.assetBundle != null)
								{
									wwwUPDATE.assetBundle.Unload(false);
								}
							}
							StageInfo.AddUC();
							yield return null;
						}
					}
					
				}
				//保存manifest
				UpdateFile(localManifestABPI, wwwMF.bytes, ABSettingHelper);
				if (wwwMF.assetBundle != null)
				{
					wwwMF.assetBundle.Unload(false);
				}
				StageInfo.AddUC();
				yield return null;
			}
			//最后需要保存version。xml
			UpdateFile(abpiDst, www.bytes, ABSettingHelper);
			if (www.assetBundle != null)
			{
				www.assetBundle.Unload(false);
			}

			StageInfo.AddUC();
			yield return null;
		}

		StageInfo.ToStage(UpdaterStage.Download_Over);
		if (callback != null)
		{
			callback.Invoke();
		}

	}
	private void UpdateFile(ABPInfo abpi, byte[] bytes, AssetBundleSettingHelper ABSettingHelper)
	{
		if(!Directory.Exists(abpi.Dir_Full))
		{
			Directory.CreateDirectory(abpi.Dir_Full);
		}
		if (File.Exists(abpi.FullName))
		{
			File.Delete(abpi.FullName);
		}
		FileStream fsDes = File.Create(abpi.FullName);
		fsDes.Write(bytes, 0, bytes.Length);
		fsDes.Flush();
		fsDes.Close();
	}
	private void DeleteFile(List<string> deleteList, AssetBundleSettingHelper ABSettingHelper)
	{
		if(deleteList == null || deleteList.Count == 0)
		{
			return;
		}
		foreach(var f in deleteList)
		{
			var abpi = ABSettingHelper.GetCurPlatformABPath(f);
			File.Delete(abpi.FullName);
			StageInfo.AddUC();
		}
	}

	private void GetNeedUpdateList(AssetBundleManifest local, AssetBundleManifest online, out List<string> updateList, out List<string> deleteList)
	{
		updateList = null;
		deleteList = null;
		if (local == null && online != null)
		{
			//本地什么资源都没有的时候直接从网络下载全资源
			updateList = online.GetAllAssetBundles().ToList();
		}
		else if (online == null)
		{
			throw new NullReferenceException("online == null");
		}
		else if(local != null && online != null)
		{
			var localList = local.GetAllAssetBundles().ToDictionary(s => s);
			var onlineList = online.GetAllAssetBundles();
			updateList = new List<string>();
			deleteList = new List<string>();
			foreach (var o in onlineList)
			{
				var oHash = online.GetAssetBundleHash(o);
				var lHash = local.GetAssetBundleHash(o);
				if (!oHash.Equals(lHash))
				{
					updateList.Add(o);
				}
				if (localList.ContainsKey(o))
				{
					localList.Remove(o);
				}
			}
			foreach (var l in localList)
			{
				deleteList.Add(l.Key);
			}
		}
	}
	#region 初始拷贝所有文件到 私有目录
	private VersionXmlInfo GetVersion(string path)
	{

		try
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(path);
			var xmlInfo = new VersionXmlInfo();
			xmlInfo.InputXmlDoc(doc);
			return xmlInfo;
		}
		catch (Exception e)
		{
			AssetBundleHelper.TestLog("GetVersion2:" + e);
		}
		return null;
	}
	private VersionXmlInfo GetVersion(byte[] bytes)
	{
		try
		{
			XmlDocument doc = new XmlDocument();
			Stream steam = new MemoryStream(bytes);
			doc.Load(steam);
			steam.Close();
			var xmlInfo = new VersionXmlInfo();
			xmlInfo.InputXmlDoc(doc);
			return xmlInfo;
		}
		catch (Exception e)
		{
			AssetBundleHelper.TestLog("GetVersion:" + e);
		}
		return null;
	}
	private AssetBundleManifest GetPathManifest(string path)
	{
		var ab = AssetBundle.LoadFromFile(path);
		AssetBundleManifest mf = null;
		if (ab != null)
		{
			mf = (AssetBundleManifest)ab.LoadAsset("AssetBundleManifest");
			ab.Unload(false);
		}
		return mf;
	}
	private AssetBundleManifest GetPathManifest(byte[] bytes)
	{
		var ab = AssetBundle.LoadFromMemory(bytes);
		AssetBundleManifest mf = null;
		if (ab != null)
		{
			mf = (AssetBundleManifest)ab.LoadAsset("AssetBundleManifest");
			ab.Unload(false);
		}
		return mf;
	}
	/// <summary>
	/// 这是无脑拷贝StreamingAssets资源到PD目录
	/// </summary>
	private IEnumerator CopyAllABToPD(AssetBundleSettingHelper ABSettingHelper, AssetBundleHelper abh, Action callback = null)
	{
		StageInfo.ToStage(UpdaterStage.CopySAToPD);
		//这里是无脑拷贝版本，为了防止资源出错，可以重新搞一次
		var abpiSrc = ABSettingHelper.GetCurABVersionXmlStreamingAssetPath();
		var abpiDst = ABSettingHelper.GetCurABVersionXmlPersistentDataPath();

		//不存在，这个时候无脑删除文件夹，然后拷贝，防止文件夹里残留了奇怪的东西
		if (Directory.Exists(abpiDst.Dir_Full))
		{
			Directory.Delete(abpiDst.Dir_Full, true);
		}
// 		AssetBundleHelper.TestLog("abpiSrc.Dir_Full:" + abpiSrc.Dir_Full);
// 		AssetBundleHelper.TestLog("abpiSrc.FullName:" + abpiSrc.FullName);
// 		AssetBundleHelper.TestLog("Application.dataPath:" + Application.dataPath);
// 		AssetBundleHelper.TestLog("Application.streamingAssetsPath:" + Application.streamingAssetsPath);
// 		AssetBundleHelper.TestLog("Application.persistentDataPath:" + Application.persistentDataPath);

		var localSAManifestABPI = ABSettingHelper.GetCurPlatformStreamingAssetManifestPath();
		var localSAManifest = GetPathManifest(localSAManifestABPI.FullName);
		AssetBundleHelper.TestLog("ia am here *****"+ localSAManifestABPI.FullName);
		if (localSAManifest != null)
		{
			//AssetBundleHelper.TestLog("if(localManifest != null) )))))))))))))))))))))))))))))))))))))))))))");
			var ABs = localSAManifest.GetAllAssetBundles();
			StageInfo.SetNUC(ABs.Length);
			foreach (var f in ABs)
			{
				var abpi = ABSettingHelper.GetCurPlatformStreamingABPath(f);
				var abpiTar = ABSettingHelper.GetCurPlatformABPath(f);
				using (WWW www = new WWW(abpi.URI))
				{
					while (true)
					{
						if (www.error != null)
						{
							Debug.LogError(www.error);
							AssetBundleHelper.TestLog(www.error);
							throw new NullReferenceException(www.url);
						}
						else if (www.isDone)
						{
							break;
						}
						yield return null;
					}
					CreateFile(abpiTar, www.bytes);
					if (www.assetBundle != null)
					{
						www.assetBundle.Unload(false);
					}
				}
				StageInfo.AddUC();
				yield return null;
			}


			//最后把manifeat和version也拷过去
			//manifest
			using (WWW www = new WWW(localSAManifestABPI.URI))
			{
				while (true)
				{
					if (www.error != null)
					{
						Debug.LogError(www.error);
						AssetBundleHelper.TestLog(www.error);
						throw new NullReferenceException(www.url);
					}
					else if (www.isDone)
					{
						break;
					}
					yield return null;
				}
				var localManifestABPI = ABSettingHelper.GetCurPlatformManifestPath();
				CreateFile(localManifestABPI, www.bytes);
				if (www.assetBundle != null)
				{
					www.assetBundle.Unload(false);
				}
				StageInfo.AddUC();
				yield return null;
			}
			//version XML
			using (WWW www = new WWW(abpiSrc.URI))
			{
				while (true)
				{
					if (www.error != null)
					{
						Debug.LogError(www.error);
						AssetBundleHelper.TestLog(www.error);
						throw new NullReferenceException(www.url);
					}
					else if (www.isDone)
					{
						break;
					}
					yield return null;
				}
				//var localManifestABPI = ABSettingHelper.GetCurPlatformManifestPath();
				CreateFile(abpiDst, www.bytes);

				StageInfo.AddUC();
				yield return null;
			}
		}
		else
		{
			//SA目录里没有manifest的时候就什么都别干了
		}
		//CopyFolder(abpiSrc.Dir_Full, abpiDst.Dir_Full);
		if (callback != null)
		{
			callback.Invoke();
		}
	}
	private static void CreateFile(ABPInfo abpi, byte[] bytes)
	{
		if(!Directory.Exists(abpi.Dir_Full))
		{
			Directory.CreateDirectory(abpi.Dir_Full);
		}
		if (File.Exists(abpi.FullName))
		{
			AssetBundleHelper.TestLog("File.Delete(name);"+ abpi.FullName);
			File.Delete(abpi.FullName);
		}
		try
		{
			FileStream fsDes = File.Create(abpi.FullName);
			fsDes.Write(bytes, 0, bytes.Length);
			fsDes.Flush();
			fsDes.Close();
			AssetBundleHelper.TestLog("File.CreateFile(name);" + abpi.FullName);
		}
		catch(Exception e)
		{
			AssetBundleHelper.TestLog("CreateFile:" + e);
		}
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
			AssetBundleHelper.TestLog("!Directory.Exists(srcPath):" + srcPath);
			return;
		}
		if (!Directory.Exists(tarPath))
		{
			AssetBundleHelper.TestLog("Directory.CreateDirectory(tarPath)" + tarPath);
			Directory.CreateDirectory(tarPath);
		}
		string[] directionName = Directory.GetDirectories(srcPath);
		foreach (string dirPath in directionName)
		{
			string directionPathTemp = AssetBundleSettingHelper.Combine(tarPath, dirPath.Substring(srcPath.Length + 1));
			CopyFolder(dirPath, directionPathTemp);
		}
		//让最外层在最后拷贝
		CopyFile(srcPath, tarPath);
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
		foreach (string f in filesList.Where(f=>!f.EndsWith(".meta")))
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
			AssetBundleHelper.TestLog("File.Copy:" + f);
		}
	}
	#endregion
}
