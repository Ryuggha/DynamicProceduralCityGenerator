using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HexagonalGrid))]
public class RoadGeneration : MonoBehaviour
{
    public static RoadGeneration instance;

    [Header("City Parameters")]
    [SerializeField] [Range(0, 15)] public float maxWidth;
    [SerializeField] [Range(0, 15)] public float minWidth;
    [SerializeField] [Range(0, 15)] float initialWidth;
    [SerializeField] [Range(0, 6)] public float straightRoadsBias;
    [SerializeField] AnimationCurve roadDiversionCurve;

    [SerializeField] float TEST_ActivationDistance = 20;
    
    public AnimationCurveSampler curveSampler;

    List<Road> roadList;
    List<Road> openRoadList;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public Vector3 InitializeRoads()
    {
        curveSampler = new AnimationCurveSampler(roadDiversionCurve);
        roadList = new List<Road>();
        openRoadList = new List<Road>();

        Point startingPoint = HexagonalGrid.instance.getFirstPoint();

        Road firstRoad = new Road(startingPoint.lines[0], initialWidth);
        roadList.Add(firstRoad);
        openRoadList.Add(firstRoad);

        return (roadList[0].getPositionEnd() - roadList[0].getPositionStart()) / 2 + roadList[0].getPositionStart();
    }

    public void Tick(float delta)
    {
        if (openRoadList == null) return;


        List<Road> roadsToOpen = new List<Road>(openRoadList);

        for (int i = 0; i < roadsToOpen.Count; i++)
        {
            Road road = roadsToOpen[i];
            if (    (road.getPositionStart() - PlayerInteraction.instance.getPlayerPosition()).magnitude < TEST_ActivationDistance ||
                    (road.getPositionEnd() - PlayerInteraction.instance.getPlayerPosition()).magnitude < TEST_ActivationDistance)
            {
                if (road != null && road.hasOpenRoads())
                {
                    var roads = road.expand();
                    roadList.AddRange(roads);
                    openRoadList.AddRange(roads);
                }
            }
            
        }
    }

    public Road findIntersection(Point point)
    {
        foreach (var road in roadList)
        {
            if (road.getPositionStart().Equals(point.getLocationVector3())) return road;
            if (road.getPositionEnd().Equals(point.getLocationVector3())) return road;
        }
        return null;
    }

    private Road findRandomToExpand()
    {
        foreach (var r in roadList)
        {
            if (r.hasOpenRoads()) return r; 
        }
        return null;
    }
    
    public float calculateNewWidth(float actualWidth, float angle)
    {
        return initialWidth;
    }

    public void generateRoadColliders(Road road)
    {
        float height;
        SphereCollider col;
        GameObject colGO = new GameObject($"Road - {road.getPositionStart()}:{road.getPositionEnd()}");
        colGO.layer = 14;
        colGO.transform.parent = transform;
        colGO.transform.position = road.getPositionStart();
        colGO.transform.LookAt(road.getPositionEnd(), Vector3.up);

        Vector3 roadVector = road.getPositionEnd() - road.getPositionStart();

        for (float i = 0; i < roadVector.magnitude; i += road.getWidth() * .9f / 1.7f)
        {
            col = colGO.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0, 0, i);
            height = TerrainShape.instance.getSurfacePointAtPosition(col.bounds.center).y;
            col.center = new Vector3(0, height, i);
            col.radius = road.getWidth() * .9f / 2;
        }

        col = colGO.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.center = new Vector3(0, 0, roadVector.magnitude);
        height = TerrainShape.instance.getSurfacePointAtPosition(col.bounds.center).y;
        col.center = new Vector3(0, height, roadVector.magnitude);
        col.radius = road.getWidth() * .9f / 2;
    }
}
