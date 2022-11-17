using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadraticBezier
{
    private Vector3[] _controlPolygon;
    private int _nSamples;
    private List<float[]> _pathLUTs;

    public QuadraticBezier(Vector3[] controlPolygon, int nSamples = 5)
    {
        _controlPolygon = controlPolygon;
        _nSamples = nSamples;

        _pathLUTs = new List<float[]>();
        for (int i = 0; i + 2 < _controlPolygon.Length; i += 2)
        {
            _pathLUTs.Add(BuildCumDistLUT(startIndex: i, nSamples: nSamples));
        }
    }

    public Vector3 GetPosition(float t, int firstPoint = 0)
    {
        return _controlPolygon[firstPoint] * Mathf.Pow((1 - t), 2) +
               _controlPolygon[firstPoint + 1] * 2 * t * (1 - t) +
               _controlPolygon[firstPoint + 2] * Mathf.Pow(t, 2);
    }

    public Vector3 GetFirstDerivative(float t, int firstPoint = 0)
    {
        return _controlPolygon[firstPoint] * (t - 1f) +
               _controlPolygon[firstPoint + 1] * (1f - (2f * t)) +
               _controlPolygon[firstPoint + 2] * t;
    }

    public static Vector3[] BuildControlPolygon(Vector3[] arr)
    {
        List<Vector3> _tmp = new List<Vector3>();
        _tmp.Add(arr[0]);

        for (int i = 1; i < arr.Length; i++)
        {
            _tmp.Add(arr[i]);
            if ((i + 1) < arr.Length)
            {
                _tmp.Add(Vector3.Lerp(arr[i], arr[i + 1], 0.5f));
            }
        }
        _tmp.Add(arr[arr.Length - 1]);

        return _tmp.ToArray();
    }

    private float[] BuildCumDistLUT(int startIndex, int nSamples = 5)
    {
        float[] arcLength = new float[nSamples + 1];
        float tStep = (1f / (float)nSamples);
        float _tmp = 0f;

        arcLength[0] = 0f;
        for (int j = 1; j <= nSamples; j++)
        {
            _tmp += Vector3.Distance(GetPosition(tStep * (j), firstPoint: startIndex), GetPosition(tStep * (j - 1), firstPoint: startIndex));
            arcLength[j] = _tmp;
        }

        return arcLength;
    }

    public float ConvDistToT(float dist, int curveIndex)
    {
        float[] arcLUT = _pathLUTs[curveIndex];
        float arcLength = arcLUT[arcLUT.Length - 1];
        int nSamples = arcLUT.Length;

        for (int i = 0; i < nSamples - 1; i++)
        {
            if (dist >= arcLUT[i] && dist < arcLUT[i + 1])
            {
                return Remap(dist, arcLUT[i], arcLUT[i + 1], i / ((float)nSamples - 1f), (i + 1f) / ((float)nSamples - 1f));
            }
        }
        return dist / arcLength;
    }

    public float GetArcLength(int curveIndex)
    {
        return _pathLUTs[curveIndex][_pathLUTs[curveIndex].Length - 1];
    }

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return from2 + (value - from1) * (to2 - from2) / (to1 - from1);
    }
}
