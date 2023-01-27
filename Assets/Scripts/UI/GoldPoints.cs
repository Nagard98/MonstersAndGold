using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoldPoints : MonoBehaviour
{
    public FloatVariable playerGP;
    private TextMeshProUGUI score;

    void Start()
    {
        score = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        score.SetText(((int)playerGP.Value).ToString());
    }
}
