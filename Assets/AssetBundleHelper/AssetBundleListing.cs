using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class AssetBundleListing : ScriptableObject {
	public bool gatherDependencies = true;
	public bool compressed = true;
	public List<AssetBundleEntry> assets;
	public AssetBundleManifest manifest;
	
	public List<Object> GetListingForPlatform(string platform){
		return assets.ConvertAll<Object>((x) => {

			var result = x.GetAssetForPlatform(platform);
			if(!result){
				return x.GetAssetForPlatform(AssetBundleRuntimeSettings.DefaultPlatform);
			}
			else {return result;}
		}).ToList();
	}

#if UNITY_EDITOR
	public void UpdateManifest(){
		if(!manifest){
			manifest = ScriptableObject.CreateInstance<AssetBundleManifest>();						
			var path = AssetDatabase.GetAssetPath(this);
			string dir = Path.GetDirectoryName(path);
			string filename = Path.GetFileNameWithoutExtension(path) + " Manifest.asset";
			path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dir, filename));
			AssetDatabase.CreateAsset(manifest, path);
			AssetDatabase.SaveAssets();
		}
		
		manifest.assetBundleListingName = name;
		if(AssetBundleRuntimeSettings.Hotload){
			manifest.sourceListing = this;
		}
		else {
			manifest.sourceListing = null;
		}
		manifest.assets.Clear();
		foreach(var entry in assets){
			manifest.assets.Add(new AssetBundleManifestEntry(entry));
		}
		EditorUtility.SetDirty(manifest);
	}
#endif
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