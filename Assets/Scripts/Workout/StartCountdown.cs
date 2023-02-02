using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Countdown for the beginning of the workout
public class StartCountdown : MonoBehaviour
{
    private AudioSource _source;
    private bool _hasPlayed;
    public UnityEvent CountdownTerminated;
    private GameStateVariable _gameState;

    // Start is called before the first frame update
    void Start()
    {
        _source = GetComponent<AudioSource>();
        _gameState = Resources.Load<GameStateVariable>("GameState");
        _hasPlayed = false;
    }

    public void Countdown()
    {
        _source.Play();
        _hasPlayed = true;
    }

    public void CleanUp()
    {
        _hasPlayed = false;
        _source.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_source.isPlaying && !_gameState.isPaused && _hasPlayed)
        {
            CountdownTerminated.Invoke();
            _hasPlayed = false;
        }
    }
}
