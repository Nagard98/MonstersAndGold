using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StartCountdown : MonoBehaviour
{

    private AudioSource source;
    private bool hasPlayed;
    public UnityEvent CountdownTerminated;
    private GameStateVariable gameState;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        gameState = Resources.Load<GameStateVariable>("GameState");
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
        if (!source.isPlaying && !gameState.isPaused && hasPlayed)
        {
            CountdownTerminated.Invoke();
            //Debug.Log("Count terminato");
            hasPlayed = false;
        }
    }
}
