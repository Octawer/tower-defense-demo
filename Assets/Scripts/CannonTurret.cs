using UnityEngine;
using System.Collections;

public class CannonTurret : BaseTurret
{
	
		public Bullet projectile;				// turret projectile
		public float aimDelay = 0.5f;
		public GameObject[] cannonSockets;		// transforms for instantiating bullets
		private float aimEventTime;
	
		void Start ()
		{
				// set its level and color
				level = 1;
				turretSentinel.renderer.material.color = lvlUpColors [level - 1];
				// cannon turret occupies 1x1 cells
				size = new Vector2 (1, 1);	
				fireDelay = 2.5f;
				// reposition its height
				transform.position = new Vector3 (transform.position.x, transform.position.y + 2, transform.position.z);
				name = "Cannon Turret";
				// set its damage proportional to its level
				fireDmg = baseDmg * level;
		}

		void Update ()
		{
				// check if there is a target inside collider range
				if (target) {
						// if aim lapse reached, it aims at target
						Debug.Log ("targetInRange");
						if (LapseReached (aimDelay)) {
								//aim
								AimTarget ();
						}
						if (LapseReached (fireEventTime)) {
								// fire
								FireProjectile ();
						}
				}
	
		}

		// check if we have reached a deadline
		private bool LapseReached (float limit)
		{
				// check if game time reached the limit time param
				return Time.time >= limit;
		}

		// shoot
		public void FireProjectile ()
		{
				if (target) {
						audio.Play ();
						// update the next fireEvent time
						fireEventTime = Time.time + fireDelay;
						// instantiate bullets for every cannon (in case we have more)
						foreach (GameObject cannon in cannonSockets) {
								// create the bullet in the cannon positions
								Bullet cannonBullet = (Bullet)Instantiate (projectile, cannon.transform.position, cannon.transform.rotation);
								// set the bullet damage as the current fire damage
								cannonBullet.damage = fireDmg;

						}
				}
		}

		// aim
		public void AimTarget ()
		{
				if (target) {
						// update the next aim event time
						aimEventTime = Time.time + aimDelay;
						// we rotate the sentinel smoothly according to the aim position
						Vector3 aimPosition = target.transform.position - transform.position;
						Quaternion aimRotation = Quaternion.LookRotation (aimPosition);
						turretSentinel.transform.rotation = Quaternion.Lerp (turretSentinel.transform.rotation, aimRotation, 0.5f); 
				}
		}

		private void OnTriggerEnter (Collider other)
		{
				Debug.Log ("trigger entered");
				if (other.gameObject.tag == "Enemy") {
						// when an enemy enters range we grab our target position
						target = other.gameObject.transform;
				}
		}

		private void OnTriggerExit (Collider other)
		{
				Debug.Log ("trigger exited");
				if (other.gameObject.tag == "Enemy") {
						// when an enemy exits range we release our target
						target = null;
				}
		}

		public override void LevelUp ()
		{
				// we call the parent class method
				base.LevelUp ();
				// we upgrade its attirbutes according to its level
				if (level < maxLevel) {
						fireDmg = baseDmg * level;
						upgradeCost += level * 50;
				}
		}
}
