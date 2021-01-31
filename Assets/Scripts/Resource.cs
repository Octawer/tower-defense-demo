using UnityEngine;
using System.Collections;

public class Resource : MonoBehaviour
{
		public GameObject empyEffect;		// particle effect that plays when its depleted
		public int amount = 50;				// amount of resource
	
		void Start ()
		{

		}

		void Update ()
		{
	
		}

		// we deduct an amount and check if its empty, destroying it accordingly
		private void SubtractResource ()
		{
				amount -= 1;
				if (amount <= 0) {
						Instantiate (empyEffect, transform.position, Quaternion.identity);
						Destroy (gameObject);
				}
		}

		// if an enemy enters its triggered collider we deduct a certain amount of resource
		private void OnTriggerEnter (Collider other)
		{
				Debug.Log ("Enemy stole crystal!");
				if (other.gameObject.tag == "Enemy") {
						// when an enemy enters range we grab our target position
						SubtractResource ();
				}
		}
}
