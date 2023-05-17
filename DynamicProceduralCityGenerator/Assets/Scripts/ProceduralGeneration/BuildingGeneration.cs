using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGeneration : MonoBehaviour
{
    [SerializeField] List<GameObject> buildingList;

    [Header("Parameters")]
    [SerializeField] float maxRotation = 5;
    [SerializeField] float initialDeviation = 2;
    [SerializeField] float finalDeviation = .1f;
    [SerializeField] float deviationShrinkRate = .5f;

    BuildingBoundingBox lastTestBuilding;

    void Start()
    {
        lastTestBuilding = Instantiate(buildingList[Random.Range(0, buildingList.Count)], Vector3.zero, Quaternion.identity).GetComponent<BuildingBoundingBox>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) generateBuilding(lastTestBuilding, Vector3.right, Vector3.forward);
    }

    private void generateBuilding(BuildingBoundingBox neighbor, Vector3 directionToMove, Vector3 directionToFace)
    {
        BuildingBoundingBox newBuilding = Instantiate(buildingList[Random.Range(0, buildingList.Count)], neighbor.transform.position, Quaternion.LookRotation(directionToFace, Vector3.up)).GetComponent<BuildingBoundingBox>();
        
        newBuilding.transform.eulerAngles += new Vector3(0, Random.Range(-maxRotation, maxRotation), 0);

        if (newBuilding.getCollisions().Count != 0) testDeviation(newBuilding, initialDeviation, directionToMove);

    }

    private bool testDeviation(BuildingBoundingBox newBuilding, float deviationToTest, Vector3 directionToMove)
    {
        newBuilding.transform.position += directionToMove * deviationToTest;
        
        var collisions = newBuilding.getCollisions();
        if (collisions.Count == 0)
        {
            if (deviationToTest <= finalDeviation)
            {
                lastTestBuilding = newBuilding;
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
            if (!collisions.Contains(lastTestBuilding))
            {
                Debug.Log("Building could not be created becouse other buildings are on the way.");
                Destroy(newBuilding.gameObject);
                return false;
            }
            else {
                return testDeviation(newBuilding, deviationToTest, directionToMove);
            }
        }
    }
}
