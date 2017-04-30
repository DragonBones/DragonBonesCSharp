using UnityEngine;
using System.Collections;

public class LightTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		transform.RotateAround(new Vector3(1.2f,1.2f,-0.5f),Vector3.forward,4);
	}
}
