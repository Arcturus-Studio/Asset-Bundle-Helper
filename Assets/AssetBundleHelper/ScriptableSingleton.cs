﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using System.Collections;

/** Utility class for handling singleton ScriptableObjects for data management. The objects are automatically created
	when first used in the editor and are loaded from Resources so they can be freely accessed from script at any time.
	The type parameter restriction is a bit wierd. Derived class definitions look like Foo : ScriptableSingleton<Foo>
*/
public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T>{
	
	private static string FileName{
		get{
			return typeof(T).Name;
		}
	}

#if UNITY_EDITOR
	//Path to asset
	private static string AssetPath{
		get{
			return Path.Combine(Path.Combine(AssetBundleEditorSettings.DirectoryPath, "Resources"), FileName + ".asset");
		}
	}
#endif
	
	//Path for loading from Resources.Load()
	private static string ResourcePath{
		get{
			return FileName;
		}
	}

	//Get the instance of this singleton. If it does not exist, it will be created.
	public static T Instance{
		get{
			if(cachedInstance == null){
				cachedInstance = Resources.Load(ResourcePath) as T;
			}
#if UNITY_EDITOR
			if(cachedInstance == null){
				cachedInstance = CreateAndSave();
			}
#endif
			if(cachedInstance == null){
				Debug.LogWarning("No instance of " + FileName + " found, using default values");
				cachedInstance = ScriptableObject.CreateInstance<T>();
				cachedInstance.OnCreate();
			}
			
			return cachedInstance;
		}
	}	
	private static T cachedInstance;

#if UNITY_EDITOR
	protected static T CreateAndSave(){
		T instance = ScriptableObject.CreateInstance<T>();
		instance.OnCreate();
		//Saving during Awake() will crash Unity, delay saving until next editor frame
		if(EditorApplication.isPlayingOrWillChangePlaymode){
			EditorApplication.delayCall += () => SaveAsset(instance);
		}
		else{
			SaveAsset(instance);
		}
		return instance;
	}

	private static void SaveAsset(T obj){
		string dirName = Path.GetDirectoryName(AssetPath);
		if(!Directory.Exists(dirName)){
			Directory.CreateDirectory(dirName);
		}
		AssetDatabase.CreateAsset(obj, AssetPath);
		AssetDatabase.SaveAssets();
		Debug.Log("Saved " + FileName + " instance");
	}
#endif

	protected virtual void OnCreate(){
		// Do setup particular to your class here
	}
}