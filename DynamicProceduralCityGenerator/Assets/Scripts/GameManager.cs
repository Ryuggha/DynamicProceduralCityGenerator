using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TerrainShape))]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] bool GenerateRoads = true;

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
        var initialPlayerPosition = TerrainShape.instance.getSurfacePointAtPosition(RoadGeneration.instance.InitializeRoads()) + new Vector3(0, 3, 0);
        PlayerInteraction.instance.InitializePlayer(initialPlayerPosition);
    }

    private void Update()
    {
        float delta = Time.deltaTime;
        if (GenerateRoads) RoadGeneration.instance.Tick(delta);
    }
}
