using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class AssetBundleListing : ScriptableObject {
	public bool gatherDependencies = true;
	public bool compressed = true;
	public List<AssetBundleEntry> assets;
	
	public List<Object> GetListingForPlatform(string platform){
		return assets.ConvertAll<Object>((x) => {

			var result = x.GetAssetForPlatform(platform);
			if(!result){
				return x.GetAssetForPlatform("Default");
			}
			else {return result;}
		}).ToList();
	}
}

[System.Serializable]
public class AssetBundleEntry{
	public string name = "";
	public void Add(Object obj, string platform){
		if(name == ""){
			name = obj.name;
		}
		PlatformAssetPair pa = platformToAsset.FirstOrDefault(
			(x) => {
				return x.platform == platform;
			});
		if(pa == null){
			pa = new PlatformAssetPair();
			platformToAsset.Add(pa);
		}
		pa.platform = platform;
		pa.asset = obj;
	}
	public Object GetAssetForPlatform(string platform){
		if(platformToAsset == null){
			platformToAsset = new List<PlatformAssetPair>();
		}
		var pap = platformToAsset.FirstOrDefault(
			(x) => {
				return x.platform == platform;
			});
		return pap == null ? null : pap.asset;
	}
	public List<PlatformAssetPair> platformToAsset = new List<PlatformAssetPair>();
}

[System.Serializable]
public class PlatformAssetPair{
	public string platform;
	public Object asset;
}