using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[CustomEditor(typeof(AssetBundleListing))]
public class AssetBundleListingEditor : Editor {
	
	static AssetBundleHelperSettings settings;
	
	public static AssetBundleHelperSettings Settings{
		get{
			if(settings == null){
				settings = AssetBundleHelperSettings.GetInstance();
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
	
	List<ListingEditorEntry> assets;	
	List<ListingEditorEntry> toRemove = new List<ListingEditorEntry>();
	
	public void OnEnable(){
		UpdateListingEntries();
	}
	
	private void UpdateListingEntries(){
		assets = new List<ListingEditorEntry>();
		var assetsById = new Dictionary<int, ListingEditorEntry>();
		AssetBundleListing listing = target as AssetBundleListing;		
		
		//Create entries for existing AssetBundleContents for this tag set
		string defaultTagString = BuildTagString(listing.ActiveTagGroups.Select(x => x.tags.FirstOrDefault()));
		//Tag strings for combinations of active tags
		List<string> tagStrings = new List<string>(TagComboBuilder(listing.ActiveTagGroups, 0).Select(x => BuildTagString(x)));
		foreach(var pair in listing.assets.Where(x => tagStrings.Contains(x.tags))){
			AssetBundleContents contents = pair.Load();
			if(contents != null){
				foreach(var entry in contents.assets){
					if(!assetsById.ContainsKey(entry.id)){
						var newEntry = new ListingEditorEntry();
						newEntry.id = entry.id;
						newEntry.defaultTagString = defaultTagString;
						assetsById[entry.id] = newEntry;
						assets.Add(newEntry);
					}
					assetsById[entry.id].assets[contents.tags] = entry.isInherited ? null : entry.asset;
				}
			}
		}
	}
	
	private void AssignEntryIds(){
		for(int i = 0; i < assets.Count; i++){
			assets[i].id = i;
		}
	}
	
	public override void OnInspectorGUI() {
		serializedObject.Update();
		AssetBundleListing listing = target as AssetBundleListing;
		EditorGUIUtility.LookLikeControls();
		
		if(GUILayout.Button("DEBUG DUMP LISTINGEDITORENTRIES")){
			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < assets.Count; i++){				
				sb.AppendLine("Entry " + (i+1));
				sb.AppendLine("Default TagString: " + assets[i].defaultTagString);
				foreach(var kvp in assets[i].assets){
					sb.AppendLine(kvp.Key + "->" + kvp.Value);
				}
			}
			Debug.Log(sb.ToString());
		}
		
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
			UpdateListingEntries();
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
			if(entry.defaultTagString == null){
				Debug.LogWarning("null default tag string");
			}
			GUILayout.BeginHorizontal();
			GUILayout.Space(2);			
			GUILayout.BeginHorizontal();
			BundledAssetField(entry, entry.defaultTagString, wideLayoutParams); //Default/Key asset
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
						BundledAssetField(entry, BuildTagString(horizontalTags[i].Yield().Concat(verticalTags[j])), tagLayoutParams);						
					}
					else{
						GUILayout.Label("", tagLayoutParams);
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
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
			var newEntry = new ListingEditorEntry();
			newEntry.defaultTagString = BuildTagString(listing.ActiveTagGroups.Select(x => x.tags.FirstOrDefault()));
			assets.Add(newEntry);
			AssignEntryIds();
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
		System.Console.WriteLine("Updating bundle contents");
		var listing = target as AssetBundleListing;
		int tagSetCount = 0;
		foreach(var tagSet in TagComboBuilder(listing.ActiveTagGroups, 0)){
			System.Console.WriteLine("UPD: Processing tag set " + (++tagSetCount) );
			string tagString = BuildTagString(tagSet);
			System.Console.WriteLine("UPD: " + tagString);
			AssetBundleContents bundle = listing.GetBundleContentsForTags(tagString);
			bundle.assets.Clear();
			foreach(ListingEditorEntry entry in assets){
				bool isInherited;
				Object obj = entry.GetAssetForTagsOrInherited(tagString, out isInherited);
				Object defaultObj = entry.GetDefaultAsset();
				if(obj != null){
					BundleContentsEntry bundleEntry = new BundleContentsEntry();
					bundleEntry.id = entry.id;
					bundleEntry.asset = obj;
					bundleEntry.name = defaultObj == null ? "" : defaultObj.name;
					bundleEntry.isInherited = isInherited;
					bundle.assets.Add(bundleEntry);
					System.Console.WriteLine("UPD: Added asset " + bundleEntry.id + ": " + bundleEntry.asset + (isInherited ? " (inherited)" : " (override)"));
				}
			}
			EditorUtility.SetDirty(bundle);
		}
		EditorUtility.SetDirty(listing);
	}
	
	//Helper function for building all combinations of tag strings
	private IEnumerable<List<BundleTag>> TagComboBuilder(List<BundleTagGroup> list, int index){
		//End condition 1: reached list end, yield list for each tag
		if(index == list.Count-1){
			foreach(BundleTag tag in list[index].tags){
				var newList = new List<BundleTag>(1);
				newList.Add(tag);
				yield return newList;
			}
		}
		//End condition 2: Past end of list (most likely due to list being empty): Return list containing "No Tag"
		else if (index >= list.Count){
			var newList = new List<BundleTag>();
			newList.Add(BundleTag.NoTag);
			yield return newList;
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
	
	private string BuildTagString(IEnumerable<BundleTag> tags){		
		if(tags != null){
			StringBuilder result = new StringBuilder();
			foreach(BundleTag tag in tags){
				if(tag == BundleTag.NoTag){
					continue;
				}
				if(result.Length > 0){
					result.Append(".");
				}
				result.Append(tag.name);
			}
			return result.ToString();
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
	
	private void BundledAssetField(ListingEditorEntry entry, string tagString, params GUILayoutOption[] layout){		
		Object targetObj = null;
		Object defaultObj = null;
		bool usingDefault = false;
		
		targetObj = entry.GetAssetForTags(tagString);
		defaultObj = entry.GetAssetForTags(entry.defaultTagString);

		if(targetObj == null && defaultObj != null){
			usingDefault = true;
		}
		if(usingDefault){
			GUI.backgroundColor = Color.grey;
			Object newObj = EditorGUILayout.ObjectField(defaultObj, typeof(Object), false, layout);
			GUI.backgroundColor = Color.white;
			if(newObj != defaultObj){
				entry.Add(newObj, tagString);
				UpdateBundleContents();
			}
		}
		else{
			if(defaultObj != null && targetObj.GetType() != defaultObj.GetType()){
				GUI.backgroundColor = Color.yellow;
			}
			Object newObj = EditorGUILayout.ObjectField(targetObj, typeof(Object), false, layout);
			GUI.backgroundColor = Color.white;
			if(newObj != targetObj){
				if(newObj == defaultObj){
					entry.Add(null, tagString);
				}
				else{		
					entry.Add(newObj, tagString);
				}
				UpdateBundleContents();
			}
		}
	}
}

public class ListingEditorEntry{
	public int id;
	public string defaultTagString;
	public Dictionary<string, Object> assets = new Dictionary<string, Object>(); //tag string -> Asset
	
	public Object GetAssetForTags(string tagString){
		Object obj = null;
		assets.TryGetValue(tagString, out obj);
		return obj;
	}
	
	public Object GetDefaultAsset(){
		Object obj = null;
		assets.TryGetValue(defaultTagString, out obj);
		return obj;
	}
	
	public Object GetAssetForTagsOrInherited(string tagString, out bool isInherited){
		Object obj = null;
		assets.TryGetValue(tagString, out obj);
		if(obj == null && !string.IsNullOrEmpty(tagString)){
			assets.TryGetValue(defaultTagString, out obj);
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