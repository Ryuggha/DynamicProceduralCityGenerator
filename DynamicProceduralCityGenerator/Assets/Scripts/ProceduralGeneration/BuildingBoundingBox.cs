using System.Collections.Generic;
using UnityEngine;

public class BuildingBoundingBox : MonoBehaviour
{
    [SerializeField] GameObject colliderHolder;
    [SerializeField] List<Transform> entranceGameObjectList;
    [SerializeField] GameObject floorHolder;
    [SerializeField] GameObject model;
    GameObject elementsObject;

    List<Collider> colliderColliders;

    void Awake()
    {
        elementsObject = transform.GetChild(0).gameObject;

        colliderColliders = new List<Collider>();

        var auxColliders = colliderHolder.GetComponents<Collider>();
        
        foreach (var collider in auxColliders)
        {
            if (collider.gameObject.layer == 12) colliderColliders.Add(collider); 
        }
    }

    private void Update()
    {
        if ((PlayerInteraction.instance.getPlayerPosition() - model.transform.position).magnitude < Camera.main.farClipPlane)
        {
            model.SetActive(true);
        }
        else
        {
            model.SetActive(false);
        }
    }

    public float getColliderSize()
    {
        float r = 0;
        foreach (var collider in colliderColliders)
        {
            r += collider.bounds.size.magnitude;
        }
        return r;
    }

    public void getPositionZeroOfObject (Vector3 vector)
    {
        elementsObject.transform.localPosition -= vector;
    }

    public List<Transform> getEntranceList() { return entranceGameObjectList; }

    public List<BoxCollider> getFloorColliders()
    {
        List<BoxCollider> boxColliders = new List<BoxCollider>();

        var colliders = floorHolder.GetComponentsInChildren<Collider>();

        foreach (var collider in colliders)
        {
            if (collider.GetType() == typeof(BoxCollider))
            {
                BoxCollider col = (BoxCollider)collider;
                boxColliders.Add(col);
            }
            else
            {
                Debug.LogError($"Collider of type {collider.GetType()} not supported");
            }
        }

        return boxColliders;
    }

    public HashSet<GameObject> getCollisions()
    {
        HashSet<GameObject> collisions = new HashSet<GameObject>();
        Collider[] list = new Collider[0];

        Physics.SyncTransforms();

        foreach (var collider in colliderColliders)
        {
            if (collider.GetType() == typeof(BoxCollider))
            {
                BoxCollider col = (BoxCollider)collider;
                list = Physics.OverlapBox(
                    colliderHolder.transform.rotation * col.center + colliderHolder.transform.position,
                    new Vector3(col.size.x * colliderHolder.transform.localScale.x, col.size.y * colliderHolder.transform.localScale.y, col.size.z * colliderHolder.transform.localScale.z) / 2,
                    col.transform.rotation,
                    BuildingGeneration.instance.buildingGenerationLayerMask
                );
            }
            else if (collider.GetType() == typeof(SphereCollider))
            {
                SphereCollider col = (SphereCollider)collider;
                list = Physics.OverlapSphere(
                    col.center + colliderHolder.transform.position,
                    col.radius,
                    BuildingGeneration.instance.buildingGenerationLayerMask
                );
            }
            else if (collider.GetType() == typeof(CapsuleCollider))
            {
                CapsuleCollider col = (CapsuleCollider)collider;

                float distanceFromCentre = (col.height / 2) - col.radius;
                Vector3 direction = new Vector3(col.direction == 0 ? distanceFromCentre : 0, col.direction == 1 ? distanceFromCentre : 0, col.direction == 2 ? distanceFromCentre : 0);
                if (col.radius * 2 > col.height) direction = Vector3.zero;
                list = Physics.OverlapCapsule(
                    col.center + colliderHolder.transform.position + direction,
                    col.center + colliderHolder.transform.position - direction,
                    col.radius,
                    BuildingGeneration.instance.buildingGenerationLayerMask
                );
            }

            foreach (var c in list)
            {
                if (!c.transform.gameObject.Equals(colliderHolder)) collisions.Add(c.gameObject);
            }
        }
        return collisions;
    }


}
