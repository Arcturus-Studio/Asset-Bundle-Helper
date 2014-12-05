using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

/* Contains user-configurable data and some utility functions.
The contents of this class are not accessible outside of the editor,
but the script itself cannot be put in the editor assembly because that makes
it inaccessible to non-editor scripts.
*/
public class AssetBundleEditorSettings : ScriptableObject {
#if UNITY_EDITOR
	//Base path for all AssetBundleHelper-related project data to be stored
	public static string DirectoryPath{
		get{
			return "Assets/AssetBundleHelper/";
		}
	}

	public static AssetBundleEditorSettings GetInstance(){
		//Fetch existing instance if it exists
		var path = Path.Combine(DirectoryPath, "Settings.asset");		
		var so = AssetDatabase.LoadMainAssetAtPath(path) as AssetBundleEditorSettings;
		if(so){
			return so;
		}
		//If no existing instance, create new instance
		so = ScriptableObject.CreateInstance<AssetBundleEditorSettings>();
		so.RestoreDefaultSettings();
		//Save asset
		DirectoryInfo di = new DirectoryInfo(DirectoryPath);
		if(!di.Exists){
			di.Create();
		}
		AssetDatabase.CreateAsset(so, path);
		AssetDatabase.SaveAssets();
		return so;
	}
	
	public void RestoreDefaultSettings(){
		platforms = new BundlePlatform[1];
		platforms[0] = new BundlePlatform();
		platforms[0].name = "Default";
		platforms[0].unityBuildTarget = BuildTarget.WebPlayer;
		
		tagGroups = new BundleTagGroup[0];
		bundleDirectoryRelativeToProjectFolder = "Bundles";
	}
	
	public BundlePlatform[] platforms; //User-defined set of platforms
	public BundleTagGroup[] tagGroups; //User-defined set of tag groups
	public string bundleDirectoryRelativeToProjectFolder; //User-defined, project-relative path to directory where asset bundles should be kept.

	//Returns bundle platforms applicable to the current build target.
	public List<BundlePlatform> GetPlatformsForCurrentBuildTarget(BuildTarget target){
		var result = platforms.Where((x) => x.unityBuildTarget == target).ToList();
		var defaultList = new List<BundlePlatform>();
		defaultList.Add(platforms[0]);
		return result.Count == 0 ? defaultList : result;
	}
	
	//Takes a bitmask and returns PlatformAndTagGroups filtered by the mask.
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
	
	//Returns an array of all BundleTagGroups. The first element of the array
	//is the platform tag group, followed by the user-set tag groups.
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
	
	//Returns the platform tag group
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
	
	private void OnValidate(){
		//Keep runtime tag groups in sync with editor settings
		AssetBundleRuntimeSettings.TagGroups = PlatformAndTagGroups;
	}
#endif
}