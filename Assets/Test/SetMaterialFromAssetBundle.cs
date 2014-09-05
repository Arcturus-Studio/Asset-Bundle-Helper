using UnityEngine;
using System.Collections;

public class SetMaterialFromAssetBundle : MonoBehaviour {

	public AssetBundleListing listing;
	public string materialName;

	private bool loaded;

	// Use this for initialization
	IEnumerator Start () {
		AssetBundleLoader.Platform = "Default";
		yield return StartCoroutine(LoadMaterial());
	}
	
	void OnDestroy(){
		if(loaded){
			listing.Release(materialName);
		}
	}
	
	IEnumerator LoadMaterial(){
		Coroutine<Material> loader = StartCoroutine<Material>(listing.Get(materialName));
		yield return loader.coroutine;
		loaded = true;
		GetComponent<Renderer>().sharedMaterial = loader.Value;
	}
}
