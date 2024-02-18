using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterXR : MonoBehaviour
{
    public Transform KatDevice;
    public Vector3Variable playerPosition;

    // Start is called before the first frame update
    void Start()
    {
        playerPosition.Value = KatDevice.position;
    }

    // Update is called once per frame
    void Update()
    {
        playerPosition.Value = KatDevice.position;
    }
}
