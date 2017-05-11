using UnityEngine;
using System.Collections;
using System.IO;
/// <summary>
/// 这是一个测试加载资源的Bundle的类
/// </summary>
public class AssetBundleTest : MonoBehaviour
{
	public bool load = true;
	public int UpdateCount = 0;
	public string TestLoadName3 = @"Assets/__ArtRes/reS/Prefabs/Tank/hero_xiniu_01.prefab";
	public string TestLoadName1 = @"assets/__artres/reS/prefabs/tank/hero_xiniu_01.prefab";
	public string TestLoadName2 = @"assets/__artres/reS/prefabs/tank/hero_xiniu_01.prefab";
	void Update()
	{
		UpdateCount++;
		if (load && AssetBundleHelper.EnableUseRes())
		{
			load = !load;
			AssetBundleHelper.PushResToNeedLoad(TestLoadName1, (o) => Debug.Log(o.name));
			var go = AssetBundleHelper.LoadResource_Sync<GameObject>(TestLoadName1);
			go = Instantiate(go);
			go.transform.parent = this.transform;
			//AssetBundleHelper.PushResToNeedLoad(TestLoadName2, (o) => Debug.Log(o.name));
			//AssetBundleHelper.PushResToNeedLoad(TestLoadName3, (o) => Debug.Log(o.name));
			// 			StartCoroutine(AssetBundleHelper.LoadResourceAsyn(TestLoadName1, (o)=>Debug.Log(o.name)));
			// 			StartCoroutine(AssetBundleHelper.LoadResourceAsyn(TestLoadName2, (o)=>Debug.Log(o.name)));
			// 			StartCoroutine(AssetBundleHelper.LoadResourceAsyn(TestLoadName3, (o) => Debug.Log(o.name)));
		}
	}


}
