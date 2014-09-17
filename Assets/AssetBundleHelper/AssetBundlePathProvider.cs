using UnityEngine;
using System.Collections;
using System.IO;

public class AssetBundlePathProvider {
	private string cachedPath;
	
	public virtual string GetPath(){
		if(cachedPath != null){
			return cachedPath;
		}
		
		string fileProtocol = UseTripleSlashFileProtocol ? "file:///" : "file://";
		#if UNITY_EDITOR
		//Editors: Load out of assets directory
		if (Application.isEditor)
		{
			cachedPath = fileProtocol + Path.Combine(Directory.GetCurrentDirectory(), AssetBundleHelperSettings.GetInstance().bundleDirectoryRelativeToProjectFolder);            
		}
		else
		#endif
		if (Application.isWebPlayer){
			cachedPath = Application.streamingAssetsPath;
		}
		else {
            cachedPath = fileProtocol + Application.streamingAssetsPath;
		}
		
		return cachedPath;
	}
	
	private bool UseTripleSlashFileProtocol{
		get{
			return Application.platform == RuntimePlatform.WindowsPlayer
			|| Application.platform == RuntimePlatform.WindowsWebPlayer
			|| Application.platform == RuntimePlatform.WindowsEditor
			|| Application.platform == RuntimePlatform.WSAPlayerX86
			|| Application.platform == RuntimePlatform.WSAPlayerX64
			|| Application.platform == RuntimePlatform.WSAPlayerARM;
			//TODO: Also Windows phone?
		}
	}
}
