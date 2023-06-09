using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationManager : MonoBehaviour
{
    public static DecorationManager instance;

    [SerializeField] GameObject decorationHolder;
    [SerializeField] public LayerMask terrainLayers;

    [SerializeField] List<GameObject> RoadFloorDecorations;
    [SerializeField] [Range(0, 2)] float floorDecorationDensity = 1;
    [SerializeField] [Range(0, 5)] float floorDecorationSpacing = 1.5f;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public void decorateRoad(Road road)
    {
        float numberOfDecorations = Mathf.FloorToInt(road.getWidth() * (road.getPositionEnd() - road.getPositionStart()).magnitude * floorDecorationDensity);
        for (int i = 0; i < numberOfDecorations; i++)
        {
            var decor = Instantiate(RoadFloorDecorations[Random.Range(0, RoadFloorDecorations.Count)]);
            if (decorationHolder != null)
            {
                decor.transform.parent = decorationHolder.transform;
            }

            Vector3 position = Vector3.Lerp(road.getPositionStart(), road.getPositionEnd(), Random.Range(0f, 1f));
            Vector3 perpendicularDirection = Vector3.Cross((road.getPositionEnd() - road.getPositionStart()).normalized, Vector3.up).normalized;
            position += Vector3.Lerp(-perpendicularDirection * floorDecorationSpacing, perpendicularDirection * floorDecorationSpacing, Random.Range(0f, 1f));
            
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * TerrainShape.instance.altitude * 2, Vector3.down, out hit, TerrainShape.instance.altitude * 2, terrainLayers)) {
                decor.transform.rotation = Quaternion.LookRotation(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)), hit.normal);
            }
            else
            {
                decor.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), 0);
                Debug.Log("n" + position);
            }

            decor.transform.position = TerrainShape.instance.getSurfacePointAtPosition(position) + new Vector3(0, -.06f, 0);

            
        }
    }
}
