using UnityEngine;
using System.Collections;
using System.IO;

public class AssetBundlePathProvider {

	public virtual string GetPath(){
		//Editors: Load out of assets directory
		if (	Application.platform == RuntimePlatform.OSXEditor
			|| Application.platform == RuntimePlatform.WindowsEditor){
            return "file://" + Path.Combine(Directory.GetCurrentDirectory(), "Bundles"); //TODO: Not a magic string
		}
		else if (Application.platform == RuntimePlatform.WindowsWebPlayer
				|| Application.platform == RuntimePlatform.OSXWebPlayer){
			return Application.streamingAssetsPath;
		}
		else if (Application.platform == RuntimePlatform.PS3 	//???
			|| Application.platform == RuntimePlatform.XBOX360  //???
			|| Application.platform == RuntimePlatform.WindowsPlayer
			|| Application.platform == RuntimePlatform.OSXPlayer){
            return "file://" + Application.streamingAssetsPath;
		}
		throw new System.Exception("Unsupported runtime platform " + Application.platform);
	}
}
