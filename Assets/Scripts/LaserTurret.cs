using UnityEngine;
using System.Collections;

public class LaserTurret : BaseTurret
{

		public LaserBeam beam;					// projectile. Continuous laser beam
		public GameObject laserSocket;			// position from where the laser starts
		public GameObject laserEmit;			// laser emission effect
		private LaserBeam myLaserBeam;			// actual laser beam
		private GameObject mylaserEmit;			// actual laser emission effect


		void Start ()
		{
				// we initialize several variables (level, color, delay, damage, size, position, etc.)
				level = 1;
				turretSentinel.renderer.material.color = lvlUpColors [level - 1];
				// laser turret occupies 1x2 cells
				size = new Vector2 (1, 2);	
				fireDelay = 5.0f;
				// reposition its heighta bit
				transform.position = new Vector3 (transform.position.x, transform.position.y + 6, transform.position.z);
				name = "Laser Turret";
				fireDmg = baseDmg * level;
		}

		void Update ()
		{
				// check if there is a target inside collider range
				if (target) {
						// if fire lapse reached, it fires at target
						Debug.Log ("targetInRange");
						if (LapseReached (fireEventTime)) {
								// fire
								FireLaser ();
						}
				} else {
						// if we have no target we destroy the beam and effects
						DisposeLaser ();	
				}
		
		}

		// check if we have reached a deadline
		private bool LapseReached (float limit)
		{
				// check if game time reached the limit time param
				return Time.time >= limit;
		}

		// shoot the laser beam
		public void FireLaser ()
		{
				if (target) {
						audio.Play ();
						// update the fire event time
						fireEventTime = Time.time + fireDelay;
						// create the laser emit effect and the real beam at start position
						mylaserEmit = (GameObject)Instantiate (laserEmit, laserSocket.transform.position, laserSocket.transform.rotation);
						myLaserBeam = (LaserBeam)Instantiate (beam, laserSocket.transform.position, laserSocket.transform.rotation);
						// pass the target and the damage to the beam
						myLaserBeam.target = target;
						myLaserBeam.damage = fireDmg;
						
				}
		}

		// destroy the beam, the emit effect, and stop the audio
		private void DisposeLaser ()
		{
				if (myLaserBeam) {
						myLaserBeam.EndHit (0);
						Destroy (myLaserBeam.gameObject, 0);
						
				}	
				if (mylaserEmit) {
						Destroy (mylaserEmit.gameObject, 0);
				}
				if (audio.isPlaying) {
						audio.Stop ();
				}
				
		}
	
		private void OnTriggerEnter (Collider other)
		{
				Debug.Log ("trigger entered");
				if (other.gameObject.tag == "Enemy") {
						// when an enemy enters range we get its transform
						target = other.gameObject.transform;
				}
		}
	
		private void OnTriggerExit (Collider other)
		{
				Debug.Log ("trigger exited");
				if (other.gameObject.tag == "Enemy") {
						// when an enemy exits range we destroy the laser and effects
						// and release the target
						DisposeLaser ();
						target = null;

				}
		}
		
		// upgrade the turret 
		public override void LevelUp ()
		{
				// if we have not reached the level cap
				if (level < maxLevel) {
						// we call the base class method
						base.LevelUp ();
						fireDmg = baseDmg * level;
						upgradeCost += level * 100;
				}
		}
}
