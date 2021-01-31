using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cell : MonoBehaviour
{
	
		// texture variables
		public Material normalMaterial;
		public Material highlightMaterial;
		public Material occupiedMaterial;
		public Material resourceMaterial;
		private Material currentMaterial;	

		// cell states
		public static int normal = 0;
		public static int highlight = 1;
		public static int occupied = 2;
		public static int resource = 3;
		
		// game variables
		public bool isEmpty;
		public BaseTurret cellTurret;

		// graph variables
		public Cell parent;
		public Queue<Cell> children;
		public bool isStart;
		public bool isEnd;
		public bool visited;
		public int xIndex;		// x index in cell grid
		public int zIndex;		// z index in cell grid

		// Use this for initialization
		void Awake ()
		{
				// gameObject.renderer.material = normalMaterial;
				// gameObject.tag = "Cell_Empty";
				isEmpty = true;
				children = new Queue<Cell> ();
		}
	
		// Update is called once per frame
		void Update ()
		{
	
		}

}
