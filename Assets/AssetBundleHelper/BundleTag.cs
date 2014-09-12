using UnityEngine;

[System.Serializable]
public class BundleTag{
	public string name = "";
	public Texture2D icon32;
	
	public static BundleTag NoTag{
		get{
			return noTag;
		}
	}
	private static BundleTag noTag = new BundleTag();
}