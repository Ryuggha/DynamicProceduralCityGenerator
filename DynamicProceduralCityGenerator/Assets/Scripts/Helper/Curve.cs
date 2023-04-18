using UnityEngine;

public class Curve
{
    Vector3 p0;
    Vector3 p1;
    Vector3 pf;

    public Curve(Vector3 p0, Vector3 p1, Vector3 pf)
    {
        this.p0 = p0;
        this.p1 = p1;
        this.pf = pf;
    }

    public Vector3 getPoint(float t)
    {
        if (t < 0 || t > 1) throw new System.Exception("The Value of t must be between 0 and 1");

        return Mathf.Pow(1 - t, 2) * p0  +  2 * (1 - t) * p1 + t*t * pf;
    }
}
