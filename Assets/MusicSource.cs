using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MusicSource : MonoBehaviour
{

    private AudioSource audioSource;
    public float fadeInDuration, fadeOutDuration;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void FadeOut()
    {
        audioSource.DOFade(0, fadeOutDuration);
    }

    public void FadeIn()
    {
        audioSource.DOFade(1, fadeInDuration);
    }
}
