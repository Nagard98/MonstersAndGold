using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class SlideInOut : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private RectTransform _rectTransform;
    private Vector2 _exitPosition, _startPosition;
    public WorkoutPhasesVariable workoutPhases;
    public float timeToCenter;
    public float slideOutDelay;

    // Start is called before the first frame update
    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _rectTransform = GetComponent<RectTransform>();
        _startPosition = _rectTransform.anchoredPosition;
        _exitPosition = _rectTransform.anchoredPosition;
        _exitPosition.x *= -1;
    }

    private void SlideOut()
    {
        _rectTransform.DOAnchorPos(_exitPosition, timeToCenter).SetEase(Ease.InBack).SetDelay(slideOutDelay);
    }

    public void Animate(float phase)
    {
        _rectTransform.anchoredPosition = _startPosition;
        _text.text = "Fase " + workoutPhases.Value[(int)phase].phaseName;
        //Slides in
        _rectTransform.DOAnchorPos(Vector2.zero, timeToCenter).SetEase(Ease.OutBack).onComplete = SlideOut;
    }

}
