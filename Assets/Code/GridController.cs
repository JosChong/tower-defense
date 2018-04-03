using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UI;

public class GridController : MonoBehaviour {


	//specifies how big the board is (woo flexibility)
	public Index dimensions;

	//a struct that keeps the inspector UI nice and clean
	[System.Serializable]
	public struct Index{
		public int x;
		public int y;

		public Index(int x, int y){
			this.x = x;
			this.y = y;
		}

		public override bool Equals(System.Object obj){
			return obj != null && obj is Index && ((Index) obj).x == x && ((Index) obj).y == y;
		}

		public override int GetHashCode(){
			return 31*(x + 31*y);
		}

		public override string ToString()
		{
			return "[" + x + ", " + y + "]";
		}
	}

	//the amount of distance between each block in the grid
	public float spacing;

	//the prefab to use to make the blocks in the grid
	public GameObject gridBlock;


	//a list of objects to spawn into the grid initially. (the base for example)
	[Space(10)]
	public List<GridObject> initialObjectsInGrid;

	//another thing to make the inpector UI nice and clean
	[System.Serializable]
	public class GridObject{

		public GameObject prefab;
		public Index position;

	}

	//stores all the grid blocks and objects on those blocks
	public GameObject[,] grid;

	private List<Index> spawnerLocations;
	private List<Index> baseLocations;

    public GameObject Go;

	void Start(){

		//init arrays
		grid = new GameObject[dimensions.x, dimensions.y];

		spawnerLocations = new List<Index>(1);
		baseLocations = new List<Index>(1);
	    Index pos;
		//create the blocks
		for(int i=0; i<dimensions.x; i++){
			for(int j=0; j<dimensions.y; j++){

                var cube = GameObject.Instantiate<GameObject>(gridBlock, gameObject.transform);
			    cube.GetComponent<CubeController>().Position = new Index(i, j);

			    grid[i,j] = cube;
				grid[i,j].name = "Cube " + (i * dimensions.x + j);
				grid[i,j].transform.localPosition = new Vector3(j * spacing, 0, i * spacing);
			}
		}
    	/*
	    BuyMenu = GameObject.Find("Buy Menu");
	    SellMenu = GameObject.Find("Sell Menu");

        BuyMenu.SetActive(false);
	    SellMenu.SetActive(false);
        */

        //add the initial objects to the grid
        foreach (GridObject obj in initialObjectsInGrid){
			int x = obj.position.x;
			int y = obj.position.y;
			GameObject go = GameObject.Instantiate<GameObject>(obj.prefab, grid[x, y].transform);


			if(go.GetComponent<SpawnerController>() != null)
			{
			    pos = new Index(x, y);
                spawnerLocations.Add(pos);
			    go.GetComponent<SpawnerController>().Position = pos;
			}
            else if(go.GetComponent<BaseController>() != null)
            {
                pos = new Index(x, y);
                baseLocations.Add(pos);
                go.GetComponent<BaseController>().Position = pos;
            }

		}

	}

	public void setObjectAtLocation(Index pos, GameObject go){
		setObjectAtLocation(pos.x, pos.y, go);
	}

	public void setObjectAtLocation(int x, int y, GameObject go){
		grid[x, y].transform.DetachChildren();
		go.transform.SetParent(grid[x, y].transform);
	}

    public void RemoveObjectAtLocation(Index pos)
    {
        RemoveObjectAtLocation(pos.x, pos.y);
    }

    public void RemoveObjectAtLocation(int x, int y)
    {
        grid[x, y].transform.DetachChildren();
    }

    public void DestroyObjectAtLocation(Index pos)
    {
        foreach (var tower in grid[pos.x, pos.y].transform.GetComponentsInChildren<BasicTowerController>())
            Destroy(tower.gameObject);
	    foreach (var tower in grid[pos.x, pos.y].transform.GetComponentsInChildren<ShockTowerController>())
		    Destroy(tower.gameObject);
	    foreach (var tower in grid[pos.x, pos.y].transform.GetComponentsInChildren<FreezeTowerController>())
		    Destroy(tower.gameObject);
        grid[pos.x, pos.y].transform.DetachChildren();
    }

    public GameObject getObjectAtLocation(Index pos){
		return getObjectAtLocation(pos.x, pos.y);
	}

	public GameObject getObjectAtLocation(int x, int y){
		return grid[x, y].transform.childCount==0 ? null : grid[x, y].transform.GetChild(0).gameObject;
	}

    public GameObject getCubeAtLocation(Index pos)
    {
        return grid[pos.x, pos.y];
    }

    public List<Index> findEnemyPath(Index start){
		//get the position of the spawner

		//find the shortest path to the closest base (yes, I know that there is only one... BUT GENERALITY)
		List<Index> path = null;
		foreach(Index end in baseLocations){
			List<Index> p = dijkstra(start, end);
			if(path==null || p.Count < path.Count){
				path = p;
			}
		}

		return path;
	}

    public bool NewPathExists(Index start, Index newPos)
    {
        var temp = new GameObject();
        setObjectAtLocation(newPos, temp);
        List<Index> path = null;
        foreach(Index end in baseLocations){
            List<Index> p = dijkstra(start, end);
            if(path==null || p.Count<path.Count){
                path = p;
            }
        }
        
        RemoveObjectAtLocation(newPos);
        Destroy(temp);
		
        return (path != null);
    }

	//finds the shortest path through the grid from the given start index to the given end index
	//If there is no path at all, then null is returned
	List<Index> dijkstra(Index start, Index end){

		BinaryMinHeap<Node> unvisited = new BinaryMinHeap<Node>(dimensions.x*dimensions.y+50);

		Dictionary<Index, Node> graph = new Dictionary<Index, Node>();

		for(int i=0; i<dimensions.x; i++){
			for(int j=0; j<dimensions.y; j++){
				if(i == start.x && j == start.y){
					Node n = createNode(start, end);
					n.visited = true;
					n.dist = 0;
					graph.Add(start, n);
					unvisited.addWithPriority(0, n);
				}else{
					Node n = createNode(new Index(i, j), end);
					unvisited.addWithPriority(Double.PositiveInfinity, n);
					graph.Add(n.pos, n);
				}
				
			}
		}


		Node current;
		while(unvisited.Size>0){
			current = unvisited.extractMin();
			current.visited = true;

			foreach(Index p in validNeighbors(current.pos)) {
				Node n = graph[p];
				if(!n.visited){
					double newDist = current.dist + distance(current.pos, n.pos);
					if(newDist < n.dist){
						n.dist = newDist;
						n.prev = current;
						unvisited.updatePriority(newDist, n);
					}
				}
			}

		}

		//check to make sure we actually found a path
		if(graph[end].prev == null){
			return null;
		}

		//reconstruct the path
		List<Index> path = new List<Index>(Math.Max(dimensions.x, dimensions.y));

		current = graph[end];
		path.Add(current.pos);
		while(current.prev != null){
			path.Add(current.prev.pos);
			current = current.prev;
		}

		//flip the path
		path.Reverse();


		return path;
	}

	class Node {

		public Index pos;

		public double dist;

		public Node prev;
		public bool visited;

	}

	private static Node createNode(Index pos, Index end){
		Node n = new Node();
		n.pos = pos;

		n.dist = Double.PositiveInfinity;

		n.prev = null;
		n.visited = false;
		return n;
	}

	private static double distance(Index p1, Index p2){
		//return Math.Max(Math.Abs(p1.x-p2.x), Math.Abs(p1.y-p2.y));
		return Math.Sqrt((p1.x-p2.x)*(p1.x-p2.x) + (p1.y-p2.y)*(p1.y-p2.y));
	}

	public List<Index> validNeighbors(Index pos){
		List<Index> list = new List<Index>(8);

	    Index right = new Index(pos.x + 1, pos.y);
	    Index left = new Index(pos.x - 1, pos.y);
        Index up = new Index(pos.x, pos.y + 1);
	    Index down = new Index(pos.x, pos.y - 1);

        addIfValid(right, list);//right
		addIfValid(up, list);//up
		addIfValid(left, list);//left
		addIfValid(down, list);//down

        if (Valid(up) || Valid(right))
            addIfValid(new Index(pos.x + 1, pos.y + 1), list);//up-right
        if (Valid(up) || Valid(left))
            addIfValid(new Index(pos.x - 1, pos.y + 1), list);//up-left
        if (Valid(down) || Valid(right))
            addIfValid(new Index(pos.x + 1, pos.y - 1), list);//down-right
        if (Valid(down) || Valid(left))
            addIfValid(new Index(pos.x - 1, pos.y - 1), list);//down-left

        return list;
	}

    private bool Valid(Index pos)
    {
        return !outOfBounds(pos) && !containsObject(pos);
    }

	private void addIfValid(Index pos, List<Index> list){
		if(!outOfBounds(pos) && !containsObject(pos)){
			list.Add(pos);
		}
	}

	private bool outOfBounds(Index pos){
		return pos.x < 0 || pos.x >= dimensions.x || pos.y < 0 || pos.y >= dimensions.y;
	}

	private bool containsObject(Index pos){
		return getObjectAtLocation(pos.x, pos.y)!=null && 
			getObjectAtLocation(pos.x, pos.y).GetComponent<BaseController>()==null && 
			getObjectAtLocation(pos.x, pos.y).GetComponent<SpawnerController>()==null;
	}

	private class BinaryMinHeap<T> {

		private int size;

		public int Size {get {return size;}}
		public int Capacity {get {return contents.Length;}}

		//the data
		private T[] contents;
		private double[] priority;

		//the binary tree representing the heap
		private int[] heap;

		//a backwards orientated stack saying which elements of contents are free
		private int[] emptySlotStack;

		//a map relating the index in the heap each content is at
		private Dictionary<T, int> slots;

		public BinaryMinHeap(int capacity){
			contents = new T[capacity];
			priority = new double[capacity];
			heap = new int[capacity];

			emptySlotStack = new int[capacity];
			for(int i=0; i<capacity; i++){
				emptySlotStack[i] = i;
			}

			size = 0;

			slots = new Dictionary<T, int>();
		}

		public void addWithPriority(double p, T e){

			if(size >= Capacity) throw new IndexOutOfRangeException("Heap too small.");

			//get the next empty spot in the data sets
			int index = emptySlotStack[size];

			//add the data
			contents[index] = e;
			priority[index] = p;
			size++;

			//determine the data's position in the binary tree
			int current = size-1;
			while(current > 0 && priority[heap[parent(current)]]>priority[index]){
				setHeapSlot(current, heap[parent(current)]);
				current = parent(current);
			}
			slots.Add(e, current);
			setHeapSlot(current, index);
		}

		public T findMin(){
			return contents[heap[0]];
		}

		public T extractMin(){
			//store the min element
			T min = findMin();

			remove(0);

			//return the min element
			return min;
		}

		public bool contains(T e){
			return slots.ContainsKey(e);
		}

		public void remove(T e){
			remove(slots[e]);
		}

		public void updatePriority(double priority, T e){
			remove(e);
			addWithPriority(priority, e);
		}

		private void remove(int i){
			//set the old min to null so GC can do its thing
			slots.Remove(contents[heap[i]]);
			contents[heap[i]] = default(T);

			//make sure we know that the previously min index is free
			size--;
			emptySlotStack[size] = heap[i];

			//now re-heap the heap
			if(i!=size){
				setHeapSlot(i, heap[size]);
				reheap(i);
			}
		}

		private void reheap(int i){
			int l = childLeft(i);
			int r = childRight(i);

			int smallest = i;
			if(r < size && priority[heap[r]] < priority[heap[smallest]]) smallest = r;
			if(l < size && priority[heap[l]] < priority[heap[smallest]]) smallest = l;

			if(smallest!=i){

				int temp = heap[i];
				setHeapSlot(i, heap[smallest]);
				setHeapSlot(smallest, temp);

				reheap(smallest);
			}

		}

		private void setHeapSlot(int i, int j){
			slots[contents[j]] = i;
			heap[i] = j;
		}

		private static int childLeft(int parent){
			return 2 * parent + 1;
		}

		private static int childRight(int parent){
			return 2 * parent + 2;
		}

		private static int parent(int child){
			return (child-1) / 2;
		}

		public override string ToString(){
			string s = "";

			for(int i=0; i<size; i++){
				s += priority[heap[i]]+" ";
			}

			return s;
		}

	}

    public bool GameOver = false;
	public void EndGame(string condition)
	{
	    GameOver = true;

        if (Go != null) Destroy(Go);

	    FindObjectOfType<WaveController>().enabled = false;
        StopAllCoroutines();
	    FindObjectOfType<WaveInformationController>().enabled = false;
        FindObjectOfType<SpawnerController>().enabled = false;
	    FindObjectOfType<BaseController>().enabled = false;

		var basicTowers = FindObjectsOfType<BasicTowerController>();
		foreach (var tower in basicTowers)
		{
		    tower.enabled = false;
		}
		var shockTowers = FindObjectsOfType<ShockTowerController>();
		foreach (var tower in shockTowers)
		{
			tower.enabled = false;
		}
		var freezeTowers = FindObjectsOfType<FreezeTowerController>();
		foreach (var tower in freezeTowers)
		{
			tower.enabled = false;
		}
	    var bullets = FindObjectsOfType<BulletController>();
	    foreach (var bullet in bullets)
	    {
            bullet.Die();
	    }

	    var enemies = FindObjectsOfType<EnemyController>();
	    foreach (var enemy in enemies)
	    {
	        enemy.GetComponent<Rigidbody>().velocity = Vector3.zero;
	        enemy.enabled = false;
	    }
    
        if (condition == "victory")
		{
			GameObject.Find("End Game Text").GetComponent<Text>().text = "YOU WIN!";
		}
		else if (condition == "defeat")
		{
			GameObject.Find("End Game Text").GetComponent<Text>().text = "YOU LOSE!";
		}
	}
	
	/*
	public void EndGame(string condition)
	{
		var basicTowers = FindObjectsOfType<BasicTowerController>();
		var shockTowers = FindObjectsOfType<ShockTowerController>();
		var freezeTowers = FindObjectsOfType<FreezeTowerController>();
		var enemies = FindObjectsOfType<EnemyController>();
		var cubes = FindObjectsOfType<CubeController>();

		Destroy(FindObjectOfType<WaveController>());
		Destroy(FindObjectOfType<WaveInformationController>());
		Destroy(FindObjectOfType<SpawnerController>());
		foreach (var tower in basicTowers) Destroy(tower);
		foreach (var tower in shockTowers) Destroy(tower);
		foreach (var tower in freezeTowers) Destroy(tower);
		foreach (var enemy in enemies)
		{
			enemy.gameObject.GetComponent<Rigidbody>().velocity = new Vector3.zero;
			Destroy(enemy);
		}
		foreach (var cube in cubes)
		{
			Destroy(cube);
		}
		

		if (condition == "victory")
		{
			GameObject.Find("End Game Text").GetComponent<Text>().text = "YOU WIN!";
		}
		else if (condition == "defeat")
		{
			GameObject.Find("End Game Text").GetComponent<Text>().text = "YOU LOSE!";
		}
	}
	*/
}
