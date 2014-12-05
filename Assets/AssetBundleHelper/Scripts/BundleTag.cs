using UnityEngine;

/*	Class representing one configuration of a BundleTagGroup.
	Ideally, BundleTags should be named as members of the category described by the BundleTagGroup.
	BundleTags must have unique names.
*/
[System.Serializable]
public class BundleTag{
	public string name = "";
#if UNITY_EDITOR
	public Texture2D icon32; //Optional icon, used in inspectors for quick identification of tags. Target dimension 32x32.
#endif
	
	//The "No Tag" tag, representing the lack of any tag. This is used instead of null to make things easier
	//when handling tag combinations, where a set of zero tags
	//has one combination (the "no tag" combination) rather than an empty set, and a null would require
	//special processing.
	public static BundleTag NoTag{
		get{
			return noTag;
		}
	}
	private static BundleTag noTag = new BundleTag();
}