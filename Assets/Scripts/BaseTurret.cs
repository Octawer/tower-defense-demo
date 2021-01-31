using UnityEngine;
using System.Collections;

public class BaseTurret : MonoBehaviour
{
		// this is the base turret class from where every other one inherits

		public Vector2 size;				// size of the turret in cells x cells (rows, cols)
		public int level;					// level of the turret
		public int maxLevel = 5;
		public float baseDmg;				// the base damage
		public float upgradeTime;
		public int upgradeCost;
		public int deployCost;
		public float fireDelay;				// delay between shots
		public Transform target;			// the target enemy that is grabbed on entering its triggered collider
		public GameObject turretSentinel;	// the central rotating (not always; e.g. laser) piece of geometry
		public string name;					// name of the turret
		public UISlider lvlUpBar;			// NOT USED
		public Vector3 lvlUpBarOffset = new Vector3 (0, 5, 0);
		//
		protected float fireEventTime;		// next fire time event 
		protected Color[] lvlUpColors = {	// colors of diferent levels of turrets
				Color.white,
				Color.yellow,
				Color.green,
				Color.blue,
				Color.black
		};
		protected float fireDmg;			// actual fire damage


		// Use this for initialization
		void Start ()
		{
		}
	
		// Update is called once per frame
		void Update ()
		{
	
		}

		// level up the turret
		public virtual void LevelUp ()
		{	
				// if we have not reached max level
				if (level < maxLevel) {
						// increase level
						level++;
						// change sentinel color (level goes from 1 to 5, so index is level-1)
						turretSentinel.renderer.material.color = lvlUpColors [level - 1];		
				}
			
		}

		// NOT USED
		private void UpdateSliderHP ()
		{
				lvlUpBar.gameObject.transform.position = Camera.main.WorldToViewportPoint (transform.position + lvlUpBarOffset);
				// float percentHP = currentHP / maxHP;
				// float hpBarWidth = percentHP * 100;
				// Debug.Log ("Hp bar width = " + hpBarWidth);
				// Rect hpRect = new Rect (hpBar.guiTexture.pixelInset.x, hpBar.guiTexture.pixelInset.y, hpBarWidth, hpBar.guiTexture.pixelInset.height); 
				// hpBar.guiTexture.pixelInset = hpRect;
				// GUI.DrawTexture (hpBar.guiTexture.pixelInset, hpBar.texture);
		}
}
