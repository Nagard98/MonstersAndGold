using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class AlertPanelPOI : MonoBehaviour
{
    private RectTransform rectTransform;
    private float poiDuration, poiDistance;
    //private string poiName;
    private bool countdownRunning, endCountdownStarted;
    private TextMeshProUGUI timerText, alertText;
    private Image poiImage;
    public float scaleDuration;
    public UnityEvent<float> Countdown;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        TextMeshProUGUI[] textComps = GetComponentsInChildren<TextMeshProUGUI>();
        poiImage = GetComponentsInChildren<Image>()[1];
        timerText = textComps[1];
        alertText = textComps[0];
        countdownRunning = false;
        endCountdownStarted = false;
    }

    public void ShowAlert(POIVariable poi, SpawnSettings spawnSettings)
    {
        poiDistance = spawnSettings.distance;
        poiDuration = spawnSettings.TTL;
        poiImage.sprite = poi.sprite;
        poiImage.color = poi.GetSpriteColor();
        alertText.text = "Attention! There is a "+poi.poiName+" in "+poiDistance+"m";
        countdownRunning = true;
        endCountdownStarted = false;
        rectTransform.DOScale(1f, scaleDuration);
    }

    public void HideAlert(float delay)
    {
        countdownRunning = false;
        rectTransform.DOScale(0f, scaleDuration).SetDelay(delay);
    }

    public void CleanUp()
    {
        countdownRunning = false;
        rectTransform.DOKill();
        rectTransform.localScale = Vector3.zero;
    }

    private void OnDisable()
    {
        CleanUp();
    }

    // Update is called once per frame
    void Update()
    {
        if (countdownRunning)
        {
            poiDuration -= Time.deltaTime;
            if (poiDuration >= 0f)
            {
                float seconds = Mathf.FloorToInt(poiDuration % 60f);
                float minutes = Mathf.FloorToInt(poiDuration / 60f);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                if (poiDuration < 10f && !endCountdownStarted)
                {
                    Countdown.Invoke(10f-poiDuration);
                    endCountdownStarted = true;
                }
            }
            else
            {
                poiDuration = 0f;
                countdownRunning = false;
                HideAlert(2f);
            }
            
        }
    }
}
