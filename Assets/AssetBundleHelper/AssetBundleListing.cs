using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/*	Defines an asset set and optionally alternative asset sets to use based on the current active BundleTags.
	This class is the primary interaction point when configuring the contents of asset bundles, and is 
	used as a key when fetching bundles and bundled assets through AssetBundleLoader.
	Also contains inter-listing dependency information.
*/
public class AssetBundleListing : ScriptableObject {
	//AssetBundle creation options. Currently obsolete, since at the moment all asset bundles must be built with the same options.
	public bool gatherDependencies = true;
	public bool compressed = true;
	
	public int tagMask = 0;	//Bitmask for which BundleTagGroups this listing cares about
	public List<AssetBundleContentsWeakReference> contentWeakRefs = new List<AssetBundleContentsWeakReference>();
	public List<AssetBundleListing> dependencies = new List<AssetBundleListing>();
	
	public string Id {
		get { return id; }
#if UNITY_EDITOR
		set {
			id = value;
			EditorUtility.SetDirty(this);
		}
#endif
	}
	[SerializeField] private string id; //Unique identifier for this listing, used for constructing file paths for asset bundles and AssetBundleContents
	
	//Fetch an asset contained in this listing by name.
	//Warning: If you do not match GetAsset calls with ReleaseAsset calls, the asset will never be unloaded.
	public Coroutine<T> GetAsset<T>(string assetName) where T : UnityEngine.Object{
		return AssetBundleLoader.GetAsset<T>(this, assetName);
	}
	
	//Release asset by name.
	public void ReleaseAsset(string assetName){
		AssetBundleLoader.ReleaseAsset(this, assetName);
	}
	
	//File name for the asset bundle for this listing based on the currently active tag set
	public string ActiveFileName {
		get {
			return FileName(
				BundleTagUtils.BuildTagString(AssetBundleRuntimeSettings.MaskedActiveTags(tagMask | 1)) // | 1 -> Always include platform
			);
		}
	}
	
	//File name for the asset bundle for this listing corresponding to the passed tag set string.
	public string FileName(string tagString){
		return FileNamePrefix + "_" + tagString.ToLower();
	}
	
	//Unique file prefix for this listing
	public string FileNamePrefix {
		get{
			return Id;
		}
	}
	
	//Returns the FileNamePrefix part of a full listing filename. I.e. strips the tag string.
	public static string GetFileNamePrefix(string fileName){
		return fileName.Substring(0, fileName.LastIndexOf("_"));
	}
	
#if UNITY_EDITOR
	//Returns the list of BundleTagGroups that this listing cares about (i.e. offers varying content sets for)
	public List<BundleTagGroup> SelectedTagGroups{
		get{
			return AssetBundleEditorSettings.GetInstance().MaskToTagGroups(tagMask);
		}
	}
	
	//As SelectedTagGroups, but forces inclusion of the Platform tag group.
	public List<BundleTagGroup> SelectedTagGroupsForcePlatformGroup{
		get{
			return AssetBundleEditorSettings.GetInstance().MaskToTagGroups(tagMask | 1);
		}
	}
	
	//Returns all assets associated with the given tag set string
	public List<Object> GetAssetsForTags(string tags){
		return contentWeakRefs.FirstOrDefault(x => x.tags == tags).Load().GetAssets();
	}
	
	//Returns all asset names associated with the given tag set string
	public List<string> GetNamesForTags(string tags){
		return contentWeakRefs.FirstOrDefault(x => x.tags == tags).Load().GetNames();
	}
	
	//Returns the AssetBundleContents asset associated with the given tag set string.
	//If it does not exist, it will be created.
	public AssetBundleContents GetBundleContentsForTags(string tags){
		var weakRef = contentWeakRefs.FirstOrDefault(x => x.tags == tags);
		if(weakRef == null){
			System.Console.WriteLine("ABL " + this + ": No reference to BundleContents for \"" + tags + "\", establishing now");
			weakRef = new AssetBundleContentsWeakReference();
			weakRef.tags = tags;
			contentWeakRefs.Add(weakRef);
		}
		return weakRef.LoadOrCreate(this);
	}
#endif
}

/* Manages a weak reference to an AssetBundleContents asset. AssetBundleListings are meant to be used at runtime,
	so they cannot strongly reference the AssetBundleContents without dragging them into the build.*/
[System.Serializable]
public class AssetBundleContentsWeakReference {
	public string tags; //Period-delimited tag set string
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
		string dir = Path.Combine(AssetBundleEditorSettings.DirectoryPath, "BundleContents");
		if(!Directory.Exists(dir)){
			Directory.CreateDirectory(dir);
		}
		contentsPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dir, sourceListing.Id + (string.IsNullOrEmpty(tags) ? "" : "_") + tags + ".asset"));
		AssetBundleContents	contents = ScriptableObject.CreateInstance<AssetBundleContents>();
		contents.listing = sourceListing;
		contents.tags = tags;
		AssetDatabase.CreateAsset(contents, contentsPath);
		EditorUtility.SetDirty(sourceListing);
		AssetDatabase.SaveAssets();
	}
#endif
}