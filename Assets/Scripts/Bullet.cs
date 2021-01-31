using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{

		public float speed = 20;
		public float maxRange = 50;
		public bool pierce;				// see if it can pierce enemies (not used ATM)
		public float damage;			// actual damage
		public GameObject explosion;	// explosion effect when destroyed
		//
		private float distance;

		// Use this for initialization
		void Start ()
		{
				pierce = false;
				distance = 0;
		}
	
		// Update is called once per frame
		void Update ()
		{
				// we move it a certain distance on a per second basis, and update the distance field
				transform.Translate (Vector3.forward * Time.deltaTime * speed);
				distance += Time.deltaTime * speed;
				// if we reach the max distance we destroy the bullet
				if (distance >= maxRange) {
						Destroy (gameObject);
				}
		}

		// when something enters its triggered collider (i.e it hits something)
		private void OnTriggerEnter (Collider other)
		{
				// Debug.Log ("trigger entered");
				// if it hits an enemy
				if (other.gameObject.tag == "Enemy") {
						// if it doesnt pierce we create the explosion and destroy the bullet
						if (!pierce) {
								Instantiate (explosion, transform.position, Quaternion.identity);
								Destroy (gameObject);
						}
						// we call the enemy function "GetHit" passing the damage to it 
						other.gameObject.SendMessage ("GetHit", damage);



				}
		}
}
