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
    [SerializeField] [Range(0, 3)] public float straightRoadsBias;
    [SerializeField] AnimationCurve roadDiversionCurve;

    [SerializeField] float TEST_ActivationDistance = 20;
    
    public AnimationCurveSampler curveSampler;

    List<Road> roadList;

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

        Point startingPoint = HexagonalGrid.instance.getFirstPoint();

        roadList.Add(new Road(startingPoint.lines[0], initialWidth));

        return (roadList[0].getPositionEnd() - roadList[0].getPositionStart()) / 2 + roadList[0].getPositionStart();
    }

    public void Tick(float delta)
    {
        if (roadList == null) return;
        for (int i = 0; i < roadList.Count; i++)
        {
            Road road = roadList[i];
            if (road.hasOpenRoads())
            {
                if (
                    (road.getPositionStart() - PlayerInteraction.instance.getPlayerPosition()).magnitude < TEST_ActivationDistance ||
                    (road.getPositionEnd() - PlayerInteraction.instance.getPlayerPosition()).magnitude < TEST_ActivationDistance)
                {   
                    if (road != null) roadList.AddRange(road.expand());
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
        SphereCollider col;
        GameObject colGO = new GameObject($"Road - {road.getPositionStart()}:{road.getPositionEnd()}");
        colGO.layer = 14;
        colGO.transform.parent = transform;
        colGO.transform.position = road.getPositionStart();
        colGO.transform.LookAt(road.getPositionEnd(), Vector3.up);

        Vector3 roadVector = road.getPositionEnd() - road.getPositionStart();

        for (float i = 0; i < roadVector.magnitude; i += road.getWidth() + .75f)
        {
            col = colGO.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0, 0, i);
            col.center = TerrainShape.instance.getSurfacePointAtPosition(col.center);
            Debug.LogWarning("TODO: Height not correct");
            col.radius = road.getWidth() * .75f;
        }

        col = colGO.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.center = new Vector3(0, roadVector.magnitude);
        col.center = TerrainShape.instance.getSurfacePointAtPosition(col.center);
        col.radius = road.getWidth() * .75f;
    }
}
