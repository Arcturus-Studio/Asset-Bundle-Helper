using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

/* Manages a weak reference to an AssetBundleContents asset. AssetBundleListings are meant to be used at runtime,
	so they cannot strongly reference the AssetBundleContents without dragging them into the build.*/
[System.Serializable]
public class AssetBundleContentsWeakReference {
	public string tags; //Period-delimited tag set string

#if UNITY_EDITOR
	public string ContentsPath {
		get { return contentsPath; }		
		set{ contentsPath = value; }
	}
#endif
	[SerializeField] private string contentsPath; //Path to AssetBundleContents asset

#if UNITY_EDITOR
	public AssetBundleContents Load(){
		return AssetDatabase.LoadMainAssetAtPath(contentsPath) as AssetBundleContents;
	}
	
	public AssetBundleContents LoadOrCreate(AssetBundleListing sourceListing){
		if(string.IsNullOrEmpty(contentsPath)){
			Create(sourceListing);
		}
		AssetBundleContents loadedContents = Load();
		if(!loadedContents){
			Debug.LogWarning("Missing AssetBundleContents at \"" + contentsPath + "\". A replacement will be created.");
			Create(sourceListing);
			loadedContents = Load();
		}
		else{
			System.Console.WriteLine("Loaded AssetBundleContents at " + contentsPath + " for " + sourceListing + " with tags \"" + tags + "\"");
		}
		return loadedContents;
	}
	
	public void Create(AssetBundleListing sourceListing){
		System.Console.WriteLine("Creating AssetBundleContents for " + sourceListing + " with tags \"" + tags + "\"");
		contentsPath = GetPath(sourceListing.Id, tags);
		if(!Directory.Exists(Path.GetDirectoryName(contentsPath))){
			Directory.CreateDirectory(Path.GetDirectoryName(contentsPath));
		}
		AssetBundleContents	contents = ScriptableObject.CreateInstance<AssetBundleContents>();
		contents.listing = sourceListing;
		contents.tags = tags;
		AssetDatabase.CreateAsset(contents, contentsPath);
		EditorUtility.SetDirty(sourceListing);
		AssetDatabase.SaveAssets();
	}
	
	//Returns the target path for a contents with the given listing ID and tag set.
	public static string GetPath(string listingId, string tags){
		string dir = Path.Combine(AssetBundleEditorSettings.DirectoryPath, "BundleContents");
		return Path.Combine(dir, listingId + (string.IsNullOrEmpty(tags) ? "" : AssetBundleChars.BundleSeparator.ToString()) + tags + ".asset");
	}
#endif
}