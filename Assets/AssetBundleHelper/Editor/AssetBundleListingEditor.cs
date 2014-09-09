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
		var assetsByKeyAsset = new Dictionary<Object, ListingEditorEntry>();
		AssetBundleListing listing = target as AssetBundleListing;
		
		foreach(var pair in listing.assets){
			AssetBundleContents contents = pair.Load();
			if(contents != null){
				foreach(var entry in contents.assets){
					if(!assetsByKeyAsset.ContainsKey(entry.asset)){
						var newEntry = new ListingEditorEntry();
						newEntry.asset = entry.asset;						
						assetsByKeyAsset[entry.asset] = newEntry;
						assets.Add(newEntry);
					}
					assetsByKeyAsset[entry.asset].assets[contents.tags] = entry.isInherited ? null : entry.asset;
				}
			}
		}
	}
	
	public override void OnInspectorGUI() {
		serializedObject.Update();
		AssetBundleListing listing = target as AssetBundleListing;
		EditorGUIUtility.LookLikeControls();
		
		//TODO: Move this someplace else
		string[] tagMaskOptions = new string[Settings.tagGroups.Length+1];
		tagMaskOptions[0] = "Platform";
		for(int i = 0; i < Settings.tagGroups.Length; i++){
			tagMaskOptions[i+1] = Settings.tagGroups[i].name;
		}
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Bundle Contents",EditorStyles.boldLabel);
		GUILayout.FlexibleSpace();		
		int newMask = EditorGUILayout.MaskField("Tags", listing.tagMask, tagMaskOptions);
		if(newMask != listing.tagMask){
			listing.tagMask = newMask;
			UpdateBundleContents();
		}
		GUILayout.EndHorizontal();
		
		GUILayout.Label("[DBG: Tagstring = " + Settings.MaskToTagString(listing.tagMask) + "]", EditorStyles.miniLabel);
		List<BundleTagGroup> tagGroups = Settings.MaskToTagGroups(listing.tagMask);
		
		IList<BundleTag> horizontalTags;
		if(tagGroups.Count > 0){
			horizontalTags = tagGroups[0].tags;
		}
		else{
			horizontalTags = new BundleTag[]{BundleTag.NoTag};
		}
		
		List<List<BundleTag>> verticalTags = new List<List<BundleTag>>();
		if(tagGroups.Count > 1){
			verticalTags.AddRange(TagComboBuilder(tagGroups, 1));
		}
		else{
			var noTagList = new List<BundleTag>();
			noTagList.Add(BundleTag.NoTag);
			verticalTags.Add(noTagList);
		}
		
		GUILayout.BeginVertical(GUI.skin.box);
		//Header
		GUILayout.BeginHorizontal(EditorStyles.toolbar);
		const int normalColumnWidth = 60;
		const int wideColumnWidth = 80;
		GUILayoutOption[] tagLayoutParams = new GUILayoutOption[]{GUILayout.Height(16), GUILayout.Width(normalColumnWidth)};
		GUILayoutOption[] wideLayoutParams = new GUILayoutOption[]{GUILayout.Height(16), GUILayout.Width(wideColumnWidth)};
		GUILayout.Label("Asset", wideLayoutParams);	
		GUILayout.Space(normalColumnWidth);
		foreach(var tag in horizontalTags){
			GUILayout.Label(new GUIContent(tag.name, tag.icon32), tagLayoutParams);
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		//Asset listing
		foreach(var entry in assets){
			GUILayout.BeginHorizontal();
			GUILayout.Space(2);
			//Default/Key asset
			GUILayout.BeginHorizontal();			
			EditorGUILayout.ObjectField(null, typeof(Object), false, wideLayoutParams);
			GUILayout.BeginVertical();
			foreach(List<BundleTag> tags in verticalTags){
				if(tags.Count == 1){
					GUILayout.Label(new GUIContent(tags[0].name, tags[0].icon32), tagLayoutParams);
				}
				else{
					GUILayout.Label(BuildTagString(tags), tagLayoutParams);
				}
			}
			GUILayout.EndVertical();
			for(int i = 0; i < horizontalTags.Count; i++){
				GUILayout.BeginVertical();
				for(int j = 0; j < verticalTags.Count; j++){
					if(i > 0 || j > 0){
						EditorGUILayout.ObjectField(null, typeof(Object), false, tagLayoutParams);
					}
					else{
						GUILayout.Label("", tagLayoutParams);
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
			/*
			string name = GUILayout.TextField(entry.name, GUILayout.MinWidth(100));
			if(entry.name != name){
				entry.name = name;
				UpdateBundleContents();
			}			
			GUILayout.FlexibleSpace();
			actual object fields
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
			*/
			GUILayout.FlexibleSpace();
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
		Debug.LogWarning("Did not update bundle contents, fix this");
		/*
		foreach(var plat in Settings.platforms){
			AssetBundleContents bundle = listing.GetBundleForTags(plat.name);
			bundle.assets.Clear();
			foreach(ListingEditorEntry entry in assets){
				BundleContentsEntry bundleEntry = new BundleContentsEntry();
				bundleEntry.name = entry.name;
				bundleEntry.asset = entry.GetAssetForPlatformOrInherited(plat.name, out bundleEntry.isInherited);
				bundle.assets.Add(bundleEntry);
			}
			EditorUtility.SetDirty(bundle);
		}
		*/
		EditorUtility.SetDirty(listing);
	}
	
	//Helper function for building all combinations of tag strings
	private IEnumerable<List<BundleTag>> TagComboBuilder(List<BundleTagGroup> list, int index){
		//End condition: reached list end, yield list for each tag
		if(index == list.Count-1){
			foreach(BundleTag tag in list[index].tags){
				var newList = new List<BundleTag>(1);
				newList.Add(tag);
				yield return newList;
			}
		}
		//Recurse: Yield each tag followed by all combinations of later tags
		else{
			foreach(List<BundleTag> suffix in TagComboBuilder(list, index+1)){
				foreach(BundleTag tag in list[index].tags){
					var newList = new List<BundleTag>(suffix.Count + 1);
					newList.Add(tag);
					newList.AddRange(suffix);
					yield return newList;
				}
			}
		}
	}
	
	private string BuildTagString(List<BundleTag> list, int index = 0){
		if(index == list.Count - 1){
			return list[index].name;
		}
		else if(index < list.Count){
			return list[index].name + "." + BuildTagString(list, index+1);
		}
		else{
			return "";
		}
	}
	
	public static void BuildBundleForCurrentPlatforms(AssetBundleListing listing){
		Debug.LogWarning("TODO: Build bundle for current platforms");
		/*
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
		*/
	}
}

public class ListingEditorEntry{
	public Object asset;
	public Dictionary<string, Object> assets = new Dictionary<string, Object>(); //tag string -> Asset
	
	public Object GetAssetForTags(string tagString){
		Object obj = null;
		assets.TryGetValue(tagString, out obj);
		return obj;
	}
	
	public Object GetAssetForTagsOrInherited(string tagString, out bool isInherited){
		Object obj = null;
		assets.TryGetValue(tagString, out obj);
		if(obj == null && !string.IsNullOrEmpty(tagString)){
			assets.TryGetValue("", out obj);
			isInherited = true;
		}
		else{
			isInherited = false;
		}
		return obj;
	}
	
	public void Add(Object asset, string tagString){
		assets[tagString] = asset;
	}
}