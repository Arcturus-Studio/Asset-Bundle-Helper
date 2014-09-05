using UnityEngine;
using UnityEditor;
using System.Collections;

public class TestNewPipeline : MonoBehaviour {

	[MenuItem("Test/Build")]
	static void Buildmap() {
		// Create the array of bundle build details.
		AssetBundleBuild[] altBuildMap = new AssetBundleBuild[1];
		altBuildMap[0].assetBundleName = "materialB";
		altBuildMap[0].assetNames = new string[]{"Assets/Test/MaterialB/Material.mat"};
		BuildPipeline.BuildAssetBundles("Bundles/", altBuildMap);
		
		AssetBundleBuild[] baseBuildMap = new AssetBundleBuild[4];
		baseBuildMap[0].assetBundleName = "scenes";
		baseBuildMap[0].assetNames = new string[]{"Assets/Test/TestScene.unity"};
		baseBuildMap[1].assetBundleName = "bundleCube";
		baseBuildMap[1].assetNames = new string[]{"Assets/Test/BundleCube/Cube.prefab"};
		baseBuildMap[2].assetBundleName = "bundleSphere";
		baseBuildMap[2].assetNames = new string[]{"Assets/Test/BundleSphere/Sphere.prefab"};
		baseBuildMap[3].assetBundleName = "materialA";
		baseBuildMap[3].assetNames = new string[]{"Assets/Test/MaterialA/Material.mat"};		
		BuildPipeline.BuildAssetBundles("Bundles/", baseBuildMap);
		
		
	}
}
