using System.Collections.Generic;
using UnityEngine;

public class BuildingBoundingBox : MonoBehaviour
{
    [SerializeField] GameObject colliderHolder;

    List<Collider> colliderColliders;

    void Awake()
    {
        colliderColliders = new List<Collider>();

        var auxColliders = colliderHolder.GetComponents<Collider>();
        
        foreach (var collider in auxColliders)
        {
            if (collider.gameObject.layer == 12) colliderColliders.Add(collider); 
        }
    }

    public HashSet<BuildingBoundingBox> getCollisions()
    {
        HashSet<BuildingBoundingBox> buildingSet = new HashSet<BuildingBoundingBox>();
        List<Collider> collisions = new List<Collider>();

        foreach (var collider in colliderColliders)
        {
            if (collider.GetType() == typeof(BoxCollider))
            {
                BoxCollider col = (BoxCollider)collider;
                collisions.AddRange(Physics.OverlapBox(
                    col.center + colliderHolder.transform.position, 
                    new Vector3(col.size.x * colliderHolder.transform.localScale.x, col.size.y * colliderHolder.transform.localScale.y, col.size.z * colliderHolder.transform.localScale.z) / 2, 
                    col.transform.rotation, 
                    LayerMask.GetMask("Collider")
                ));
            }
            else if (collider.GetType() == typeof(SphereCollider))
            {
                SphereCollider col = (SphereCollider)collider;
                collisions.AddRange(Physics.OverlapSphere(
                    col.center + colliderHolder.transform.position,
                    col.radius,
                    LayerMask.GetMask("Collider")
                ));
            }
            else if (collider.GetType() == typeof(CapsuleCollider))
            {
                CapsuleCollider col = (CapsuleCollider)collider;

                float distanceFromCentre = (col.height / 2) - col.radius;
                Vector3 direction = new Vector3(col.direction == 0 ? distanceFromCentre : 0, col.direction == 1 ? distanceFromCentre : 0, col.direction == 2 ? distanceFromCentre : 0);
                if (col.radius * 2 > col.height) direction = Vector3.zero;

                collisions.AddRange(Physics.OverlapCapsule(
                    col.center + colliderHolder.transform.position + direction,
                    col.center + colliderHolder.transform.position - direction,
                    col.radius,
                    LayerMask.GetMask("Collider")
                ));
            }
        }

        for (int i = collisions.Count-1; i >= 0; i--)
        {
            if (collisions[i].transform.gameObject.Equals(colliderHolder)) 
                collisions.RemoveAt(i); 
            else
            {
                buildingSet.Add(collisions[i].transform.GetComponentInParent<BuildingBoundingBox>());
            }
        }

        return buildingSet;
    }


}
