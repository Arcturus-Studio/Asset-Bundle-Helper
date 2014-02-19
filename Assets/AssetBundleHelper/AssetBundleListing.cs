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
	public List<PlatformContentsPair> assets = new List<PlatformContentsPair>();

#if UNITY_EDITOR
	public List<Object> GetAssetsForPlatform(string platform){
		return assets.FirstOrDefault(x => x.platform == platform).Load().GetAssets();
	}
	public List<string> GetNamesForPlatform(string platform){
		return assets.FirstOrDefault(x => x.platform == platform).Load().GetNames();
	}
	public AssetBundleContents GetBundleForPlatform(string platform){
		var pair = assets.FirstOrDefault(x => x.platform == platform);
		if(pair == null){
			pair = new PlatformContentsPair();
			pair.platform = platform;
			assets.Add(pair);
		}
		return pair.LoadOrCreate(this);
	}
#endif
}

[System.Serializable]
public class PlatformContentsPair{
	public string platform;
	public string contentsPath; //Maintain weak reference

#if UNITY_EDITOR	
	public AssetBundleContents Load(){
		return AssetDatabase.LoadMainAssetAtPath(contentsPath) as AssetBundleContents;
	}
	
	public AssetBundleContents LoadOrCreate(AssetBundleListing sourceListing){
		if(string.IsNullOrEmpty(contentsPath)){
			string dir = "Assets/AssetBundleHelper/BundleContents/";
			if(!Directory.Exists(dir)){
				Directory.CreateDirectory(dir);
			}
			contentsPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dir, sourceListing.name + "_" + platform + ".asset"));
			AssetBundleContents	contents = ScriptableObject.CreateInstance<AssetBundleContents>();
			contents.listing = sourceListing;
			contents.platform = platform;
			AssetDatabase.CreateAsset(contents, contentsPath);
			EditorUtility.SetDirty(sourceListing);
			AssetDatabase.SaveAssets();
		}
		return Load();
	}
#endif
}