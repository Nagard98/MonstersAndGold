using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoldPoints : MonoBehaviour
{
    public FloatVariable playerGP;
    private TextMeshProUGUI _score;

    void Start()
    {
        _score = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        _score.SetText(((int)playerGP.Value).ToString());
    }
}
