using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if !UNITY_METRO
using System.Reflection;
#endif

public enum HorizontalAlignments
{
	Left,
	Right,
	Center
}

public enum VerticalAlignments
{
	Top,
	Bottom,
	Center
}

public class PlatformSpecifics : MonoBehaviour {
	
	public Platform[] restrictPlatform;
	
	[System.Serializable]
	public class MaterialPerPlatform {
		public Platform platform;
		public Material mat;
		
		public MaterialPerPlatform (Platform _platform, Material _mat) {
			platform = _platform;
			mat = _mat;
		}
	}
	public MaterialPerPlatform[] materialPerPlatform;
	
	
	[System.Serializable]
	public class LocalScalePerPlatform {
		public Platform platform;
		public Vector3 localScale;
		
		public LocalScalePerPlatform (Platform _platform, Vector3 _localScale) {
			platform = _platform;
			localScale = _localScale;
		}
	}
	public LocalScalePerPlatform[] localScalePerPlatform;
	
	[System.Serializable]
	public class LocalScalePerAspectRatio {
		public AspectRatio aspectRatio;
		public Vector3 localScale;
		
		public LocalScalePerAspectRatio (AspectRatio _aspectRatio, Vector3 _localScale) {
			aspectRatio = _aspectRatio;
			localScale = _localScale;
		}
	}
	public LocalScalePerAspectRatio[] localScalePerAspectRatio;
	
	[System.Serializable]
	public class LocalPositionPerPlatform {
		public Platform platform;
		public Vector3 localPosition;
		
		public LocalPositionPerPlatform (Platform _platform, Vector3 _localPosition) {
			platform = _platform;
			localPosition = _localPosition;
		}
	}
	public LocalPositionPerPlatform[] localPositionPerPlatform;
	
	[System.Serializable]
	public class LocalPositionPerAspectRatio {
		public AspectRatio aspectRatio;
		public Vector3 localPosition;
		
		public LocalPositionPerAspectRatio (AspectRatio _aspectRatio, Vector3 _localPosition) {
			aspectRatio = _aspectRatio;
			localPosition = _localPosition;
		}
	}
	public LocalPositionPerAspectRatio[] localPositionPerAspectRatio;
	
	[System.Serializable]
	public class AnchorPosition {
		public Camera alignmentCamera;
		public HorizontalAlignments horizontalAlignment;
		public VerticalAlignments verticalAlignment;
		public bool positionXAsPercent, positionYAsPercent, centerX = true, centerY = true;
		public Vector2 screenPosition;
		
		public AnchorPosition (HorizontalAlignments _horizontalAlignment, VerticalAlignments _verticalAlignment, bool _positionXAsPercent, bool _positionYAsPercent, bool _centerX, bool _centerY, Vector2 _screenPosition) {
			horizontalAlignment = _horizontalAlignment;
			verticalAlignment = _verticalAlignment;
			positionXAsPercent = _positionXAsPercent;
			positionYAsPercent = _positionYAsPercent;
			centerX = _centerX;
			centerY = _centerY;
			screenPosition = _screenPosition;
		}
	}
	public AnchorPosition[] anchorPositions;
	
	[System.Serializable]
	public class FontPerPlatform {
		public Platform platform;
		public Font font;
		public Material mat;
		
		public FontPerPlatform (Platform _platform, Font _font, Material _mat) {
			platform = _platform;
			font = _font;
			mat = _mat;
		}
	}
	public FontPerPlatform[] fontPerPlatform;
	
	[System.Serializable]
	public class TextMeshTextPerPlatform {
		public Platform platform;
		public string text;
		
		public TextMeshTextPerPlatform (Platform _platform, string _text) {
			platform = _platform;
			text = _text;
		}
	}
	public TextMeshTextPerPlatform[] textMeshTextPerPlatform;
	
	[System.Serializable]
	public class FieldValuesPerPlatform {
		public Platform platform;
		public List<FieldValueOverride> values = new List<FieldValueOverride>();
		
		public FieldValuesPerPlatform(Platform _platform){
			platform = _platform;
		}
		
		public void AddNew(){
			values.Add(new FieldValueOverride());
		}
		
		public void RemoveAt(int index){
			values.RemoveAt(index);
		}
		
		public void ApplyValues(GameObject target){
			foreach(FieldValueOverride valueOverride in values){
				if(valueOverride != null && valueOverride.ValidTarget){
					valueOverride.ApplyValue(target);
				}
			}
		}		
	}
	public FieldValuesPerPlatform[] fieldValuesPerPlatform;
	
	[System.Serializable]
	public class FieldValueOverride{
		public string componentTypeName;
		public string fieldName;
		public PlatformSpecificFieldValue fieldValue;
		
		public bool ValidTarget{
			get{
				return !string.IsNullOrEmpty(componentTypeName) && !string.IsNullOrEmpty(fieldName);
			}
		}
		
		public void ApplyValue(GameObject target){
#if !UNITY_METRO
			Component component = target.GetComponent(componentTypeName);
			if(component != null){
				FieldInfo field = component.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
				if(field != null){
					object val = fieldValue.GetValue();
					if(val == null || field.FieldType.IsAssignableFrom(val.GetType())){
						field.SetValue(component, val);
					}
					else{
						field.SetValue(component, null);
					}
					return;
				}
				PropertyInfo property = component.GetType().GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public);
				if(property != null){
					object val = fieldValue.GetValue();
					if(val == null || property.PropertyType.IsAssignableFrom(val.GetType())){
						property.SetValue(component, fieldValue.GetValue(), null);
					}
					else{
						property.SetValue(component, null, null);
					}
					return;
				}
				Debug.LogError("No public field or property " + fieldName + " on component " + component, component);
			}
#else
			//TODO: Add WSA support
			Debug.LogError("Field value overrides not supported in WSA");
#endif
		}
	}
	
	#if UNITY_EDITOR
	public static bool UseEditorApplyMode;
	#endif
	
	void Awake () {
		Init();
		ApplySpecifics(Platforms.platform);
	}
	
	public void Init() {
		if(restrictPlatform == null) restrictPlatform = new Platform[0];
		if(materialPerPlatform == null) materialPerPlatform = new MaterialPerPlatform[0];
		if(localScalePerPlatform == null) localScalePerPlatform = new LocalScalePerPlatform[0];
		if(localScalePerAspectRatio == null) localScalePerAspectRatio = new LocalScalePerAspectRatio[0];
		if(localPositionPerPlatform == null) localPositionPerPlatform = new LocalPositionPerPlatform[0];
		if(localPositionPerAspectRatio == null) localPositionPerAspectRatio = new LocalPositionPerAspectRatio[0];
		if(anchorPositions == null) anchorPositions = new AnchorPosition[0];
		if(fontPerPlatform == null) fontPerPlatform = new FontPerPlatform[0];
		if(textMeshTextPerPlatform == null) textMeshTextPerPlatform = new TextMeshTextPerPlatform[0];
		if(fieldValuesPerPlatform == null) fieldValuesPerPlatform = new FieldValuesPerPlatform[0];
	}
	
	private bool isCompatiblePlatform(Platform platform1, Platform platform2) {
		if (Platforms.IsiOSPlatform(platform1) && platform2 == Platform.iOS) return true;
		else return (platform1 == platform2);
	}
	
	public bool PlatformSupportsFieldOverrides(Platform platform){
		//Due to restrictions on use of reflection in Windows Store Apps,
		//Field overrides are currently not supported on that platform.
		return platform != Platform.Windows8;
	}
	
	public void ApplySpecifics(Platform platform) {
		ApplySpecifics(platform, true);
	}
	
	public void ApplySpecifics(Platform platform, bool applyPlatformRestriction) {
		if(applyPlatformRestriction) {
			bool currentPlatformActive = ApplyRestrictPlatform(platform);
			if(!currentPlatformActive)
				return;
		}
		
		ApplyMaterial(platform);
		ApplyLocalScale(platform);
		ApplyLocalPosition(platform);
		ApplyFont(platform);
		ApplyTextMeshText(platform);
		ApplyFieldValues(platform);
	}
	
	public bool ApplyRestrictPlatform(Platform platform) {
		if(restrictPlatform != null && restrictPlatform.Length > 0) {
			bool foundPlatform = false;
			for(int i=0; i<restrictPlatform.Length; i++) {
				if (isCompatiblePlatform(platform, restrictPlatform[i])) {
					foundPlatform = true;
					break;
				}
			}
			if(!foundPlatform) {
				#if UNITY_EDITOR
				if (!UseEditorApplyMode) {
					if(Application.isEditor)
						DestroyImmediate(gameObject, true);
					else
						Destroy(gameObject);
				}
				#else
				if(Application.isEditor)
					DestroyImmediate(gameObject, true);
				else
					Destroy(gameObject);
				#endif
				
				return false;
			} else {
				return true;
			}
		}
		return true;
	}
	
	public void ApplyMaterial(Platform platform) {
		if(materialPerPlatform != null) {
			foreach(MaterialPerPlatform pair in materialPerPlatform) {
				if (isCompatiblePlatform(platform, pair.platform)) {
					GetComponent<Renderer>().sharedMaterial = pair.mat;
					break;
				}
			}
		}
	}
	
	public void ApplyLocalScale(Platform platform) {
		#if UNITY_4_6
		var rectTransform = GetComponent<RectTransform>();
		#endif

		if(localScalePerPlatform != null) {
			foreach(LocalScalePerPlatform pair in localScalePerPlatform) {
				if (isCompatiblePlatform(platform, pair.platform)) {
					#if UNITY_4_6
					if (rectTransform != null) rectTransform.localScale = pair.localScale;
					else transform.localScale = pair.localScale;
					#else
					transform.localScale = pair.localScale;
					#endif
					break;
				}
			}
		}
		
		if(Platforms.IsPlatformAspectBased(platform.ToString()) && localScalePerAspectRatio != null) {
			foreach(LocalScalePerAspectRatio pair in localScalePerAspectRatio) {
				if(AspectRatios.GetAspectRatio() == pair.aspectRatio) {
					#if UNITY_4_6
					if (rectTransform != null) rectTransform.localScale = pair.localScale;
					else transform.localScale = pair.localScale;
					#else
					transform.localScale = pair.localScale;
					#endif
					break;
				}
			}
		}
	}
	
	public void ApplyLocalPosition(Platform platform) {
		#if UNITY_4_6
		var rectTransform = GetComponent<RectTransform>();
		#endif
		
		if(localPositionPerPlatform != null) {
			foreach(LocalPositionPerPlatform pair in localPositionPerPlatform) {
				if (isCompatiblePlatform(platform, pair.platform)) {
					#if UNITY_4_6
					if (rectTransform != null) rectTransform.localPosition = pair.localPosition;
					else transform.localPosition = pair.localPosition;
					#else
					transform.localPosition = pair.localPosition;
					#endif
					break;
				}
			}
		}
		
		if(Platforms.IsPlatformAspectBased(platform.ToString()) && localPositionPerAspectRatio != null) {
			foreach(LocalPositionPerAspectRatio pair in localPositionPerAspectRatio) {
				if(AspectRatios.GetAspectRatio() == pair.aspectRatio) {
					#if UNITY_4_6
					if (rectTransform != null) rectTransform.localPosition = pair.localPosition;
					else transform.localPosition = pair.localPosition;
					#else
					transform.localPosition = pair.localPosition;
					#endif
					break;
				}
			}
		}
		
		// if an anchor position, it will override others
		#if UNITY_4_6
		if(anchorPositions != null && rectTransform == null) {
		#else
		if(anchorPositions != null) {
		#endif
			foreach(AnchorPosition pair in anchorPositions) {
				// make sure we have a camera to align to
				var cam = pair.alignmentCamera;
				if (cam == null)
				{
					Debug.LogError("No alignment camera is attached to GameObject: " + gameObject.name);
					continue;
				}
				
				// get screen size
				float screenWidth = Screen.width, screenHeight = Screen.height;
				#if UNITY_EDITOR
				if (UseEditorApplyMode) AspectRatios.getScreenSizeHack(out screenWidth, out screenHeight);
				#endif
				
				// get position
				var pos = pair.screenPosition;
				
				if (pair.positionXAsPercent) 
					pos.x *= screenWidth;// * 0.01f;
				if (pair.positionYAsPercent) 
					pos.y *= screenHeight;// * 0.01f;
				
				// global calculations
				var ray = cam.ScreenPointToRay(pos);
				var planePoint = RayInersectPlane(ray, -cam.transform.forward, transform.position);
				
				ray = cam.ScreenPointToRay(Vector3.zero);
				var bottomLeft = RayInersectPlane(ray, -cam.transform.forward, transform.position);
				
				ray = cam.ScreenPointToRay(new Vector3(screenWidth, screenHeight, 0));
				var topRight = RayInersectPlane(ray, -cam.transform.forward, transform.position);
				
				// get object radius
				float radius = 0;
				if (GetComponent<Renderer>() != null) radius = (GetComponent<Renderer>().bounds.max - GetComponent<Renderer>().bounds.min).magnitude * .5f;
				
				// get new projected width and height
				var screenSize = (topRight - bottomLeft);
				screenWidth = Vector3.Dot(screenSize, cam.transform.right);
				screenHeight = Vector3.Dot(screenSize, cam.transform.up);
				
				// horizontal alignment types
				switch (pair.horizontalAlignment)
				{
					case HorizontalAlignments.Left: planePoint += cam.transform.right * (!pair.centerX ? radius : 0); break;
					case HorizontalAlignments.Right: planePoint += cam.transform.right * (screenWidth - (!pair.centerX ? radius : 0)); break;
					case HorizontalAlignments.Center: planePoint += cam.transform.right * (screenWidth * .5f); break;
				}
				
				// vertical alignment types
				switch (pair.verticalAlignment)
				{
					case VerticalAlignments.Bottom: planePoint += cam.transform.up * (!pair.centerY ? radius : 0); break;
					case VerticalAlignments.Top: planePoint += cam.transform.up * (screenHeight - (!pair.centerY ? radius : 0)); break;
					case VerticalAlignments.Center: planePoint += cam.transform.up * (screenHeight * .5f); break;
				}
				
				// set final position
				transform.position = planePoint;
			}
		}
	}
	
	private Vector3 PointInersectPlane(Vector3 vector, Vector3 planeNormal, Vector3 planeLocation)
	{
		return vector - (planeNormal * Vector3.Dot(vector-planeLocation, planeNormal));
	}
	
	private Vector3 RayInersectPlane(Ray ray, Vector3 planeNormal, Vector3 planeLocation)
	{
		float dot = (-(planeNormal.x*planeLocation.x) - (planeNormal.y*planeLocation.y) - (planeNormal.z*planeLocation.z));
		float dot3 = (planeNormal.x*ray.direction.x) + (planeNormal.y*ray.direction.y) + (planeNormal.z*ray.direction.z);
		float dot2 = -((dot + (planeNormal.x*ray.origin.x) + (planeNormal.y*ray.origin.y) + (planeNormal.z*ray.origin.z)) / dot3);
		return (ray.origin + (dot2*ray.direction));
	}
	
	public void ApplyFont(Platform platform) {
		if(fontPerPlatform != null) {
			foreach(FontPerPlatform pair in fontPerPlatform) {
				if (isCompatiblePlatform(platform, pair.platform)) {
					this.GetComponent<TextMesh>().font = pair.font;
					GetComponent<Renderer>().sharedMaterial = pair.mat;
					break;
				}
			}
		}
	}
	
	public void ApplyTextMeshText(Platform platform) {
		if(textMeshTextPerPlatform != null) {
			foreach(TextMeshTextPerPlatform pair in textMeshTextPerPlatform) {
				if (isCompatiblePlatform(platform, pair.platform)) {
					this.GetComponent<TextMesh>().text = pair.text;
					break;
				}
			}
		}
	}
	
	public void ApplyFieldValues(Platform platform){
		if(fieldValuesPerPlatform != null){
			foreach(FieldValuesPerPlatform pair in fieldValuesPerPlatform){
				if(isCompatiblePlatform(platform, pair.platform)) {
					pair.ApplyValues(gameObject);
					break;
				}				
			}
		}
	}
}
