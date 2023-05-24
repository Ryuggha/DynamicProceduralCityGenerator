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

    private bool generateBuilding(Vector3 InitialPosition, Vector3 directionToMove, Vector3 directionToFace)
    {
        BuildingBoundingBox newBuilding = Instantiate(buildingList[Random.Range(0, buildingList.Count)], InitialPosition, Quaternion.LookRotation(directionToFace, Vector3.up)).GetComponent<BuildingBoundingBox>();

        newBuilding.getPositionZeroOfObject(newBuilding.getEntranceList()[Random.Range(0, newBuilding.getEntranceList().Count)].localPosition);
        newBuilding.transform.eulerAngles += new Vector3(0, Random.Range(-maxRotation, maxRotation), 0);
        newBuilding.transform.position = TerrainShape.instance.getSurfacePointAtPosition(newBuilding.transform.position);
        if (newBuilding.getCollisions().Count != 0)
        {
            if (!testDeviation(newBuilding, initialDeviation, directionToMove))
            {
                Destroy(newBuilding.gameObject);
                return false;
            }
        }
        lastBuildingBuilt = newBuilding;
        return true;
    }

    public void generateBuildingLayer(Vector3 directionToFace, Vector3 initialPosition, Vector3 lastAvailablePosition)
    {
        Vector3 directionToMove = (lastAvailablePosition - initialPosition).normalized;
        Vector3 nextPosition = initialPosition;
        bool noCollisions;

        do
        {
            noCollisions = generateBuilding(nextPosition, directionToMove, directionToFace);
            if (noCollisions)
            {
                nextPosition = lastBuildingBuilt.transform.position;
                noCollisions = !reachedLatAvailablePosition(directionToMove, initialPosition, lastAvailablePosition);
                if (noCollisions)
                {
                    foreach (var collider in lastBuildingBuilt.getFloorColliders())
                    {
                        TerrainShape.instance.generateBuildingFundations(collider, lastBuildingBuilt.transform.position);
                    }
                    
                }
            }
        } while (noCollisions);
    }

    public static Vector3 getPositionWithY0(Vector3 vector) { return new Vector3(vector.x, 0, vector.z); }

    private bool reachedLatAvailablePosition(Vector3 directionToMove, Vector3 initialPosition, Vector3 lastAvailablePosition)
    {
        if (Vector3.Dot(getPositionWithY0(directionToMove), (getPositionWithY0(lastAvailablePosition) - getPositionWithY0(lastBuildingBuilt.transform.position)).normalized) < 0f && 
            Vector3.Dot(getPositionWithY0(-directionToMove), (getPositionWithY0(lastAvailablePosition) - getPositionWithY0(initialPosition)).normalized) < 0f)
        {
            Destroy(lastBuildingBuilt.gameObject);
            lastBuildingBuilt = null;
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
        Vector3 initialPosition = road.getPositionStart() + (perpendicularDirection * road.getWidth() / 2);
        Vector3 endPosition = road.getPositionEnd() + (perpendicularDirection * road.getWidth() / 2);

        RaycastHit hit;
        if (Physics.CapsuleCast(initialPosition + perpendicularDirection * 2 - Vector3.up * 30, initialPosition + perpendicularDirection * 2 + Vector3.up * 30, 1, roadDirection, out hit, (road.getPositionEnd() - road.getPositionStart()).magnitude / 2, buildingGenerationLayerMask))
        {
            if (hit.transform.gameObject.layer == 12) initialPosition = getPositionWithY0(hit.transform.position);
            else initialPosition = getPositionWithY0(hit.point);
        }
        else if (Physics.CapsuleCast(endPosition + perpendicularDirection * 2 - Vector3.up * 30, endPosition + perpendicularDirection * 2 + Vector3.up * 30, 1, roadDirection * -1, out hit, (road.getPositionEnd() - road.getPositionStart()).magnitude / 2, buildingGenerationLayerMask))
        {
            endPosition = initialPosition;
            if (hit.transform.gameObject.layer == 12) initialPosition = getPositionWithY0(hit.transform.position);
            else initialPosition = getPositionWithY0(hit.point);
        }
        Debug.DrawLine(initialPosition, endPosition, Color.red, 160);
        generateBuildingLayer(-perpendicularDirection, initialPosition, endPosition);
        
        perpendicularDirection *= -1;

        initialPosition = road.getPositionStart() + (perpendicularDirection * road.getWidth() / 2);
        endPosition = road.getPositionEnd() + (perpendicularDirection * road.getWidth() / 2);
        if (Physics.CapsuleCast(initialPosition + perpendicularDirection * 2 - Vector3.up * 30, initialPosition + perpendicularDirection * 2 + Vector3.up * 30, 1, roadDirection, out hit, (road.getPositionEnd() - road.getPositionStart()).magnitude / 2, buildingGenerationLayerMask))
        {
            if (hit.transform.gameObject.layer == 12) initialPosition = getPositionWithY0(hit.transform.position);
            else initialPosition = getPositionWithY0(hit.point);
        }
        else if (Physics.CapsuleCast(endPosition + perpendicularDirection * 2 - Vector3.up * 30, endPosition + perpendicularDirection * 2 + Vector3.up * 30, 1, roadDirection * -1, out hit, (road.getPositionEnd() - road.getPositionStart()).magnitude / 2, buildingGenerationLayerMask))
        {
            endPosition = initialPosition;
            if (hit.transform.gameObject.layer == 12) initialPosition = getPositionWithY0(hit.transform.position);
            else initialPosition = getPositionWithY0(hit.point);
        }
        Debug.DrawLine(initialPosition, endPosition, Color.red, 160);
        generateBuildingLayer(-perpendicularDirection, initialPosition, endPosition);
        
    }

    private bool testDeviation(BuildingBoundingBox newBuilding, float deviationToTest, Vector3 directionToMove)
    {
        newBuilding.transform.position += directionToMove * deviationToTest;
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
                return testDeviation(newBuilding, newDeviation, directionToMove);
            }
        }
        else
        {
            if (!collisions.Contains(lastBuildingBuilt) && false) //Deactivated with False
            {
                Debug.Log("Building could not be created becouse other buildings are on the way.");
                return false;
            }
            else {
                return testDeviation(newBuilding, deviationToTest, directionToMove);
            }
        }
    }
}
