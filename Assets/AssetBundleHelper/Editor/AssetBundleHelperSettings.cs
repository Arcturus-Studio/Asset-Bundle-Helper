using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class AssetBundleHelperSettings : ScriptableObject {
	public bool hotload; //For development purposes, bypass need to update assetbundles constantly
	public BundlePlatform[] platforms;
	public BundleTagGroup[] tagGroups;
	public string defaultPlatform = "Default";
	public string bundleDirectoryRelativeToProjectFolder;
	public GUIStyle headerStyle;
	public GUIStyle deleteButtonStyle;
	public GUIStyle addButtonStyle;
	public Texture2D box, checkedBox, outOfDate;
	
	public DateTime GetLastWriteTime(AssetBundleListing listing, string platform){
		string path = bundleDirectoryRelativeToProjectFolder
		+ Path.DirectorySeparatorChar + listing.name + "_" + platform+".unity3d";
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
	
	public string MaskToTagString(int mask){
		StringBuilder result = new StringBuilder();
		BundleTagGroup[] allTags = PlatformAndTagGroups;
		for(int i = 0; i < allTags.Length && i < 32; i++){
			if((mask & (1 << i)) != 0){
				if(result.Length > 0){
					result.Append(".");
				}
				result.Append(allTags[i].name);
			}
		}
		return result.ToString();
	}
	
	public List<BundleTagGroup> MaskToTagGroups(int mask){
		var result = new List<BundleTagGroup>();
		BundleTagGroup[] allTags = PlatformAndTagGroups;
		for(int i = 0; i < allTags.Length && i < 32; i++){
			if((mask & (1 << i)) != 0){
				result.Add(allTags[i]);
			}
		}
		return result;
	}
	
	public BundleTagGroup[] PlatformAndTagGroups{
		get{
			var result = new BundleTagGroup[tagGroups.Length + 1];
			result[0] = PlatformGroup;
			for(int i = 0; i < tagGroups.Length; i++){
				result[i+1] = tagGroups[i];
			}
			return result;
		}
	}
	
	private BundleTagGroup PlatformGroup{
		get{
			if(_platformGroup == null){
				_platformGroup = new BundleTagGroup();
				_platformGroup.name = "Platform";
				_platformGroup.tags = platforms;
			}
			return _platformGroup;
		}
	}
	[System.NonSerialized]
	BundleTagGroup _platformGroup;
}

[System.Serializable]
public class BundleTag{
	public string name = "";
	public Texture2D icon32;
	
	public static BundleTag NoTag{
		get{
			return noTag;
		}
	}
	private static BundleTag noTag = new BundleTag();
}

[System.Serializable]
public class BundlePlatform : BundleTag{
	public BuildTarget unityBuildTarget;
}

[System.Serializable]
public class BundleTagGroup{
	public string name;
	public BundleTag[] tags;
}
