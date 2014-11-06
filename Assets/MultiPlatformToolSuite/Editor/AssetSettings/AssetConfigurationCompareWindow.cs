using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class AssetConfigurationCompareWindow : EditorWindow {
	
	enum ComparisonMode {ShowAll, ShowSame, ShowExclusive, ShowDifferent};
	ComparisonMode comparisonMode = ComparisonMode.ShowAll;
	
	enum AssetComparisonStates {Same, Different, Exclusive};
	
	AssetSettingsWindow assetSettings;
	
	int configurationAIndex;
	int configurationBIndex;
	
	Vector2 configurationAScrollPosition;
	Vector2 configurationBScrollPosition;
	
	//Styles
	[SerializeField] GUIStyle edgeBoxStyle;
	[SerializeField] Texture2D edgeBoxTexture;
	[SerializeField] GUIStyle assetButtonStyle;
	[SerializeField] GUIStyle italicLabelStyle;
	
	public void AssignAssetSettingsWindow(AssetSettingsWindow assetSettings) {
		this.assetSettings = assetSettings;
	}
	
	void OnEnable() {
		
		GetAssetPaths();
		
		//Create GUIStyles
		edgeBoxStyle = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).box);
//		edgeBoxTexture = CreateBoxEdgeTexture(2, 2, edgeColor, true, true);
		edgeBoxStyle.normal.background = edgeBoxTexture;
		edgeBoxStyle.margin = new RectOffset(0,0,0,0);
		edgeBoxStyle.padding = new RectOffset(0,0,0,0);
		edgeBoxStyle.border = new RectOffset(0,0,0,0);
		
		assetButtonStyle = new GUIStyle();
		assetButtonStyle.normal.background = null;
		assetButtonStyle.normal.textColor = Color.black;
		
		italicLabelStyle = new GUIStyle();
		italicLabelStyle.fontStyle = FontStyle.Italic;
		italicLabelStyle.normal.textColor = Color.black;
	}
	
	void GetAssetPaths()
	{
		string path = string.Empty;
		int index;
		foreach(string assetPath in AssetDatabase.GetAllAssetPaths()) 
		{
			index = assetPath.IndexOf("MultiPlatformToolSuite");
			if(index >= 0)
			{
				path = assetPath.Substring(0, index);
				break;		
			}
		}
		string editorPath = path + "MultiPlatformToolSuite" + Path.DirectorySeparatorChar + "Editor" + Path.DirectorySeparatorChar;
		path = editorPath + "Textures" + Path.DirectorySeparatorChar;
		
		edgeBoxTexture = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "edgeBoxTexture.tga", typeof(Texture2D));
	}

	void OnGUI() {
		if(assetSettings == null) {
			GUILayout.Label("AssetSettings window could not be found.");
		} else {
			GUILayout.BeginHorizontal();
				//Configuration A
				GUILayout.BeginVertical();
					DrawConfiguration(ref configurationAIndex, ref configurationBIndex, ref configurationAScrollPosition);
				GUILayout.EndVertical();
				
				//Separator
				GUILayout.Box(string.Empty, edgeBoxStyle, GUILayout.Width(1), GUILayout.MaxWidth(1), GUILayout.ExpandHeight(true));
			
				//Configuration B
				GUILayout.BeginVertical();
					DrawConfiguration(ref configurationBIndex, ref configurationAIndex, ref configurationBScrollPosition);
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			
			GUILayout.Space(5);
			
			//Comparison mode
			comparisonMode = (ComparisonMode) EditorGUILayout.EnumPopup("Mode: ", comparisonMode, GUILayout.MaxWidth(300));
		}
	}
	
	void DrawConfiguration(ref int thisConfigurationIndex, ref int otherConfigurationIndex, ref Vector2 scrollPosition) {
		thisConfigurationIndex = EditorGUILayout.Popup("Configuration A:", thisConfigurationIndex, assetSettings.configurationDropdownStrings, GUILayout.MaxWidth(300));
		
		//Is this configuration index out of bounds?
		if(thisConfigurationIndex < 0 || thisConfigurationIndex >= assetSettings.configurations.Count) {
			GUILayout.Label("Can't find this configuration at index: " + thisConfigurationIndex);
			return;
		}
		
		//Is other configuration index out of bounds?
		if(otherConfigurationIndex < 0 || otherConfigurationIndex >= assetSettings.configurations.Count) {
			GUILayout.Label("Can't find other configuration at index: " + otherConfigurationIndex);
			return;
		}
		
		AssetConfiguration currentConfiguration = assetSettings.configurations[thisConfigurationIndex];
		AssetConfiguration otherConfiguration = assetSettings.configurations[otherConfigurationIndex];
		
		//Draw configuration according to the comparison rule
		if(currentConfiguration != null) {
			scrollPosition = GUILayout.BeginScrollView(scrollPosition);
				GUILayout.BeginHorizontal();
					GUILayout.Space(3);
					GUILayout.BeginVertical();
						for(int i=0; i<currentConfiguration.assets.Count; i++) {
							Asset asset = currentConfiguration.assets[i];
	
							Asset otherAsset = FindAssetInConfiguration(asset, otherConfiguration);
							AssetComparisonStates comparisonState;
			
							if(otherAsset == null) {
								comparisonState = AssetComparisonStates.Exclusive;
							} else {
								if(asset.Equals(otherAsset)) {
									comparisonState = AssetComparisonStates.Same;
								} else {
									comparisonState = AssetComparisonStates.Different;
								}
							}
				
							if(comparisonMode != ComparisonMode.ShowAll &&
				   				(
				    			(comparisonMode == ComparisonMode.ShowExclusive && comparisonState != AssetComparisonStates.Exclusive) || 
				    			(comparisonMode == ComparisonMode.ShowDifferent && comparisonState != AssetComparisonStates.Different) ||
				    			(comparisonMode == ComparisonMode.ShowSame && comparisonState != AssetComparisonStates.Same)
				   				)) {
								continue;
							}
				
							GUILayout.BeginHorizontal();
								if(comparisonState == AssetComparisonStates.Different) {
									GUILayout.Label("(Diff)", italicLabelStyle);
								} else if(comparisonState == AssetComparisonStates.Exclusive) {
									GUILayout.Label("(Exc)", italicLabelStyle);
								} else if(comparisonState == AssetComparisonStates.Same) {
									GUILayout.Label("(Same)", italicLabelStyle);
								}
								GUILayout.Space(2);
				
								GUILayout.Label(currentConfiguration.assets[i].name, assetButtonStyle);
								GUILayout.FlexibleSpace();
							GUILayout.EndHorizontal();
						}
						GUILayout.Space(2);
					GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			GUILayout.EndScrollView();
		}
	}
	
	Asset FindAssetInConfiguration(Asset asset, AssetConfiguration configuration) {
		for(int i=0; i<configuration.assets.Count; i++) {
			if(configuration.assets[i].guid == asset.guid) {
				return configuration.assets[i];
			}
		}
		return null;
	}
}
