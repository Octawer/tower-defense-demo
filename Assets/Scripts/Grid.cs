using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{

		// grid variables
		public int gridRows = 5;
		public int gridCols = 5;
		public GameObject[,] cells;				// bidimensional array of cells
		public GameObject cell;					// cell asset (plane with cell script)
		private float cellSize = 10;			// plane width / length

		// texture variables
		public Material normalMaterial;
		public Material highlightMaterial;
		public Material occupiedMaterial;
		public Material resourceMaterial;

		// spawn variables
		public float spawnDelay = 4;
		private float nextSpawnTime = 0;
		public bool spawnEnabled;

		// enemies
		public EnemyFactory enemyTypes;								// currently there is only one type
		public GameObject enemy;									// current enemy
		public Vector3 enemyOffset = new Vector3 (0, 5, 0);			// position.y coord to move them above the cells when spawning
		public int maxPortals = 5;									// max enemy spawn points
		private Transform[] enemyPortals; 							// enemy spawn points

		// resources
		public GameObject crystalResource;							// current resource
		public int maxResourceVeins = 3;							// max crystals
		private Transform[] resourceVeins;							// crystal spawn points
		private Resource[] curResources;							// storage of crystals
		

		void Awake ()
		{		
				// we set the array dimensions
				cells = new GameObject[gridRows, gridCols];
				transform.position = new Vector3 (0, 0, 0);
				// we create the grid
				CreateGrid ();
				// we set the spawn variables
				enemyPortals = new Transform[maxPortals];
				resourceVeins = new Transform[maxResourceVeins];
				curResources = new Resource[maxResourceVeins];
				// we set the spawn points
				SetupKeyCells ();
				// spawn actual resources (crystals)
				SpawnResources ();
				spawnEnabled = false;
		}

		void Update ()
		{
				SpawnEnemies ();
		}

		// we create a grid composed of a series of cells and attach them to this object
		private void CreateGrid ()
		{
				float initX = transform.position.x;
				float initZ = transform.position.z;
				//	we create a square grid of grid size smaller planes (cells)
				for (int x = 0; x < gridRows; x++) {
						for (int z = 0; z < gridCols; z++) {
								
								// we create them with a difference of cellSize distance to avoid overlapping
								// we also move them a bit up to get them above the ground itself
								GameObject cellCopy = (GameObject)Instantiate (cell, new Vector3 (initX + x * cellSize, 5, initZ + z * cellSize), Quaternion.identity);
								// make each cellcopy a child of the grid
								cellCopy.transform.parent = transform;
								// put them in the gridLayer layer (for raycasting)
								cellCopy.layer = gameObject.layer;
								cellCopy.tag = "Cell_Empty";
								// we store the grid indexes in each cell for convenience
								cellCopy.GetComponent<Cell> ().xIndex = x;
								cellCopy.GetComponent<Cell> ().zIndex = z;
								// we store each cell in the array of the grid
								cells [x, z] = cellCopy;
								
						}
				}
				// change the layer to default once the actual grid of cells has been created, so as to avoid raycasting hits (they should only hit the cells, not the "ground")
				gameObject.layer = LayerMask.GetMask ("Default");

		}

		// set the enemy and crystal spawn points from random positions
		private void SetupKeyCells ()
		{
				// watch out for small grids here (less than 2 rows/cols)
				for (int i = 0; i < maxPortals; i++) {
						int enemyRow = Random.Range (gridRows - 2, gridRows);		// enemies can spawn just at the two last rows (-2 and -1)
						int enemyCol = Random.Range (0, gridCols);					// lower bound inclusive , upper bound exclusive (to make sure it is within array limits)

						Transform portal = cells [enemyRow, enemyCol].transform;
						enemyPortals [i] = portal;
				}

				for (int i = 0; i < maxResourceVeins; i++) {
						int resourceRow = Random.Range (0, 2);						// crystals spawn just at the two first rows (0 and 1)
						int resourceCol = Random.Range (0, gridCols);	
			
						Transform portal = cells [resourceRow, resourceCol].transform;
						resourceVeins [i] = portal;
				}
		}

		// spawn enemies at fixed intervals if spawning is enabled (spawn switch button)
		private void SpawnEnemies ()
		{
				
				if (spawnEnabled) {
						Debug.Log ("Spawning enemies");
						// if we disable the spawning it takes several cycles for the nextSpawnTime to catch up with Time.time
						// and therefore several enemies are spawned. The else part takes care of this

						// if we reached next spawn time
						if (Time.time >= nextSpawnTime) {
								// update the spawn time
								nextSpawnTime += spawnDelay;

								// we get a random portal
								int index = Random.Range (0, maxPortals);
								Transform enemyTransform = enemyPortals [index];
								// spawn an enemy at the portal and a bit up (so as to show it above the ground)
								GameObject newEnemy = (GameObject)Instantiate (enemy, enemyTransform.position + enemyOffset, enemyTransform.rotation);
								// randomize the enemy level between the limits
								int enemyLvl = Random.Range (1, 5);
								// set the enemy level, grid (for BFS pathfinding) and start cell
								newEnemy.GetComponent<Enemy> ().SetLevel (enemyLvl);
								// newEnemy.GetComponent<Enemy> ().enemyGrid = cells;
								newEnemy.GetComponent<Enemy> ().startCell = enemyTransform.GetComponent<Cell> ();
						}
				} else {
						// we update the nextSpawnTime anyway to avoid spawning in bulk glitch
						if (Time.time >= nextSpawnTime) {
								nextSpawnTime += spawnDelay;
						}
				}
		}

		// spawn actual crystal resources in resource veins
		private void SpawnResources ()
		{
				for (int i = 0; i < maxResourceVeins; i++) {
						Transform vein = resourceVeins [i];
						GameObject newResource = (GameObject)Instantiate (crystalResource, vein.position + enemyOffset, vein.rotation);
						// we mark the cell as a resource one and set its material
						vein.gameObject.tag = "Cell_Resource";
						vein.renderer.material = resourceMaterial;
						Debug.Log ("Vein tag[" + i + "] = " + vein.tag + vein.position);
						// store the crystal in the array (to update GUI later)
						curResources [i] = newResource.GetComponent<Resource> ();
				}
		}
		
		// get the cell index in the 2Darray. Not much use now as each cell has its own indexes as fields
		private Vector2 LocateCellIndex (GameObject targetCell)
		{
				Vector2 index = new Vector2 (-1, -1);		// establish a negative default value if targetCell is not found
				for (int x = 0; x < gridRows; x++) {
						for (int z = 0; z < gridCols; z++) {
								if (cells [x, z].Equals (targetCell)) {
										index = new Vector2 (x, z);
								}
						}
				}
				return index;
		}

		// get the neighboring cells of a given one based on the turret size (used when placing turrets)
		public List<GameObject> GetNeighborhood (Vector2 size, GameObject targetcell)
		{
				// we get the cell indexes
				Vector2 pivotCellIndex = new Vector2 (targetcell.GetComponent<Cell> ().xIndex, targetcell.GetComponent<Cell> ().zIndex);
				// Debug.Log ("Highlight cell index corresponds to (" + pivotCellIndex.x + "," + pivotCellIndex.y + ")");

				List<GameObject> neighborCells = new List<GameObject> ();

				// start at pivot cell (0,0) and get the rest based on the size
				// relative to that pivot one (e.g.:	size (2x2) ---> +0+0, +0+1, +1+0, +1+1)
				for (int i = 0; i < size.x; i++) {
						for (int j = 0; j < size.y; j++) {
								int xCoord = (int)pivotCellIndex.x + i;
								int yCoord = (int)pivotCellIndex.y + j;
								if (InsideGrid (xCoord, yCoord)) {
										neighborCells.Add (cells [xCoord, yCoord]);
										Debug.Log ("Cell index added: (" + xCoord + "," + yCoord + ")");
								}
						}
				}
				
				return neighborCells;
			
		}

		// check if a pair of indexes are inside the 2Darray
		private bool InsideGrid (int x, int y)
		{
				return x >= 0 && x < gridRows && y >= 0 && y < gridCols;
		}

		// switch spawning (called from pushing GUI's spawn button)
		public void SpawnToggle ()
		{
				if (!spawnEnabled) {
						spawnEnabled = true;
				} else {
						spawnEnabled = false;
				}
		}

		// get the current resource amount, so as to update the GUI
		public int ResourceAmount ()
		{
				int amount = 0;
				foreach (Resource r in curResources) {
						amount += r.amount;
				}
				return amount;
		}


}
