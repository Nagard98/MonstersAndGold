using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRailMovement : MonoBehaviour
{

    public float charSpeed = 4f;
    public Transform path;
    public float totalDistance;

    private float _t;
    private float _dist;
    private Vector3[] _waypoints;
    private Vector3[] _controlPolygon;
    private QuadraticBezier _quadBezierCurve;
    private int _curveIndex = 0;
    private int _pathwayIndex;


    // Start is called before the first frame update
    void Start()
    {
        totalDistance = 0;
        _waypoints = GetWaypoints(path);       

        transform.position = _waypoints[0];

        _controlPolygon = QuadraticBezier.BuildControlPolygon(_waypoints);
        _quadBezierCurve = new QuadraticBezier(_controlPolygon, nSamples:10);
    }

    // Update is called once per frame
    void Update()
    {
        totalDistance += _dist;
        float arcLength = _quadBezierCurve.GetArcLength(_curveIndex);
        
        if (_dist >= arcLength && _pathwayIndex + 2 < _controlPolygon.Length - 2)
        {
            _dist = 0f;
            _pathwayIndex += 2;
            _curveIndex += 1;
        }

        _dist = Mathf.Clamp(_dist + (charSpeed * Time.deltaTime), 0, arcLength);

        _t = _quadBezierCurve.ConvDistToT(_dist, _curveIndex);
        
        transform.position = _quadBezierCurve.GetPosition(_t, firstPoint: _pathwayIndex);
        transform.forward = _quadBezierCurve.GetFirstDerivative(_t, firstPoint: _pathwayIndex).normalized;
    }

    private Vector3[] GetWaypoints(Transform path)
    {
        int numWaypoints = path.childCount;
        Vector3[] _tmp = new Vector3[numWaypoints];
        for (int i = 0; i < numWaypoints; i++)
        {
            _tmp[i] = path.GetChild(i).position;
        }

        return _tmp;
    }
}
