using System.Collections.Generic;
using UnityEngine;

public class TerrainShape : MonoBehaviour
{
    public static TerrainShape instance;

    [SerializeField] GameObject terrainHolder;
    [SerializeField] int squareResolution = 256;
    [SerializeField] int size = 256;
    [SerializeField] public float altitude;
    [SerializeField] float scale;
    [SerializeField] TerrainLayer[] terrainLayers;
    [SerializeField] Material terrainMaterial;

    Dictionary<Vector2Int, Terrain> terrainDictionary;
    Dictionary<Vector2Int, float[,]> auxHeights;
    HashSet<Vector2Int> changedTerrains;

    private void Awake()
    {

        if (instance == null) instance = this;
        else Destroy(this);

        changedTerrains = new HashSet<Vector2Int>();
        auxHeights = new Dictionary<Vector2Int, float[,]>();

        terrainDictionary = new Dictionary<Vector2Int, Terrain>();
        addTerrain(0, 0);
        addTerrain(-1, 0);
        addTerrain(0, -1);
        addTerrain(-1, -1);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public Vector2Int getTerrainChunkIndex(Vector2 vector)
    {
        return getTerrainChunkIndex(new Vector3(vector.x, 0, vector.y));
    }

    public Vector2Int getTerrainChunkIndex(Vector3 vector)
    {
        Vector2Int index = new Vector2Int(Mathf.FloorToInt(vector.x / size), Mathf.FloorToInt(vector.z / size));
        if (!terrainDictionary.ContainsKey(index)) { addTerrain(index); }
        return index;
    }


    private void addTerrain(Vector2Int index)
    {
        addTerrain(index.x, index.y);
    }

    private void addTerrain(int x, int y)
    {
        var index = new Vector2Int(x, y);
        if (terrainDictionary.ContainsKey(index)) throw new System.Exception("This Index is already in existance");

        GameObject terGo = new GameObject($"Terrain - {x}:{y}");
        terGo.layer = 9;
        if (terrainHolder != null)
        {
            terGo.transform.parent = terrainHolder.transform;
        }
        
        terGo.transform.position = new Vector3(x * size, 0, y * size);

        Terrain terrain = terGo.AddComponent<Terrain>();
        terrainDictionary.Add(index, terrain);
        terrain.terrainData = new TerrainData();
        terrain.terrainData.SetTerrainLayersRegisterUndo(terrainLayers, "Set Terrain Textures");
        terrain.materialTemplate = terrainMaterial;

        TerrainCollider collider = terGo.AddComponent<TerrainCollider>();
        collider.terrainData = terrain.terrainData;

        updateTerrain(index);
    }

    public void generateBuildingFundations(BoxCollider plain, Vector3 doorPoint)
    {
        float targetHeight = doorPoint.y / altitude;

        Vector3[] edges = new Vector3[4];
        edges[0] = Quaternion.AngleAxis(plain.transform.eulerAngles.y, Vector3.up) * new Vector3(plain.center.x + plain.size.x / 2, 0, plain.center.z + plain.size.z / 2) + plain.transform.position;
        edges[1] = Quaternion.AngleAxis(plain.transform.eulerAngles.y, Vector3.up) * new Vector3(plain.center.x - plain.size.x / 2, 0, plain.center.z + plain.size.z / 2) + plain.transform.position;
        edges[2] = Quaternion.AngleAxis(plain.transform.eulerAngles.y, Vector3.up) * new Vector3(plain.center.x - plain.size.x / 2, 0, plain.center.z - plain.size.z / 2) + plain.transform.position;
        edges[3] = Quaternion.AngleAxis(plain.transform.eulerAngles.y, Vector3.up) * new Vector3(plain.center.x + plain.size.x / 2, 0, plain.center.z - plain.size.z / 2) + plain.transform.position;

        List<Vector2Int> keys = new List<Vector2Int>();

        for (int i = 0; i < edges.Length; i++)
        {
            Vector2Int key = getTerrainChunkIndex(edges[i]);
            if (!keys.Contains(key)) keys.Add(key);
        }

        foreach (var key in keys)
        {
            changedTerrains.Add(key);

            if (!auxHeights.ContainsKey(key)) auxHeights.Add(key, terrainDictionary[key].terrainData.GetHeights(0, 0, squareResolution, squareResolution));

            for (int j = 0; j < squareResolution; j++)
            {
                for (int i = 0; i < squareResolution; i++)
                {
                    Vector2 point = new Vector2(((float)j) * size / squareResolution + key.x * squareResolution, ((float)i) * size / squareResolution + key.y * squareResolution);
                    if (pointInsideTriangle2D(point, vector3ToVector2TopDown(edges[0]), vector3ToVector2TopDown(edges[1]), vector3ToVector2TopDown(edges[2])) || pointInsideTriangle2D(point, vector3ToVector2TopDown(edges[0]), vector3ToVector2TopDown(edges[3]), vector3ToVector2TopDown(edges[2])))
                        auxHeights[key][i, j] = targetHeight;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (changedTerrains.Count > 0)
        {
            foreach (var key in changedTerrains)
            {
                terrainDictionary[key].terrainData.SetHeights(0, 0, auxHeights[key]);
            }

            changedTerrains.Clear();
        }
    }

    public static Vector2 vector3ToVector2TopDown(Vector3 v) { return new Vector2(v.x, v.z); }

    private bool pointInsideTriangle2D(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = sign(point, a, b);
        d2 = sign(point, b, c);
        d3 = sign(point, c, a);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }

    private float sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    private void updateTerrain(Vector2Int key)
    {
        terrainDictionary[key].terrainData = generateTerrain(terrainDictionary[key].terrainData, key);
    }

    private TerrainData generateTerrain(TerrainData data, Vector2Int coords)
    {
        var heights = generateHeights(coords.x, coords.y);
        data.heightmapResolution = heights.GetLength(0);
        data.size = new Vector3(size, altitude, size);
        data.SetHeights(0, 0, heights);
        return data;
    }

    private float[,] generateHeights(int x, int y)
    {
        int r = squareResolution + 1;

        float[,] heights = new float[r, r];

        for (int i = 0; i < r; i++)
        {
            for (int j = 0; j < r; j++)
            {
                heights[j, i] = Mathf.PerlinNoise(((x * size) + (((float)i) * size / squareResolution)) * scale / 200, ((y * size) + (((float)j) * size / squareResolution)) * scale / 200);
            }
        }

        return heights;
    }

    public Vector3 getSurfacePointAtPosition(Vector2 vector)
    {
        return getSurfacePointAtPosition(new Vector3(vector.x, 0, vector.y));
    }

    public Vector3 getSurfacePointAtPosition(Vector3 vector)
    {
        Vector2Int index = getTerrainChunkIndex(vector);

        float height = terrainDictionary[index].SampleHeight(vector);

        return new Vector3(vector.x, height, vector.z);
    }

}
