/*	Class representing a category of possible configurations for a particular aspect.
	For example, "language", "resolution", and "effectsQuality" could all be BundleTagGroups.
	Each tag group has a list of BundleTags which specify the possible members of the category.
	For example, the "effectsQuality" tag group might have "highFX", "mediumFX", and "lowFX" tags.
*/
[System.Serializable]
public class BundleTagGroup{
	public string name;
	public BundleTag[] tags;
}