using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

//References the assets that will be contained in an asset bundle
public class AssetBundleContents : ScriptableObject {
	public string platform = "";
	public AssetBundleListing listing;
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
	public string name;  //Name the asset is included in the bundle as
	public Object asset;
	public bool isInherited; //Whether the asset was inherited from the default specification
}
