using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Type = System.Type;

[CustomEditor (typeof(PlatformSpecifics))]
public class PlatformSpecificsEditor : Editor {
	
	class FieldSelectOption{
		public Component component;
		public string fieldName;
		public Type fieldType;
		
		public FieldSelectOption(Component comp, FieldInfo field){
			component = comp;
			fieldName = field.Name;
			fieldType = field.FieldType;
		}
		
		public FieldSelectOption(Component comp, PropertyInfo property){
			component = comp;
			fieldName = property.Name;
			fieldType = property.PropertyType;
		}
		
		public string MenuPath{
			get{
				return component.GetType().Name + "/" + fieldName;
			}
		}
	}
	
	public Texture2D addTextureUp;
	public Texture2D addTextureDown;
	public Texture2D removeTextureUp;
	public Texture2D removeTextureDown;
	
	public Texture2D moveItemAboveUp;
	public Texture2D moveItemAboveDown;
	public Texture2D moveItemBelowUp;
	public Texture2D moveItemBelowDown;
	
	[SerializeField] GUIStyle addButtonStyle;
	[SerializeField] GUIStyle removeButtonStyle;
	[SerializeField] GUIStyle moveItemAboveStyle;
	[SerializeField] GUIStyle moveItemBelowStyle;
	[SerializeField] GUIStyle enumPopupStyle;
	
	delegate void DrawArray<T>(int index, ref T[] array, ref int itemToDelete, ref int moveItemIdx, ref int moveItemDirection);
	delegate T Constructor<T>();
	
	PlatformSpecifics specifics;
	
	bool showRestrictPlatforms = false;
	bool showMaterials = false;
	bool showLocalScalePerPlatform = false;
	bool showLocalScalePerAspectRatio = false;
	bool showLocalPositionPerPlatform = false;
	bool showLocalPositionPerAspectRatio = false;
	bool showAnchorPositions = false;
	bool showFonts = false;
	bool showTextMeshText = false;
	bool showFieldValues = false;
	Rect[][] lastFieldDropdownRects;
	
	void OnEnable() {
		GetAssetPaths();
		
		specifics = target as PlatformSpecifics;
		specifics.Init();
		
		addButtonStyle = new GUIStyle();
		addButtonStyle.normal.background = addTextureUp;
		addButtonStyle.active.background = addTextureDown;
		addButtonStyle.fixedWidth = addTextureUp.width;
		addButtonStyle.fixedHeight = addTextureUp.height;
		addButtonStyle.margin = new RectOffset(4,4,4,4);
		
		removeButtonStyle = new GUIStyle(addButtonStyle); //Inherit margin and fixedWidth/Height from addButtonStyle
		removeButtonStyle.normal.background = removeTextureUp;
		removeButtonStyle.active.background = removeTextureDown;
		
		moveItemAboveStyle = new GUIStyle(addButtonStyle); //Inherit margin and fixedWidth/Height from addButtonStyle
		moveItemAboveStyle.normal.background = moveItemAboveUp;
		moveItemAboveStyle.active.background = moveItemAboveDown;
		
		moveItemBelowStyle = new GUIStyle(addButtonStyle); //Inherit margin and fixedWidth/Height from addButtonStyle
		moveItemBelowStyle.normal.background = moveItemBelowUp;
		moveItemBelowStyle.active.background = moveItemBelowDown;
		
		showRestrictPlatforms = (specifics.restrictPlatform != null && specifics.restrictPlatform.Length > 0);
		showMaterials = (specifics.materialPerPlatform != null && specifics.materialPerPlatform.Length > 0);
		showLocalScalePerPlatform = (specifics.localScalePerPlatform != null && specifics.localScalePerPlatform.Length > 0);
		showLocalScalePerAspectRatio = (specifics.localScalePerAspectRatio != null && specifics.localScalePerAspectRatio.Length > 0);
		showLocalPositionPerPlatform = (specifics.localPositionPerPlatform != null && specifics.localPositionPerPlatform.Length > 0);
		showLocalPositionPerAspectRatio = (specifics.localPositionPerAspectRatio != null && specifics.localPositionPerAspectRatio.Length > 0);
		showAnchorPositions = (specifics.anchorPositions != null && specifics.anchorPositions.Length > 0);
		showFonts = (specifics.fontPerPlatform != null && specifics.fontPerPlatform.Length > 0);
		showTextMeshText = (specifics.textMeshTextPerPlatform != null && specifics.textMeshTextPerPlatform.Length > 0);
		showFieldValues = (specifics.fieldValuesPerPlatform != null && specifics.fieldValuesPerPlatform.Length > 0);
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
		
		addTextureUp = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "editorAddButtonUp.tga", typeof(Texture2D));
		addTextureDown = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "editorAddButtonDown.tga", typeof(Texture2D));
		removeTextureUp = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "editorRemoveButtonUp.tga", typeof(Texture2D));
		removeTextureDown = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "editorRemoveButtonDown.tga", typeof(Texture2D));
		moveItemAboveUp = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "moveItemAboveUp.tga", typeof(Texture2D));
		moveItemAboveDown = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "moveItemAboveDown.tga", typeof(Texture2D));
		moveItemBelowUp = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "moveItemBelowUp.tga", typeof(Texture2D));
		moveItemBelowDown = (Texture2D) AssetDatabase.LoadAssetAtPath(path + "moveItemBelowDown.tga", typeof(Texture2D));
	}
	
	void GatherStyles() {
		if(enumPopupStyle == null) {
			enumPopupStyle = new GUIStyle(EditorStyles.popup);
			enumPopupStyle.fontStyle = FontStyle.Bold;
			enumPopupStyle.fontSize = 10;
			enumPopupStyle.fixedHeight = 18;
		}
	}

	public override void OnInspectorGUI() {
		//Styles that we derive from EditorStyles can't be initialized in OnEnable (causes null-ref)
		GatherStyles();
		
		GUILayout.Space(4f);
		
		//Draw restrict to platforms
		DrawSection<Platform>
		(ref showRestrictPlatforms,
		"Restrict to Platforms",
		ref specifics.restrictPlatform,
		DrawRestrictToPlatforms<Platform>,
		() => {return Platform.Standalone;}, true);
		
		//Draw materials per platform
		DrawSection<PlatformSpecifics.MaterialPerPlatform>
		(ref showMaterials,
		"Materials",
		ref specifics.materialPerPlatform,
		DrawMaterials<PlatformSpecifics.MaterialPerPlatform>,
		() => {return new PlatformSpecifics.MaterialPerPlatform(Platform.Standalone, null);}, true);
		
		//Draw local scale per platform
		DrawSection<PlatformSpecifics.LocalScalePerPlatform>
		(ref showLocalScalePerPlatform,
		"Local Scale per Platform",
		ref specifics.localScalePerPlatform,
		DrawLocalScalePerPlatform<PlatformSpecifics.LocalScalePerPlatform>,
		() => {return new PlatformSpecifics.LocalScalePerPlatform(Platform.Standalone, Vector3.zero);}, true);
		
		//Draw local scale per aspect ratio
		DrawSection<PlatformSpecifics.LocalScalePerAspectRatio>
		(ref showLocalScalePerAspectRatio,
		"Local Scale per Aspect Ratio",
		ref specifics.localScalePerAspectRatio,
		DrawLocalScalePerAspectRatio<PlatformSpecifics.LocalScalePerAspectRatio>,
		() => {return new PlatformSpecifics.LocalScalePerAspectRatio(AspectRatio.Aspect4by3, Vector3.zero);}, true);
		
		//Draw local position per platform
		DrawSection<PlatformSpecifics.LocalPositionPerPlatform>
		(ref showLocalPositionPerPlatform,
		"Local Position per Platform",
		ref specifics.localPositionPerPlatform,
		DrawLocalPositionPerPlatform<PlatformSpecifics.LocalPositionPerPlatform>,
		() => {return new PlatformSpecifics.LocalPositionPerPlatform(Platform.Standalone, Vector3.zero);}, true);
		
		//Draw local position per aspect ratio
		DrawSection<PlatformSpecifics.LocalPositionPerAspectRatio>
		(ref showLocalPositionPerAspectRatio,
		"Local Position per Aspect Ratio",
		ref specifics.localPositionPerAspectRatio,
		DrawLocalPositionPerAspectRatio<PlatformSpecifics.LocalPositionPerAspectRatio>,
		() => {return new PlatformSpecifics.LocalPositionPerAspectRatio(AspectRatio.Aspect4by3, Vector3.zero);}, true);
			
		//Draw anchor positions
		#if UNITY_4_6
		if (specifics.GetComponent<RectTransform>() == null)
		{
			DrawSection<PlatformSpecifics.AnchorPosition>
			(ref showAnchorPositions,
			"Anchor Position",
			ref specifics.anchorPositions,
			DrawAnchorPositions<PlatformSpecifics.AnchorPosition>,
			() => {return new PlatformSpecifics.AnchorPosition(HorizontalAlignments.Left, VerticalAlignments.Top, false, false, false, false, Vector3.zero);}, false);
		}
		#else
		DrawSection<PlatformSpecifics.AnchorPosition>
		(ref showAnchorPositions,
		"Anchor Position",
		ref specifics.anchorPositions,
		DrawAnchorPositions<PlatformSpecifics.AnchorPosition>,
		() => {return new PlatformSpecifics.AnchorPosition(HorizontalAlignments.Left, VerticalAlignments.Top, false, false, false, false, Vector3.zero);}, false);
		#endif
		
		//Draw font per platform
		DrawSection<PlatformSpecifics.FontPerPlatform>
		(ref showFonts,
		"Fonts",
		ref specifics.fontPerPlatform,
		DrawFonts<PlatformSpecifics.FontPerPlatform>,
		() => {return new PlatformSpecifics.FontPerPlatform(Platform.Standalone, null, null);}, true);
		
		//Draw text mesh text per platform
		DrawSection<PlatformSpecifics.TextMeshTextPerPlatform>
		(ref showTextMeshText,
		"Text mesh text",
		ref specifics.textMeshTextPerPlatform,
		DrawTextMeshText<PlatformSpecifics.TextMeshTextPerPlatform>,
		() => {return new PlatformSpecifics.TextMeshTextPerPlatform(Platform.Standalone, string.Empty);}, true);
		
		//Draw field value per platform
		DrawSection<PlatformSpecifics.FieldValuesPerPlatform>(
			ref showFieldValues,
			"Field values",
			ref specifics.fieldValuesPerPlatform,
			DrawFieldValues<PlatformSpecifics.FieldValuesPerPlatform>,
			() => {return new PlatformSpecifics.FieldValuesPerPlatform(Platform.Standalone);},
			true
		);
	}
	
	void DrawRestrictToPlatforms<T>(int index, ref T[] array, ref int itemToDelete, ref int moveItemIdx, ref int moveItemDirection) {
		Platform[] restrictPlatform = array as Platform[];
		DrawPlatformEnum(index, ref itemToDelete, ref moveItemIdx, ref moveItemDirection, ref restrictPlatform[index]);
	}
	
	void DrawMaterials<T>(int index, ref T[] array, ref int itemToDelete, ref int moveItemIdx, ref int moveItemDirection) {
		PlatformSpecifics.MaterialPerPlatform[] materialsPerPlatform = array as PlatformSpecifics.MaterialPerPlatform[];
		DrawPlatformEnum(index, ref itemToDelete, ref moveItemIdx, ref moveItemDirection, ref materialsPerPlatform[index].platform);
		
		GUILayout.Space(10f);
		materialsPerPlatform[index].mat = EditorGUILayout.ObjectField(materialsPerPlatform[index].mat, typeof(Material), false) as Material;
		GUILayout.BeginHorizontal();
			if(GUILayout.Button("Get")) {
				if(specifics.GetComponent<Renderer>() != null) {
					#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
					Undo.RegisterUndo(specifics, "get material");
					#else
					Undo.RecordObject(specifics, "get material");
					#endif
					materialsPerPlatform[index].mat = specifics.GetComponent<Renderer>().sharedMaterial;
				} else {
					Debug.Log("There is no Renderer component on this game object.");
				}
			}
			if(GUILayout.Button("Set")) {
				if(specifics.GetComponent<Renderer>() != null) {
					#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
					Undo.RegisterUndo(specifics.renderer, "set material");
					#else
					Undo.RecordObject(specifics.GetComponent<Renderer>(), "set material");
					#endif
					specifics.GetComponent<Renderer>().sharedMaterial = materialsPerPlatform[index].mat;
				} else {
					Debug.Log("There is no Renderer component on this game object.");
				}
			}
		GUILayout.EndHorizontal();
	}
	
	void DrawLocalScalePerPlatform<T>(int index, ref T[] array, ref int itemToDelete, ref int moveItemIdx, ref int moveItemDirection) {
		PlatformSpecifics.LocalScalePerPlatform[] localScalePerPlatform = array as PlatformSpecifics.LocalScalePerPlatform[];
		DrawPlatformEnum(index, ref itemToDelete, ref moveItemIdx, ref moveItemDirection, ref localScalePerPlatform[index].platform);
		
		localScalePerPlatform[index].localScale = EditorGUILayout.Vector3Field("Local scale", localScalePerPlatform[index].localScale);
		GUILayout.BeginHorizontal();
			//GUILayout.Space(20f);
			GUILayout.BeginVertical();
			//GUILayout.Space(18f);
			GUILayout.BeginHorizontal();
		
			if(GUILayout.Button("Get")) {
				#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
				Undo.RegisterUndo(specifics, "get local scale");
				#else
				Undo.RecordObject(specifics, "get local scale");
				#endif

				#if UNITY_4_6
				var rectTransform = specifics.GetComponent<RectTransform>();
				if (rectTransform != null) localScalePerPlatform[index].localScale = rectTransform.localScale;
				else localScalePerPlatform[index].localScale = specifics.transform.localScale;
				#else
				localScalePerPlatform[index].localScale = specifics.transform.localScale;
				#endif
			}
			if(GUILayout.Button("Set")) {
				#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
				Undo.RegisterUndo(specifics.transform, "set local scale");
				#else
				Undo.RecordObject(specifics.transform, "set local scale");
				#endif

				#if UNITY_4_6
				var rectTransform = specifics.GetComponent<RectTransform>();
				if (rectTransform != null) rectTransform.localScale = localScalePerPlatform[index].localScale;
				else specifics.transform.localScale = localScalePerPlatform[index].localScale;
				#else
				specifics.transform.localScale = localScalePerPlatform[index].localScale;
				#endif
			}
		
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}
	
	void DrawLocalScalePerAspectRatio<T>(int index, ref T[] array, ref int itemToDelete, ref int moveItemIdx, ref int moveItemDirection) {
		PlatformSpecifics.LocalScalePerAspectRatio[] localScalePerAspectRatio = array as PlatformSpecifics.LocalScalePerAspectRatio[];
		DrawAspectRatiosEnum(index, ref itemToDelete, ref moveItemIdx, ref moveItemDirection, ref localScalePerAspectRatio[index].aspectRatio);
		
		localScalePerAspectRatio[index].localScale = EditorGUILayout.Vector3Field("Local scale", localScalePerAspectRatio[index].localScale);
		GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			//GUILayout.Space(18f);
			GUILayout.BeginHorizontal();
		
			if(GUILayout.Button("Get")) {
				#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
				Undo.RegisterUndo(specifics, "get local scale");
				#else
				Undo.RecordObject(specifics, "get local scale");
				#endif

				#if UNITY_4_6
				var rectTransform = specifics.GetComponent<RectTransform>();
				if (rectTransform != null) localScalePerAspectRatio[index].localScale = rectTransform.localScale;
				else localScalePerAspectRatio[index].localScale = specifics.transform.localScale;
				#else
				localScalePerAspectRatio[index].localScale = specifics.transform.localScale;
				#endif
			}
			if(GUILayout.Button("Set")) {
				#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
				Undo.RegisterUndo(specifics.transform, "set local scale");
				#else
				Undo.RecordObject(specifics.transform, "set local scale");
				#endif

				#if UNITY_4_6
				var rectTransform = specifics.GetComponent<RectTransform>();
				if (rectTransform != null) rectTransform.localScale = localScalePerAspectRatio[index].localScale;
				else specifics.transform.localScale = localScalePerAspectRatio[index].localScale;
				#else
				specifics.transform.localScale = localScalePerAspectRatio[index].localScale;
				#endif
			}
		
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}
	
	void DrawLocalPositionPerPlatform<T>(int index, ref T[] array, ref int itemToDelete, ref int moveItemIdx, ref int moveItemDirection) {
		PlatformSpecifics.LocalPositionPerPlatform[] localPositionPerPlatform = array as PlatformSpecifics.LocalPositionPerPlatform[];
		DrawPlatformEnum(index, ref itemToDelete, ref moveItemIdx, ref moveItemDirection, ref localPositionPerPlatform[index].platform);
		
		localPositionPerPlatform[index].localPosition = EditorGUILayout.Vector3Field("Local position", localPositionPerPlatform[index].localPosition);
		GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			//GUILayout.Space(18f);
			GUILayout.BeginHorizontal();
		
			if(GUILayout.Button("Get")) {
				#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
				Undo.RegisterUndo(specifics, "get local position");
				#else
				Undo.RecordObject(specifics, "get local position");
				#endif

				#if UNITY_4_6
				var rectTransform = specifics.GetComponent<RectTransform>();
				if (rectTransform != null) localPositionPerPlatform[index].localPosition = rectTransform.localPosition;
				else localPositionPerPlatform[index].localPosition = specifics.transform.localPosition;
				#else
				localPositionPerPlatform[index].localPosition = specifics.transform.localPosition;
				#endif
			}
			if(GUILayout.Button("Set")) {
				#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
				Undo.RegisterUndo(specifics.transform, "set local position");
				#else
				Undo.RecordObject(specifics.transform, "set local position");
				#endif

				#if UNITY_4_6
				var rectTransform = specifics.GetComponent<RectTransform>();
				if (rectTransform != null) rectTransform.localPosition = localPositionPerPlatform[index].localPosition;
				else specifics.transform.localPosition = localPositionPerPlatform[index].localPosition;
				#else
				specifics.transform.localPosition = localPositionPerPlatform[index].localPosition;
				#endif
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}
	
	void DrawLocalPositionPerAspectRatio<T>(int index, ref T[] array, ref int itemToDelete, ref int moveItemIdx, ref int moveItemDirection) {
		PlatformSpecifics.LocalPositionPerAspectRatio[] localPositionPerAspectRatio = array as PlatformSpecifics.LocalPositionPerAspectRatio[];
		DrawAspectRatiosEnum(index, ref itemToDelete, ref moveItemIdx, ref moveItemDirection, ref localPositionPerAspectRatio[index].aspectRatio);
		
		localPositionPerAspectRatio[index].localPosition = EditorGUILayout.Vector3Field("Local position", localPositionPerAspectRatio[index].localPosition);
		GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			//GUILayout.Space(18f);
			GUILayout.BeginHorizontal();
		
			if(GUILayout.Button("Get")) {
				#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
				Undo.RegisterUndo(specifics, "get local position");
				#else
				Undo.RecordObject(specifics, "get local position");
				#endif

				#if UNITY_4_6
				var rectTransform = specifics.GetComponent<RectTransform>();
				if (rectTransform != null) localPositionPerAspectRatio[index].localPosition = rectTransform.localPosition;
				else localPositionPerAspectRatio[index].localPosition = specifics.transform.localPosition;
				#else
				localPositionPerAspectRatio[index].localPosition = specifics.transform.localPosition;
				#endif
			}
			if(GUILayout.Button("Set")) {
				#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
				Undo.RegisterUndo(specifics.transform, "set local position");
				#else
				Undo.RecordObject(specifics.transform, "set local position");
				#endif

				#if UNITY_4_6
				var rectTransform = specifics.GetComponent<RectTransform>();
				if (rectTransform != null) rectTransform.localPosition = localPositionPerAspectRatio[index].localPosition;
				else specifics.transform.localPosition = localPositionPerAspectRatio[index].localPosition;
				#else
				specifics.transform.localPosition = localPositionPerAspectRatio[index].localPosition;
				#endif
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}
	
	void DrawAnchorPositions<T>(int index, ref T[] array, ref int itemToDelete, ref int moveItemIdx, ref int moveItemDirection) {
		PlatformSpecifics.AnchorPosition[] anchorPositions = array as PlatformSpecifics.AnchorPosition[];
		
		GUILayout.BeginHorizontal();
			GUILayout.Space(20f);
			anchorPositions[index].alignmentCamera = (Camera)EditorGUILayout.ObjectField(anchorPositions[index].alignmentCamera, typeof(Camera), true);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
			GUILayout.Space(20f);
			anchorPositions[index].screenPosition = EditorGUILayout.Vector2Field("Position Offset", anchorPositions[index].screenPosition);
			EditorGUILayout.Space();
			DrawRemoveButton(index, ref itemToDelete);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
			GUILayout.Space(28f);
			int value = anchorPositions[index].positionXAsPercent ? 0 : 1;
			value = GUILayout.SelectionGrid(value, new GUIContent[]{new GUIContent("X As Percent", "For example, enter 0.5 for 50%"), new GUIContent("X As Pixels")}, 1, EditorStyles.radioButton);
			anchorPositions[index].positionXAsPercent = value == 0;
			
			value = anchorPositions[index].positionYAsPercent ? 0 : 1;
			value = GUILayout.SelectionGrid(value, new GUIContent[]{new GUIContent("Y As Percent", "For example, enter 0.5 for 50%"), new GUIContent("Y As Pixels")}, 1, EditorStyles.radioButton);
			anchorPositions[index].positionYAsPercent = value == 0;
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
			GUILayout.Space(20f);
			EditorGUILayout.LabelField("CenterX:", GUILayout.Width(75));
			anchorPositions[index].centerX = EditorGUILayout.Toggle(anchorPositions[index].centerX, GUILayout.Width(20));
			EditorGUILayout.LabelField("CenterY:", GUILayout.Width(75));
			anchorPositions[index].centerY = EditorGUILayout.Toggle(anchorPositions[index].centerY, GUILayout.Width(20));
		GUILayout.EndHorizontal();
	
		GUILayout.BeginHorizontal();
			GUILayout.Space(20f);
			EditorGUILayout.LabelField("Alignments:", GUILayout.Width(70));
			anchorPositions[index].horizontalAlignment = (HorizontalAlignments)EditorGUILayout.EnumPopup((System.Enum)anchorPositions[index].horizontalAlignment, enumPopupStyle, GUILayout.Width(70));
			anchorPositions[index].verticalAlignment = (VerticalAlignments)EditorGUILayout.EnumPopup((System.Enum)anchorPositions[index].verticalAlignment, enumPopupStyle, GUILayout.Width(70));
			EditorGUILayout.Space();
			DrawMoveButtons(index, ref moveItemIdx, ref moveItemDirection);			
		GUILayout.EndHorizontal();
	}
	
	void DrawFonts<T>(int index, ref T[] array, ref int itemToDelete, ref int moveItemIdx, ref int moveItemDirection) {
		PlatformSpecifics.FontPerPlatform[] fontsPerPlatform = array as PlatformSpecifics.FontPerPlatform[];
		DrawPlatformEnum(index, ref itemToDelete, ref moveItemIdx, ref moveItemDirection, ref fontsPerPlatform[index].platform);
		
		GUILayout.BeginHorizontal();
			GUILayout.Space(20f);
			fontsPerPlatform[index].font = EditorGUILayout.ObjectField(fontsPerPlatform[index].font, typeof(Font), false) as Font;
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
			GUILayout.Space(20f);
			fontsPerPlatform[index].mat = EditorGUILayout.ObjectField(fontsPerPlatform[index].mat, typeof(Material), false) as Material;
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
			GUILayout.Space(20f);
			if(GUILayout.Button("Get")) {
				TextMesh textMesh = specifics.GetComponent<TextMesh>();
				if(textMesh != null && specifics.GetComponent<Renderer>() != null) {
					#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
					Undo.RegisterUndo(specifics, "get font and material");
					#else
					Undo.RecordObject(specifics, "get font and material");
					#endif
					fontsPerPlatform[index].font = textMesh.font;
					fontsPerPlatform[index].mat = specifics.GetComponent<Renderer>().sharedMaterial;
				} else {
					Debug.Log("There is no TextMesh or Renderer component on this game object.");
				}
			}
			if(GUILayout.Button("Set")) {
				TextMesh textMesh = specifics.GetComponent<TextMesh>();
				if(textMesh != null && specifics.GetComponent<Renderer>() != null) {
					#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
					Undo.RegisterUndo(textMesh, "set font");
					Undo.RegisterUndo(specifics.renderer, "set material");
					#else
					Undo.RecordObject(textMesh, "set font");
					Undo.RecordObject(specifics.GetComponent<Renderer>(), "set material");
					#endif
					textMesh.font = fontsPerPlatform[index].font;
					specifics.GetComponent<Renderer>().sharedMaterial = fontsPerPlatform[index].mat;
				} else {
					Debug.Log("There is no TextMesh or Renderer component on this game object.");
				}
			}
		GUILayout.EndHorizontal();
	}
	
	void DrawTextMeshText<T>(int index, ref T[] array, ref int itemToDelete, ref int moveItemIdx, ref int moveItemDirection) {
		PlatformSpecifics.TextMeshTextPerPlatform[] textMeshTextPerPlatform = array as PlatformSpecifics.TextMeshTextPerPlatform[];
		DrawPlatformEnum(index, ref itemToDelete, ref moveItemIdx, ref moveItemDirection, ref textMeshTextPerPlatform[index].platform);
		
		GUILayout.Space(10f);
		textMeshTextPerPlatform[index].text = EditorGUILayout.TextField(textMeshTextPerPlatform[index].text);
		GUILayout.BeginHorizontal();
			if(GUILayout.Button("Get")) {
				TextMesh textMesh = specifics.GetComponent<TextMesh>();
				if(textMesh != null) {
					#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
					Undo.RegisterUndo(specifics, "get TextMesh text");
					#else
					Undo.RecordObject(specifics, "get TextMesh text");
					#endif
					textMeshTextPerPlatform[index].text = textMesh.text;
				} else {
					Debug.Log("There is no TextMesh component on this game object.");
				}
			}
			if(GUILayout.Button("Set")) {
				TextMesh textMesh = specifics.GetComponent<TextMesh>();
				if(textMesh != null) {
					#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
					Undo.RegisterUndo(textMesh, "set TextMesh text");
					#else
					Undo.RecordObject(textMesh, "set TextMesh text");
					#endif
					textMesh.text = textMeshTextPerPlatform[index].text;
				} else {
					Debug.Log("There is no TextMesh component on this game object.");
				}
			}
		GUILayout.EndHorizontal();
	}
	
	void DrawFieldValues<T>(int index, ref T[] array, ref int deleteIndex, ref int moveIndex, ref int moveDirection){		
		PlatformSpecifics.FieldValuesPerPlatform[] fieldValuesPerPlatform = array as PlatformSpecifics.FieldValuesPerPlatform[];
		DrawPlatformEnum(index, ref deleteIndex, ref moveIndex, ref moveDirection, ref fieldValuesPerPlatform[index].platform);
		PlatformSpecifics.FieldValuesPerPlatform fieldSetter = fieldValuesPerPlatform[index];
		
		//Platform unsupported warning
		if(!specifics.PlatformSupportsFieldOverrides(fieldSetter.platform)){
			Color oldColor = GUI.color;
			GUI.color = Color.yellow;
			GUILayout.Box("Field Overrides Not Supported On This Platform");
			GUI.color = oldColor;
		}
		
		//Init / resize lastFieldDropdownRects
		if(lastFieldDropdownRects == null || lastFieldDropdownRects.Length != fieldValuesPerPlatform.Length){
			lastFieldDropdownRects = new Rect[fieldValuesPerPlatform.Length][];
		}
		if(lastFieldDropdownRects[index] == null || lastFieldDropdownRects[index].Length != fieldSetter.values.Count){
			lastFieldDropdownRects[index] = new Rect[fieldSetter.values.Count];
		}
				
		GUILayout.Space(10f);
		
		int? valueOverrideIndexToRemove = null;
		
		//Screen.width returns inspector view width in this context
		//-20 fudges space for remove button, accounts for margins
		int dropdownWidth = Mathf.Max(0, Screen.width / 2 - 20); 
		int propertyFieldWidth = Mathf.Max(0, Screen.width / 2 - 20);

		for(int valueOverrideIndex = 0; valueOverrideIndex < fieldSetter.values.Count; valueOverrideIndex++){
			PlatformSpecifics.FieldValueOverride valueOverride = fieldSetter.values[valueOverrideIndex];
			GUILayout.BeginHorizontal();
			//Draw field selection dropdown target
			string fieldSelectionDropdownLabel;
			bool fieldSelected = valueOverride.ValidTarget;
			if(!fieldSelected){
				fieldSelectionDropdownLabel = "No Field";
			}
			else{
				fieldSelectionDropdownLabel = valueOverride.componentTypeName + "." + valueOverride.fieldName;
			}		
			bool displayFieldSelectionDropdown = GUILayout.Button(fieldSelectionDropdownLabel, EditorStyles.popup, GUILayout.Width(dropdownWidth));
			if(Event.current.type == EventType.Repaint){
				lastFieldDropdownRects[index][valueOverrideIndex] = GUILayoutUtility.GetLastRect();
			}
		
			//Draw value field
			if(fieldSelected){
				//Dig through serialized object to get SerializedProperty for this value override
				SerializedObject platformSpecificsSerObj = new SerializedObject(specifics);
				SerializedProperty currentOverrideProperty = GetFieldValueOverrideProperty(platformSpecificsSerObj,
					index,
					valueOverrideIndex,
					valueOverride
				);
				//Draw field
				EditorGUILayout.PropertyField(currentOverrideProperty, new GUIContent(""), GUILayout.Width(propertyFieldWidth));
				//Clear obj reference value and warn if not assignable to target
				Type componentType = specifics.GetComponent(valueOverride.componentTypeName).GetType();			
				if(currentOverrideProperty.propertyType == SerializedPropertyType.ObjectReference
					&& currentOverrideProperty.objectReferenceValue != null
					&& !IsValidObjectAssignment(
						currentOverrideProperty.objectReferenceValue,
						componentType,
						valueOverride.fieldName)
				){
					Debug.LogWarning("Cannot convert type "
						+ currentOverrideProperty.objectReferenceValue.GetType().Name
						+ " to target type "
						+ TargetFieldOrPropertyType(componentType, valueOverride.fieldName)
					);
					currentOverrideProperty.objectReferenceValue = null;				
				}
				platformSpecificsSerObj.ApplyModifiedProperties();			
			}
			else{
				GUILayout.FlexibleSpace();
			}
			
			//Draw remove button
			if(GUILayout.Button(string.Empty, removeButtonStyle)){
				valueOverrideIndexToRemove = valueOverrideIndex;
			}

			GUILayout.EndHorizontal();
		
			//Draw field selection dropdown
			if(displayFieldSelectionDropdown){
				DrawFieldSelectionDropdown(lastFieldDropdownRects[index][valueOverrideIndex], valueOverride, fieldSelectionDropdownLabel);				
			}
		}
		
		//Apply deletion, if any
		if(valueOverrideIndexToRemove.HasValue){
			Undo.RecordObject(specifics, "delete field");
			fieldSetter.RemoveAt(valueOverrideIndexToRemove.Value);
			EditorUtility.SetDirty(specifics);
		}
		
		//Draw add field button
		GUILayout.BeginHorizontal();
		GUILayout.BeginHorizontal(); //Start clickable area def
		GUILayout.Label("Add Field");
		GUILayout.Label(string.Empty, addButtonStyle);		
		GUILayout.EndHorizontal(); //End clickable area def
		if(GUI.Button(GUILayoutUtility.GetLastRect(), string.Empty, EditorStyles.label)){
			Undo.RecordObject(specifics, "add field");
			fieldSetter.AddNew();
			EditorUtility.SetDirty(specifics);
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
	}
	
	//Helper function for DrawFieldValues
	void DrawFieldSelectionDropdown(Rect dropdownRect, PlatformSpecifics.FieldValueOverride valueOverride, string currentlySelected){
		//Gather dropdown options
		Component[] components = specifics.gameObject.GetComponents<Component>();
		List<FieldSelectOption> dropDownOptions = new List<FieldSelectOption>();
		foreach(Component comp in components){
			//Fields
			FieldInfo[] publicFields = comp.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach(FieldInfo field in publicFields){
				if(PlatformSpecificFieldValue.IsValidType(field.FieldType)){
					dropDownOptions.Add(new FieldSelectOption(comp, field));
				}
			}
		
			//Properties
			PropertyInfo[] publicProperties = comp.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach(PropertyInfo property in publicProperties){
				if(property.CanWrite && PlatformSpecificFieldValue.IsValidType(property.PropertyType)){
					dropDownOptions.Add(new FieldSelectOption(comp, property));
				}
			}
		}
	
		//Construct menu			
		GenericMenu menu = new GenericMenu();
		//Menu on-select action definition
		GenericMenu.MenuFunction2 assignSelection = (object x) => {
			Undo.RecordObject(specifics, "set field target");
			FieldSelectOption option = (FieldSelectOption)x;			
			if(option == null){				
				valueOverride.componentTypeName = "";
				valueOverride.fieldName = "";
				valueOverride.fieldValue = new PlatformSpecificFieldValue();
			}
			else{
				valueOverride.componentTypeName = option.component.GetType().Name;
				valueOverride.fieldName = option.fieldName;
				valueOverride.fieldValue = new PlatformSpecificFieldValue(option.fieldType);
			}
			EditorUtility.SetDirty(specifics);
		};
		//Add constant menu items
		menu.AddItem(new GUIContent("No Field"), false, assignSelection, null);
		if(dropDownOptions.Count > 0){
			menu.AddSeparator("");
		}
		//Add dynamic menu items
		foreach(FieldSelectOption option in dropDownOptions){
			menu.AddItem(new GUIContent(option.MenuPath), currentlySelected.Replace(".", "/") == option.MenuPath, assignSelection, option);
		}
		//Show menu
		menu.DropDown(dropdownRect);
	}
	
	void DrawSection<T>(ref bool show, string header, ref T[] array, DrawArray<T> drawArray, Constructor<T> construct, bool drawAddButton) {
		show = EditorGUILayout.Foldout(show, header);
		
		if(!show) return;
		
		int itemToDelete = -1;
		int moveItemIdx = -1;
		int moveItemDirection = 0;
		for(int i=0; i < array.Length; i++) {
			drawArray(i, ref array, ref itemToDelete, ref moveItemIdx, ref moveItemDirection);
			GUILayout.Space(5f);
		}
		
		if(itemToDelete != -1) {
			#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
			Undo.RegisterUndo(specifics, "delete array item");
			#else
			Undo.RecordObject(specifics, "delete array item");
			#endif
			List<T> list = new List<T>(array as IEnumerable<T>);
			list.RemoveAt(itemToDelete);
			array = list.ToArray();
			EditorUtility.SetDirty(specifics);
		}
		
		if(moveItemIdx != -1) {
			int newIdx = moveItemIdx + moveItemDirection;
			if(newIdx > -1 && newIdx < array.Length) {
				#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
				Undo.RegisterUndo(specifics, "move array item");
				#else
				Undo.RecordObject(specifics, "move array item");
				#endif
				List<T> list = new List<T>(array as IEnumerable<T>);
				T item = list[moveItemIdx];
				list.RemoveAt(moveItemIdx);
				list.Insert(newIdx, item);
				array = list.ToArray();
				EditorUtility.SetDirty(specifics);
			}
		}
		
		if (drawAddButton || array == null || array.Length == 0) {
			GUILayout.BeginHorizontal();
				GUILayout.Space(18f);
				if(GUILayout.Button(string.Empty, addButtonStyle)) {
					#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
					Undo.RegisterUndo(specifics, "add array item");
					#else
					Undo.RecordObject(specifics, "add array item");
					#endif
					List<T> list = new List<T>(array as IEnumerable<T>);
					list.Add(construct());
					array = list.ToArray();
					EditorUtility.SetDirty(specifics);
				}
			GUILayout.EndHorizontal();
		}
	}
	
	void DrawMoveButtons(int index, ref int moveItemIdx, ref int moveItemDirection) {
		if(GUILayout.Button(string.Empty, moveItemAboveStyle)) {
			moveItemIdx = index;
			moveItemDirection = -1;
		}
		if(GUILayout.Button(string.Empty, moveItemBelowStyle)) {
			moveItemIdx = index;
			moveItemDirection = 1;
		}
	}
	
	void DrawRemoveButton(int index, ref int itemToDelete) {
		if(GUILayout.Button(string.Empty, removeButtonStyle))
			itemToDelete = index;
	}
	
	void DrawAspectRatiosEnum(int index, ref int itemToDelete, ref int moveItemIdx, ref int moveItemDirection, ref AspectRatio _enum) {
		GUILayout.BeginHorizontal();
			//GUILayout.Space(20f);
			_enum = (AspectRatio) EditorGUILayout.EnumPopup((System.Enum)_enum, enumPopupStyle, GUILayout.Width(100f));
			EditorGUILayout.Space();
			DrawMoveButtons(index, ref moveItemIdx, ref moveItemDirection);
			EditorGUILayout.Space();
			DrawRemoveButton(index, ref itemToDelete);
		GUILayout.EndHorizontal();
	}
	
	void DrawPlatformEnum(int index, ref int itemToDelete, ref int moveItemIdx, ref int moveItemDirection, ref Platform _enum) {
		GUILayout.BeginHorizontal();
			//GUILayout.Space(20f);
			_enum = (Platform) EditorGUILayout.EnumPopup((System.Enum)_enum, enumPopupStyle, GUILayout.Width(100f));
			EditorGUILayout.Space();
			DrawMoveButtons(index, ref moveItemIdx, ref moveItemDirection);
			EditorGUILayout.Space();
			DrawRemoveButton(index, ref itemToDelete);
		GUILayout.EndHorizontal();
	}
	
	void OnDisable() {
		specifics = null;
	}
	
	//Helper function for platform-specific field values
	bool IsValidObjectAssignment(Object obj, Type componentType, string fieldName){
		if(obj == null){
			return true;
		}
		
		Type targetType = TargetFieldOrPropertyType(componentType, fieldName);
		return targetType != null && targetType.IsAssignableFrom(obj.GetType());
	}
	
	//Returns the type of the field or property with the given name on the given type.
	//If there is no field or property with the given name, returns null.
	Type TargetFieldOrPropertyType(Type componentType, string fieldName){
		FieldInfo field = componentType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
		if(field != null){
			return field.FieldType;
		}
		PropertyInfo property = componentType.GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance);
		if(property != null){
			return property.PropertyType;
		}
		return null;
	}
	
	//Digs through a PlatformSpecifics serialized object for the SerializedProperty for a specific part of a PlatformSpecificFieldValue.
	SerializedProperty GetFieldValueOverrideProperty(
			SerializedObject platformSpecificsSerObj,
			int platformIndex,
			int valueOverrideIndex,
			PlatformSpecifics.FieldValueOverride valueOverride
	){
		SerializedProperty perPlatformArray = platformSpecificsSerObj.FindProperty("fieldValuesPerPlatform");
		SerializedProperty currentPlatform = perPlatformArray.GetArrayElementAtIndex(platformIndex);
		SerializedProperty overridesArray = currentPlatform.FindPropertyRelative("values");
		SerializedProperty currentOverride = overridesArray.GetArrayElementAtIndex(valueOverrideIndex);
		SerializedProperty currentOverrideValueCollection = currentOverride.FindPropertyRelative("fieldValue");				
		return currentOverrideValueCollection.FindPropertyRelative(valueOverride.fieldValue.GetValueFieldName());
	}
}
