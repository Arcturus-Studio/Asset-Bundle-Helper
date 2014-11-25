using UnityEngine;
using System.Collections;

public class PlatformFieldTest : MonoBehaviour {

	public string Property{
		get{
			return prop;
		}
		set{
			prop = value;
		}
	}
	[SerializeField] private string prop;

	public string Field;
	public GameObject obj;
	
	void Start(){
		Debug.Log("Field: " + Field + " Property: " + Property );
	}
}
