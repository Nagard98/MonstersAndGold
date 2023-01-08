/*using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class PathSegment : MonoBehaviour
{
    private Mesh _pathMesh;
    private Vector3[] _vertices;
    private int[] _triangles;

    public Vector3[] _poss = new Vector3[128];
    public Texture2D texture;

    // Start is called before the first frame update
    void Start()
    {
        texture = new Texture2D(128, 128, TextureFormat.RGB24, false);
        Color[] pixels = new Color[128*128];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.black;
        }
        for(int i=0; i< 128; i++)
        {
            float z = MathUtils.Remap(Mathf.PerlinNoise(i*0.01f, 0f), 0f, 1f, -50f, +50f);
            pixels[(i * 128) + 64 + (int)z] = Color.white;
            pixels[(i * 128) + 64 + (int)z + 1] = Color.white;
            pixels[(i * 128) + 64 + (int)z + 2] = Color.white;
            pixels[(i * 128) + 64 + (int)z + 3] = Color.white;
            pixels[(i * 128) + 64 + (int)z - 1] = Color.white;
            pixels[(i * 128) + 64 + (int)z - 2] = Color.white;
            pixels[(i * 128) + 64 + (int)z - 3] = Color.white;

        }
        texture.SetPixels(pixels);
        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/Saved/";
        if (!Directory.Exists(dirPath))
        { 
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "Image" + ".png", bytes);

        _pathMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _pathMesh;
        float step = 10f;
        Vector3[] waypoints = new Vector3[10];

        for(int i=0; i < 10; i++)
        {
            float horOffset = Mathf.PerlinNoise(i*.1f, .3f);
            waypoints[i] = new Vector3(x: i*step, y:0, z: MathUtils.Remap(horOffset, 0f, 1f, -10f, 10f));
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.position = waypoints[i];
        }
        QuadraticBezierSpline bezCurve = new QuadraticBezierSpline(waypoints, nSamples:5);
        int numPoints = bezCurve.ControlPolyLength;

        bezCurve.Reset();
        float totalDist = bezCurve.TotalDist;
        for (int i=0; i < 128; i++)
        {
            _poss[i] = bezCurve.MoveAlong(dist: i * (totalDist / 128f));
        }

        List<Vector3> _tmp = new List<Vector3>();
        for(int i=0,v=0; i+2 < numPoints -2; i+=2)
        { 
            for(int j=1; j<=5; j++)
            {
                Vector3 velocity = bezCurve.GetFirstDerivative(0.2f * j, firstPoint: i);
                Vector3 position = bezCurve.GetPosition(t: 0.2f * j, arcIndex: i);
                Vector3 lp = (Vector3.Cross(velocity, Vector3.up).normalized) * 2f;
                Vector3 rp = -lp;
                _tmp.Add(position + rp);
                _tmp.Add(position + lp);
                GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = (position + rp);
                GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = (position + lp);
                //_vertices[v] = rp;
                //_vertices[v + 1] = lp;
                v += 2;
            }

        }

        _vertices = _tmp.ToArray();
        _triangles = new int[(_vertices.Length-2)*3];

        for (int i = 0, t = 0; i + 2 < _vertices.Length; i += 2) 
        {
            _triangles[t] = i;
            _triangles[t + 1] = i + 1;
            _triangles[t + 2] = i + 3;

            _triangles[t + 3] = i;
            _triangles[t + 4] = i + 3;
            _triangles[t + 5] = i + 2;

            t += 6;
        }

        _pathMesh.Clear();

        _pathMesh.vertices = _vertices;
        _pathMesh.triangles = _triangles;
        _pathMesh.RecalculateNormals();

    }

}
*/