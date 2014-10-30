using UnityEngine;
using System.Collections;

public class AssetBundleLoadTest : MonoBehaviour {

	public AssetBundleListing sceneBundle;
	public string levelName;

	private IEnumerator Start(){
		AssetBundleRuntimeSettings.SetActiveTag("Resolution", "sd");
		yield return AssetBundleLoader.GetBundle(sceneBundle).coroutine;
		Application.LoadLevel(levelName);
	}
}