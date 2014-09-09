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
	public int tagMask = 0;	
	public List<TagPathPair> assets = new List<TagPathPair>();
	
	public IEnumerator Get(string assetName){
		return AssetBundleLoader.Get(this, assetName);
	}
	
	public void Release(string assetName){
		AssetBundleLoader.Release(this, assetName);
	}
	
#if UNITY_EDITOR
	public List<Object> GetAssetsForTags(string tags){
		return assets.FirstOrDefault(x => x.tags == tags).Load().GetAssets();
	}
	public List<string> GetNamesForTags(string tags){
		return assets.FirstOrDefault(x => x.tags == tags).Load().GetNames();
	}
	public AssetBundleContents GetBundleContentsForTags(string tags){
		var pair = assets.FirstOrDefault(x => x.tags == tags);
		if(pair == null){
			pair = new TagPathPair();
			pair.tags = tags;
			assets.Add(pair);
		}
		return pair.LoadOrCreate(this);
	}
#endif
}

[System.Serializable]
public class TagPathPair{
	public string tags; //Period-delimited tag string
	public string contentsPath; //Path to AssetBundleContents (used as weak reference)

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
			contentsPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dir, sourceListing.name + "_" + tags + ".asset"));
			AssetBundleContents	contents = ScriptableObject.CreateInstance<AssetBundleContents>();
			contents.listing = sourceListing;
			contents.tags = tags;
			AssetDatabase.CreateAsset(contents, contentsPath);
			EditorUtility.SetDirty(sourceListing);
			AssetDatabase.SaveAssets();
		}
		return Load();
	}
#endif
}