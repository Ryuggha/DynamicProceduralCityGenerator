using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TerrainShape))]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] bool GenerateRoads = true;

    public GameObject o1, o2, o3;

    void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        var initialPlayerPosition = RoadGeneration.instance.InitializeRoads();
        PlayerInteraction.instance.transform.GetChild(0).position = TerrainShape.instance.getSurfacePointAtPosition(initialPlayerPosition) + new Vector3(0, 3, 0);
    }

    private void Update()
    {
        float delta = Time.deltaTime;
        if (GenerateRoads) RoadGeneration.instance.Tick(delta);
    }
}
