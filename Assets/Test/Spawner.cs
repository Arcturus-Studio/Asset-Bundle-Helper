using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {

	public GameObject thing;
	public Color color;

	void Start () {
		GameObject obj = Instantiate(thing, transform.position, Quaternion.identity) as GameObject;
		obj.GetComponent<Renderer>().sharedMaterial.color = color;
	}
}
