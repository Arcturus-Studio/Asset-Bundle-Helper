using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/*	Custom inspector for AssetBundleListing */
[CustomEditor(typeof(AssetBundleListing))]
public class AssetBundleListingEditor : Editor {
	
	//Cached (and short form) version of AssetBundleEditorSettings instance
	public static AssetBundleEditorSettings Settings{
		get{
			if(settings == null){
				settings = AssetBundleEditorSettings.GetInstance();
			}
			return settings;
		}
	}
	static AssetBundleEditorSettings settings;
	
	public static AssetBundleEditorUIResources UIResources{
		get{
			if(uiResources == null){
				uiResources = AssetBundleEditorUIResources.GetInstance();
			}
			return uiResources;
		}
	}
	static AssetBundleEditorUIResources uiResources;
	
	//Returns the path of the directory of the selected asset, or the path of the selected directory if the selection is a directory.
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
	
	//Creates a new AssetBundleListing asset next to the selected asset or in the selected directory.
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
		string defaultTagString = BundleTagUtils.BuildTagString(listing.SelectedTagGroups.Select(x => x.tags.FirstOrDefault()));
		//Tag strings for combinations of active tags
		List<string> tagStrings = new List<string>(BundleTagUtils.TagCombinations(listing.SelectedTagGroups, 0).Select(x => BundleTagUtils.BuildTagString(x)));
		foreach(var weakRef in listing.contentWeakRefs.Where(x => tagStrings.Contains(x.tags))){
			AssetBundleContents contents = weakRef.Load();
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
		
		//Tag group filter options
		string[] tagMaskOptions = new string[Settings.tagGroups.Length+1];
		tagMaskOptions[0] = "Platform";
		for(int i = 0; i < Settings.tagGroups.Length; i++){
			tagMaskOptions[i+1] = Settings.tagGroups[i].name;
		}
		
		//Header and mask field
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
		
		List<BundleTagGroup> tagGroups = Settings.MaskToTagGroups(listing.tagMask);
		//First tag group runs horizontally
		IList<BundleTag> horizontalTags;
		if(tagGroups.Count > 0){
			horizontalTags = tagGroups[0].tags;
		}
		else{
			horizontalTags = new BundleTag[]{BundleTag.NoTag};
		}
		
		//All other tag groups run vertically
		List<List<BundleTag>> verticalTags = new List<List<BundleTag>>();
		if(tagGroups.Count > 1){
			verticalTags.AddRange(BundleTagUtils.TagCombinations(tagGroups, 1));
		}
		else{
			var noTagList = new List<BundleTag>();
			noTagList.Add(BundleTag.NoTag);
			verticalTags.Add(noTagList);
		}
		
		//Calc layout configuration
		bool showHorizontalTags = horizontalTags[0] != BundleTag.NoTag;
		bool showVerticalTags = verticalTags[0][0] != BundleTag.NoTag;
		bool extraWideLeftColumn = !showHorizontalTags || !showVerticalTags;
		const int normalColumnWidth = 60;
		const int wideColumnWidth = 100;
		const int extraWideColumnWidth = 160; //For best results, should be aprox normalColumnWidth + wideColumnWidth
		GUILayoutOption[] tagLayoutParams = new GUILayoutOption[]{GUILayout.Height(16), GUILayout.Width(normalColumnWidth)};
		GUILayoutOption[] wideLayoutParams = new GUILayoutOption[]{GUILayout.Height(16), GUILayout.Width(extraWideLeftColumn ? extraWideColumnWidth : wideColumnWidth)};		
		GUILayout.BeginVertical(GUI.skin.box);
		
		//Header
		GUILayout.BeginHorizontal(EditorStyles.toolbar);
		//Key asset header
		//If we don't have vertical tags, include the first horizontal tag in this label
		if(showHorizontalTags && !showVerticalTags){
			GUILayout.BeginHorizontal(wideLayoutParams);
			GUILayout.Label("Asset (");
			GUILayout.Label(new GUIContent(horizontalTags[0].name, horizontalTags[0].icon32), GUILayout.Height(16));
			GUILayout.Label(")");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		else{
			GUILayout.Label("Asset", wideLayoutParams);	
		}
		GUILayout.Space(normalColumnWidth);
		//Draw horizontal tag labels
		//If we don't have vertical tags, skip the first horizontal tag since it's already been drawn
		for(int i = (showVerticalTags ? 0 : 1); i < horizontalTags.Count; i++){
			GUILayout.Label(new GUIContent(horizontalTags[i].name, horizontalTags[i].icon32), tagLayoutParams);
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		//Asset listing
		foreach(var entry in assets){
			GUILayout.BeginHorizontal();
			GUILayout.Space(2);			
			GUILayout.BeginHorizontal();
			BundledAssetField(entry, entry.defaultTagString, wideLayoutParams); //Default/Key asset
			if(showVerticalTags){
				GUILayout.BeginVertical();
				foreach(List<BundleTag> tags in verticalTags){
					if(tags.Count == 1){
						GUILayout.Label(new GUIContent(tags[0].name, tags[0].icon32), tagLayoutParams);
					}
					else{
						GUILayout.Label(BundleTagUtils.BuildTagString(tags), tagLayoutParams);
					}
				}
				GUILayout.EndVertical();
			}
			for(int i = 0; i < horizontalTags.Count; i++){
				GUILayout.BeginVertical();
				for(int j = 0; j < verticalTags.Count; j++){
					if(i > 0 || j > 0){ //Skip "all defaults" field, since that's the key asset
						BundledAssetField(entry, BundleTagUtils.BuildTagString(horizontalTags[i].Yield().Concat(verticalTags[j])), tagLayoutParams);						
					}
					else{
						GUILayout.Label("", tagLayoutParams);
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			//Delete entry button
			if(GUILayout.Button("", UIResources.deleteButtonStyle)){
				toRemove.Add(entry);
			}
			GUILayout.Space(2);
			GUILayout.EndHorizontal();
		}
		//New entry button
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("", UIResources.addButtonStyle)){
			var newEntry = new ListingEditorEntry();
			newEntry.defaultTagString = BundleTagUtils.BuildTagString(listing.SelectedTagGroups.Select(x => x.tags.FirstOrDefault()));
			assets.Add(newEntry);
			AssignEntryIds();
			UpdateBundleContents();
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical(); //End of Box
		
		//Dependency list
		GUIStyle miniLabelWordWrap = new GUIStyle(EditorStyles.miniLabel);
		miniLabelWordWrap.wordWrap = true;
		miniLabelWordWrap.margin = new RectOffset();
		miniLabelWordWrap.padding = new RectOffset(0, 0, -4, -4);
		if(listing.dependencies.Count > 0){
			GUILayout.Label("Bundle Dependencies: " + string.Join(", ", listing.dependencies.Select(x => !x ? "MISSING BUNDLE LISTING" : x.name).ToArray()), miniLabelWordWrap);
		}
		else{
			GUILayout.Label("Bundle Dependencies: None", miniLabelWordWrap);			
		}
		GUILayout.Label("(As of last build)", EditorStyles.miniLabel);
		
		//Handle removed entries
		if(toRemove.Count > 0){
			assets.RemoveAll((x) => toRemove.Contains(x));
			toRemove.Clear();
			UpdateBundleContents();
		}
	}
	
	protected void UpdateBundleContents(){
		System.Console.WriteLine("Updating bundle contents");
		var listing = target as AssetBundleListing;
		int tagSetCount = 0;
		foreach(var tagSet in BundleTagUtils.TagCombinations(listing.SelectedTagGroups, 0)){
			System.Console.WriteLine("UPD: Processing tag set " + (++tagSetCount) );
			string tagString = BundleTagUtils.BuildTagString(tagSet);
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