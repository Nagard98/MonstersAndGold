using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

//Manages beahviour of a panel on the HUD that shows how far is the next POI
public class AlertPanelPOI : MonoBehaviour
{
    private RectTransform _rectTransform;
    private float _poiDuration, _poiDistance;
    private bool _countdownRunning, _endCountdownStarted;
    private TextMeshProUGUI _timerText, _alertText;
    private Image _poiImage;

    public float scaleAnimDuration;
    public UnityEvent<float> Countdown;

    // Start is called before the first frame update
    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        TextMeshProUGUI[] textComps = GetComponentsInChildren<TextMeshProUGUI>();
        _poiImage = GetComponentsInChildren<Image>()[1];
        _timerText = textComps[1];
        _alertText = textComps[0];
        _countdownRunning = false;
        _endCountdownStarted = false;
    }

    public void ShowAlert(POIVariable poi, SpawnSettings spawnSettings)
    {
        _poiDistance = spawnSettings.distance;
        _poiDuration = spawnSettings.TTL;
        _poiImage.sprite = poi.sprite;
        _poiImage.color = poi.GetSpriteColor();
        _alertText.text = "Attenzione! "+poi.alertMessage+" "+_poiDistance+"m";
        _countdownRunning = true;
        _endCountdownStarted = false;
        _rectTransform.DOScale(1f, scaleAnimDuration);
    }

    public void HideAlert(float delay)
    {
        _countdownRunning = false;
        _rectTransform.DOScale(0f, scaleAnimDuration).SetDelay(delay);
    }

    public void CleanUp()
    {
        _countdownRunning = false;
        _rectTransform.DOKill();
        _rectTransform.localScale = Vector3.zero;
    }

    private void OnDisable()
    {
        CleanUp();
    }

    // Update is called once per frame
    void Update()
    {
        if (_countdownRunning)
        {
            _poiDuration -= Time.deltaTime;
            if (_poiDuration >= 0f)
            {
                float seconds = Mathf.FloorToInt(_poiDuration % 60f);
                float minutes = Mathf.FloorToInt(_poiDuration / 60f);
                _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

                //Starts audio countdown if less than 10s remaining
                if (_poiDuration < 10f && !_endCountdownStarted)
                {
                    Countdown.Invoke(10f - _poiDuration);
                    _endCountdownStarted = true;
                }
            }
            else
            {
                _poiDuration = 0f;
                _countdownRunning = false;
                HideAlert(2f);
            }
            
        }
    }
}
