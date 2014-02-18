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
	
	
	public static string AssetBundleFileNameForPlatform(string assetBundleListingName, string platform){
		return assetBundleListingName + "_" + platform + ".unity3d";
	}
	
	public static string AssetBundlePath(string assetBundleListingName){
		return Path.Combine(BasePath, AssetBundleFileNameForPlatform(assetBundleListingName, Platform));
	}
	
	
}
