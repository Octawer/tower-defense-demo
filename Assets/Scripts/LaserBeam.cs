using UnityEngine;
using System.Collections;

[RequireComponent (typeof(LineRenderer))]

public class LaserBeam : MonoBehaviour
{
	
		public float laserWidth = 1.0f;
		public float maxLength = 50.0f;
		public float damage = 0.5f;				// we set the damage somewhat low cause it's gonna do damage over time
		public Transform target;				
		public GameObject laserHit;				// hit effect
		private LineRenderer lineRenderer;
		private GameObject currentHit;			// current hit effect (instance clones)
	

		void Start ()
		{
				lineRenderer = GetComponent<LineRenderer> ();
				lineRenderer.SetWidth (laserWidth, laserWidth);
				// there will only be 2 vertexes: start at turret sentinel, and end at target
				lineRenderer.SetVertexCount (2);
		}

		void Update ()
		{
				// if we have a target we draw the laser line from turret sentinel to target
				// and start dealing DoT (as it is in the update function its damage per frame)
				if (target) {
						lineRenderer.SetPosition (0, gameObject.transform.position);
						lineRenderer.SetPosition (1, target.position);
						LaserHit ();
				} 
				
		}

		private void LaserHit ()
		{	
				// if we have a target
				if (target) {
						// if there are no end hit effects (hit effect at target, flame effect in this case)
						if (!currentHit) {
								// we create a hit effect at target position
								currentHit = (GameObject)Instantiate (laserHit, target.position, Quaternion.identity);
						} else {
								// if there are existing hit effects, we update its position to target position
								currentHit.transform.position = target.position;
						}
						// we deal damage over time
						LaserDoT ();
				}
				
		}

		// if there is a current hit we destroy it with a delay
		public void EndHit (float delay)
		{
				if (currentHit) {
						Destroy (currentHit.gameObject, delay);
				}
		}

		// damage dealing over time
		private void LaserDoT ()
		{
				// if we have a target and it's an enemy we send it a message to receive damage
				// as long as it is in the turret range (collider range)
				if (target && target.gameObject.tag == "Enemy") {
						target.gameObject.SendMessage ("GetHit", damage);
				}
				
		}
}