using UnityEngine;
using System.Collections;

public class LightTest : MonoBehaviour
{
	public Transform target;

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {
		transform.RotateAround(new Vector3(1.2f,1.2f,-0.5f),Vector3.forward,4);
		target.localEulerAngles = new Vector3(Mathf.Sin(Time.realtimeSinceStartup)*10+5,0f,0f);
	}
}
