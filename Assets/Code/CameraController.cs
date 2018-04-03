using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _cam;
     
	void Start ()
	{
	    _cam = GetComponent<Camera>();
	}
	
	void Update ()
	{
	    HandleInput();
	}

    private void HandleInput()
    {
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            x += 1f;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            x -= 1f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            y += 1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            y -= 1f;

        x = transform.rotation.eulerAngles.x + x;
        y = transform.rotation.eulerAngles.y + y;

        if (x > 40f && x < 50f)
            x = 40f;
        if (x < 335f && x > 325f)
            x = 335f;

        transform.rotation = Quaternion.Euler(x, y, 0);
    }
}
