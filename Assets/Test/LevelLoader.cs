using UnityEngine;
using System.Collections;

public class LevelLoader : MonoBehaviour {

	public string package = "";

	// Use this for initialization
	IEnumerator Start () {
		Coroutine<AssetBundle> sceneLoad = StartCoroutine<AssetBundle>(LoadAssetBundle("scenes"));
		yield return sceneLoad.coroutine;
		Coroutine<AssetBundle> matLoad = StartCoroutine<AssetBundle>(LoadAssetBundle("material." + package));
		yield return matLoad.coroutine;
		Application.LoadLevel("TestScene");
	}
	
	IEnumerator LoadAssetBundle(string bundleName){
		WWW www = new WWW("file://" + Application.dataPath +"/../Bundles/" + bundleName);
		yield return www;
		AssetBundle bundle = www.assetBundle;
		yield return bundle;
	}
}
