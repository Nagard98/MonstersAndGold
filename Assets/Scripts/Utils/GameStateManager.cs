using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
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

    public void SetInGame(bool value)
    {
        gameState.isInGame = value;
    }

    public void SetIsTutorialOpen(bool value)
    {
        gameState.isTutorialOpen = value;
    }

    public void SetIsInGameMenuOpen(bool value)
    {
        gameState.isInGameMenuOpen = value;
    }

    public void PauseGame()
    {
        AudioListener.pause = gameState.isInGameMenuOpen || gameState.isTutorialOpen;
        Time.timeScale = gameState.isInGameMenuOpen || gameState.isTutorialOpen ? 0 : 1;
        gameState.isPaused = gameState.isTutorialOpen || gameState.isInGameMenuOpen;
    }

    public void UnpauseGame()
    {
        AudioListener.pause = !(!gameState.isInGameMenuOpen && !gameState.isTutorialOpen);
        Time.timeScale = (!gameState.isInGameMenuOpen && !gameState.isTutorialOpen) ? 1 : 0;
        gameState.isPaused = !(!gameState.isInGameMenuOpen && !gameState.isTutorialOpen);
    }
}
