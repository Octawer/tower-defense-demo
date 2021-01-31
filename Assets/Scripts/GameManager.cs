using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{


		// Game variables
		public Grid ground;						// grid of cells
		public LayerMask gridLayer;				// grid layer used for raycasting
		public GameObject turretFactory;		// storage of turret types
		//
		private bool deployModeOn = false;		// deploy mode switch
		private GameObject lastHitCell;			// last hit cell used in raycasting
		private string selectedTurret;
		private BaseTurret turret;
		private BaseTurret turretToLvlUp;
		
		// player variables
		private int crystals;					// resources to protect
		private int credits;					// money
	
		// GUI variables
		public UILabel crystalLabel;
		public UILabel creditsLabel;
		public UILabel turretCostLabel;
		public TweenPosition lvlUpPanelTween;
		public UILabel lvlUpLabel;
		public UISlider lvlUpBar;
		public GameObject[] lvlUpButtons;
		//
		private bool lvlUpPanelVisible;
		private bool lvlUpBarActive;

		void Awake ()
		{
				// we initialize some needed variables before the gaem start
				crystals = 0;
				credits = 1000;
				//turret = turretFactory.GetComponent<TurretFactory> ().currentTurret;
				lvlUpPanelVisible = false;
				lvlUpBarActive = false;
				// default initial turret (cannon turret)
				turret = turretFactory.GetComponent<TurretFactory> ().turrets [0];
				lvlUpBar.sliderValue = 0;
				UpdateScore ();

		}

		void Start ()
		{
				// we update the score (credits, crystals, etc.)
				// we do this in the start function as we need to retrieve data from the grid (crystals)
				// that is set in its awake function
				UpdateScore ();
		}

		void Update ()
		{
				// on deploy mode we cast rays to the grid layer (cells)
				if (deployModeOn) {
						Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
						RaycastHit hit;
						// if we hit a cell (they are the only ones that lay in the gridlayer)
						if (Physics.Raycast (ray, out hit, 1000, gridLayer)) {
								// if we had hit something before we unhighlight it (restore it to normal)
								if (lastHitCell) {
										TagNeighboringCells ("Cell_Empty");
								}

								// if we hadnt hit anything we get the cell where the ray hit
								// and highlight it
								lastHitCell = hit.collider.gameObject;
								TagNeighboringCells ("Cell_Highlight");
								Debug.Log ("Hit cell = " + lastHitCell);
								// if we hit nothing (i.e. outside of grid)
						} else {
								// if we had hit something before we unmark that cell and assign null to the last hit
								if (lastHitCell) {
										TagNeighboringCells ("Cell_Empty");
										lastHitCell = null;
								}
						}

						// if we left click on a cell and the upgrade panel is hidden
						if (Input.GetMouseButtonDown (0) && lastHitCell && !lvlUpPanelVisible) {
								// if we are over an empty cell we place a new one if possible
								if (lastHitCell.tag == "Cell_Empty") {
										// get the dimensions of the current turret
										Vector2 turretCellSize = turret.size;
										// get the neighboring cells 
										List<GameObject> placementCells = ground.GetNeighborhood (turretCellSize, lastHitCell);
										// if every neighbor is tagged "Cell_Empty" (i.e. valid deploy) and we have enough money
										if (ValidDeploy (placementCells, turretCellSize) && credits >= turret.deployCost) {
												// get the mid point of those cells
												Vector3 turretPos = GetCellsMidPoint (placementCells);
												// place a new turret there
												BaseTurret placedTurret = (BaseTurret)Instantiate (turret, turretPos, Quaternion.identity);
												// mark them as occupied
												TagNeighboringCells ("Cell_Occupied");
												// we need to add the same turret instance to every cell in the neighborhood
												// to retrieve it later for the upgrade
												foreach (GameObject cell in placementCells) {
														cell.GetComponent<Cell> ().cellTurret = placedTurret;
												}
												// update money
												credits -= turret.deployCost;
												UpdateScore ();
												
										}
										// if we are over an occupied cell	we level up that turret if possible
								} else if (lastHitCell.tag == "Cell_Occupied" && !lvlUpBarActive) {
										// obtain the turret on those cells to upgrade it
										turretToLvlUp = lastHitCell.GetComponent<Cell> ().cellTurret.GetComponent<BaseTurret> ();
										Debug.Log ("Turret to level up = " + turretToLvlUp);
										// show level up panel
										ShowLvlUpPanel ();
										
								}
						}
				} 
	
		}

		// switch between deploy mode on / off, triggered when we push GUI's deploy button
		public void DeployModeSwitch ()
		{
				if (!deployModeOn) {
						deployModeOn = true;
						// enable the cells renderer
						foreach (GameObject cell in ground.cells) {
								// Debug.Log ("Cell position = " + cell.transform.position);
								cell.renderer.enabled = true;
						}
				} else {
						deployModeOn = false;
						// disable the cells renderer
						foreach (GameObject cell in ground.cells) {
								// Debug.Log ("Cell position = " + cell.transform.position);
								cell.renderer.enabled = false;
						}
				
				}
		}

		// when we select a turret from the GUI list we call this function
		public void OnSelectionChange (string selection)
		{
				// we get the current turret from the factory array of types
				if (selection == "Cannon Turret") {
						turret = turretFactory.GetComponent<TurretFactory> ().turrets [0];
				} else if (selection == "Laser Turret") {
						turret = turretFactory.GetComponent<TurretFactory> ().turrets [1];
				} else {
						turret = turretFactory.GetComponent<TurretFactory> ().turrets [0];
				}
				// we update the GUI
				turretCostLabel.text = "$ " + turret.deployCost;
				selectedTurret = selection;
				EnoughCreditsToggle (turretCostLabel.gameObject);
		
		}

		// we tag the neighbor cells according to the type of turret selected and the tag string
		private void TagNeighboringCells (string tag)
		{
				// obtain the turret size (how many cells it occupies)
				Vector2 size = turret.size;
				// look for the current highlight cell (object hit by raycast) in the cell array (ground(Grid)) 
				if (lastHitCell) {
						// get the neighbors
						List<GameObject> neighbors = ground.GetNeighborhood (size, lastHitCell);
						Debug.Log ("neighbors added");
						// if there are neighbors we mark each one according to the tag param
						// and assign them a material accordingly
						if (neighbors != null) {
								foreach (GameObject neighbor in neighbors) {
										Debug.Log ("Neighbor cell tag is: " + neighbor.tag);
										if (neighbor.tag != "Cell_Occupied" && neighbor.tag != "Cell_Resource") {
												if (tag == "Cell_Empty") {
														neighbor.renderer.material = ground.normalMaterial;
														neighbor.tag = tag;
												} else if (tag == "Cell_Highlight") {
														neighbor.renderer.material = ground.highlightMaterial;
														neighbor.tag = "Cell_Empty";	// this breaks homogeneity. See how it can be fixed
												} else if (tag == "Cell_Occupied") {
														neighbor.renderer.material = ground.occupiedMaterial;
														neighbor.tag = tag;
														neighbor.GetComponent<Cell> ().isEmpty = false;
												} else if (tag == "Cell_Resource") {
														neighbor.renderer.material = ground.resourceMaterial;
														neighbor.tag = tag;
												} else {
														// if we pass other parameter we establish cell as normal
														neighbor.renderer.material = ground.normalMaterial;
														neighbor.tag = "Cell_Empty";
												}
										}
								}
						}
				}


		}

		// update the GUI labels
		private void UpdateScore ()
		{
				crystals = ground.ResourceAmount ();
				crystalLabel.text = "Crystals: " + crystals;
				creditsLabel.text = "Credits: " + credits;
				EnoughCreditsToggle (turretCostLabel.gameObject);
		}

		// update the money. Called when we kill an enemy
		public void AddCash (int amount)
		{
				credits += amount;
				Debug.Log ("credits = " + credits);
				UpdateScore ();
		}

		// get the middle point of a cell neighborhood
		private Vector3 GetCellsMidPoint (List<GameObject> neighborCells)
		{
				// vector that will contain the cell coordinates sum
				Vector3 sumCoords = Vector3.zero;
				int numberOfCells = neighborCells.Count;

				// get the coords of each cell and add them to the auxiliary vector
				foreach (GameObject cell in neighborCells) {
						Vector3 cellPos = cell.transform.position;
						sumCoords += cellPos;
				}
				// we check that the List is not empty, so as not to divide per 0
				if (numberOfCells != 0) {
						// return the average vector3 point
						return new Vector3 (sumCoords.x / numberOfCells, sumCoords.y / numberOfCells, sumCoords.z / numberOfCells);
				} else {
						return sumCoords;
				}
				
		}

		// check if we can deploy a turret in a cell neighborhood
		private bool ValidDeploy (List<GameObject> neighborCells, Vector2 size)
		{
				
				// we make a cast cause Vector2 stores float values
				int cellNumber = (int)(size.x * size.y);
				int cellOks = 0;
				foreach (GameObject cell in neighborCells) {
						if (cell.tag == "Cell_Empty") {
								cellOks++;
						}
				}
				// check if the number of empty cells equals the turret cell size 
				// this way we avoid false positives in grid edges (if real neighborhood is < turret cell size)
				return cellOks == cellNumber;
		}

		// we show the level up option panel
		private void ShowLvlUpPanel ()
		{
				// if the turret can be upgraded
				if (turretToLvlUp.level < turretToLvlUp.maxLevel) {
				
						// we update the level up panel text
						string upgradeName = turretToLvlUp.name;
						int lvlUpCost = turretToLvlUp.upgradeCost;
						float lvlUpTime = turretToLvlUp.upgradeTime;

						lvlUpLabel.text = "Upgrade " + upgradeName + " to level " + (turretToLvlUp.level + 1) + " for " + lvlUpCost + " credits ?";
						// enable / disable the Ok button based on remaining money
						EnoughCreditsToggle (lvlUpButtons [0]);
						// we play in the panel animation and set it to visible
						lvlUpPanelTween.Play (true);
						lvlUpPanelVisible = true;
				}
		}
		
		// if we click ok button we hide the panel and start the level up process coroutine
		public void lvlUpOk ()
		{
				lvlUpPanelTween.Play (false);
				lvlUpPanelVisible = false;
				// we start the level up progress coroutine
				StartCoroutine (LevelUpProgress ());
		}

		// if we click no we just hide the panel
		public void lvlUpNo ()
		{
				lvlUpPanelTween.Play (false);
				lvlUpPanelVisible = false;
		}

		// we enable or disable the button based on the money we have left
		private void EnoughCreditsToggle (GameObject guiItem)
		{
				// if we are treating with a button
				if (guiItem.GetComponent<UIButton> ()) {
				
						// if we cant afford the level up cost we disable the button collider and change its appearance to disabled
						// otherwise we enable its collider and return its look to a normal one
						if (credits < turretToLvlUp.upgradeCost) {
								guiItem.GetComponent<UIButton> ().isEnabled = false;
						} else {
								guiItem.GetComponent<UIButton> ().isEnabled = true;
						}
						// otherwise if we are dealing with a label
				} else if (guiItem.GetComponent<UILabel> ()) {
						// if we cant afford deploying a turret we change the cost label to red
						// and turn it back green otherwise
						if (credits < turret.deployCost) {
								turretCostLabel.GetComponent<UILabel> ().color = Color.red;
						} else {
								turretCostLabel.GetComponent<UILabel> ().color = Color.green;
						}
				}
		}

		// turret level up process coroutine 
		private IEnumerator LevelUpProgress ()
		{
				// play the progress bar animation for upgradeTime seconds and mark it as active 
				// so as to block the resource (the progress bar), forbidding other upgrades until its release
				// when the coroutine ends
				if (!lvlUpBarActive) {
						// deduct the upgrade cost
						credits -= turretToLvlUp.upgradeCost;
						UpdateScore ();
						// mark bar as active
						lvlUpBarActive = true;
						float progressTime = 0.0f;

						// fill the level up slider bar over time using a lerp function
						while (progressTime < turretToLvlUp.upgradeTime) {
								progressTime += Time.deltaTime;
								lvlUpBar.sliderValue = Mathf.Lerp (0, 1, progressTime / turretToLvlUp.upgradeTime);
								yield return null;
						}

						// we level up the turret
						turretToLvlUp.LevelUp ();
						
						// we reset the levelUpBar and enable it
						lvlUpBarActive = false;
						lvlUpBar.sliderValue = 0;
				}
				
				
		}

		// exits the game by pressing the exit button
		public void ExitGame ()
		{
				Application.Quit ();
		}



}
		