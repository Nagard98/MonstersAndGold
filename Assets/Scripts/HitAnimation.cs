using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class HitAnimation : MonoBehaviour
{
    private Image image;
    private CanvasGroup canvasGroup;
    private float fadeInSpeed, fadeOutSpeed, fadeSpeed;
    private bool fade;
    private bool fadingIn, fadingOut;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        fadeInSpeed = 0.1f;
        fadeOutSpeed = -0.4f;
        fade = false;
        fadingIn = false;
        fadingOut = false;
        fadeSpeed = fadeInSpeed;
    }

    private void FadeOut()
    {
        image.DOFade(0f, 0.4f);
    }

    private void FadeIn()
    {

    }

    public void AnimateHit(float level)
    {
        switch ((Accuracy)(int)level)
        {
            case Accuracy.Perfect:
                image.color = Color.cyan;
                break;
            case Accuracy.Great:
                image.color = Color.green;
                break;
            case Accuracy.Good:
                image.color = Color.yellow;
                break;
            case Accuracy.Miss:
                image.color = Color.red;
                break;

        }
        fade = true;
        fadingIn = true;
        fadingOut = false;
        canvasGroup.alpha = 0f;
        fadeSpeed = fadeInSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (fade)
        {
            canvasGroup.alpha += (Time.deltaTime / fadeSpeed);
            if (fadingIn && canvasGroup.alpha >= 0.9f)
            {
                fadingIn = false;
                fadingOut = true;
                fadeSpeed = fadeOutSpeed;
            }
            else if (fadingOut && canvasGroup.alpha <= 0f)
            {
                fadingOut = false;
                fade = false;
            }
        }
    }
}
