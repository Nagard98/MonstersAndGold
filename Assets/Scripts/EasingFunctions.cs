using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasingFunctions
{

    public static float Flip(float x)
    {
        return 1 - x;
    }

    public static float EaseIn(float t)
    {
        return t * t;
    }

    public static float EaseOut(float t)
    {
        return Flip(Mathf.Pow(Flip(t), 3f));
    }

    public static float EaseInOut(float t)
    {
        return Mathf.Lerp(EaseIn(t), EaseOut(t), t);
    }

    public static float Spike(float t)
    {
        if (t <= .5f)
            return EaseIn(t / .5f);

        return EaseIn(Flip(t) / .5f);
    }
    public static float EaseInOutQuad(float t)
    {
        return t < 0.5 ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
    }

    public static float Custom(float t)
    {
        if (t <= .5f)
        {
            return EaseInOutQuad(t * 2f);
        }
        return Flip(EaseInOutQuad((t - 0.5f) * 2f));
    }

}
