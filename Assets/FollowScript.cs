using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assistent : MonoBehaviour
{
    public Transform playerBodyTransform;
    //public Vector3Variable playerBodyPosition;

    // Start is called before the first frame update
    void Start()
    {
        transform.localRotation = playerBodyTransform.localRotation;
        transform.position = playerBodyTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = playerBodyTransform.localRotation;
        transform.position = playerBodyTransform.position;
    }
}
