using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class AssetBundleListingFinder {
	public static List<AssetBundleListing> GetAssetBundleListings(){
		List<AssetBundleListing> detectedBundles = new List<AssetBundleListing>();
		DirectoryInfo di = new DirectoryInfo(Application.dataPath); //Assets directory
		FileInfo[] files = di.GetFiles("*.asset", SearchOption.AllDirectories);
		foreach(FileInfo fi in files){
			string projectRelativePath = fi.FullName.Substring(di.Parent.FullName.Length + 1); //+1 includes slash
			AssetBundleListing abl = AssetDatabase.LoadAssetAtPath(projectRelativePath, typeof(AssetBundleListing)) as AssetBundleListing;
			if(abl != null){
				detectedBundles.Add(abl);
			}
		}
		return detectedBundles;
	}
}
