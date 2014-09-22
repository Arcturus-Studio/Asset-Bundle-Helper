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
	private static DictionaryCache<AssetBundleContentsLink> fastPathCache = new DictionaryCache<AssetBundleContentsLink>();
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
	
	public static IEnumerator Get(AssetBundleListing listing, string assetName){
		if(AssetBundleRuntimeSettings.FastPath){
			//TODO: Load dependencies
			string key = ContentsLinkResourcePath(listing.ActiveFileName);
			//Load contents link if necessary
			if(!fastPathCache.ContainsKey(key)){
				Debug.Log("Load ContentsLink " + key);
				fastPathCache.Add(key, Resources.Load<AssetBundleContentsLink>(key));
			}
			yield return fastPathCache.Get(key).bundleContents.Get(assetName);
		}
		else{
			Coroutine<AssetBundle> bundleCoroutine = LoaderHelper.StartCoroutine<AssetBundle>(Get(listing));
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
	}
	
	public static IEnumerator Get(AssetBundleListing listing){
		if(AssetBundleRuntimeSettings.FastPath){
			throw new System.NotSupportedException("Fastpath not supported yet");
		}
		else{
			foreach(AssetBundleListing dependency in listing.dependencies){
				yield return LoaderHelper.StartCoroutine(Get(dependency));
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
	
	public static void Release(AssetBundleListing listing, string assetName){
		Release(listing);		
	}
	
	public static void Release(AssetBundleListing listing){
		if(AssetBundleRuntimeSettings.FastPath){
			string key = ContentsLinkResourcePath(listing.ActiveFileName);
			if(!fastPathCache.ContainsKey(key)){
				Debug.LogWarning("No contents link with id " + key);
				return;
			}
			fastPathCache.Release(key);
		}
		else{
			string key = listing.ActiveFileName;
			if(!bundleCache.ContainsKey(key)){
				Debug.LogWarning("No bundle with id " + key);
				return;
			}
			var bundle = bundleCache.GetUntracked(key);
			if(bundleCache.Release(key)){
				bundle.Unload(false);
			}
			foreach(AssetBundleListing dependency in listing.dependencies){
				Release(dependency);
			}
		}
	}
}
