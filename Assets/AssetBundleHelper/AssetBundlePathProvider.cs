using UnityEngine;
using System.Collections;
using System.IO;

/*	Class responsible for providing the path to the directory where asset bundles are stored at runtime. */
public class AssetBundlePathProvider {
	private string cachedPath;
	
	public virtual string GetPath(){
		if(cachedPath != null){
			return cachedPath;
		}
		
		//According to the documentation, WWW is supposed to use a triple-slash file protocol on Windows
		string fileProtocol = UseTripleSlashFileProtocol ? "file:///" : "file://";
#if UNITY_EDITOR
		//Editors: Load out of assets directory
		if (Application.isEditor)
		{
			cachedPath = fileProtocol + Path.Combine(Directory.GetCurrentDirectory(), AssetBundleEditorSettings.GetInstance().bundleDirectoryRelativeToProjectFolder);            
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
