using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitGame : MonoBehaviour
{
    private GameStateVariable gameState;

    private void Start()
    {
        gameState = Resources.Load<GameStateVariable>("GameState");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void PauseGame()
    {
        AudioListener.pause = true;
        Time.timeScale = 0;
        gameState.isPaused = true;
    }

    public void UnpauseGame()
    {
        AudioListener.pause = false;
        Time.timeScale = 1;
        gameState.isPaused = false;
    }
}
