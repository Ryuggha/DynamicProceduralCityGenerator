using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGeneration : MonoBehaviour
{
    public static BuildingGeneration instance;

    [SerializeField] List<GameObject> buildingList;

    [Header("Parameters")]
    [SerializeField] float maxRotation = 5;
    [SerializeField] float initialDeviation = 2;
    [SerializeField] float finalDeviation = .1f;
    [SerializeField] float deviationShrinkRate = .5f;
    [SerializeField] public LayerMask buildingGenerationLayerMask;
    [SerializeField] int maxNumberOfGenerationTries = 3;

    BuildingBoundingBox lastBuildingBuilt;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private bool generateBuilding(Vector3 buildingPosition, Vector3 directionToMove, Vector3 directionToFace, Vector3 initialPosition, Vector3 lastAvailablePosition)
    {
        
        float colliderSize = -1;
        for (int i = 0; i < maxNumberOfGenerationTries; i++)
        {
            BuildingBoundingBox auxBuilding;
            do
            {
                auxBuilding = buildingList[Random.Range(0, buildingList.Count)].GetComponent<BuildingBoundingBox>();
            } while (colliderSize != -1 && auxBuilding.getColliderSize() >= colliderSize);

            BuildingBoundingBox newBuilding = Instantiate(auxBuilding.gameObject, buildingPosition, Quaternion.LookRotation(directionToFace, Vector3.up)).GetComponent<BuildingBoundingBox>();
            newBuilding.getPositionZeroOfObject(newBuilding.getEntranceList()[Random.Range(0, newBuilding.getEntranceList().Count)].localPosition);
            newBuilding.transform.eulerAngles += new Vector3(0, Random.Range(-maxRotation, maxRotation), 0);
            newBuilding.transform.position = TerrainShape.instance.getSurfacePointAtPosition(newBuilding.transform.position);
            
            if (newBuilding.getCollisions().Count != 0)
            {
                if (!testDeviation(newBuilding, initialDeviation, directionToMove, initialPosition, lastAvailablePosition))
                {
                    Destroy(newBuilding.gameObject);
                    newBuilding = null;
                }
            }
            lastBuildingBuilt = newBuilding;
            if (lastBuildingBuilt != null) return true;
        }
        return false;
    }

    public void generateBuildingLayer(Vector3 directionToFace, Vector3 initialPosition, Vector3 lastAvailablePosition)
    {
        Vector3 directionToMove = (lastAvailablePosition - initialPosition).normalized;
        Vector3 nextPosition = initialPosition;
        bool buildingBuilt;

        do
        {
            buildingBuilt = generateBuilding(nextPosition, directionToMove, directionToFace, initialPosition, lastAvailablePosition);
            if (buildingBuilt)
            {
                nextPosition = lastBuildingBuilt.transform.position;
                foreach (var collider in lastBuildingBuilt.getFloorColliders())
                {
                    TerrainShape.instance.generateBuildingFundations(collider, lastBuildingBuilt.transform.position);
                }
            }
        } while (buildingBuilt);
    }

    public static Vector3 getPositionWithY0(Vector3 vector) { return new Vector3(vector.x, 0, vector.z); }

    private bool reachedLastAvailablePosition(Vector3 actualPosition, Vector3 directionToMove, Vector3 initialPosition, Vector3 lastAvailablePosition)
    {
        if (Vector3.Dot(getPositionWithY0(directionToMove).normalized, (getPositionWithY0(lastAvailablePosition) - getPositionWithY0(actualPosition)).normalized) < 0f && 
            Vector3.Dot(getPositionWithY0(-directionToMove).normalized, (getPositionWithY0(lastAvailablePosition) - getPositionWithY0(initialPosition)).normalized) < 0f)
        {
            return true;
        }
        return false;
    }

    public void populateRoad(Road road)
    {
        lastBuildingBuilt = null;

        Vector3 roadDirection = (road.getPositionEnd() - road.getPositionStart()).normalized;

        if (roadDirection.Equals(Vector3.zero)) 
            throw new System.Exception("Direction to Move can't be 0");

        Vector3 perpendicularDirection = Vector3.Cross(roadDirection, Vector3.up).normalized;

        for (int iterations = 0; iterations < 2; iterations++)
        {
            Vector3 initialPosition = road.getPositionStart() + (perpendicularDirection * road.getWidth() / 2);
            Vector3 endPosition = road.getPositionEnd() + (perpendicularDirection * road.getWidth() / 2);
            Debug.DrawLine(initialPosition, endPosition, Color.blue, 160);

            RaycastHit hit;
            if (Physics.CapsuleCast(initialPosition - (Vector3.up * 30), initialPosition + (Vector3.up * 30), .1f, roadDirection * -1, out hit, (road.getPositionEnd() - road.getPositionStart()).magnitude / 2, buildingGenerationLayerMask))
            {
                initialPosition = getPositionWithY0(hit.point);
            }
            if (Physics.CapsuleCast(endPosition - (Vector3.up * 30), endPosition + (Vector3.up * 30), .1f, roadDirection, out hit, (road.getPositionEnd() - road.getPositionStart()).magnitude / 2, buildingGenerationLayerMask))
            {
                endPosition = getPositionWithY0(hit.point);
            }
            Debug.DrawLine(initialPosition, endPosition, Color.red, 160);
            Debug.DrawLine(initialPosition, initialPosition + (endPosition - initialPosition) / 2, Color.yellow, 160);
            generateBuildingLayer(-perpendicularDirection, initialPosition, endPosition);

            perpendicularDirection *= -1;
        }
    }

    private bool testDeviation(BuildingBoundingBox newBuilding, float deviationToTest, Vector3 directionToMove, Vector3 initialPosition, Vector3 lastAvailablePosition)
    {
        newBuilding.transform.position += directionToMove * deviationToTest;
        if (reachedLastAvailablePosition(newBuilding.transform.position, directionToMove, initialPosition, lastAvailablePosition))
        {
            return false;
        }
        newBuilding.transform.position = TerrainShape.instance.getSurfacePointAtPosition(newBuilding.transform.position);

        var collisions = newBuilding.getCollisions();
        if (collisions.Count == 0)
        {
            if (deviationToTest <= finalDeviation)
            {
                return true;
            }
            else
            {
                newBuilding.transform.position -= directionToMove * deviationToTest;
                float newDeviation = deviationToTest * deviationShrinkRate;
                if (newDeviation < finalDeviation) newDeviation = finalDeviation;
                return testDeviation(newBuilding, newDeviation, directionToMove, initialPosition, lastAvailablePosition);
            }
        }
        else
        {
            return testDeviation(newBuilding, deviationToTest, directionToMove, initialPosition, lastAvailablePosition);
        }
    }
}
