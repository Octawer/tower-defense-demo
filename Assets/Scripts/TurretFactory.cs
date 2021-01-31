using UnityEngine;
using System.Collections;

public class TurretFactory : MonoBehaviour
{

		public BaseTurret currentTurret;
		public BaseTurret[] turrets;				// array of turret types
		public static int cannon = 0;
		public static int laser = 1;
		public static int unknown = 2;				// we can add more types here if we had them
	
		void Start ()
		{
		// if array is not empty we assign the first element as the default current turret type
				if (turrets.Length > 0) {
						Debug.Log ("turret types = " + turrets.Length);
						currentTurret = turrets [0];
				}
	
		}
	
		// Update is called once per frame
		void Update ()
		{
	
		}

		// NOT USED ATM
		public BaseTurret GetTurret (int type)
		{
				return turrets [type];
		}
}
