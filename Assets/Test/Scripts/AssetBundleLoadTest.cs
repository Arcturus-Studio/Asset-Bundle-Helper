using UnityEngine;
using System.Collections;

public class AssetBundleLoadTest : MonoBehaviour {

	public AssetBundleListing sceneBundle;
	public string levelName;

	public IEnumerator Start(){
		yield return StartCoroutine(AssetBundleLoader.Get(sceneBundle));
		Application.LoadLevel(levelName);
	}
}