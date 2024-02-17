using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assistent : MonoBehaviour
{
    public Transform playerBodyTransform;
    public float rotSpeedMultiplier;

    //public Vector3Variable playerBodyPosition;
    private Quaternion initRotation;

    // Start is called before the first frame update
    void Start()
    {
        transform.localRotation = initRotation = playerBodyTransform.localRotation;
        transform.position = playerBodyTransform.position;
        StartCoroutine(SmoothRotation());
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = playerBodyTransform.position;
    }

    IEnumerator SmoothRotation()
    {
        while (true)
        {
            Quaternion rotateTo = playerBodyTransform.localRotation;
            float sumTime = 0.0f;

            while (sumTime < 1.0f)
            {
                transform.localRotation = Quaternion.Slerp(initRotation, rotateTo, sumTime);
                sumTime += Time.deltaTime * rotSpeedMultiplier;

                yield return null;
            }

            initRotation = rotateTo;
        }
    }

}
