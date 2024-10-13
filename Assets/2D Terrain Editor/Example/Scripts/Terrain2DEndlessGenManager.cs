using System.Collections.Generic;
using System.Linq;
using T2D.Modules;
using UnityEngine;
using Random = UnityEngine.Random;

namespace T2D.Example
{
    public class Terrain2DEndlessGenManager : MonoBehaviour
    {
        [Range(10, 1000)]
        public int TerrainWidth = 50;
        [Range(10, 100)]
        public float TerrainMinHeight = 10;
        public float TerrainAmplitude = 10;
        [Range(0f, 1f)]
        public float TerrainPathPeriod = 0.05f;
        public int SnapToPathObjectsPerTerrain = 5;


        [Header("References")]
        public Terrain2DEndlessGenPlayer Player;
        public Material FillMaterial;
        public Material CapMaterial;
        public GameObject SnapToPathObject;


        private List<Terrain2D> _terrainsPool = new List<Terrain2D>();
        private float _perlinOffset;


        void Awake()
        {
            //Generate random offset for Perlin noise which will be used to generate terrains Path
            _perlinOffset = Random.Range(0f, 1f);

            //Create left, middle and right terrain objects under Player position
            for (int i = 0; i < 3; i++)
            {
                float position = Player.transform.position.x + (i - 1) * TerrainWidth;
                var newTerrain = CreateNewTerrain(new Vector3(position, 0, 0));

                //Generate initial path based on position
                GeneratePath(newTerrain, position);

                for (int j = 0; j < SnapToPathObjectsPerTerrain; j++)
                {
                    //Create new SnapToPathObject
                    GameObject newSnapToPathObj = Instantiate(SnapToPathObject);
                    newSnapToPathObj.transform.parent = newTerrain.transform;

                    //Calculate X position for newSnapToPathObj
                    float snapToPathObjPos = j * (TerrainWidth / SnapToPathObjectsPerTerrain);
                    newSnapToPathObj.transform.localPosition = new Vector3(snapToPathObjPos, 0, 0);
                }

                //Add terrain to pool
                _terrainsPool.Add(newTerrain);
            }
        }

        void LateUpdate()
        {
            //Get the middle terrain and it's world space bounds from Renderer
            var midTerrain = _terrainsPool[1];
            var midTerrainBounds = midTerrain.MeshRenderer.bounds;

            //Check if the Player crossed the end point of middle terrain
            if (Player.transform.position.x > midTerrainBounds.max.x)
            {
                //Get the first terrain from the pool
                var outOfViewTerrain = _terrainsPool.First();

                //Calculate and assign new position for outOfViewTerrain
                var newPos = _terrainsPool.Last().transform.position + new Vector3(TerrainWidth, 0, 0);
                outOfViewTerrain.transform.position = newPos;
                
                //Generate new path based on X position
                GeneratePath(outOfViewTerrain, newPos.x);

                //Swap first terrain with last terrain in the pool
                _terrainsPool.Remove(outOfViewTerrain);
                _terrainsPool.Add(outOfViewTerrain);
            }
            //Check if the Player crossed the start point of middle terrain
            else if (Player.transform.position.x < midTerrainBounds.min.x)
            {
                //Get the last terrain from the pool
                var outOfViewTerrain = _terrainsPool.Last();

                //Calculate and assign new position for outOfViewTerrain
                var newPos = _terrainsPool.First().transform.position - new Vector3(TerrainWidth, 0, 0);
                outOfViewTerrain.transform.position = newPos;

                //Generate terrain path based on X position
                GeneratePath(outOfViewTerrain, newPos.x);

                //Swap last terrain with first terrain in the pool
                _terrainsPool.Remove(outOfViewTerrain);
                _terrainsPool.Insert(0, outOfViewTerrain);
            }
        }

        void GeneratePath(Terrain2D target, float position)
        {
            //Create new path curve
            AnimationCurve newPath = new AnimationCurve();
            
            for (int i = 0; i <= TerrainWidth; i++)
            {
                //Get position for the new key
                float pathPointPosition = i;

                //Sample value for Perlin noise
                float samplePos = (position + i) * TerrainPathPeriod;

                //Calculate Path Point keyframe value for the curve
                float height = Mathf.PerlinNoise(samplePos + _perlinOffset, samplePos - _perlinOffset) * TerrainAmplitude;
                newPath.AddKey(pathPointPosition, TerrainMinHeight + height);
            }

            //Assign generated path to terrain
            target.Path = newPath;

            //Build terrain mesh and all it's modules
            target.Build();
        }

        Terrain2D CreateNewTerrain(Vector3 atPosition)
        {
            //Create new object Terrain2D with fill material
            Terrain2D newTerrain = new GameObject(nameof(Terrain2D)).AddComponent<Terrain2D>();
            newTerrain.transform.position = atPosition;
            newTerrain.MeshRenderer.material = FillMaterial;

            //Add collider module
            newTerrain.AddModule<Terrain2DEdgeCollider>();

            //Add cap module
            var cap = newTerrain.AddModule<Terrain2DCap>();
            cap.MeshRenderer.material = CapMaterial;

            //Add path snap group module
            newTerrain.AddModule<Terrain2DPathSnapGroup>();

            return newTerrain;
        }
    }
}
