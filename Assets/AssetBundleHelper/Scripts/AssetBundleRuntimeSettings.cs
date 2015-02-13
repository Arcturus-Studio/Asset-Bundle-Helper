using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//Pseudo-typedef
using TagTable = System.Collections.Generic.Dictionary<string, BundleTag>;

public class AssetBundleRuntimeSettings : ScriptableSingleton<AssetBundleRuntimeSettings> {
	//Set of currently active tags
	public static BundleTagSelection ActiveTags{
		get{
			List<BundleTag> tags = Instance.tagGroups.Select(x => GetActiveTagObject(x.name)).ToList(); //inits allTagsMask if necessary
			var activeTags = new BundleTagSelection(Instance.allTagsMask, tags);
			return activeTags;
		}
	}
	
	//Returns the currently active tag for the given tag group
	public static string GetActiveTag(string groupName){
		return GetActiveTagObject(groupName).name;
	}
	
	private static BundleTag GetActiveTagObject(string groupName){
		BundleTag activeTag = null;
		if(Instance._ActiveTags.TryGetValue(groupName, out activeTag)){
			return activeTag;
		}
		else{
			throw new System.ArgumentException("no tag group '" + groupName + "'", "groupName");
		}
	}
	
	//Sets the currently active tag name for the given tag group
	public static void SetActiveTag(string groupName, string activeTag){
		//Find group with matching name
		foreach(BundleTagGroup group in Instance.tagGroups){
			if(group.name == groupName){
				//Find tag with matching name
				foreach(BundleTag tag in group.tags){
					if(tag.name == activeTag){
						//Assign as active tag
						Instance._ActiveTags[groupName] = tag;
						return; //Bail out to avoid throwing
					}
				}
				//No tag with that name
				throw new System.ArgumentException("no tag '" + activeTag + "' in group " + groupName);
			}
		}
		//No group with that name
		throw new System.ArgumentException("no tag group '" + groupName + "'", "groupName");
	}
	
#if UNITY_EDITOR
	//Collection of all existing tag groups. Note that due to the way unity serializes data,
	//these tag groups must be seperate instances from the ones in AssetBundleEditorSettings.
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
	private TagTable _activeTags; //Tag group name -> active tag name
	
	private int allTagsMask;
	
	//Auto-initalizing activeTags accessor
	private TagTable _ActiveTags{
		get{
			if(_activeTags == null){
				InitDefaultTags();
			}
			return _activeTags;
		}
	}
	
	//Sets all tag groups to have the default tag be the active tag and configures allTagsMask
	private void InitDefaultTags(){
		allTagsMask = 0;
		_activeTags = new TagTable();
		foreach(BundleTagGroup tagGroup in tagGroups){
			allTagsMask |= (1 << _activeTags.Count);
			_activeTags[tagGroup.name] = tagGroup.tags[0];
		}
	}
}
