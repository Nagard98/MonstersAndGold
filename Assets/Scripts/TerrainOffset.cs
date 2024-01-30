using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainOffset : MonoBehaviour
{

    [SerializeField]
    public GameObject KATObject;

    private float initHeight;

    // Start is called before the first frame update
    void Start()
    {
        initHeight = KATObject.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.SetLocalPositionAndRotation(new Vector3(0.0f, KATObject.transform.position.y/* - initHeight*/, 0.0f), Quaternion.identity);
    }
}
