using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
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

	private static AssetBundleLoaderHelper LoaderHelper{
		get{
			if(!loaderHelper){
#if UNITY_EDITOR
				//Find pre-existing if possible so we don't leak objects with every recompile
				var obj = GameObject.Find("/__AssetBundleLoader");
				if(obj){
					loaderHelper = obj.GetComponent<AssetBundleLoaderHelper>();
				}
				if(!loaderHelper){
					loaderHelper = EditorUtility.CreateGameObjectWithHideFlags("__AssetBundleLoader", HideFlags.HideAndDontSave, typeof(AssetBundleLoaderHelper)).GetComponent<AssetBundleLoaderHelper>();
				}
#else
				loaderHelper = new GameObject("__AssetBundleLoader", typeof(AssetBundleLoaderHelper)).GetComponent<AssetBundleLoaderHelper>();
				loaderHelper.gameObject.hideFlags = HideFlags.HideAndDontSave;
#endif
			}
			return loaderHelper;
		}
	}
	private static AssetBundleLoaderHelper loaderHelper;
	
	public static string AssetBundlePath(string assetBundleFileName){
		return Path.Combine(BasePath, assetBundleFileName);
	}
	
	public static string ContentsLinkResourcePath(string assetBundleFileName){
		return assetBundleFileName; //In Resources
	}
	
	public static Coroutine<T> GetAsset<T>(AssetBundleListing listing, string assetName){
		return LoaderHelper.StartCoroutine<T>(GetAssetCoroutine(listing, assetName));
	}
	
	public static Coroutine<AssetBundle> GetBundle(AssetBundleListing listing){
		return LoaderHelper.StartCoroutine<AssetBundle>(GetListingCoroutine(listing));
	}
		
	public static void ReleaseAsset(AssetBundleListing listing, string assetName){
		ReleaseBundle(listing);		
	}
	
	public static void ReleaseBundle(AssetBundleListing listing){		
		string key = listing.ActiveFileName;
		if(!bundleCache.ContainsKey(key)){
			Debug.LogError("No bundle with id " + key);
			return;
		}
		var bundle = bundleCache.GetUntracked(key);
		if(bundleCache.Release(key)){
			bundle.Unload(false);
		}
		foreach(AssetBundleListing dependency in listing.dependencies){
			ReleaseBundle(dependency);
		}
	}
	
	private static IEnumerator GetAssetCoroutine(AssetBundleListing listing, string assetName){
		Coroutine<AssetBundle> bundleCoroutine = GetBundle(listing);
		yield return bundleCoroutine.coroutine;
		AssetBundle bundle = bundleCoroutine.Value;
		//Load asset from bundle
		if(bundle.Contains(assetName)){
			yield return bundle.LoadAsset(assetName);
		}
		else{
			Debug.LogWarning("Bundle " + listing + "does not contain " + assetName);
			yield break;
		}
	}
	
	private static IEnumerator GetListingCoroutine(AssetBundleListing listing){
		foreach(AssetBundleListing dependency in listing.dependencies){
			yield return GetBundle(dependency).coroutine;
		}
		string key = listing.ActiveFileName;
		if(!bundleCache.ContainsKey(key)){
			Debug.Log("Load AssetBundle from " + AssetBundlePath(key));
			var www = new WWW(AssetBundlePath(key));
			yield return www;
			var wwwBundle = www.assetBundle;
			if(wwwBundle){
				bundleCache.Add(key, wwwBundle);
			}
			www.Dispose();
		}
		var bundle = bundleCache.Get(key);
		yield return bundle;
	}
}
