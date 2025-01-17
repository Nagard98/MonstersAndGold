using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Implementation of a Bezier Spline composed of Quadratic Bezier Curves
//Used to build the path on the chunks of terrain and as a path following mechanism for the player
//I used the word curve and arc interchangeably
public class BezierSpline : ICloneable
{
    public Vector3[] _controlPolygon;
    private int _nSamples;
    public List<float[]> _pathLUTs;

    private float _currentArcDist;
    private int _currentArcIndex;
    public float _totalDist;

    public float _buildArcDist;
    public int _buildArcIndex;


    public BezierSpline(Vector3[] controlPoints, int nSamples = 5)
    {
         _controlPolygon = BuildControlPolygon(controlPoints);
         _nSamples = nSamples;
         _totalDist = 0f;

         _pathLUTs = BuildPathLUTs(_controlPolygon, 0, ref _totalDist);

         _currentArcIndex = 0;
         _currentArcDist = 0;

        _buildArcDist = 0f;
        _buildArcIndex = 0;
    }

    //Builds the Lookup Tables
    private List<float[]> BuildPathLUTs(Vector3[] controlPoly, int firstChunkPointIndex,ref float totalDist)
    {
        List<float[]> tmp = new List<float[]>();
        for (int i = 0; i + 3 <= controlPoly.Length; i += 3)
        {
            tmp.Add(BuildArcCumDistLUT(curveStartIndex: firstChunkPointIndex + i, nSamples: _nSamples));
            totalDist += tmp[tmp.Count - 1][_nSamples];
        }
        return tmp;
    }

    //Extends the original bezier spline with new controlpoints
    public void AddPathWaypoints(Vector3[] controlPoints, int chunkIndex, int numCurvesChunk)
    {
        Vector3[] newChunkControlPoly = BuildControlPolygon(controlPoints, start: false);

        _controlPolygon = _controlPolygon.Concat(newChunkControlPoly).ToArray();
        List<float[]> newChunkPathLUTs = BuildPathLUTs(newChunkControlPoly, chunkIndex * numCurvesChunk * 3, ref _totalDist);
        _pathLUTs = _pathLUTs.Concat(newChunkPathLUTs).ToList();

    }

    private float[] BuildArcCumDistLUT(int curveStartIndex, int nSamples = 5)
    {
        float[] arcLengths = new float[nSamples + 1];
        float tStep = (1f / (float)nSamples);
        float _tmp = 0f;

        arcLengths[0] = 0f;
        for (int j = 1; j <= nSamples; j++)
        {
            _tmp += Vector3.Distance(GetPosition(t: tStep * (j), firstPoint: curveStartIndex), GetPosition(t: tStep * (j - 1), firstPoint: curveStartIndex));
            arcLengths[j] = _tmp;
        }

        return arcLengths;
    }


    public int ControlPolyLength
    {
        get { return _controlPolygon.Length; }
    }

    public float TotalDist
    {
        get { return _totalDist; }  
        set { _totalDist = value; }  
    }


    public float GetPathChunkDist(int chunkIndex, int numCurvesChunk)
    {
        float tmpDist = 0f;
        for(int i = 0; i < numCurvesChunk; i++)
        {
            tmpDist += _pathLUTs[(chunkIndex * numCurvesChunk) + i][_nSamples];
        }
        return tmpDist;
    }

    //-------------------------------------------------------------------------------------------
    //Gets the position on the path at a certain distance from the current position
    public Vector3 MoveAlong(float dist)
    {
        float t = UpdateT(dist);

        return _controlPolygon[_currentArcIndex * 3] * Mathf.Pow((1 - t), 2) +
               _controlPolygon[(_currentArcIndex * 3) + 1] * 2 * t * (1 - t) +
               _controlPolygon[(_currentArcIndex * 3) + 2] * Mathf.Pow(t, 2);
    }

    public Vector3 BuildAlong(float dist)
    {

        float t = UpdateBuildT(dist);

        return _controlPolygon[_buildArcIndex * 3] * Mathf.Pow((1 - t), 2) +
               _controlPolygon[(_buildArcIndex * 3) + 1] * 2 * t * (1 - t) +
               _controlPolygon[(_buildArcIndex * 3) + 2] * Mathf.Pow(t, 2);
    }

    public Vector3 GetFirstDerivative()
    {
        float t = ConvDistToT(_currentArcDist, _currentArcIndex);

        return _controlPolygon[_currentArcIndex * 3] * (t - 1f) +
               _controlPolygon[(_currentArcIndex * 3) + 1] * (1f - (2f * t)) +
               _controlPolygon[(_currentArcIndex * 3) + 2] * t;
    }

    //-------------------------------------------------------------------------------------------

    public Vector3 GetPosition(float t, int firstPoint)
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

    //-------------------------------------------------------------------------------------------

    private float UpdateT(float dist)
    {
        float arcLength = GetArcLength(_currentArcIndex);

        if (_currentArcDist >= arcLength && (_currentArcIndex * 3) + 3 < ControlPolyLength)
        {
            _currentArcDist = 0f;
            _currentArcIndex += 1;
        }

        _currentArcDist = Mathf.Clamp(_currentArcDist + dist, 0, arcLength);

        return ConvDistToT(_currentArcDist, _currentArcIndex);
    }

    private float UpdateBuildT(float dist)
    {
        float arcLength = GetArcLength(_buildArcIndex);

        if (_buildArcDist >= arcLength && (_buildArcIndex * 3) + 3 < ControlPolyLength)
        {
            _buildArcDist = 0f;
            _buildArcIndex += 1;
        }

        _buildArcDist = Mathf.Clamp(_buildArcDist + dist, 0, arcLength);

        return ConvDistToT(_buildArcDist, _buildArcIndex);
    }


    public void Reset()
    {
        _currentArcDist = 0f;
        _currentArcIndex = 0;
    }

    public Vector3[] BuildControlPolygon(Vector3[] controlPoints, bool start = true)
    {
        if (!start)
        {
            controlPoints[0] = _controlPolygon[_controlPolygon.Length - 1]; 
            controlPoints[1] = 2 * _controlPolygon[_controlPolygon.Length - 1] - _controlPolygon[_controlPolygon.Length - 2];
        }
        for (int i =  3; i < controlPoints.Length; i += 3)
        {
            controlPoints[i] = controlPoints[i - 1];
            controlPoints[i + 1] = 2 * controlPoints[i - 1] - controlPoints[i - 2];
        }

        return controlPoints;
    }


    public float ConvDistToT(float dist, int curveIndex)
    {
        float[] arcLUT = _pathLUTs[curveIndex];
        float arcLength = arcLUT[_nSamples];

        for (int i = 0; i < _nSamples; i++)
        {
            if (dist >= arcLUT[i] && dist < arcLUT[i + 1])
            {
                return MathUtils.Remap(dist, arcLUT[i], arcLUT[i + 1], i / ((float)_nSamples), (i + 1f) / ((float)_nSamples));
            }
        }

        return dist / arcLength;
    }

    public float GetArcLength(int curveIndex)
    {
        return _pathLUTs[curveIndex][_pathLUTs[curveIndex].Length - 1];
    }

    //Allows moving along the spline by distances bigger than the lengths of the single curves
    public Vector3 MoveLongDistance(float distance, out Vector3 orthoVector)
    {
        float leftoverArcDist = 0;
        float leftoverTotalDist = distance;
        Vector3 destination = Vector3.zero;
        bool reached = false;

        while(!reached)
        {
            leftoverArcDist = GetArcLength(_currentArcIndex) - _currentArcDist;

            if(leftoverArcDist > leftoverTotalDist)
            {
                destination = MoveAlong(leftoverTotalDist);
                reached = true;
            }
            else
            {
                destination = MoveAlong(leftoverArcDist);
            }

            leftoverTotalDist -= leftoverArcDist;

        }
        //TO-DO: cambia 10 con valore giusto
        //chunkIndex = (_currentArcIndex / 10);
        orthoVector = Vector3.Cross(GetFirstDerivative(), Vector3.up).normalized;

        return destination;
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
