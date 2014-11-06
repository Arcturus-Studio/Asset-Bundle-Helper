using UnityEngine;
using System.Collections;

public class MeshSwapExample : MonoBehaviour {

	public GameObject highPolyObject;
	public GameObject lowPolyObject;
	
	void Awake () {
		if(Platforms.platform == Platform.iPhone)
			Instantiate(lowPolyObject, transform.position, Quaternion.identity);
		else 
			Instantiate(highPolyObject, transform.position, Quaternion.identity);
	}
}
