using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameStateVariable : ScriptableObject
{
    public bool isPaused;
    public bool isFirstTutorial;
    public bool isInGame;
    public bool isInGameMenuOpen;
    public bool isTutorialOpen;
    public bool isWorkoutStarted;
}
