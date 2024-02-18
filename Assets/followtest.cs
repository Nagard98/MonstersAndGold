using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followtest : MonoBehaviour
{
    public Vector3Variable playerPathPosition;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = playerPathPosition.Value;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = playerPathPosition.Value;
    }
}
