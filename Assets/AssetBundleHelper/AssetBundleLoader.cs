using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class AssetBundleLoader {

	public static string Platform = "Default";
	public static string BasePath{
		get{
			return basePathProvider.GetPath();
		}
	}
	public static AssetBundlePathProvider basePathProvider = new AssetBundlePathProvider();
	
	private static DictionaryCache<AssetBundle> bundleCache = new DictionaryCache<AssetBundle>();
	
	public static string AssetBundleFileNameForPlatform(string assetBundleListingName, string platform){
		return assetBundleListingName + "_" + platform + ".unity3d";
	}
	
	public static string AssetBundlePath(string assetBundleListingName){
		return Path.Combine(BasePath, AssetBundleFileNameForPlatform(assetBundleListingName, Platform));
	}
	
	public static IEnumerator Get(AssetBundleListing listing, string assetName){
		string key = AssetBundleFileNameForPlatform(listing.name, Platform);
		if(!bundleCache.ContainsKey(key)){
			Debug.Log("Load AssetBundle from " + AssetBundlePath(listing.name));
			var www = new WWW(AssetBundlePath(listing.name));
			yield return www;
			if(www.assetBundle){
				bundleCache.Add(key, www.assetBundle);
			}
			www.Dispose();
		}
		var bundle = bundleCache.Get(key);
		if(bundle.Contains(assetName)){
			yield return bundle.Load(assetName);
		}
		else{
			Debug.LogWarning("Bundle does not contain " + assetName);
			yield break;
		}
	}
	
	public static void Release(AssetBundleListing listing, string assetName){
		string key = AssetBundleFileNameForPlatform(listing.name, Platform);
		if(!bundleCache.ContainsKey(key)){
			Debug.LogWarning("No bundle with id " + key);
			return;
		}
		var bundle = bundleCache.GetUntracked(key);
		if(bundleCache.Release(key)){
			bundle.Unload(false);
		}
	}
}
