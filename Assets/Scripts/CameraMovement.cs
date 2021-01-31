using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{

		public float speed = 1.0f;
		public float rotationSpeed = 5.0f;
		private Vector3 originalPos = Vector3.zero;
		private Quaternion originalRot = Quaternion.identity;

		// Use this for initialization
		void Start ()
		{
				// we store the camera initial position and rotation
				originalPos = transform.position;
				originalRot = transform.rotation;
				Debug.Log ("Camera position: " + originalPos);
		}
	
		// Update is called once per frame
		void Update ()
		{

				// get the movements multiplying the inputs by the speeds
				float translationV = Input.GetAxis ("Vertical") * speed;
				float translationH = Input.GetAxis ("Horizontal") * speed;
				float rotationH = Input.GetAxis ("Mouse X") * rotationSpeed;
				float rotationV = Input.GetAxis ("Mouse Y") * rotationSpeed;

				// we make the movement frame rate independent
				translationV *= Time.deltaTime;
				translationH *= Time.deltaTime;
				rotationH *= Time.deltaTime;
				rotationV *= Time.deltaTime;

				// we make a forward / sideways movement depending on the inputs
				transform.Translate (translationH, 0, translationV);
				// we rotate the camera when right click is pressed
				if (Input.GetMouseButton (1)) {
						transform.Rotate (-rotationV, rotationH, 0);
				}
				// we reset the camera with middle click
				if (Input.GetMouseButton (2)) {
						Debug.Log ("Camera position reset to: " + originalPos);
						transform.position = originalPos;
						transform.rotation = originalRot;
				}
		}
}
