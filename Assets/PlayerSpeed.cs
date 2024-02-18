using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerSpeed : MonoBehaviour
{
    public FloatVariable playerSpeed;
    private TextMeshProUGUI speed;

    // Start is called before the first frame update
    void Start()
    {
        speed = GetComponent<TextMeshProUGUI>();
        decimal d = decimal.Round(((decimal)playerSpeed.Value), 2);
        speed.SetText(d.ToString() + " m/s");
    }

    // Update is called once per frame
    void Update()
    {
        decimal d = decimal.Round(((decimal)playerSpeed.Value), 2);
        speed.SetText(d.ToString() + " m/s");
    }
}
