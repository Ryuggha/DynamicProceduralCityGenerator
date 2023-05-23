using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TerrainShape))]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] bool GenerateRoads = true;
    [SerializeField] bool GenerateTerrain = true;

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
        PlayerInteraction.instance.transform.position += initialPlayerPosition;
    }

    private void Update()
    {
        float delta = Time.deltaTime;
        if (GenerateTerrain) TerrainShape.instance.Tick(delta);
        if (GenerateRoads) RoadGeneration.instance.Tick(delta);
    }
}
