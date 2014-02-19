using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(AssetBundleListing))]
public class AssetBundleListingEditor : Editor {
	
	static AssetBundleHelperSettings settings;
	
	public static AssetBundleHelperSettings Settings{
		get{
			if(settings == null){
				settings = CreateAssetBundleHelperSettings();
			}
			return settings;
		}
	}
	
	private static string FolderPathFromSelection(){
		var selection = Selection.GetFiltered(typeof(Object),SelectionMode.Assets);
		string path = "Assets";
		if(selection.Length > 0){
			path = AssetDatabase.GetAssetPath(selection[0]);
			var dummypath = System.IO.Path.Combine(path, "fake.asset");
		    var assetpath = AssetDatabase.GenerateUniqueAssetPath(dummypath);
			if(assetpath != ""){
				return path;
			}
			else{
				return System.IO.Path.GetDirectoryName(path);
			}
		}
		return path;
	}
	
	[MenuItem("Assets/Create/AssetBundleListing")]
	public static void CreateAssetBundleListing(){
		var so = ScriptableObject.CreateInstance<AssetBundleListing>();
		var path = System.IO.Path.Combine(FolderPathFromSelection(),"Listing.asset");
		path = AssetDatabase.GenerateUniqueAssetPath(path);
		AssetDatabase.CreateAsset(so,path);
		Selection.activeObject = so;
		AssetDatabase.SaveAssets();
	}
	
	public static AssetBundleHelperSettings CreateAssetBundleHelperSettings(){
		var path = System.IO.Path.Combine("Assets/AssetBundleHelper","Settings.asset");
		var so = AssetDatabase.LoadMainAssetAtPath(path) as AssetBundleHelperSettings;
		if(so)
			return so;
		so = ScriptableObject.CreateInstance<AssetBundleHelperSettings>();
		so.platforms = new BundlePlatform[1];
		so.platforms[0] = new BundlePlatform();
		so.platforms[0].name = AssetBundleRuntimeSettings.DefaultPlatform;
		so.platforms[0].unityBuildTarget = BuildTarget.WebPlayer;
		DirectoryInfo di = new DirectoryInfo(Application.dataPath+"/AssetBundleHelper");
		if(!di.Exists){
			di.Create();
		}
		AssetDatabase.CreateAsset(so,path);
		AssetDatabase.SaveAssets();
		return so;
	}
	
	List<ListingEditorEntry> assets;	
	List<ListingEditorEntry> toRemove = new List<ListingEditorEntry>();
	
	public void OnEnable(){
		//Construct more readily editable asset mapping
		assets = new List<ListingEditorEntry>();
		var assetsByName = new Dictionary<string, ListingEditorEntry>();
		AssetBundleListing listing = target as AssetBundleListing;
		
		foreach(var pair in listing.assets){
			AssetBundleContents contents = pair.Load();
			if(contents != null){
				foreach(var entry in contents.assets){
					if(!assetsByName.ContainsKey(entry.name)){
						var newEntry = new ListingEditorEntry();
						newEntry.name = entry.name;						
						assetsByName[entry.name] = newEntry;
						assets.Add(newEntry);
					}
					assetsByName[entry.name].assets[contents.platform] = entry.isInherited ? null : entry.asset;
				}
			}
		}
	}
	
	public override void OnInspectorGUI() {
		serializedObject.Update();
		AssetBundleListing listing = target as AssetBundleListing;
		EditorGUIUtility.LookLikeControls();
		GUILayout.Label("Bundle Contents",EditorStyles.boldLabel);
		GUILayout.BeginVertical(GUI.skin.box);
		//Header
		GUILayout.BeginHorizontal(EditorStyles.toolbar);		
		GUILayout.Label("Name", GUILayout.MinWidth(100));
		GUILayout.FlexibleSpace();
		foreach(var plat in Settings.platforms){
			GUILayout.Label(new GUIContent(plat.name,plat.icon32),GUILayout.Height(14), GUILayout.Width(60));
		}
		GUILayout.Space(16);
		GUILayout.EndHorizontal();
		
		//Asset listing
		foreach(var entry in assets){
			GUILayout.BeginHorizontal();
			GUILayout.Space(2);
			string name = GUILayout.TextField(entry.name, GUILayout.MinWidth(100));
			if(entry.name != name){
				entry.name = name;
				UpdateBundleContents();
			}			
			GUILayout.FlexibleSpace();
			foreach(var plat in Settings.platforms){
				Object o = null;
				Object d = null;
				bool usingDefault = false;

				d = entry.GetAssetForPlatform(AssetBundleRuntimeSettings.DefaultPlatform);
				o = entry.GetAssetForPlatform(plat.name);

				if(o == null && d != null){
					usingDefault = true;
				}
				if(usingDefault){
					GUI.backgroundColor = Color.grey;
					Object n = EditorGUILayout.ObjectField(d,typeof(Object), false, GUILayout.Width(60));
					GUI.backgroundColor = Color.white;
					if(n != d){
						entry.Add(n, plat.name);
						UpdateBundleContents();
					}
				}
				else{
					if(d != null && o.GetType() != d.GetType()){
						GUI.backgroundColor = Color.yellow;
					}
					Object n = EditorGUILayout.ObjectField(o,typeof(Object), false, GUILayout.Width(60));
					GUI.backgroundColor = Color.white;
					if(n != o){						
						entry.Add(n, plat.name);
						UpdateBundleContents();
					}
				}
			}
			if(GUILayout.Button("",Settings.deleteButtonStyle)){
				toRemove.Add(entry);
			}
			GUILayout.Space(2);
			GUILayout.EndHorizontal();
		}
		//New entry
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("",Settings.addButtonStyle)){
			assets.Add(new ListingEditorEntry());
			UpdateBundleContents();
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		//Handle removed entries
		if(toRemove.Count > 0){
			assets.RemoveAll((x) => toRemove.Contains(x));
			toRemove.Clear();
			UpdateBundleContents();
		}
		
		//Settings
		GUILayout.Label("Bundle Build Options",EditorStyles.boldLabel);
		listing.gatherDependencies =  EditorGUILayout.Toggle("Gather Dependencies", listing.gatherDependencies);
		listing.compressed =  EditorGUILayout.Toggle("Compressed", listing.compressed);
		if(GUI.changed){
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);
		}
		
		var curPlats = Settings.GetPlatformsForCurrentBuildTarget(EditorUserBuildSettings.activeBuildTarget);
		string platList = "";
		foreach(var plat in curPlats){
			platList += " "+plat.name;
		}
		
		if(GUILayout.Button("Build AssetBundle ("+platList+")")){
			BuildBundleForCurrentPlatforms(listing);
		}
	}
	
	protected void UpdateBundleContents(){
		var listing = target as AssetBundleListing;
		foreach(var plat in Settings.platforms){
			AssetBundleContents bundle = listing.GetBundleForPlatform(plat.name);
			bundle.assets.Clear();
			foreach(ListingEditorEntry entry in assets){
				BundleContentsEntry bundleEntry = new BundleContentsEntry();
				bundleEntry.name = entry.name;
				bundleEntry.asset = entry.GetAssetForPlatformOrInherited(plat.name, out bundleEntry.isInherited);
				bundle.assets.Add(bundleEntry);
			}
			EditorUtility.SetDirty(bundle);
		}
		EditorUtility.SetDirty(listing);
	}
	
	public static void BuildBundleForCurrentPlatforms(AssetBundleListing listing){
		var curPlats = Settings.GetPlatformsForCurrentBuildTarget(EditorUserBuildSettings.activeBuildTarget);
		foreach(BundlePlatform plat in curPlats){
			string path = Settings.bundleDirectoryRelativeToProjectFolder;
			DirectoryInfo di = new DirectoryInfo(path);
			path += "/" + listing.name + "_" + plat.name +".unity3d";
			if(!di.Exists)
				di.Create();
			BuildAssetBundleOptions babOpts = BuildAssetBundleOptions.CompleteAssets;
			if(listing.gatherDependencies)
				babOpts |= BuildAssetBundleOptions.CollectDependencies;
			if(!listing.compressed)
				babOpts |= BuildAssetBundleOptions.UncompressedAssetBundle;
			var files = listing.GetAssetsForPlatform(plat.name);
			var names = listing.GetNamesForPlatform(plat.name);
			BuildPipeline.BuildAssetBundleExplicitAssetNames(files.ToArray(),names.ToArray(), path, babOpts, plat.unityBuildTarget);
		}
	}
}

public class ListingEditorEntry{
	public string name = "";
	public Dictionary<string, Object> assets = new Dictionary<string, Object>(); //Platform -> Asset
	
	public Object GetAssetForPlatform(string platform){
		Object obj = null;
		assets.TryGetValue(platform, out obj);
		return obj;
	}
	
	public Object GetAssetForPlatformOrInherited(string platform, out bool isInherited){
		Object obj = null;
		assets.TryGetValue(platform, out obj);
		if(obj == null && platform != AssetBundleRuntimeSettings.DefaultPlatform){
			assets.TryGetValue(AssetBundleRuntimeSettings.DefaultPlatform, out obj);
			isInherited = true;
		}
		else{
			isInherited = false;
		}
		return obj;
	}
	
	public void Add(Object asset, string platform){
		assets[platform] = asset;
	}
}