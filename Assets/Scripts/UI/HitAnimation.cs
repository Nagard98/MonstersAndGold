using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Manages animation of the target box when hitting notes
[RequireComponent(typeof(CanvasGroup))]
public class HitAnimation : MonoBehaviour
{
    private Image _image;
    private CanvasGroup _canvasGroup;
    private float _fadeInSpeed, _fadeOutSpeed, _fadeSpeed;
    private bool _fade;
    private bool _fadingIn, _fadingOut;

    // Start is called before the first frame update
    void Start()
    {
        _image = GetComponent<Image>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _fadeInSpeed = 0.1f;
        _fadeOutSpeed = -0.4f;
        _fade = false;
        _fadingIn = false;
        _fadingOut = false;
        _fadeSpeed = _fadeInSpeed;
    }


    public void AnimateHit(float level)
    {
        switch ((Accuracy)(int)level)
        {
            case Accuracy.Perfect:
                _image.color = Color.cyan;
                break;
            case Accuracy.Great:
                _image.color = Color.green;
                break;
            case Accuracy.Good:
                _image.color = Color.yellow;
                break;
            case Accuracy.Miss:
                _image.color = Color.red;
                break;

        }
        _fade = true;
        _fadingIn = true;
        _fadingOut = false;
        _canvasGroup.alpha = 0f;
        _fadeSpeed = _fadeInSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (_fade)
        {
            _canvasGroup.alpha += (Time.deltaTime / _fadeSpeed);
            if (_fadingIn && _canvasGroup.alpha >= 0.9f)
            {
                _fadingIn = false;
                _fadingOut = true;
                _fadeSpeed = _fadeOutSpeed;
            }
            else if (_fadingOut && _canvasGroup.alpha <= 0f)
            {
                _fadingOut = false;
                _fade = false;
            }
        }
    }
}
