using UnityEngine;
using UnityEditor;

public enum EditorPlatform {iPhone, WebPlayer, iPad, iPhoneRetina, Standalone, Android, FlashPlayer, NaCl, iPadRetina, iPhone5, iPhone6, WP8, Windows8, BB10};

public class EasyPlatform : EditorWindow {
	
	static Platform currentPlatform;
	static EditorPlatform currentEditorPlatform;
	
    [MenuItem("Window/MultiPlatform ToolKit/Easy Platform %#l", false, 3)] //Cmd+Shift+L
    static void Init () {
        EasyPlatform window = (EasyPlatform)EditorWindow.GetWindow(typeof(EasyPlatform));
		window.Show();
    }
    
    [MenuItem("Window/MultiPlatform ToolKit/Easy Platform Apply %k", false, 3)] //Cmd+Shift+L
    static void ApplyToScene () {
        applyPlatformToScene();
    }
	
	void OnEnable() {
		currentPlatform = (Platform) System.Enum.Parse(typeof(Platform), EditorPrefs.GetString(Platforms.editorPlatformOverrideKey, "Standalone"));
		if (currentPlatform == Platform.iOS) currentPlatform = Platform.iPhone;
		currentEditorPlatform = convertPlatform(currentPlatform);
		
		minSize = new Vector2(160, 20);
		maxSize = new Vector2(300, 40);
	}
	
	void OnGUI() {
		currentEditorPlatform = (EditorPlatform)EditorGUILayout.EnumPopup("Platform: ", (System.Enum)currentEditorPlatform);
		currentPlatform = convertPlatform(currentEditorPlatform);
		if(GUI.changed) {
			EditorPrefs.SetString(Platforms.editorPlatformOverrideKey, currentPlatform.ToString());
		}
		
		// apply changes to scene
		if (GUILayout.Button("Apply To Scene")) applyPlatformToScene();
	}
	
	private Platform convertPlatform(EditorPlatform platform) {
		switch (platform)
		{
			case EditorPlatform.iPhone: return Platform.iPhone;
			case EditorPlatform.WebPlayer: return Platform.WebPlayer;
			case EditorPlatform.iPad: return Platform.iPad;
			case EditorPlatform.iPhoneRetina: return Platform.iPhoneRetina;
			case EditorPlatform.Standalone: return Platform.Standalone;
			case EditorPlatform.Android: return Platform.Android;
			case EditorPlatform.FlashPlayer: return Platform.FlashPlayer;
			case EditorPlatform.NaCl: return Platform.NaCl;
			case EditorPlatform.iPadRetina: return Platform.iPadRetina;
			case EditorPlatform.iPhone5: return Platform.iPhone5;
			case EditorPlatform.iPhone6: return Platform.iPhone6;
			case EditorPlatform.WP8: return Platform.WP8;
			case EditorPlatform.Windows8: return Platform.Windows8;
			case EditorPlatform.BB10: return Platform.BB10;
			default:
				Debug.LogError("Unable to convert EditorPlatform to Platform!");
				return Platform.iPhone;
		}
	}
	
	private EditorPlatform convertPlatform(Platform platform) {
		switch (platform)
		{
			case Platform.iPhone: return EditorPlatform.iPhone;
			case Platform.WebPlayer: return EditorPlatform.WebPlayer;
			case Platform.iPad: return EditorPlatform.iPad;
			case Platform.iPhoneRetina: return EditorPlatform.iPhoneRetina;
			case Platform.Standalone: return EditorPlatform.Standalone;
			case Platform.Android: return EditorPlatform.Android;
			case Platform.FlashPlayer: return EditorPlatform.FlashPlayer;
			case Platform.NaCl: return EditorPlatform.NaCl;
			case Platform.iPadRetina: return EditorPlatform.iPadRetina;
			case Platform.iPhone5: return EditorPlatform.iPhone5;
			case Platform.iPhone6: return EditorPlatform.iPhone6;
			case Platform.WP8: return EditorPlatform.WP8;
			case Platform.Windows8: return EditorPlatform.Windows8;
			case Platform.BB10: return EditorPlatform.BB10;
			default:
				Debug.LogError("Unable to convert Platform to EditorPlatform!");
				return EditorPlatform.iPhone;
		}
	}
	
	private static void applyPlatformToScene()
	{
		AspectRatios.UseEditorResolutionHack = true;
		PlatformSpecifics.UseEditorApplyMode = true;
		try
		{
			foreach (GameObject obj in GameObject.FindObjectsOfType(typeof(GameObject)))
			{
				var platformSpecifics = obj.GetComponent<PlatformSpecifics>();
				if (platformSpecifics == null) continue;
				
				platformSpecifics.Init();
				platformSpecifics.ApplySpecifics(currentPlatform);
			}
		}
		catch (System.Exception e)
		{
			AspectRatios.UseEditorResolutionHack = false;
			PlatformSpecifics.UseEditorApplyMode = false;
			Debug.LogError(e.Message);
		}
		
		AspectRatios.UseEditorResolutionHack = false;
		PlatformSpecifics.UseEditorApplyMode = false;
	}
	
	//Do we want to remove the override when the editor window is closed?
//	void OnDisable() {
//		EditorPrefs.DeleteKey(PrefKeys.editorPlatformOverride);
//	}
}
