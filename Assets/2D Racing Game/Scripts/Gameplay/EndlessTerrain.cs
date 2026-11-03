using System.Collections;
using System.Collections.Generic;
using T2D;
using T2D.Modules;
using UnityEngine;

// Endless mode: builds the ground in chunks ahead of the player with the
// 2D Terrain Editor runtime (T2D), drops coins and fuel on it, and deletes chunks
// that are far behind. Hills get taller and fuel gets rarer the further you drive.
//
// Every chunk starts and ends exactly where its neighbours do, and the chunk
// width times TextureSize is a whole number, so the ground texture has no seams.
public class EndlessTerrain : MonoBehaviour
{
	[Header("Look (same materials as the normal levels)")]
	public Material fillMaterial;
	public Material capMaterial;
	public PhysicsMaterial2D groundPhysicsMaterial;
	[Tooltip("UV scale of the fill texture. Level meshes use ~0.0235.")]
	public float textureSize = 0.025f;
	public float capSize = 0.5f;
	[Tooltip("DeadTrigger on the car checks this tag")]
	public string groundTag = "Ground";
	public int groundLayer = 10;

	[Header("Chunks")]
	[Tooltip("Keep chunkWidth * textureSize a whole number so the texture lines up")]
	public int chunkWidth = 80;
	public float startX = -30f;
	[Tooltip("World Y of the bottom edge of the ground mesh")]
	public float bottomY = -29f;
	[Tooltip("Chunks kept ready in front of the player")]
	public int chunksAhead = 2;
	[Tooltip("Chunks further behind the player than this get destroyed")]
	public float keepBehind = 120f;

	[Header("Hills")]
	[Tooltip("World Y of the ground at the start")]
	public float baseY = -5f;
	[Tooltip("Flat run-up after the start, in meters")]
	public float flatLength = 40f;
	public float startAmplitude = 3f;
	public float maxAmplitude = 14f;
	[Tooltip("Distance over which hills grow from start to max amplitude")]
	public float rampDistance = 5000f;
	[Tooltip("Average length of one big hill, in meters")]
	public float hillLength = 70f;
	[Tooltip("Small bumps on top of the hills, as a share of the hill height")]
	[Range(0f, 0.5f)] public float bumpiness = 0.15f;
	[Tooltip("Steepest allowed slope (1 = 45 degrees)")]
	public float maxSlope = 0.85f;
	[Tooltip("0 = random every run")]
	public int seed = 0;

	[Header("Items")]
	[Tooltip("Coin prefabs, cheapest first. Higher ones show up further away.")]
	public GameObject[] coinPrefabs;
	public GameObject fuelPrefab;
	[Tooltip("Height of items above the ground (levels use 1)")]
	public float itemHeight = 1f;
	public float firstCoinsAt = 30f;
	public float coinSetSpacing = 32f;
	public int coinsPerSet = 7;
	public float coinSpacing = 1.4f;
	[Tooltip("Distance between coin tiers (the next prefab in the list is used after this many meters)")]
	public float coinTierDistance = 1500f;
	public float firstFuelAt = 300f;
	public float fuelSpacing = 350f;
	[Tooltip("Extra fuel spacing per 1000 m driven")]
	public float fuelSpacingGrowth = 40f;
	public float maxFuelSpacing = 700f;

	[Header("Left wall (follows the oldest chunk)")]
	public BoxCollider2D leftWall;
	[Tooltip("How far right of the oldest chunk's start the wall sits")]
	public float leftWallOffset = 20f;

	class Chunk
	{
		public float x;
		public GameObject root;
	}

	readonly List<Chunk> chunks = new List<Chunk>();
	Transform player;
	float nextChunkX;
	float lastHeight;
	float nextCoinSetX;
	float nextFuelX;
	Vector2 noiseOffset;

	IEnumerator Start()
	{
		if (seed != 0)
			Random.InitState(seed);
		noiseOffset = new Vector2(Random.Range(0f, 1000f), Random.Range(0f, 1000f));

		nextChunkX = startX;
		lastHeight = baseY;
		nextCoinSetX = firstCoinsAt;
		nextFuelX = firstFuelAt;

		// Build the first chunks right away so the car has ground when it spawns.
		for (int i = 0; i <= chunksAhead; i++)
			AddChunk();

		// The car is spawned by StartPoint in its own Start, so find it a frame later.
		while (player == null)
		{
			var go = GameObject.FindGameObjectWithTag("Player");
			if (go != null)
				player = go.transform;
			else
				yield return null;
		}
	}

	void Update()
	{
		if (player == null)
			return;

		float px = player.position.x;

		while (nextChunkX - px < chunksAhead * chunkWidth)
			AddChunk();

		while (chunks.Count > 2 && chunks[0].x + chunkWidth < px - keepBehind)
		{
			Destroy(chunks[0].root);
			chunks.RemoveAt(0);
			MoveLeftWall();
		}
	}

	void AddChunk()
	{
		float x0 = nextChunkX;
		var heights = new float[chunkWidth + 1];
		heights[0] = lastHeight;
		for (int i = 1; i <= chunkWidth; i++)
		{
			float target = HeightAt(x0 + i);
			heights[i] = Mathf.Clamp(target, heights[i - 1] - maxSlope, heights[i - 1] + maxSlope);
		}

		var path = new AnimationCurve();
		for (int i = 0; i <= chunkWidth; i++)
			path.AddKey(i, Mathf.Max(0f, heights[i] - bottomY));

		var terrain = new GameObject("Endless Ground " + chunks.Count).AddComponent<Terrain2D>();
		terrain.transform.SetParent(transform, false);
		terrain.transform.position = new Vector3(x0, bottomY, 0f);
		terrain.gameObject.tag = groundTag;
		terrain.gameObject.layer = groundLayer;
		terrain.MeshResolution = 1f;
		terrain.TextureSize = textureSize;
		terrain.MeshRenderer.sharedMaterial = fillMaterial;

		var edge = terrain.AddModule<Terrain2DEdgeCollider>();
		edge.EdgeCollider2D.sharedMaterial = groundPhysicsMaterial;

		var cap = terrain.AddModule<Terrain2DCap>();
		cap.MeshRenderer.sharedMaterial = capMaterial;
		cap.Size = capSize;

		terrain.Path = path;
		terrain.Build();

		SpawnItems(terrain.transform, x0, heights);

		chunks.Add(new Chunk { x = x0, root = terrain.gameObject });
		nextChunkX = x0 + chunkWidth;
		lastHeight = heights[chunkWidth];

		if (chunks.Count == 1)
			MoveLeftWall();
	}

	float HeightAt(float x)
	{
		float distance = x - startX - flatLength;
		if (distance <= 0f)
			return baseY;

		// Ease in from the flat start, then grow the hills with distance.
		float easeIn = Mathf.SmoothStep(0f, 1f, distance / 60f);
		float amplitude = Mathf.Lerp(startAmplitude, maxAmplitude, Mathf.Clamp01(distance / rampDistance));

		float big = Mathf.PerlinNoise(x / hillLength + noiseOffset.x, noiseOffset.y) - 0.45f;
		float small = Mathf.PerlinNoise(x / (hillLength * 0.2f) + noiseOffset.y, noiseOffset.x) - 0.5f;

		return baseY + easeIn * amplitude * 2f * (big + small * bumpiness);
	}

	void SpawnItems(Transform parent, float x0, float[] heights)
	{
		float x1 = x0 + chunkWidth;

		while (nextCoinSetX < x1 - coinsPerSet * coinSpacing)
		{
			GameObject prefab = PickCoin(nextCoinSetX - startX);
			if (prefab != null)
				for (int i = 0; i < coinsPerSet; i++)
					Spawn(prefab, parent, nextCoinSetX + i * coinSpacing, x0, heights);
			nextCoinSetX += coinSetSpacing;
		}

		while (nextFuelX < x1)
		{
			if (fuelPrefab != null)
				Spawn(fuelPrefab, parent, nextFuelX, x0, heights);
			float distanceFromStart = nextFuelX - startX;
			nextFuelX += Mathf.Min(maxFuelSpacing, fuelSpacing + fuelSpacingGrowth * distanceFromStart / 1000f);
		}
	}

	GameObject PickCoin(float distance)
	{
		if (coinPrefabs == null || coinPrefabs.Length == 0)
			return null;
		int tier = Mathf.Clamp((int)(distance / coinTierDistance), 0, coinPrefabs.Length - 1);
		return coinPrefabs[tier];
	}

	void Spawn(GameObject prefab, Transform parent, float x, float x0, float[] heights)
	{
		float local = Mathf.Clamp(x - x0, 0f, chunkWidth);
		int i = Mathf.Min((int)local, chunkWidth - 1);
		float y = Mathf.Lerp(heights[i], heights[i + 1], local - i);
		Instantiate(prefab, new Vector3(x, y + itemHeight, 0f), Quaternion.identity, parent);
	}

	void MoveLeftWall()
	{
		if (leftWall == null || chunks.Count == 0)
			return;
		var t = leftWall.transform;
		t.position = new Vector3(chunks[0].x + leftWallOffset, t.position.y, t.position.z);
	}
}
