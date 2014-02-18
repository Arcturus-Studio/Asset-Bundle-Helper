using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class AssetBundleRuntimeSettings : ScriptableSingleton<AssetBundleRuntimeSettings> {

	public static string DefaultPlatform{
		get{
			return Instance.defaultPlatform;
		}
		set{
			Instance.defaultPlatform = value;
#if UNITY_EDITOR
			EditorUtility.SetDirty(Instance);
#endif
		}
	}
	[SerializeField]
	protected string defaultPlatform = "Default";
	
	public static bool Hotload{
		get{
			return Instance.hotload;
		}
#if UNITY_EDITOR
		set{
			Instance.hotload = value;
			EditorUtility.SetDirty(Instance);
		}
#endif
	}
	[SerializeField]
	protected bool hotload = false;

}
