using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{

		public int level;
		public float speed;
		public GameObject explosion;
		public GUITexture hpBar;
		public Vector3 hpBarOffset = new Vector3 (0, 10, 0);
		public int cash = 25;
		private float maxHP;
		private int maxLevel = 5;
		private bool alive;
		private float currentHP;
		private Color[] enemyColors = {
				Color.cyan,		// this index is not used (to avoid division per 0 -- Setlevel())
				Color.blue,
				Color.green,
				Color.yellow,
				Color.red
		};

		// AI variables
		public Queue<Cell> pathQueue;		// path from start to end stored in a queue
		public GameObject[,] enemyGrid;		// the ground cell grid
		public Cell startCell;				// initial cell				
		private Cell nextWP;				// next waypoint in path

		void Awake ()
		{
				// we initialize the enemy level at 1 by default 
				// it will be changed later in the spawning function of the grid
				level = 1;
				// init of health , speed, etc, relative to enemy level
				maxHP = 100 * level;
				currentHP = maxHP;
				speed += level;
				cash *= level;
				alive = true;

				gameObject.renderer.material.color = enemyColors [level];
				pathQueue = new Queue<Cell> ();
		}

		void Start ()
		{
				// init nextWP to the spawn cell
				nextWP = startCell;
				// get cell grid
				enemyGrid = GameObject.FindGameObjectWithTag ("GroundGrid").GetComponent<Grid> ().cells;
				// get initial path. 
				pathQueue = BFSGraph (nextWP);							// UNCOMMENT THIS FOR AI MOVEMENT
				// get next waypoint in path if queue has more elements
				if (pathQueue != null && pathQueue.Count > 0) {
						nextWP = pathQueue.Dequeue ();
				}
				
		}

		void Update ()
		{
				if (alive) {
						// movement according BFS algorithm towards next waypoint in path
						MoveTowardsWP ();								// UNCOMMENT THIS FOR AI MOVEMENT
						// check if we have reached next wp to dequeue the following one and update the path
						SetNextWP ();									// UNCOMMENT THIS FOR AI MOVEMENT

						// linear movement ( no AI )
						// NonAIMovement ();									// COMMENT THIS FOR AI MOVEMENT (and uncomment the marked ones)
				} else {
						// destroy it if it is dead
						Explode ();
				}
				// we destroy them after a short period (testing purposes mostly)
				LifeTime (10);
		}

		// take damage and check if it is still alive
		public void GetHit (float damage)
		{
				currentHP -= damage;
				Debug.Log ("Hit: current HP = " + currentHP);
				if (currentHP <= 0) {
						alive = false;
				}
		}

		// retrieve its cash, create explosion effect and destroy it
		private void Explode ()
		{
				GameManager gameGUI = GameObject.FindWithTag ("GameMaster").GetComponent<GameManager> (); 
				gameGUI.AddCash (cash);
				Instantiate (explosion, transform.position, Quaternion.identity);
				Destroy (gameObject);
		}
		
		// attach the life bar to the enemy, and update the current hip points and hpbar width accordingly
		private void UpdateHP ()
		{
				hpBar.transform.position = Camera.main.WorldToViewportPoint (transform.position + hpBarOffset);
				float percentHP = currentHP / maxHP;
				float hpBarWidth = percentHP * 100;
				Debug.Log ("Hp bar width = " + hpBarWidth);
				Rect hpRect = new Rect (hpBar.guiTexture.pixelInset.x, hpBar.guiTexture.pixelInset.y, hpBarWidth, hpBar.guiTexture.pixelInset.height); 
				hpBar.guiTexture.pixelInset = hpRect;
		}

		// establish the enemy level and other attributes based on it
		public void SetLevel (int enemyLevel)
		{
				// we exclude 0 to avoid division per 0 later at UpdateHP (currentHP / maxHP)
				if (enemyLevel > 0 && enemyLevel < maxLevel) {

						level = enemyLevel;
						// init of health , speed, etc
						maxHP = 100 * level;
						currentHP = maxHP;
						speed += level;
						alive = true;
						gameObject.renderer.material.color = enemyColors [level];
						// GameObject lifeBar = (GameObject)Instantiate (hpBar, transform.position, Quaternion.identity);
						//lifeBar.enemy = gameObject;
					
				}
		}

		// destroy the enemy after a certain period
		private void LifeTime (float time)
		{
				Destroy (gameObject, time);
		}

		
		// movement towards the next waypoint in the path
		private void MoveTowardsWP ()
		{
				// get the direction vector as the nextWp minus the current position
				Vector3 dirVector = nextWP.gameObject.transform.position - transform.position;
				// we move the enemy towards that direction in a magnitude-1 vector basis (smooth movement)
				Vector3 moveVector = dirVector.normalized * speed * Time.deltaTime;
				// move
				transform.position += moveVector;
				// we rotate smoothly towards the direction vector
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dirVector), speed * Time.deltaTime);
				// update the life bar
				UpdateHP ();

		}

		// we check if we have reached nextWP and if so we get the next one from the path
		private void SetNextWP ()
		{
				if (Vector3.Distance (nextWP.gameObject.transform.position, transform.position) < 1) {
						
						if (pathQueue != null && pathQueue.Count > 0) {
								//pathQueue = BFSGraph (nextWP);				// uncommenting this should make it adjust the path dynamically
								nextWP = pathQueue.Dequeue ();
						}

				}
		}

		// check whether the given indexes are inside the grid
		private bool InsideGrid (int x, int y)
		{
				return x >= 0 && x < enemyGrid.GetLength (0) && y >= 0 && y < enemyGrid.GetLength (1);
		}

		// get the children of the given cell (node), based in 8-connectivity
		private Queue<Cell> GetNodeChildren (Cell node, int xIndex, int zIndex)
		{
				// we pass through all neighbors in an 8-connectivity basis and add them to the node children
				// as long as they are not visited or outside the grid or occupied by turrets
				for (int i = -1; i < 2; i++) {
						for (int j = -1; j < 2; j++) {
								if (InsideGrid (xIndex + i, zIndex + j)) {
					
										GameObject plane = enemyGrid [xIndex + i, zIndex + j];
										Cell child = plane.GetComponent<Cell> ();
					
										if (!child.visited && child.transform.tag != "Cell_Occupied") {
												child.parent = node;
												child.isStart = false;
												child.isEnd = child.transform.tag == "Cell_Resource";
												child.visited = true;
												node.children.Enqueue (child);
												Debug.Log ("Child added: index [" + (xIndex + i) + "," + (zIndex + j) + "]");
										}
								}
						}
				}
				return node.children;
		}

		// BFS pathfinding algorithm. 
		// iterates over the cells of the grid starting at the root (spawn node)
		// adding the parameters and children of each node and thus creating the tree
		// Returns the path from start node to end one as a queue 
		private Queue<Cell> BFSGraph (Cell startNode)
		{
				Cell final = null;
		
				Queue<Cell> graph = new Queue<Cell> ();
				// mark the first as root
				startNode.parent = null;
				startNode.isStart = true;
				startNode.isEnd = startNode.tag == "Cell_Resource";
				startNode.visited = true;
		
				// enqueue the root node
				graph.Enqueue (startNode);

				// get its children
				Cell[] startChildren = GetNodeChildren (startNode, startNode.xIndex, startNode.zIndex).ToArray (); 
		
				// Enqueue them all in a loop
				foreach (Cell startChild in startChildren) {
						graph.Enqueue (startChild);
				}
				// while the queue is not empty
				while (graph.Count > 0) {
						// Get the first node of the queue
						Cell c = graph.Dequeue ();
						// Check if its a resource one (target)
						if (c.isEnd) {
								// Make him final node if target and exit loop if so
								final = c;
								break;
						}
						// get the children of c and enqueue them
						Cell[] children = GetNodeChildren (c, c.xIndex, c.zIndex).ToArray (); 
						foreach (Cell child in children) {
								graph.Enqueue (child);
						}
				}
				// If final is null we could not find a path
				if (final == null) {
						Debug.Log ("Final node is NULL");
						return null;
				}
				// Get the parent node of the final node
				Cell parent = final.parent;
				List<Cell> cellList = new List<Cell> ();
		
		
				// While the parent is not the start node
				// This will go back up parent of each node
				// and then define the parent as current node
				while (parent != null) {
						cellList.Add (final);
						final = parent;
						parent = final.parent;
				}
				// We add the last one
				cellList.Add (final);
				// The list as is goes from target to start
				// so we reverse it to get the right order
				cellList.Reverse ();
				// we clear the initial queue
				graph.Clear ();
				// and we enqueue every list node
				Debug.Log ("Path is as follows");
				foreach (Cell cell in cellList) {
						Debug.Log ("Cell index = [" + cell.xIndex + "," + cell.zIndex + "]");
						graph.Enqueue (cell);
				}
				// clear visited nodes
				ClearGrid ();
				return graph;
		
		}

		// unmark the grid cells as visited
		private void ClearGrid ()
		{
				foreach (GameObject cellObj in enemyGrid) {
						cellObj.GetComponent<Cell> ().visited = false;
				}
		}
		
		private void AIMovement ()
		{
				// movement according BFS algorithm towards next waypoint in path
				MoveTowardsWP ();
				// check if we have reached next wp to dequeue the following one and update the path
				SetNextWP ();
		}

		private void NonAIMovement ()
		{
				transform.Translate (Vector3.forward * speed * Time.deltaTime);
				// update the life bar
				UpdateHP ();
		}

		
}
