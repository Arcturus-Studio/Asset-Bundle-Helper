using UnityEngine;
using System.Collections;

public class LevelLoader : MonoBehaviour {

	public bool loadA, loadB;

	// Use this for initialization
	IEnumerator Start () {
		Coroutine<AssetBundle> sceneLoad = StartCoroutine<AssetBundle>(LoadAssetBundle("scenes"));
		yield return sceneLoad.coroutine;
		Coroutine<AssetBundle> matLoad = StartCoroutine<AssetBundle>(LoadAssetBundle("bundleCube"));
		yield return matLoad.coroutine;
		Coroutine<AssetBundle> matLoad2 = StartCoroutine<AssetBundle>(LoadAssetBundle("bundleSphere"));
		yield return matLoad2.coroutine;
		if(loadA){
			Coroutine<AssetBundle> matLoad3 = StartCoroutine<AssetBundle>(LoadAssetBundle("materialA"));
			yield return matLoad3.coroutine;
		}
		if(loadB){
			Coroutine<AssetBundle> matLoad4 = StartCoroutine<AssetBundle>(LoadAssetBundle("materialB"));
			yield return matLoad4.coroutine;
		}
		Application.LoadLevel("TestScene");
	}
	
	IEnumerator LoadAssetBundle(string bundleName){
		WWW www = new WWW("file://" + Application.dataPath +"/../Bundles/" + bundleName);
		yield return www;
		AssetBundle bundle = www.assetBundle;
		yield return bundle;
	}
}
