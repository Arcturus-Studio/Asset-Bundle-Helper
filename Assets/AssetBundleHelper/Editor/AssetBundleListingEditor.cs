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
		so.platforms[0].name = "Default";
		so.platforms[0].unityBuildTarget = BuildTarget.WebPlayer;
		DirectoryInfo di = new DirectoryInfo(Application.dataPath+"/AssetBundleHelper");
		if(!di.Exists){
			di.Create();
		}
		AssetDatabase.CreateAsset(so,path);
		AssetDatabase.SaveAssets();
		return so;
	}
	
	List<AssetBundleEntry> toRemove = new List<AssetBundleEntry>();
	public override void OnInspectorGUI() {
		serializedObject.Update();
		AssetBundleListing listing = target as AssetBundleListing;
		EditorGUIUtility.LookLikeControls();
		GUILayout.Label("Bundle Contents",EditorStyles.boldLabel);
		GUILayout.BeginVertical(GUI.skin.box);
		GUILayout.BeginHorizontal(EditorStyles.toolbar);
		GUILayout.Label("Name", GUILayout.MinWidth(100));
		GUILayout.FlexibleSpace();
		foreach(var plat in Settings.platforms){
			GUILayout.Label(new GUIContent(plat.name,plat.icon32),GUILayout.Height(14), GUILayout.Width(60));
		}
		GUILayout.Space(16);
		GUILayout.EndHorizontal();
		foreach(var entry in listing.assets){
			GUILayout.BeginHorizontal();
			GUILayout.Space(2);
			entry.name = GUILayout.TextField(entry.name, GUILayout.MinWidth(100));
			GUILayout.FlexibleSpace();
			foreach(var plat in Settings.platforms){
				Object o = null;
				Object d = null;
				bool usingDefault = false;

				d = entry.GetAssetForPlatform("Default");
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
						EditorUtility.SetDirty(target);
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
						EditorUtility.SetDirty(target);
					}
				}
			}
			if(GUILayout.Button("",Settings.deleteButtonStyle)){
				toRemove.Add(entry);
			}
			GUILayout.Space(2);
			GUILayout.EndHorizontal();
		}
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("",Settings.addButtonStyle)){
			listing.assets.Add(new AssetBundleEntry());
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		if(toRemove.Count > 0){
			listing.assets.RemoveAll((x) => toRemove.Contains(x));
			toRemove.Clear();
			EditorUtility.SetDirty(target);
		}
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
			var files = listing.GetListingForPlatform(plat.name);
			var names = listing.assets.ConvertAll<string>((x) => x.name).ToList();
			BuildPipeline.BuildAssetBundleExplicitAssetNames(files.ToArray(),names.ToArray(), path, babOpts, plat.unityBuildTarget);
		}
	}
	
}
