using UnityEngine;
using System.Collections;

public class AssetBundlePathProvider {

	public virtual string GetPath(){
		return Application.streamingAssetsPath;
	}
}
