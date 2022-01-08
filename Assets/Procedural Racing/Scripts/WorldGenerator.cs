using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WorldGenerator : MonoBehaviour {
	
	//variables visible in the inspector
	public Material meshMaterial;
	public float scale;
	public Vector2 dimensions;
	public float perlinScale;
    public float waveHeight;
    public float offset;
	public float randomness;
	public float globalSpeed;
	public int startTransitionLength;
	public BasicMovement lampMovement;
	public GameObject[] obstacles;
	public GameObject gate;
	public int startObstacleChance;
	public int obstacleChanceAcceleration;
	public int gateChance;
	public int showItemDistance;
	public float shadowHeight;
	
	//not visible in the inspector
	Vector3[] beginPoints;
	
	GameObject[] pieces = new GameObject[2];
	
	GameObject currentCylinder;

	GameManager gameManager;

	void Start(){
		//create an array to store the begin vertices for each world part (we'll need that to correctly transition between world pieces) 
		beginPoints = new Vector3[(int)dimensions.x + 1];
		
		//start by generating two world pieces
		for(int i = 0; i < 2; i++){
			GenerateWorldPiece(i);
		}

		gameManager = FindObjectOfType<GameManager>();
	}
	
	void LateUpdate(){
		//if the second piece is close enough to the player, we can remove the first piece and update the terrain
		if(pieces[1] && pieces[1].transform.position.z <= 0)
			StartCoroutine(UpdateWorldPieces());
		
		//update all items in the scene like spikes and gates
		UpdateAllItems();

		if (!gameManager.gameOver)
        {
			globalSpeed += 0.02f * Time.deltaTime;
		}
	}
	
	void UpdateAllItems(){
		//find all items
		GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
		
		//for all items
		for(int i = 0; i < items.Length; i++){
			//get all meshrenderers of this item
			foreach(MeshRenderer renderer in items[i].GetComponentsInChildren<MeshRenderer>()){
				//show this item if it's sufficiently close to the player
				bool show = items[i].transform.position.z < showItemDistance;
				
				//if we want to show this item, update it's shadowCastingMode
				//since the world is a cylinder, we only need shadows for the objects on the bottom half of the cylinder
				//otherwise you'd see weird shadows everywhere coming from the objects at the top of the cylinder
				if(show)
					renderer.shadowCastingMode = (items[i].transform.position.y < shadowHeight) ? ShadowCastingMode.On : ShadowCastingMode.Off;
				
				//only enable the renderer if we want to show this item
				renderer.enabled = show;
			}
		}
	}
	
	void GenerateWorldPiece(int i){
		//create a new cylinder and put it in the pieces array
		pieces[i] = CreateCylinder();
		//position the cylinder according to its index
		pieces[i].transform.Translate(Vector3.forward * (dimensions.y * scale * Mathf.PI) * i);
		
		//update this piece so it will have an endpoint and it will move etc.
		UpdateSinglePiece(pieces[i]);
	}
	
	IEnumerator UpdateWorldPieces(){
		//remove the first piece (that is not visible to the player anymore)
		Destroy(pieces[0]);
		
		//assign the second piece to the first piece in the world array
		pieces[0] = pieces[1];
		
		//new create a new second piece
		pieces[1] = CreateCylinder();
		
		//position the new piece and rotate it to match the first piece
		pieces[1].transform.position = pieces[0].transform.position + Vector3.forward * (dimensions.y * scale * Mathf.PI);
		pieces[1].transform.rotation = pieces[0].transform.rotation;
		
		//update this newly generated world piece
		UpdateSinglePiece(pieces[1]);
		
		//wait a frame
		yield return 0;
	}
	
	void UpdateSinglePiece(GameObject piece){
		//add the basic movement script to our newly generated piece to make it move towards the player
		BasicMovement movement = piece.AddComponent<BasicMovement>();
		//make it move with a speed of globalspeed
		movement.movespeed = -globalSpeed;
		
		//set the rotate speed to the lamp (directional light) rotate speed 
		if(lampMovement != null)
			movement.rotateSpeed = lampMovement.rotateSpeed;
		
		//create an endpoint for this piece
		GameObject endPoint = new GameObject();
		endPoint.transform.position = piece.transform.position + Vector3.forward * (dimensions.y * scale * Mathf.PI);
		endPoint.transform.parent = piece.transform;
		endPoint.name = "End Point";
		
		//change the perlin noise offset to make sure each piece is different from the last one
		offset += randomness;
		
		//change the obstacle chance which means there will be more obstacles over time
		if(startObstacleChance > 5)
			startObstacleChance -= obstacleChanceAcceleration;
	}

	public GameObject CreateCylinder(){
		//create the base object for our world piece and name it
		GameObject newCylinder = new GameObject();
		newCylinder.name = "World piece";
		
		//set the current cylinder to this newly created object
		currentCylinder = newCylinder;
		
		//add a meshfilter and a mesh renderer to the new world piece
		MeshFilter meshFilter = newCylinder.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = newCylinder.AddComponent<MeshRenderer>();
		
		//give the new piece a material
		meshRenderer.material = meshMaterial;
		//generate a new mesh and assign it to the meshfilter
		meshFilter.mesh = Generate();	
		
		//after creating the mesh, add a collider that matches the new mesh
		newCylinder.AddComponent<MeshCollider>();
		
		return newCylinder;
	}
	
	//this will return the mesh for our new world piece
	Mesh Generate(){
		//create and name a new mesh
		Mesh mesh = new Mesh();
		mesh.name = "MESH";
		
		//create arrays to hold the vertices (points in 3d space), uvs (texture coordinates) and triangles (sets of vertices that make up our mesh)
		Vector3[] vertices = null;
		Vector2[] uvs = null;
		int[] triangles = null;
		
		//create the shape for our mesh by populating the arrays
		CreateShape(ref vertices, ref uvs, ref triangles);
		
		//assign vertices, uvs and triangles
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = triangles;
		
		//recalculate the normals for our world piece
		mesh.RecalculateNormals();
		
		return mesh;
	}
	
	void CreateShape(ref Vector3[] vertices, ref Vector2[] uvs, ref int[] triangles){
		
		//get the size for this piece on the x and z axis
		int xCount = (int)dimensions.x;
		int zCount = (int)dimensions.y;
		
		//initialize the vertices and uv arrays using the desired dimensions
		vertices = new Vector3[(xCount + 1) * (zCount + 1)];
		uvs = new Vector2[(xCount + 1) * (zCount + 1)];
		
		int index = 0;
		
		//get the cylinder radius
		float radius = xCount * scale * 0.5f;
		
		//nest two loops to go through all vertices on the x and z axis
		for(int x = 0; x <= xCount; x++){
			for(int z = 0; z <= zCount; z++){
				//get the angle in the cylinder to position this vertice correctly
				float angle = x * Mathf.PI * 2f/xCount;
				
				//use cosinus and sinus of the angle to set this vertice
				vertices[index] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, z * scale * Mathf.PI);
				
				//also update the uvs so we can texture the terrain
				uvs[index] = new Vector2(x * scale, z * scale);
				
				//now use our perlin scale and offset to create x and z values for the perlin noise
				float pX = (vertices[index].x * perlinScale) + offset;
				float pZ = (vertices[index].z * perlinScale) + offset;
				
				//get the center of the cylinder but keep the z of this vertice so we can point inwards to the center
				Vector3 center = new Vector3(0, 0, vertices[index].z);
				//now move this vertice inwards towards the center using perlin noise and the desired wave height
				vertices[index] += (center - vertices[index]).normalized * Mathf.PerlinNoise(pX, pZ) * waveHeight;
				
				//this part handles smooth transition between world pieces:
				
				//check if there are begin points and if we're at the start of the mesh (z means the forward direction, so through the cylinder)
				if(z < startTransitionLength && beginPoints[0] != Vector3.zero){
					//if so, we must combine the perlin noise value with the begin points
					//we need to increase the percentage of the vertice that comes from the perlin noise 
					//and decrease the percentage from the begin point
					//this way it will transition from the last world piece to the new perlin noise values
					
					//the percentage of perlin noise in the vertices will increase while we're moving further into the cylinder
					float perlinPercentage = z * (1f/startTransitionLength);
					//don't use the z begin point since it will not have the correct position and we only care about the noise on x and y axis
					Vector3 beginPoint = new Vector3(beginPoints[x].x, beginPoints[x].y, vertices[index].z);
					
					//combine the begin point(which are the last vertices from the previous world piece) and original vertice to smoothly transition to the new world piece
					vertices[index] = (perlinPercentage * vertices[index]) + ((1f - perlinPercentage) * beginPoint);
				}
				else if(z == zCount){
					//it these are the last vertices, update the begin points so the next piece will transition smoothly as well
					beginPoints[x] = vertices[index];
				}
				
				//spawn items at random positions using the mesh vertices
				if(Random.Range(0, startObstacleChance) == 0 && !(gate == null && obstacles.Length == 0))
					CreateItem(vertices[index], x);
				
				//increase the current vertice index
				index++;
			}
		}
		
		//initialize the array of triangles (x * z is the number of squares, and each square has two triangles so 6 vertices)
		triangles = new int[xCount * zCount * 6];
		
		//create the base for our squares (which makes the generation algorithm easier)
		int[] boxBase = new int[6];
		
		int current = 0;
		
		//for all x positions
		for(int x = 0; x < xCount; x++){
			//create a new base that we can use to populate a new row of squares on the z axis
			boxBase = new int[]{ 
				x * (zCount + 1), 
				x * (zCount + 1) + 1,
				(x + 1) * (zCount + 1),
				x * (zCount + 1) + 1,
				(x + 1) * (zCount + 1) + 1,
				(x + 1) * (zCount + 1),
			};
			
			//this was used to close the mesh (it would connect the last triangles to the first triangles)
			//it messes up the texture so I decided not to use it
			/*if(x == xCount - 1){
				boxBase[2] = 0;
				boxBase[4] = 1;
				boxBase[5] = 0;
			}*/
			
			//for all z positions
			for(int z = 0; z < zCount; z++){
				//increase all vertice indexes in the box by one to go to the next square on this z row
				for(int i = 0; i < 6; i++){
					boxBase[i] = boxBase[i] + 1;
				}
				
				//assign 2 new triangles based upon 6 vertices to fill in one new square
				for(int j = 0; j < 6; j++){					
					triangles[current + j] = boxBase[j] - 1;
				}
				
				//now increase current by 6 to go to the next square
				current += 6;
			}
		}
	}
	
	void CreateItem(Vector3 vert, int x){
		//get the center of the cylinder but use the z value from the vertice
		Vector3 zCenter = new Vector3(0, 0, vert.z);
		
		//check if we get a correct angle between the center and the vertice
		if(zCenter - vert == Vector3.zero || x == (int)dimensions.x/4 || x == (int)dimensions.x/4 * 3)
			return;

		//create a new item with a small chance of being a gate (gateChance) and a big chance of being an obstacle
		GameObject newItem = Instantiate((Random.Range(0, gateChance) == 0) ? gate : obstacles[Random.Range(0, obstacles.Length)]);
		//rotate the item inwards towards the center position
		newItem.transform.rotation = Quaternion.LookRotation(zCenter - vert, Vector3.up);
		//position the item at the vertice position
		newItem.transform.position = vert;
		
		//parent the new item to the current cylinder so it will move and rotate along
		newItem.transform.SetParent(currentCylinder.transform, false);
	}
	
	public Transform GetWorldPiece(){
		//return the first world piece
		return pieces[0].transform;
	}
}
