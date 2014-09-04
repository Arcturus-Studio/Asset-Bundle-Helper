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
	private static DictionaryCache<AssetBundleContentsLink> fastPathCache = new DictionaryCache<AssetBundleContentsLink>();
	
	public static string AssetBundleFileNameForPlatform(string assetBundleListingName, string platform){
		return assetBundleListingName + "_" + platform + ".unity3d";
	}
	
	public static string AssetBundleContentsLinkResourceNameForPlatform(string assetBundleListingName, string platform){
		return assetBundleListingName + "_" + platform; //No extension when loading from resources
	}
	
	public static string AssetBundlePath(string assetBundleListingName){
		return Path.Combine(BasePath, AssetBundleFileNameForPlatform(assetBundleListingName, Platform));
	}
	
	public static IEnumerator Get(AssetBundleListing listing, string assetName){
		if(AssetBundleRuntimeSettings.FastPath){
			string key = AssetBundleContentsLinkResourceNameForPlatform(listing.name, Platform);
			//Load contents link if necessary
			if(!fastPathCache.ContainsKey(key)){
				Debug.Log("Load ContentsLink " + key);
				fastPathCache.Add(key, Resources.Load<AssetBundleContentsLink>(key));
			}
			yield return fastPathCache.Get(key).bundleContents.Get(assetName);
		}
		else{
			string key = AssetBundleFileNameForPlatform(listing.name, Platform);
			//Load asset bundle if necessary
			if(!bundleCache.ContainsKey(key)){
				Debug.Log("Load AssetBundle from " + AssetBundlePath(listing.name));
				var www = new WWW(AssetBundlePath(listing.name));
				yield return www;
				var wwwBundle = www.assetBundle;
				if(wwwBundle){
					bundleCache.Add(key, wwwBundle);
				}
				www.Dispose();
			}
			//Load asset from bundle
			var bundle = bundleCache.Get(key);
			if(bundle.Contains(assetName)){
				yield return bundle.LoadAsset(assetName);
			}
			else{
				Debug.LogWarning("Bundle does not contain " + assetName);
				yield break;
			}
		}
	}
	
	public static void Release(AssetBundleListing listing, string assetName){
		if(AssetBundleRuntimeSettings.FastPath){
			string key = AssetBundleContentsLinkResourceNameForPlatform(listing.name, Platform);
			if(!fastPathCache.ContainsKey(key)){
				Debug.LogWarning("No contents link with id " + key);
				return;
			}
			fastPathCache.Release(key);
		}
		else{
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
}
