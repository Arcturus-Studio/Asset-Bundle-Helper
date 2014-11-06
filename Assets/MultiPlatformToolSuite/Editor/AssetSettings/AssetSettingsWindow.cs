using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class AssetSettingsWindow : EditorWindow {
	
	//Reference to the script file itself
	public Object scriptAsset;
	
	//Configurations
	[System.NonSerialized] public List<AssetConfiguration> configurations;
	[System.NonSerialized] public string[] configurationDropdownStrings;
	[SerializeField] int configurationIndex;
	int ConfigurationIndex {
		get { return configurationIndex;}
		set {
			configurationIndex = value;
			currentConfiguration = (configurationIndex > -1 && configurationIndex < configurations.Count) ? configurations[configurationIndex] : null;
		}
	}
	AssetConfiguration currentConfiguration;
	
	//Assets
	Vector2 assetsListScrollPosition = Vector2.zero;
	
	int AssetIndex {
		get {
			if(assetSelectionIndices == null || assetSelectionIndices.Count == 0)
				return -1;
			else
				return assetSelectionIndices[0];
		}
		set {
			if(assetSelectionIndices.Count == 0) {
				assetSelectionIndices.Add(value);
			} else {
				if(currentConfiguration != null && value >= 0 && value < currentConfiguration.assets.Count)
					assetSelectionIndices[0] = value;
			}
		}
	}
	
	//Copied asset properties
	System.Type typeOfAssetForCopiedProperties;
	string[] copiedPropertyNames;
	object[] copiedPropertyValues;
	
	[SerializeField] List<int> assetSelectionIndices = new List<int>() {-1};
	int[] previousSelection;
	bool supressSelectFromSelection = false;
	
	bool startImportingAssets = false;
	bool importingAssets = false;
	int assetImportIndex = 0;
	Asset[] assetsToImport;
	
	const float assetItemHeight = 13f;
	
	//Configuration dialogs
	bool drawConfigurationAddDialog = false;
	bool drawConfigurationRemoveDialog = false;
	
	bool drawConfigurationEditDialog = false;
	string configurationName = string.Empty;
	
	//Configuration dialog rects
	Rect lastRect;
	Rect configurationAddDialogRect;
	Rect configurationRemoveDialogRect;
	Rect configurationEditDialogRect;
	
	//Properties dimensions
	Vector2 propertiesScrollPosition = Vector3.zero;
	
	//Add and remove button textures and styles
	public Texture2D addTextureUp;
	public Texture2D addTextureDown;
	public Texture2D removeTextureUp;
	public Texture2D removeTextureDown;
	public Texture2D editTexture;
	public Texture2D cancelTextureUp;
	public Texture2D cancelTextureDown;
	
	[SerializeField] GUIStyle addButtonStyle;
	[SerializeField] GUIStyle removeButtonStyle;
	[SerializeField] GUIStyle editButtonStyle;
	[SerializeField] GUIStyle cancelButtonStyle;
	
	[SerializeField] GUIContent addConfigurationGUIContent = new GUIContent(string.Empty, "Add Configuration");
	[SerializeField] GUIContent removeConfigurationGUIContent = new GUIContent(string.Empty, "Remove Configuration");
	[SerializeField] GUIContent editConfigurationGUIContent = new GUIContent(string.Empty, "Edit Configuration");
	[SerializeField] GUIContent applyConfigurationGUIContent = new GUIContent("Apply", "Apply Configuration");
	[SerializeField] GUIContent compareConfigurationsGUIContent = new GUIContent("Compare", "Compare Configurations");
	[SerializeField] GUIContent sortButtonGUIContent = new GUIContent("Sort", "Sort assets alphabetically");
	
	[SerializeField] GUIContent addAssetsToConfigurationGUIContent = new GUIContent(string.Empty, "Add assets in current selection to configuration");
	[SerializeField] GUIContent removeAssetsFromConfigurationGUIContent = new GUIContent(string.Empty, "Remove assets in current selection from configuration");
	[SerializeField] GUIContent readInAssetSettingsGUIContent = new GUIContent("Read in Asset Settings", "Read in project-based import settings for all assets in current selection");
	[SerializeField] GUIContent applyAssetSettingsGUIContent = new GUIContent("Apply Asset Settings", "Apply configuration-based import settings for all assets in current selection");
	
	
	[SerializeField] GUIContent copyAllAssetPropertiesGUIContent = new GUIContent("Copy All", "Copy all asset properties");
	[SerializeField] GUIContent applyCopiedAssetPropertiesGUIContent = new GUIContent("Apply", "Apply copied asset properties");
	
	[SerializeField] GUIStyle assetButtonStyle = new GUIStyle();
	
	//Styles
	[SerializeField] GUIStyle edgeBoxStyle;
	[SerializeField] Texture2D edgeBoxTexture;
	
	[SerializeField] Texture2D selectionTexture;
	
	[SerializeField] GUIStyle prefixLabelStyle;
	[SerializeField] GUIStyle miniBoldLabelStyle;
	[SerializeField] GUIStyle miniButtonStyle;
	[SerializeField] GUIStyle toolbarStyle;
	[SerializeField] public GUIStyle redTextStyle;
	
	bool originalGUIEnabled = true;
	
	FileInfo assetConfigFile;
	
	public static AssetSettingsWindow singleton;
	
	[MenuItem("Window/MultiPlatform ToolKit/Asset Settings", false, 2)]
	public static void InitWindow() {
		AssetSettingsWindow.GetWindow<AssetSettingsWindow>(false, "Asset Settings", true).Init();
	}
	
	void OnEnable() {
		Init();
	}
	
//	void OnDestroy() {
//		EditorApplication.update -= MyUpdate;
//	}
	
	void Init() {
		singleton = this;
		
		GetAssetPaths();
		
		//Load asset configurations
		LoadConfigurations();
		
		//Create/Refresh configuration dropdown list
		RefreshConfigurationDropdownList();
		
		//If serialized configurationIndex is out of array bounds, correct it
		if(ConfigurationIndex < 0 || ConfigurationIndex >= configurations.Count)
			ConfigurationIndex = 0;
		
		this.minSize = new Vector2(300, 200);
		this.maxSize = new Vector2(1000, 800);
		
		//Create GUIStyles
		edgeBoxStyle = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).box);
		edgeBoxStyle.normal.background = edgeBoxTexture;
		edgeBoxStyle.margin = new RectOffset(0,0,0,0);
		edgeBoxStyle.padding = new RectOffset(0,0,0,0);
		edgeBoxStyle.border = new RectOffset(0,0,0,0);
		
		prefixLabelStyle = new GUIStyle(GUIStyle.none);
		prefixLabelStyle.stretchWidth = false;
		prefixLabelStyle.border = new RectOffset(0,0,0,0);
		prefixLabelStyle.overflow = new RectOffset(0,0,0,0);
		prefixLabelStyle.fixedWidth = 5;
		
		addButtonStyle = new GUIStyle();
		addButtonStyle.normal.background = addTextureUp;
		addButtonStyle.active.background = addTextureDown;
		addButtonStyle.fixedWidth = addTextureUp.width;
		addButtonStyle.fixedHeight = addTextureUp.height;
		addButtonStyle.imagePosition = ImagePosition.ImageLeft;
		addButtonStyle.alignment = TextAnchor.MiddleLeft;
		addButtonStyle.clipping = TextClipping.Overflow;
		addButtonStyle.margin = new RectOffset(4,4,4,4);
		addButtonStyle.padding = new RectOffset(16,8,8,8);
		
		removeButtonStyle = new GUIStyle(addButtonStyle); //Inherit margin and fixedWidth/Height from addButtonStyle
		removeButtonStyle.normal.background = removeTextureUp;
		removeButtonStyle.active.background = removeTextureDown;
		
		editButtonStyle = new GUIStyle(addButtonStyle); //Inherit margin and fixedWidth/Height from addButtonStyle
		editButtonStyle.normal.background = editTexture;
		editButtonStyle.active.background = editTexture;
		
		cancelButtonStyle = new GUIStyle(addButtonStyle); //Inherit margin and fixedWidth/Height from addButtonStyle
		cancelButtonStyle.normal.background = cancelTextureUp;
		cancelButtonStyle.active.background = cancelTextureDown;

		//Make sure asset selection indices are valid
		if(currentConfiguration == null) {
			assetSelectionIndices.Clear();
		} else {
			for(int i = assetSelectionIndices.Count - 1; i >= 0; i++) {
				if(assetSelectionIndices[i] < 0 || assetSelectionIndices[i] >= currentConfiguration.assets.Count)
					assetSelectionIndices.RemoveAt(i);
			}
		}
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
		
		scriptAsset = AssetDatabase.LoadAssetAtPath(editorPath + "AssetSettings" + Path.DirectorySeparatorChar + "AssetSettings.xml", typeof(Object));
		addTextureUp = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "editorAddButtonUp.tga", typeof(Texture2D));
		addTextureDown = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "editorAddButtonDown.tga", typeof(Texture2D));
		removeTextureUp = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "editorRemoveButtonUp.tga", typeof(Texture2D));
		removeTextureDown = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "editorRemoveButtonDown.tga", typeof(Texture2D));
		editTexture = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "editButton.tga", typeof(Texture2D));
		cancelTextureUp = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "cancelButtonUp.tga", typeof(Texture2D));
		cancelTextureDown = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "cancelButtonDown.tga", typeof(Texture2D));
		edgeBoxTexture = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "edgeBoxTexture.tga", typeof(Texture2D));
		selectionTexture = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "selectionTexture.tga", typeof(Texture2D));
	}
	
	void LoadConfigurations() {
		string scriptAssetPath = AssetDatabase.GetAssetPath(scriptAsset);
		int lastSlashIndex = scriptAssetPath.LastIndexOf('/');
		if(lastSlashIndex == -1) {
			lastSlashIndex = scriptAssetPath.LastIndexOf('\\');
		}
		scriptAssetPath = scriptAssetPath.Substring(0, lastSlashIndex + 1);
		
		string projectPath = Application.dataPath;
		lastSlashIndex = projectPath.LastIndexOf('/');
		if(lastSlashIndex == -1) {
			lastSlashIndex = projectPath.LastIndexOf('\\');
		}
		projectPath = projectPath.Substring(0, lastSlashIndex + 1);;
		
		string assetSettingsFilePath = projectPath + scriptAssetPath + "AssetSettings.xml";
		assetConfigFile = new FileInfo(assetSettingsFilePath);
		
		configurations = new List<AssetConfiguration>();
		
		StreamReader reader = new StreamReader(assetConfigFile.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read));
		
		string line = string.Empty;
		while((line = reader.ReadLine()) != null) {
			if(line.Contains(VerySimpleXml.StartNode(AssetConfiguration.nodeName)))
				configurations.Add(new AssetConfiguration(reader));
		}
		
		reader.Close();
	}
	
	void RefreshConfigurationDropdownList() {
		configurationDropdownStrings = new string[configurations.Count];
		for(int i=0; i<configurationDropdownStrings.Length; i++)
			configurationDropdownStrings[i] = configurations[i].name;
	}
	
	void WriteToFile() {
		//Early out if there are no configurations to write to file
		if(configurations.Count == 0) return;
		
		StreamWriter writer = new StreamWriter(assetConfigFile.Open(FileMode.Truncate, FileAccess.Write, FileShare.Read));
		
		writer.WriteLine(VerySimpleXml.StartNode("AssetSettings"));
			for(int i=0; i<configurations.Count; i++) {
				configurations[i].WriteToWriter(writer);
			}
		writer.WriteLine(VerySimpleXml.EndNode("AssetSettings"));
		
		writer.Close();
	}
	
	void OnSelectionChange() {
		SelectFromSelection();
		Repaint();
	}
	
	void Update() {
		if(importingAssets) {
			if(assetsToImport == null || assetImportIndex < 0 || assetImportIndex >= assetsToImport.Length) {
				CancelImportingAssets();
				Repaint();
			} else {
				Asset tempAsset = assetsToImport[assetImportIndex];
				if(tempAsset != null)
					tempAsset.ApplyImportSettings();
				assetImportIndex++;
				Repaint();
			}
		}
		
		if(startImportingAssets) {
			importingAssets = true;
			startImportingAssets = false;
		}
	}
	
	//////////
	// GUI ///
	//////////
	void OnGUI() {
		//Gather styles as needed
		GatherStyles();
		
		GUI.enabled = !(drawConfigurationAddDialog || drawConfigurationRemoveDialog || drawConfigurationEditDialog) && !startImportingAssets && !importingAssets;
		originalGUIEnabled = GUI.enabled;
		
		//Configuration toolbar
		GUILayout.BeginHorizontal(toolbarStyle);
			//Configuration dropdown
			ConfigurationIndex = EditorGUILayout.Popup("Configuration:", ConfigurationIndex, configurationDropdownStrings, GUILayout.MaxWidth(220));
		
			GUILayout.Space(5);
			
			//Draw add configuration button
			if(GUILayout.Button(addConfigurationGUIContent, addButtonStyle))
				drawConfigurationAddDialog = true;
		
			lastRect = GUILayoutUtility.GetLastRect();
			configurationAddDialogRect = new Rect(lastRect.x, 18, 164, 45);
		
			GUILayout.Space(2);

			//Draw remove configuration button
			GUI.enabled = configurations.Count > 1 && currentConfiguration != null && originalGUIEnabled;
			if(GUILayout.Button(removeConfigurationGUIContent, removeButtonStyle))
				drawConfigurationRemoveDialog = true;
			GUI.enabled = originalGUIEnabled;
		
			lastRect = GUILayoutUtility.GetLastRect();
			configurationRemoveDialogRect = new Rect(lastRect.x, 18, 150, 54);
		
			GUILayout.Space(2);
		
			//Draw edit configuration button
			GUI.enabled = currentConfiguration != null && originalGUIEnabled;
			if(GUILayout.Button(editConfigurationGUIContent, editButtonStyle)) {
				drawConfigurationEditDialog = true;
				configurationName = currentConfiguration.name;
			}
			GUI.enabled = originalGUIEnabled;
		
			lastRect = GUILayoutUtility.GetLastRect();
			configurationEditDialogRect = new Rect(lastRect.x, 18, 164, 45);
		
			GUILayout.Space(10);
		
			GUI.enabled = currentConfiguration != null && originalGUIEnabled;
			if(GUILayout.Button(applyConfigurationGUIContent, miniButtonStyle)) {
				ApplyConfiguration();
			}
			GUI.enabled = originalGUIEnabled;
		
			if(GUILayout.Button(compareConfigurationsGUIContent, miniButtonStyle)) {
				(EditorWindow.GetWindow<AssetConfigurationCompareWindow>() as AssetConfigurationCompareWindow).AssignAssetSettingsWindow(this);
			}
		
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
			//Assets list
			GUILayout.BeginVertical(GUILayout.Width(180));
				GUILayout.Space(2);
				//Assets list
				GUILayout.BeginHorizontal();
					GUILayout.Label("Assets", miniBoldLabelStyle);
					GUILayout.FlexibleSpace();
					if(GUILayout.Button(sortButtonGUIContent, miniButtonStyle)) {
						SortAssets();
					}
				GUILayout.EndHorizontal();
				
				GUILayout.Box(string.Empty, edgeBoxStyle, GUILayout.Height(1), GUILayout.MaxHeight(1), GUILayout.ExpandWidth(true));
		
				GUILayout.Space(1);
		
				if(currentConfiguration != null) {
					assetsListScrollPosition = GUILayout.BeginScrollView(assetsListScrollPosition);
						GUILayout.BeginHorizontal();
							GUILayout.Space(3);
							GUILayout.BeginVertical();
								for(int i=0; i<currentConfiguration.assets.Count; i++) {
									Asset asset = currentConfiguration.assets[i];
				
									//If this asset button won't be visible, don't do expensive stuff
									bool assetIsVisible = assetsListScrollPosition.y <= (i+1) * assetItemHeight && assetsListScrollPosition.y + this.position.height >= i * assetItemHeight;
									if(!assetIsVisible) {
										GUILayout.Button(currentConfiguration.assets[i].name, assetButtonStyle);
										continue;
									}
				
									//Set style background texture depending on the asset being currently selected
									bool assetIsSelected = assetSelectionIndices.Exists(x => x == i);
				
									Color normalTextColor = assetIsSelected ? Color.white : Color.black;
									assetButtonStyle.normal.background = assetIsSelected ? selectionTexture : null;
				
									assetButtonStyle.normal.textColor = (asset.Importer == null) ? Color.red : normalTextColor;
									
									if(GUILayout.Button(currentConfiguration.assets[i].name, assetButtonStyle)) {
										if(Event.current.shift) {
											if(assetSelectionIndices.Count == 1 && AssetIndex != i) { //Index to index selection
												int lower = i < AssetIndex ? i : AssetIndex;
												int higher = i > AssetIndex ? i : AssetIndex;
												for(int k=lower; k<=higher; k++) {
													if(!assetSelectionIndices.Contains(k))
														assetSelectionIndices.Add(k);
												}
											} else if(!assetSelectionIndices.Contains(i)) { //Single addition to selection
												assetSelectionIndices.Add(i);
											}
										} else if(Event.current.control || Event.current.command) {
											if(assetSelectionIndices.Contains(i))
												assetSelectionIndices.Remove(i);
											else
												assetSelectionIndices.Add(i);
										} else {
											assetSelectionIndices.Clear();
											AssetIndex = i;
										}
					
										SelectToSelection();
									}
								}
			
								GUILayout.Space(2);
							GUILayout.EndVertical();
						GUILayout.EndHorizontal();
					GUILayout.EndScrollView();
				}
			GUILayout.Space(1);
			GUILayout.EndVertical();
		
			//Divider
			GUILayout.BeginVertical(GUILayout.MaxWidth(1), GUILayout.ExpandWidth(false));
				GUILayout.Box(string.Empty, edgeBoxStyle, GUILayout.Width(1), GUILayout.MaxWidth(1), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(false));
			GUILayout.EndVertical();
		
			//Properties
			GUILayout.BeginVertical();
				GUILayout.Space(2);
				//Properties list
				GUILayout.BeginHorizontal();
					GUILayout.Label("Properties", miniBoldLabelStyle);
					GUILayout.FlexibleSpace();
					if(GUILayout.Button(copyAllAssetPropertiesGUIContent, miniButtonStyle)) {
						CopyAllProperties();
					}
					if(GUILayout.Button(applyCopiedAssetPropertiesGUIContent, miniButtonStyle)) {
						ApplyCopiedProperties();
					}
				GUILayout.EndHorizontal();
				GUILayout.Box(string.Empty, edgeBoxStyle, GUILayout.Height(1), GUILayout.MaxHeight(1), GUILayout.ExpandWidth(true));
				GUILayout.Space(1);
		

				if(currentConfiguration != null) {
					if(AssetIndex >= 0 && AssetIndex < currentConfiguration.assets.Count) {
						Asset asset = currentConfiguration.assets[AssetIndex];
						if(asset != null) {
							propertiesScrollPosition = GUILayout.BeginScrollView(propertiesScrollPosition);
								asset.DrawImportSettings();
							GUILayout.EndScrollView();
						}
					}
				}
			GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal(toolbarStyle);
			if(GUILayout.Button(addAssetsToConfigurationGUIContent, addButtonStyle))
				AddAssetsFromSelection();
		
			GUI.enabled = assetSelectionIndices.Count > 0 && originalGUIEnabled;
			if(GUILayout.Button(removeAssetsFromConfigurationGUIContent, removeButtonStyle))
				RemoveAsset();

			if(GUILayout.Button(readInAssetSettingsGUIContent, miniButtonStyle))
				ReadAssetProperties();

			if(GUILayout.Button(applyAssetSettingsGUIContent, miniButtonStyle))
				ApplyAssetProperties();
			GUI.enabled = originalGUIEnabled;
		
			GUILayout.FlexibleSpace();
		
			if(startImportingAssets || importingAssets) {
				GUI.enabled = true;
				Rect position = GUILayoutUtility.GetRect(125f, 125f, 16f, 16f, EditorStyles.textField, GUILayout.MinWidth(100), GUILayout.MaxWidth(200));
				EditorGUI.ProgressBar(position, assetImportIndex / (float) assetsToImport.Length, "Importing " + assetImportIndex.ToString() + '/' + assetsToImport.Length);
			
				GUILayout.Space(2);
			
				if(GUILayout.RepeatButton(string.Empty, cancelButtonStyle))
					CancelImportingAssets();
			
				GUILayout.Space(5);
			
				GUI.enabled = originalGUIEnabled;
			}
		GUILayout.EndHorizontal();
		
		if(drawConfigurationAddDialog)
			DrawConfigurationAddDialog();
		if(drawConfigurationRemoveDialog)
			DrawConfigurationRemoveDialog();
		if(drawConfigurationEditDialog)
			DrawConfigurationEditDialog();
		
		//Process keyboard input
		if(importingAssets && Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyUp) {
			CancelImportingAssets();
		}
		
		if(originalGUIEnabled) {
			if((Event.current.control || Event.current.command) && Event.current.keyCode == KeyCode.A && Event.current.type == EventType.KeyUp) {
				SelectAllAssetsInConfiguration();
				Event.current.Use();
			}
			if(Event.current.keyCode == KeyCode.UpArrow && Event.current.type == EventType.KeyDown) {
				AssetIndex--;
				Event.current.Use();
				Repaint();
			} else if(Event.current.keyCode == KeyCode.DownArrow && Event.current.type == EventType.KeyDown) {
				AssetIndex++;
				Event.current.Use();
				Repaint();
			}
		}
	}
	
	void SelectAllAssetsInConfiguration() {
		if(currentConfiguration != null) {
			assetSelectionIndices.Clear();
			for(int i=0; i<currentConfiguration.assets.Count; i++)
				assetSelectionIndices.Add(i);
			Repaint();
		}
	}
	
	/////////////////////
	// CONFIGURATIONS ///
	/////////////////////
	void DrawConfigurationAddDialog() {
		GUI.enabled = true;
		
		GUI.BeginGroup(configurationAddDialogRect);
			GUI.Box(new Rect(0,0,configurationAddDialogRect.width, configurationAddDialogRect.height), string.Empty);
			
			GUI.Label(new Rect(2,3,80,16), "Name:");
			configurationName = GUI.TextField(new Rect(52, 3, 106, 16), configurationName);
		
			if(GUI.Button(new Rect(2, 26, 60, 16), "Add")) {
				AddConfiguration(configurationName);
				drawConfigurationAddDialog = false;
			}
			if(GUI.Button(new Rect(64, 26, 60, 16), "Cancel")) {
				drawConfigurationAddDialog = false;
			}
		GUI.EndGroup();
	}
	
	void DrawConfigurationRemoveDialog() {
		GUI.enabled = true;
		
		GUI.BeginGroup(configurationRemoveDialogRect);
			GUI.Box(new Rect(0,0,configurationRemoveDialogRect.width, configurationRemoveDialogRect.height), string.Empty);
		
			GUI.Label(new Rect(2,1,configurationRemoveDialogRect.width,32),"Are you sure you want to\nremove this configuration?");
			if(GUI.Button(new Rect(2, 36, 60, 16), "Yes")) {
				RemoveConfiguration();
				drawConfigurationRemoveDialog = false;
			}
			if(GUI.Button(new Rect(64, 36, 60, 16), "No")) {
				drawConfigurationRemoveDialog = false;
			}
		GUI.EndGroup();
	}
	
	void DrawConfigurationEditDialog() {
		GUI.enabled = true;
		
		GUI.BeginGroup(configurationEditDialogRect);
			GUI.Box(new Rect(0,0,configurationEditDialogRect.width, configurationEditDialogRect.height), string.Empty);
		
			GUI.Label(new Rect(2,3,80,16), "Name:");
			configurationName = GUI.TextField(new Rect(52, 3, 106, 16), configurationName);

			if(GUI.Button(new Rect(2, 26, 60, 16), "Apply")) {
				currentConfiguration.name = configurationName;
				RefreshConfigurationDropdownList();
				WriteToFile();
				drawConfigurationEditDialog = false;
			}
			if(GUI.Button(new Rect(64, 26, 60, 16), "Cancel")) {
				drawConfigurationEditDialog = false;
			}
		GUI.EndGroup();
	}
	
	void AddConfiguration(string name) {
		configurations.Add(new AssetConfiguration(name));
		RefreshConfigurationDropdownList();
		ConfigurationIndex = configurations.Count - 1;
		WriteToFile();
	}
	
	void RemoveConfiguration() {
		configurations.RemoveAt(ConfigurationIndex);
		RefreshConfigurationDropdownList();
		if(ConfigurationIndex >= configurations.Count)
			ConfigurationIndex--;
		WriteToFile();
	}
	
	void ApplyConfiguration() {
		//Remember current selection
		previousSelection = (int[]) Selection.instanceIDs.Clone();
		
		startImportingAssets = true;
		assetsToImport = currentConfiguration.assets.ToArray();
		
		//Clear selection so we don't get the annoying Revert/Apply dialog
		Selection.instanceIDs = new int[0];
		Repaint();
	}
	
	/////////////
	// ASSETS ///
	/////////////
	
	void AddAssetsFromSelection() {
		int previousAssetCount = currentConfiguration.assets.Count;
		
		Object[] audioSelection = Selection.GetFiltered(typeof(AudioClip), SelectionMode.Assets);
		Object[] textureSelection = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
		
		for(int i=0; i<audioSelection.Length; i++) {
			AudioClip clip = audioSelection[i] as AudioClip;
			string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(clip));
			//If the asset doesn't already exist on the assets list, add it
			if(!currentConfiguration.assets.Exists(x => x.guid == guid)) {
				AudioAsset audioAsset = new AudioAsset();
				audioAsset.ReadFromAsset(clip);
				currentConfiguration.assets.Add(audioAsset);
			} else {
				Debug.Log("Asset " + clip.name + " already exists in the " + currentConfiguration.name + " configuration at path: " + AssetDatabase.GUIDToAssetPath(guid));
			}
		}
		
		for(int i=0; i<textureSelection.Length; i++) {
			Texture2D texture = textureSelection[i] as Texture2D;
			string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(texture));
			//If the asset doesn't already exist on the assets list, add it
			if(!currentConfiguration.assets.Exists(x => x.guid == guid)) {
				TextureAsset textureAsset = new TextureAsset();
				textureAsset.ReadFromAsset(texture);
				currentConfiguration.assets.Add(textureAsset);
			} else {
				Debug.Log("Asset " + texture.name + " already exists in the " + currentConfiguration.name + " configuration at path: " + AssetDatabase.GUIDToAssetPath(guid));
			}
		}
		
		//If assets got added, select the first asset that was added
		if(currentConfiguration.assets.Count > previousAssetCount) {
			WriteToFile();
			AssetIndex = previousAssetCount;
		} else {
			Debug.Log("No new audio or texture assets were added to the " + currentConfiguration.name + " configuration.");
		}
	}
	
	void RemoveAsset() {
		int previousAssetIndex = AssetIndex;
		
		for(int i=currentConfiguration.assets.Count - 1; i>= 0; i--) {
			if(assetSelectionIndices.Contains(i))
				currentConfiguration.assets.RemoveAt(i);
		}
		
		assetSelectionIndices.Clear();
		
		if(previousAssetIndex < 0 || previousAssetIndex >= currentConfiguration.assets.Count)
			AssetIndex = currentConfiguration.assets.Count - 1;
		
		WriteToFile();
	}
	
	void SelectToSelection() {
		List<int> instanceIDs = new List<int>(assetSelectionIndices.Count);
		foreach(int i in assetSelectionIndices) {
			Object assetObject = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(currentConfiguration.assets[i].guid), typeof(Object));
			if(assetObject != null)
				instanceIDs.Add(assetObject.GetInstanceID());
		}
		
		if(instanceIDs.Count > 0) {
			Selection.instanceIDs = instanceIDs.ToArray();
		}
		
		supressSelectFromSelection = true;
	}
	
	void SelectFromSelection() {
		if(supressSelectFromSelection) {
			supressSelectFromSelection = false;
			return;
		}
		
		assetSelectionIndices.Clear();
		
		if(currentConfiguration == null)
			return;
		
		Object[] audioSelection = Selection.GetFiltered(typeof(AudioClip), SelectionMode.Assets);
		Object[] textureSelection = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
		
		for(int i=0; i<audioSelection.Length; i++) {
			string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(audioSelection[i] as AudioClip));
			int idx = currentConfiguration.assets.FindIndex(x => x.guid == guid);
			if(idx != -1 && !assetSelectionIndices.Contains(idx))
				assetSelectionIndices.Add(idx);
		}
		
		for(int i=0; i<textureSelection.Length; i++) {
			string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(textureSelection[i] as Texture2D));
			int idx = currentConfiguration.assets.FindIndex(x => x.guid == guid);
			if(idx != -1 && !assetSelectionIndices.Contains(idx))
				assetSelectionIndices.Add(idx);
		}
		
		//Move to the first item in the selection
		if(assetSelectionIndices.Count > 0) {
			assetsListScrollPosition.y = assetItemHeight * assetSelectionIndices[0];
		}
	}
	
	void ReadAssetProperties() {
		foreach(int i in assetSelectionIndices) {
			Asset asset = currentConfiguration.assets[i];
			asset.ReadFromAsset(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(asset.guid), typeof(Object)));
		}
		WriteToFile();
	}
	
	void ApplyAssetProperties() {
		//Remember current selection
		previousSelection = (int[]) Selection.instanceIDs.Clone();
		
		Asset[] tempAssetArray = new Asset[assetSelectionIndices.Count];
		for(int i=0; i<assetSelectionIndices.Count; i++) {
			tempAssetArray[i] = currentConfiguration.assets[assetSelectionIndices[i]];
		}
		
		startImportingAssets = true;
		assetsToImport = tempAssetArray;
		
		//Clear selection so we don't get the annoying Revert/Apply dialog
		Selection.instanceIDs = new int[0];
		Repaint();
	}
	
	void CancelImportingAssets() {
		importingAssets = false;
		assetImportIndex = 0;
		
		if(previousSelection != null) {
			Selection.instanceIDs = previousSelection;
			previousSelection = null;
		}
	}
	
	void GatherStyles() {
		if(miniBoldLabelStyle == null) {
			miniBoldLabelStyle = new GUIStyle(EditorStyles.miniBoldLabel);
			miniBoldLabelStyle.padding = new RectOffset(0,0,0,0);
		}
		
		if(toolbarStyle == null) {
			toolbarStyle = new GUIStyle(EditorStyles.toolbar);
		}
		
		if(miniButtonStyle == null) {
			miniButtonStyle = new GUIStyle(EditorStyles.miniButton);
		}
		
		if(redTextStyle == null) {
			redTextStyle = new GUIStyle(EditorStyles.label);
			redTextStyle.normal.textColor = Color.red;
		}
	}
	
	void SortAssets() {
		currentConfiguration.assets.Sort((x,y) => string.Compare(x.name, y.name));
		WriteToFile();
	}
	
	void CopyAllProperties() {
		if(assetSelectionIndices.Count == 0) {
			Debug.LogError("No asset selected but Copy All was hit. How!?");
			return;
		}
		
		if (assetSelectionIndices.Count > 1) {
			Debug.Log("More than one asset selected while trying to Copy All Properties. Only the properties of the first selected asset will be copied");
		}
		
		Asset asset = currentConfiguration.assets[assetSelectionIndices[0]];
		typeOfAssetForCopiedProperties = asset.GetType();
		copiedPropertyNames = asset.GetAllPropertyNames();
		copiedPropertyValues = asset.GetAllPropertyValues();
		
		Debug.Log(copiedPropertyNames.Length + " properties copied from " + asset.name);
	}
	
	void ApplyCopiedProperties() {
		if(assetSelectionIndices.Count == 0) {
			Debug.LogError("No asset selected but Apply All was hit. How!?");
			return;
		}
		
		if(copiedPropertyNames == null || copiedPropertyNames.Length == 0) {
			Debug.LogError("There are no copied properties");
			return;
		}
		
		int numberOfAssetsApplied = 0;
		foreach(int i in assetSelectionIndices) {
			if(currentConfiguration.assets[i].GetType() == typeOfAssetForCopiedProperties) {
				currentConfiguration.assets[i].ApplyCopiedValues(copiedPropertyNames, copiedPropertyValues);
				numberOfAssetsApplied++;
			}
		}
		
		WriteToFile();
		
		Debug.Log(copiedPropertyNames.Length + " properties applied to " + numberOfAssetsApplied.ToString() + " assets.");
	}
}
