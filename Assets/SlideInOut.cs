using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class SlideInOut : MonoBehaviour
{
    private TextMeshProUGUI text;
    private RectTransform rectTransform;
    private Vector2 exitPosition, startPosition;
    public float timeToCenter;
    public float slideOutDelay;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
        exitPosition = rectTransform.anchoredPosition;
        exitPosition.x *= -1;
    }

    private void SlideOut()
    {
        rectTransform.DOAnchorPos(exitPosition, timeToCenter).SetEase(Ease.InBack).SetDelay(slideOutDelay);
    }

    public void Animate(float phase)
    {
        rectTransform.anchoredPosition = startPosition;
        text.text = "Fase " + (int)phase;
        rectTransform.DOAnchorPos(Vector2.zero, timeToCenter).SetEase(Ease.OutBack).onComplete = SlideOut;
    }

}
