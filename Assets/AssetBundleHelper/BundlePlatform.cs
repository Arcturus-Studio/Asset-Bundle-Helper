using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class BundlePlatform : BundleTag{
#if UNITY_EDITOR
	public BuildTarget unityBuildTarget;
#endif
}