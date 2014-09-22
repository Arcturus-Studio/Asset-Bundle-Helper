using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AssetBundleRuntimeSettings : ScriptableSingleton<AssetBundleRuntimeSettings> {
	
	public static bool FastPath{
		get{
			return Instance.fastPath;
		}
#if UNITY_EDITOR
		set{
			Instance.fastPath = value;
			EditorUtility.SetDirty(Instance);
		}
#endif
	}
	[SerializeField]
	protected bool fastPath = false;

	public static IEnumerable<string> ActiveTags{
		get{
			return Instance.tagGroups.Select(x => GetActiveTag(x.name));
		}
	}
	
	public static IEnumerable<string> MaskedActiveTags(int mask){
		int i = 0;
		foreach(string tag in ActiveTags){
			if((mask & (1 << i)) != 0){
				yield return tag;
			}
			i++;
		}
	}

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
	//This gets set when tag config in AssetBundleEditorSettings is changed
	public static BundleTagGroup[] TagGroups {
		set{
			Instance.tagGroups = value;
			EditorUtility.SetDirty(Instance);
		}
	}
	#endif
	[SerializeField] [HideInInspector]
	private BundleTagGroup[] tagGroups;
	[System.NonSerialized]
	private Dictionary<string, string> activeTags;
	
	private void InitDefaultTags(){
		activeTags = new Dictionary<string, string>();
		foreach(BundleTagGroup tagGroup in tagGroups){
			activeTags[tagGroup.name] = tagGroup.tags[0].name;
		}
	}
}
