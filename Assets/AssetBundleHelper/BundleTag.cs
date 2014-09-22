using UnityEngine;

[System.Serializable]
public class BundleTag{
	public string name = "";
#if UNITY_EDITOR
	public Texture2D icon32;
#endif
	
	public static BundleTag NoTag{
		get{
			return noTag;
		}
	}
	private static BundleTag noTag = new BundleTag();
}