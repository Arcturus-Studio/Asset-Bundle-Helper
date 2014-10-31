using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/* Group of references to assets that will be contained in a single asset bundle. */
public class AssetBundleContents : ScriptableObject {	
	public string tags; //Period-delimited list of BundleTag names. Defines the tags the assets should be used for.
	public AssetBundleListing listing; //Source listing this was constructed from
	public List<BundleContentsEntry> assets = new List<BundleContentsEntry>();
	
	public List<Object> GetAssets(){
		return assets.ConvertAll<Object>( x => x.asset);
	}
		
	public List<string> GetNames(){
		return assets.ConvertAll<string>( x => x.name);
	}
	
	public Object Get(string name){
		return assets.FirstOrDefault<BundleContentsEntry>(x => x.name == name).asset;
	}
}

[System.Serializable]
public class BundleContentsEntry{
	public int id; //Entries with the same ID represent the same asset with different tag configurations
	public string name;  //Name the asset is included in the bundle as
	public Object asset;
	public bool isInherited; //Whether the asset was inherited from the default specification
}
