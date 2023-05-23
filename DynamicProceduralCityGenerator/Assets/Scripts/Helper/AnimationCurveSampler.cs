using UnityEngine;

public class AnimationCurveSampler
{
    private readonly AnimationCurve densityCurve;
    private readonly IntegrateFunction integratedDensity;

    public AnimationCurveSampler(AnimationCurve curve, int integrationSteps = 100)
    {
        densityCurve = curve;
        integratedDensity = new IntegrateFunction(curve.Evaluate, curve.keys[0].time, curve.keys[curve.length - 1].time, integrationSteps);
    }

    private float Invert(float s)
    {
        s *= integratedDensity.Total;
        float lower = MinT;
        float upper = MaxT;
        const float precision = 0.00001f;
        while (upper - lower > precision)
        {
            float mid = (lower + upper) / 2f;
            float d = integratedDensity.evaluate(mid);
            if (d > s) upper = mid;
            else if (d < s) lower = mid;
            else return mid;
        }

        return (lower + upper) / 2f;
    }

    public float TransformUnit(float unitValue)
    {
        return Invert(unitValue);
    }

    public float Sample()
    {
        return Invert(Random.value);
    }

    public float random(float min, float max)
    {
        return Sample() * (max - min) + min;
    }

    public int random(int min, int max) //Max exclusive
    {
        return Mathf.FloorToInt(Sample() * (max - min)) + min;
    }

    private float MinT
    {
        get { return densityCurve.keys[0].time; }
    }

    private float MaxT
    {
        get { return densityCurve.keys[densityCurve.length - 1].time; }
    }
}
