using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightShader : BaseDemo
{
    public Transform target;

	private void Awake()
	{
		this._isCreateBackground = false;
	}

    protected override void OnUpdate()
    {
        transform.RotateAround(new Vector3(1.2f, 1.2f, -0.5f), Vector3.forward, 4);
        target.localEulerAngles = new Vector3(Mathf.Sin(Time.realtimeSinceStartup) * 10 + 5, 0f, 0f);
    }
}
