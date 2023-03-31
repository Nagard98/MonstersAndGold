using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KATVR;

public class Recalibrate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void RecalCamera(Transform t)
    {
        KATDevice_Walk.Instance.ResetCamera(t);
    }
}
