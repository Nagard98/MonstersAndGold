using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitGame : MonoBehaviour
{

    public void Quit()
    {
        Application.Quit();
    }

    public void PauseGame()
    {
        AudioListener.pause = true;
        Time.timeScale = 0;
    }

    public void UnpauseGame()
    {
        AudioListener.pause = false;
        Time.timeScale = 1;
    }
}
