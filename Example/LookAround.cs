using UnityEngine;
using System.Collections;

public class LookAround : MonoBehaviour
{
	public float rotMult = 2f;
	public float zoomMult = 5f;
	public bool reduceRotation;
	public float normalFOV = 60f;
	Camera mainCam;
	float rotateMult;
	
	void Start ()
	{
		mainCam = Camera.main;
	}
	
	void Update ()
	{
		rotateMult = (reduceRotation ? (rotMult*mainCam.fieldOfView) / normalFOV : rotMult);
		transform.Rotate (Input.GetAxisRaw ("Vertical") * -rotateMult, Input.GetAxisRaw ("Horizontal") * rotateMult, 0);
		mainCam.fieldOfView -= (Input.GetAxisRaw ("Mouse ScrollWheel") * zoomMult);
		if(mainCam.fieldOfView < .5f)
		mainCam.fieldOfView = .5f;
		if(mainCam.fieldOfView > 179.5f)
		mainCam.fieldOfView = 179.5f;
	}
}
