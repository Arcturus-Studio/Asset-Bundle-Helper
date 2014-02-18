using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AssetBundleManifest : ScriptableObject {

	public string assetBundleListingName = "";	
	public AssetBundleListing sourceListing; //Used for hotloading
	public List<AssetBundleManifestEntry> assets = new List<AssetBundleManifestEntry>();
	
}

[System.Serializable]
public class AssetBundleManifestEntry {
	public string name = "";	//Name of the asset in the bundle
	
	public AssetBundleManifestEntry(){
		
	}
	
	//Create a manifest entry for a bundle entry
	public AssetBundleManifestEntry(AssetBundleEntry src){
		name = src.name;
	}
}