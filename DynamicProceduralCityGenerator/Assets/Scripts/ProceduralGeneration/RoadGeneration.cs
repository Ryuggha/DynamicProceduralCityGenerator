using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGeneration : MonoBehaviour
{
    private List<Curve> curves;

    private void Start()
    {
        curves = new List<Curve>();

        curves.Add(new Curve(
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 10),
            new Vector3(10, 0, 0)
        ));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        
        for (float i = 0.05f; i <= 1; i+=.05f)
        {
            if (curves != null && curves[0] != null) Gizmos.DrawLine(curves[0].getPoint(i-0.05f), curves[0].getPoint(i));
        }
    }

}
