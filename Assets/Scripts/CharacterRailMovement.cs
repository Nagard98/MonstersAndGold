using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRailMovement : MonoBehaviour
{

    public float charSpeed = 4f;
    public Transform path;
    public float totalDistance;

    private Vector3[] _waypoints;
    private QuadraticBezier _quadBezierCurve;


    // Start is called before the first frame update
    void Start()
    {
        totalDistance = 0;
        _waypoints = GetWaypoints(path);       

        transform.position = _waypoints[0];
        
        _quadBezierCurve = new QuadraticBezier(_waypoints, nSamples:10);
    }

    // Update is called once per frame
    void Update()
    {
        totalDistance += charSpeed + Time.deltaTime;
        transform.position = _quadBezierCurve.MoveAlong(charSpeed * Time.deltaTime);
        transform.forward = _quadBezierCurve.GetFirstDerivative();

        //TO-DO: add updateT to derivative function
        //transform.forward = _quadBezierCurve.GetFirstDerivative(_t, firstPoint: _pathwayIndex).normalized;
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

    void OnDrawGizmos()
    {
        // Draws a 5 unit long red line in front of the object
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward);
    }
}
