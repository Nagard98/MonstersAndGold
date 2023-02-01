using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StartCountdown : MonoBehaviour
{

    private AudioSource source;
    private bool hasPlayed;
    public UnityEvent CountdownTerminated;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        hasPlayed = false;
    }

    public void Countdown()
    {
        source.Play();
        hasPlayed = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!source.isPlaying && hasPlayed)
        {
            CountdownTerminated.Invoke();
            hasPlayed = false;
        }
    }
}
