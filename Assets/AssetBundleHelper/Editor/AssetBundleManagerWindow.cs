using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class AssetBundleManagerWindow : EditorWindow {
	
	public List<FileInfo> detectedBundlesFileInfos;
	public List<AssetBundleListing> detectedBundles;
	public Vector2 bundleListScrollPos;
	
	[MenuItem("Window/AssetBundleManager %&5")]
	public static void Initialize(){
		AssetBundleManagerWindow window = EditorWindow.GetWindow(typeof(AssetBundleManagerWindow)) as AssetBundleManagerWindow;
		window.Refresh();
	}

	
	public void Refresh(){
		//Search for existing AssetBundleListings
		detectedBundlesFileInfos = new List<FileInfo>();
		detectedBundles = new List<AssetBundleListing>();
		DirectoryInfo di = new DirectoryInfo(Application.dataPath); //Assets directory
		FileInfo[] files = di.GetFiles("*.asset", SearchOption.AllDirectories);
		foreach(FileInfo fi in files){
			string projectRelativePath = fi.FullName.Substring(di.Parent.FullName.Length + 1); //+1 includes slash
			AssetBundleListing abl = AssetDatabase.LoadAssetAtPath(projectRelativePath, typeof(AssetBundleListing)) as AssetBundleListing;
			if(abl != null){
				detectedBundlesFileInfos.Add(fi);
				detectedBundles.Add(abl);
			}
		}
		Repaint();
	}

	public void OnGUI(){
		if(detectedBundlesFileInfos == null || detectedBundles == null){
			Refresh();
			return;
		}
		EditorGUIUtility.LookLikeControls();
		//Hack: Layout changes during a refresh which makes mousedown event throw an exception.
		//Delaying refresh to the Repaint stage causes the window to flicker,
		//so just consume the exception and stop trying to parse mouse input this frame
		//TODO: This breaks elsewhere if bundles go missing. Fix properly
		try{ 
			GUILayout.BeginHorizontal();
		}
		catch(ArgumentException){
			Event.current.type = EventType.used;
			return;
		}
		GUILayout.Label("Bundles", EditorStyles.boldLabel);
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("Refresh",EditorStyles.miniButton)){
			Refresh();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginVertical(GUI.skin.box);
		GUILayout.BeginHorizontal(EditorStyles.toolbar);
		GUILayout.Label("Name", GUILayout.MinWidth(100));
		GUILayout.FlexibleSpace();
		GUILayout.Label("Variants");
		GUILayout.EndHorizontal();
		
		bundleListScrollPos = GUILayout.BeginScrollView(bundleListScrollPos);
		for(int i = 0; i < detectedBundles.Count; i++){
			AssetBundleListing listing = detectedBundles[i];
			if(listing == null){
				Refresh();
				return;
			}
			FileInfo listingFile = detectedBundlesFileInfos[i];
			if(listingFile == null){
				Refresh();
				return;
			}			
			GUILayout.BeginHorizontal();
			if(GUILayout.Button(listing.name,EditorStyles.miniButton,GUILayout.MinWidth(100))){
				Selection.activeObject = listing;
				EditorGUIUtility.PingObject(Selection.activeObject);
			}
			GUILayout.FlexibleSpace();
			GUILayout.Label(AssetBundleListingEditor.Settings.MaskToTagString(detectedBundles[i].tagMask));
			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();

		if(GUILayout.Button("Build AssetBundles for all platforms", GUILayout.Height(48))){
			foreach(var platform in AssetBundleListingEditor.Settings.platforms){
				BuildBundlesForPlatform(platform);
			}			
		}
		foreach(var platform in AssetBundleListingEditor.Settings.platforms){
			if(GUILayout.Button("Build AssetBundles for " + platform.name)){
				BuildBundlesForPlatform(platform);
			}
		}
	}
	
	private void BuildBundlesForPlatform(BundlePlatform platform){
		//Bundle assets with default tagstrings
		string targetDirPath = AssetBundleListingEditor.Settings.bundleDirectoryRelativeToProjectFolder;
		AssetBundleBuild[] baseBuild = new AssetBundleBuild[detectedBundles.Count];
		for(int i = 0; i < detectedBundles.Count; i++){
			List<BundleTagGroup> tagGroups = detectedBundles[i].ActiveTagGroupsForcePlatformGroup;
			List<BundleTag> defaultNonPlatformTags = BundleTagUtils.DefaultTagCombination(tagGroups, 1); //1 = skip platform group
			
			var defaultTagsIncPlatform = ((BundleTag)platform).Yield().Concat(defaultNonPlatformTags);
			string defaultTagStringIncPlatform = BundleTagUtils.BuildTagString(defaultTagsIncPlatform);
			string defaultTagString = BundleTagUtils.BuildTagString(BundleTagUtils.DefaultTagCombination(detectedBundles[i].ActiveTagGroups, 0));
			
			baseBuild[i].assetBundleName = detectedBundles[i].FileName(defaultTagStringIncPlatform);
			baseBuild[i].assetNames = detectedBundles[i].GetAssetsForTags(defaultTagString).Select(x => AssetDatabase.GetAssetPath(x)).ToArray();
		}
		BuildPipeline.BuildAssetBundles(targetDirPath, baseBuild, BuildAssetBundleOptions.None, platform.unityBuildTarget);
		
		//Read dependency information		
		DirectoryInfo targetDir = new DirectoryInfo(Application.dataPath);
		targetDir = targetDir.Parent;
		targetDir = new DirectoryInfo(Path.Combine(targetDir.FullName, targetDirPath));
		string manifestBundlePath = Path.Combine(targetDir.FullName, targetDir.Name);
		var www = new WWW("file://" + manifestBundlePath);
		AssetBundle manifestBundle = www.assetBundle;
		AssetBundleManifest manifest = (AssetBundleManifest)manifestBundle.LoadAsset("AssetBundleManifest");
		manifestBundle.Unload(false);
		foreach(string assetBundleName in manifest.GetAllAssetBundles()){
			AssetBundleListing listing = detectedBundles.Find(x => x.FileNamePrefix == AssetBundleListing.GetFileNamePrefix(assetBundleName));
			if(!listing){
				Debug.LogError("No AssetBundleListing asset found for manifest name " + assetBundleName + ", could not record dependency information");
				continue;
			}
			listing.dependencyNames.Clear();
			foreach(string dependency in manifest.GetDirectDependencies(assetBundleName)){
				listing.dependencyNames.Add(AssetBundleListing.GetFileNamePrefix(dependency));
			}
			EditorUtility.SetDirty(listing);
		}
		
		//Bundle assets with variant tagstrings
		for(int i = 0; i < detectedBundles.Count; i++){
			List<BundleTagGroup> tagGroups = detectedBundles[i].ActiveTagGroupsForcePlatformGroup;
			if(tagGroups.Count == 1){
				continue; //Bundle contents do not vary, or only vary accross platform, do not need to build
			}
			
			foreach(var tagCombo in BundleTagUtils.TagCombinations(tagGroups, 1)){
				string tagString = BundleTagUtils.BuildTagString(((BundleTag)platform).Yield().Concat(tagCombo));
				AssetBundleBuild[] variantBuild = new AssetBundleBuild[1];
				variantBuild[0].assetBundleName = detectedBundles[i].FileName(tagString);
				variantBuild[0].assetNames = detectedBundles[i].GetAssetsForTags(tagString).Select(x => AssetDatabase.GetAssetPath(x)).ToArray();
				BuildPipeline.BuildAssetBundles(targetDirPath, variantBuild, BuildAssetBundleOptions.None, platform.unityBuildTarget);
			}			
		}
	}
}
