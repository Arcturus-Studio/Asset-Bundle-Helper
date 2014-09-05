using UnityEngine;
using UnityEditor;
using System.Collections;

public class TestNewPipeline : MonoBehaviour {

	[MenuItem("Test/Build")]
	static void Buildmap() {
		// Create the array of bundle build details.
		AssetBundleBuild[] baseBuildMap = new AssetBundleBuild[2];

		baseBuildMap[0].assetBundleName = "scenes";
		baseBuildMap[0].assetNames = new string[]{"Assets/Test/TestScene.unity"};
		baseBuildMap[1].assetBundleName = "material.default";
		baseBuildMap[1].assetNames = new string[]{"Assets/Test/Material Default/Material.mat"};
		
		AssetBundleBuild[] androidBuildMap = new AssetBundleBuild[1];		
		androidBuildMap[0].assetBundleName = "material.android";
		androidBuildMap[0].assetNames = new string[]{"Assets/Test/Material iOS/Material.mat"};
		
		AssetBundleBuild[] iosBuildMap = new AssetBundleBuild[1];		
		iosBuildMap[0].assetBundleName = "material.ios";
		iosBuildMap[0].assetNames = new string[]{"Assets/Test/Material iOS/Material.mat"};

		BuildPipeline.BuildAssetBundles("Bundles/", baseBuildMap);
		BuildPipeline.BuildAssetBundles("Bundles/", androidBuildMap);
		BuildPipeline.BuildAssetBundles("Bundles/", iosBuildMap);
	}
}
