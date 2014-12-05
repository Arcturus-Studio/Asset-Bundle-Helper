using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

/* Specifies styles and graphics used in editors and inspectors. */
public class AssetBundleEditorUIResources : ScriptableObject {
	//Base path for all AssetBundleHelper-related project data to be stored
	public static string DirectoryPath{
		get{
			return AssetBundleEditorSettings.DirectoryPath;
		}
	}

	public static AssetBundleEditorUIResources GetInstance(){
		//Fetch existing instance if it exists
		var path = Path.Combine(DirectoryPath, "UIResources.asset");		
		var so = AssetDatabase.LoadMainAssetAtPath(path) as AssetBundleEditorUIResources;
		if(so){
			return so;
		}
		//If no existing instance, create new instance
		so = ScriptableObject.CreateInstance<AssetBundleEditorUIResources>();
		so.RestoreDefaultSettings();
		//Save asset
		DirectoryInfo di = new DirectoryInfo(DirectoryPath);
		if(!di.Exists){
			di.Create();
		}
		AssetDatabase.CreateAsset(so, path);
		AssetDatabase.SaveAssets();
		return so;
	}
	
	public void RestoreDefaultSettings(){
		deleteButtonStyle = CreateFixedSizeTextureStyle(GetEditorTexture("del.png"));
		addButtonStyle = CreateFixedSizeTextureStyle(GetEditorTexture("add.png"));
		
		uncheckedbox = GetEditorTexture("unchecked_checkbox.png");
		checkedBox = GetEditorTexture("checked_checkbox.png");
		outOfDate = GetEditorTexture("clock.png");
	}
	
	//GUI style definitions and asset links for use in editors/inspectors
	public GUIStyle deleteButtonStyle;
	public GUIStyle addButtonStyle;
	public Texture2D uncheckedbox, checkedBox, outOfDate;

	//Helper function for fetching a texture from the ABH editor textures directory
	private Texture2D GetEditorTexture(string texturePath){
		return AssetDatabase.LoadMainAssetAtPath(Path.Combine(DirectoryPath, "EditorTextures/" + texturePath)) as Texture2D;
	}
	
	private GUIStyle CreateFixedSizeTextureStyle(Texture2D tex){
		GUIStyle style = new GUIStyle();
		style.normal.background = tex;
		if(tex != null){
			style.fixedWidth = tex.width;
			style.fixedHeight = tex.height;
		}
		return style;
	}
}