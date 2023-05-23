using System.Collections.Generic;
using UnityEngine;

public class TerrainShape : MonoBehaviour
{
    public static TerrainShape instance;

    [SerializeField] int squareResolution = 256;
    [SerializeField] float altitude;
    [SerializeField] float scale;
    [SerializeField] TerrainLayer[] terrainLayers;
    [SerializeField] Material terrainMaterial;

    Dictionary<Vector2Int, Terrain> aux;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Start()
    {
        aux = new Dictionary<Vector2Int, Terrain>();

        addTerrain(0, 0);
        addTerrain(0, -1);
        addTerrain(-1, -1);
        addTerrain(-1, 0);

        Tick(0);
    }

    private void addTerrain(int x, int y)
    {
        GameObject terGo = new GameObject($"Terrain - {x}:{y}");
        terGo.layer = 9;
        terGo.transform.parent = transform;
        terGo.transform.position = new Vector3(x * squareResolution, 0, y * squareResolution);

        Terrain terrain = terGo.AddComponent<Terrain>();
        aux.Add(new Vector2Int(x, y), terrain);
        terrain.terrainData = new TerrainData();
        terrain.terrainData.SetTerrainLayersRegisterUndo(terrainLayers, "Set Terrain Textures");
        terrain.materialTemplate = terrainMaterial;

        TerrainCollider collider = terGo.AddComponent<TerrainCollider>();
        collider.terrainData = terrain.terrainData;
    }

    public void Tick(float delta) 
    {
        foreach (var key in aux.Keys)
        {
            aux[key].terrainData = generateTerrain(aux[key].terrainData, key);
        }
    }

    private TerrainData generateTerrain(TerrainData data, Vector2Int coords)
    {
        data.heightmapResolution = squareResolution + 1;
        data.size = new Vector3(squareResolution, altitude, squareResolution);

        data.SetHeights(0, 0, generateHeights(coords.x, coords.y));
        return data;
    }

    private float[,] generateHeights(int x, int y)
    {
        float[,] heights = new float[squareResolution + 1, squareResolution + 1];

        for (int i = 0; i < squareResolution + 1; i++)
        {
            for (int j = 0; j < squareResolution + 1; j++)
            {
                heights[j, i] = Mathf.PerlinNoise(((float)i / (squareResolution + 1) + x) * scale, ((float)j / (squareResolution + 1) + y) * scale);
            }
        }

        return heights;
    }

}
