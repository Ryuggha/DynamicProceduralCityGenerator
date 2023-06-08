using UnityEngine;
using System.Collections.Generic;

public class RoadFrustrumSensor : MonoBehaviour
{
    private List<Bounds> boundsList;
    private Road road;
    private Vector3 deviation;

    private static Plane[] cameraFrustum;

    public void Initialize(Road road, List<Collider> colList)
    {
        this.road = road;
        this.boundsList = new List<Bounds>();
        foreach (var col in colList)
        {
            boundsList.Add(col.bounds);
        }
        deviation = Vector3.Cross((road.getPositionStart() - road.getPositionEnd()).normalized, Vector3.up).normalized * road.getWidth();
    }

    private void Update()
    {
        if (cameraFrustum == null) cameraFrustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        foreach (var bounds in boundsList)
        {
            if (GeometryUtility.TestPlanesAABB(cameraFrustum, bounds))
            {
                Debug.DrawRay(bounds.center - deviation * 2, Camera.main.transform.position - (bounds.center - deviation * 2), Color.red, 0.2f);
                Debug.DrawRay(bounds.center - deviation, Camera.main.transform.position - (bounds.center - deviation), Color.red, 0.2f);
                Debug.DrawRay(bounds.center, Camera.main.transform.position - bounds.center, Color.red, 0.2f);
                Debug.DrawRay(bounds.center + deviation, Camera.main.transform.position - (bounds.center + deviation), Color.red, 0.2f);
                Debug.DrawRay(bounds.center + deviation * 2, Camera.main.transform.position - (bounds.center + deviation * 2), Color.red, 0.2f);

                if (!Physics.Raycast(bounds.center - deviation*2, Camera.main.transform.position - (bounds.center - deviation*2), (Camera.main.transform.position - bounds.center).magnitude, RoadGeneration.instance.visibleLayers) ||
                    !Physics.Raycast(bounds.center - deviation, Camera.main.transform.position - (bounds.center - deviation), (Camera.main.transform.position - bounds.center).magnitude, RoadGeneration.instance.visibleLayers) ||
                    !Physics.Raycast(bounds.center, Camera.main.transform.position - bounds.center, (Camera.main.transform.position - bounds.center).magnitude, RoadGeneration.instance.visibleLayers) ||
                    !Physics.Raycast(bounds.center + deviation, Camera.main.transform.position - (bounds.center + deviation), (Camera.main.transform.position - bounds.center).magnitude, RoadGeneration.instance.visibleLayers) ||
                    !Physics.Raycast(bounds.center + deviation*2, Camera.main.transform.position - (bounds.center + deviation*2), (Camera.main.transform.position - bounds.center).magnitude, RoadGeneration.instance.visibleLayers)) 
                {
                    RoadGeneration.instance.expandRoad(road);
                    Destroy(this);
                }
            }
        }
    }

    private void LateUpdate()
    {
        cameraFrustum = null;
    }
}
