using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AssetBundleRuntimeSettings : ScriptableSingleton<AssetBundleRuntimeSettings> {
	//Set of currently active tags
	public static IEnumerable<string> ActiveTags{
		get{
			return Instance.tagGroups.Select(x => GetActiveTag(x.name));
		}
	}
	
	//Set of currently active tags, filtered by the given bitmask
	public static IEnumerable<string> MaskedActiveTags(int mask){
		int i = 0;
		foreach(string tag in ActiveTags){
			if((mask & (1 << i)) != 0){
				yield return tag;
			}
			i++;
		}
	}

	//Returns the currently active tag name for the given tag group
	public static string GetActiveTag(string groupName){
		if(Instance.activeTags == null){
			Instance.InitDefaultTags();
		}
		string activeTag = null;
		if(Instance.activeTags.TryGetValue(groupName, out activeTag)){
			return activeTag;
		}
		else{
			throw new System.ArgumentException("no tag group '" + groupName + "'", "groupName");
		}
	}
	
	//Sets the currently active tag name for the given tag group
	public static void SetActiveTag(string groupName, string activeTag){
		if(Instance.activeTags == null){
			Instance.InitDefaultTags();
		}
		if(Instance.activeTags.ContainsKey(groupName)){
			Instance.activeTags[groupName] = activeTag;
		}
		else{
			throw new System.ArgumentException("no tag group '" + groupName + "'", "groupName");
		}
	}
	
#if UNITY_EDITOR
	//Collection of all existing tag groups. Note that due to the way unity serializes data,
	//these tag groups are seperate instances from the ones in AssetBundleEditorSettings.
	//This gets set automatically when tag config in AssetBundleEditorSettings is changed.
	public static BundleTagGroup[] TagGroups {
		set{
			Instance.tagGroups = value;
			EditorUtility.SetDirty(Instance);
		}
	}
#endif
	[SerializeField] [HideInInspector]
	private BundleTagGroup[] tagGroups; //backing field
	
	[System.NonSerialized]
	private Dictionary<string, string> activeTags; //Tag group name -> active tag name
	
	//Sets all tag groups to have the default tag be the active tag.
	private void InitDefaultTags(){
		activeTags = new Dictionary<string, string>();
		foreach(BundleTagGroup tagGroup in tagGroups){
			activeTags[tagGroup.name] = tagGroup.tags[0].name;
		}
	}
}
