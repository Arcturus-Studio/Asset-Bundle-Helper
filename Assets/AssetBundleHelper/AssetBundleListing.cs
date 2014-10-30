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
	public List<AssetBundleListing> dependencies = new List<AssetBundleListing>();
	
	public Coroutine<T> GetAsset<T>(string assetName) where T : UnityEngine.Object{
		return AssetBundleLoader.GetAsset<T>(this, assetName);
	}
	
	public void ReleaseAsset(string assetName){
		AssetBundleLoader.ReleaseAsset(this, assetName);
	}
	
	//File name for this asset bundle with the currently active tag set
	public string ActiveFileName {
		get {
			return FileName(
				BundleTagUtils.BuildTagString(AssetBundleRuntimeSettings.MaskedActiveTags(tagMask | 1)) // | 1 -> Always include platform
			);
		}
	}
	
	public string FileName(string tagString){
		return FileNamePrefix + "_" + tagString.ToLower();
	}
	
	public string FileNamePrefix {
		get{
			return name.ToLower();
		}
	}
	
	public static string GetFileNamePrefix(string fileName){
		return fileName.Substring(0, fileName.LastIndexOf("_"));
	}
	
#if UNITY_EDITOR
	public List<BundleTagGroup> ActiveTagGroups{
		get{
			return AssetBundleEditorSettings.GetInstance().MaskToTagGroups(tagMask);
		}
	}
	
	public List<BundleTagGroup> ActiveTagGroupsForcePlatformGroup{
		get{
			return AssetBundleEditorSettings.GetInstance().MaskToTagGroups(tagMask | 1);
		}
	}

	public List<Object> GetAssetsForTags(string tags){
		return assets.FirstOrDefault(x => x.tags == tags).Load().GetAssets();
	}
	public List<string> GetNamesForTags(string tags){
		return assets.FirstOrDefault(x => x.tags == tags).Load().GetNames();
	}
	public AssetBundleContents GetBundleContentsForTags(string tags){
		var pair = assets.FirstOrDefault(x => x.tags == tags);
		if(pair == null){
			System.Console.WriteLine("ABL " + this + ": No reference to BundleContents for \"" + tags + "\", establishing now");
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
			Create(sourceListing);
		}
		AssetBundleContents load = Load();
		if(!load){
			Debug.LogWarning("Missing AssetBundleContents at \"" + contentsPath + "\". A replacement will be created.");
			Create(sourceListing);
			load = Load();
		}
		else{
			System.Console.WriteLine("Loaded AssetBundleContents at " + contentsPath + " for " + sourceListing + " with tags \"" + tags + "\"");
		}
		return load;
	}
	
	public void Create(AssetBundleListing sourceListing){
		System.Console.WriteLine("Creating AssetBundleContents for " + sourceListing + " with tags \"" + tags + "\"");
		string dir = "Assets/AssetBundleHelper/BundleContents/";
		if(!Directory.Exists(dir)){
			Directory.CreateDirectory(dir);
		}
		contentsPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dir, sourceListing.name + (string.IsNullOrEmpty(tags) ? "" : "_") + tags + ".asset"));
		AssetBundleContents	contents = ScriptableObject.CreateInstance<AssetBundleContents>();
		contents.listing = sourceListing;
		contents.tags = tags;
		AssetDatabase.CreateAsset(contents, contentsPath);
		EditorUtility.SetDirty(sourceListing);
		AssetDatabase.SaveAssets();
	}
#endif
}