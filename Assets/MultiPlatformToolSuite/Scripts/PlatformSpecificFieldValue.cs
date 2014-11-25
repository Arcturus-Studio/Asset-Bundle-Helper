using UnityEngine;
using System.Collections;
using System.Reflection;
using Type = System.Type;
using Array = System.Array;

[System.Serializable]
public class PlatformSpecificFieldValue{
	private static Type[] validTypes = {
		typeof(int),
		typeof(float),
		typeof(string),
		typeof(bool),
		typeof(Object)
	};
	
	public static bool IsValidType(Type type){
		return Array.Exists(validTypes, x => x.IsAssignableFrom(type));
	}
	
	//Enum values must match index of corresponding type in validTypes
	private enum DataMode {Int = 0, Float = 1, String = 2, Bool = 3, Object = 4}
	
	[SerializeField] private DataMode mode;
	[SerializeField] private int intValue;
	[SerializeField] private float floatValue;
	[SerializeField] private string stringValue;
	[SerializeField] private bool boolValue;
	[SerializeField] private Object objectValue;
	
	public PlatformSpecificFieldValue(){
		
	}
	
	public PlatformSpecificFieldValue(Type type){
		SetModeForType(type);
	}
	
	public object GetValue(){
		switch(mode){
			case DataMode.Int:
				return intValue;
			case DataMode.Float:
				return floatValue;
			case DataMode.String:
				return stringValue;
			case DataMode.Bool:
				return boolValue;
			case DataMode.Object:
				return objectValue;
			default:
				return null;
		}
	}
	
	public string GetValueFieldName(){
		switch(mode){
			case DataMode.Int:
				return "intValue";
			case DataMode.Float:
				return "floatValue";
			case DataMode.String:
				return "stringValue";
			case DataMode.Bool:
				return "boolValue";
			case DataMode.Object:
				return "objectValue";
			default:
				return null;
		}
	}
	
	private void SetModeForType(Type type){
		for(int i = 0; i < validTypes.Length; i++){
			if(validTypes[i].IsAssignableFrom(type)){
				mode = (DataMode)i;
				return;
			}
		}
		Debug.LogError("No valid DataMode for type " + type);
	}
}