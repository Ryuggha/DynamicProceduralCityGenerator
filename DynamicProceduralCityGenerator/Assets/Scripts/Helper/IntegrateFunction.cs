using System;
using UnityEngine;

public class IntegrateFunction
{
    private Func<float, float> function;
    private float[] values;
    private float from, to;

    public IntegrateFunction(Func<float, float> function, float from, float to, int steps)
    {
        this.values = new float[steps + 1];
        this.function = function;
        this.from = from;
        this.to = to;
        computeValues();
    }

    private void computeValues()
    {
        float segment = (to - from) / (values.Length - 1);
        float lastY = function(from);
        float sum = 0;
        values[0] = 0;
        for (int i = 1; i < values.Length; i++)
        {
            float x = from + i * segment;
            float nextY = function(x);
            sum += segment * (nextY + lastY) / 2;
            lastY = nextY;
            values[i] = sum;
        }
    }

    public float evaluate(float x)
    {
        Debug.Assert(from <= x && x <= to);
        float t = Mathf.InverseLerp(from, to, x);
        int lower = (int)(t * values.Length);
        int upper = (int)(t * values.Length + .5f);
        if (lower == upper || upper >= values.Length) return values[lower];
        float innerT = Mathf.InverseLerp(lower, upper, t * values.Length);
        return (1 - innerT) * values[lower] + innerT * values[upper];
    }

    public float Total
    {
        get
        {
            return values[values.Length - 1];
        }
    }
}
