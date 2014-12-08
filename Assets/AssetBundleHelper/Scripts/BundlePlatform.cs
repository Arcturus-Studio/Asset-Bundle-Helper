using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*	A BundleTag with additional info of what build target the tag is for */
[System.Serializable]
public class BundlePlatform : BundleTag{
#if UNITY_EDITOR
	public BuildTarget unityBuildTarget;
#endif
}