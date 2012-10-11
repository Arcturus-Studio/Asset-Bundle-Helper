using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class AssetBundleHelperSettings : ScriptableObject {
	public BundlePlatform[] platforms;
	public string bundleDirectoryRelativeToProjectFolder;
	public GUIStyle headerStyle;
	public GUIStyle deleteButtonStyle;
	public GUIStyle addButtonStyle;
	public Texture2D box, checkedBox, outOfDate;
	
	public DateTime GetLastWriteTime(AssetBundleListing listing, string platform){
		string path = bundleDirectoryRelativeToProjectFolder
		+ "/" + listing.name + "_" + platform+".unity3d";
		var fileInfo = new FileInfo(path);
		if(!fileInfo.Exists){
			return new DateTime((System.Int64)0);
		}
		return fileInfo.LastWriteTimeUtc;
	}
	
	public List<BundlePlatform> GetPlatformsForCurrentBuildTarget(BuildTarget target){
		var result = platforms.Where((x) => x.unityBuildTarget == target).ToList();
		var defaultList = new List<BundlePlatform>();
		defaultList.Add(platforms[0]);
		return result.Count == 0 ? defaultList : result;
	}
}

[System.Serializable]
public class BundlePlatform{
	public string name;
	public Texture2D icon32;
	public BuildTarget unityBuildTarget;
}
