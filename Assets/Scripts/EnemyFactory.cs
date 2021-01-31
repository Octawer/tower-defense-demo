using UnityEngine;
using System.Collections;

public class EnemyFactory : MonoBehaviour {

	public GameObject currentEnemy;
	public GameObject[] enemies;			// array of enemy types
	

	void Start ()
	{
		// if array is not empty we assign the first element as the default current enemy type
		if (enemies.Length > 0) {
			Debug.Log ("turret types = " + enemies.Length);
			currentEnemy = enemies [0];
		}
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	// NOT USED - moved to GameManager
	public void OnSelectionChange (string selectedItem)
	{   
		Debug.Log ("On selection change event received: Value = " + selectedItem);
		// selectedTurret = selectedItem;
		if (selectedItem == "Cannon Turret") {
			currentEnemy = enemies [0];
		}
	}

	// NOT USED ATM
	public void NextEnemy() {
		// get random enemy type in range of enemies array
	}
}
