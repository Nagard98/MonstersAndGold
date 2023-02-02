using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    private GameStateVariable _gameState;

    private void Start()
    {
        _gameState = Resources.Load<GameStateVariable>("GameState");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void SetInGame(bool value)
    {
        _gameState.isInGame = value;
    }

    public void SetIsTutorialOpen(bool value)
    {
        _gameState.isTutorialOpen = value;
    }

    public void SetIsInGameMenuOpen(bool value)
    {
        _gameState.isInGameMenuOpen = value;
    }

    public void PauseGame()
    {
        AudioListener.pause = _gameState.isInGameMenuOpen || _gameState.isTutorialOpen;
        Time.timeScale = _gameState.isInGameMenuOpen || _gameState.isTutorialOpen ? 0 : 1;
        _gameState.isPaused = _gameState.isTutorialOpen || _gameState.isInGameMenuOpen;
    }

    public void UnpauseGame()
    {
        AudioListener.pause = !(!_gameState.isInGameMenuOpen && !_gameState.isTutorialOpen);
        Time.timeScale = (!_gameState.isInGameMenuOpen && !_gameState.isTutorialOpen) ? 1 : 0;
        _gameState.isPaused = !(!_gameState.isInGameMenuOpen && !_gameState.isTutorialOpen);
    }
}
