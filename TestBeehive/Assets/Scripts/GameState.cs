using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject this_button;
    public GameObject other_button;
    public static float currentInterval = 1f;
    
    public void ToggleActive(GameObject button)
    {
        if (button.activeSelf)
        {
            button.SetActive(false);
        }
        else
        {
            button.SetActive(true);
        }
    }

    public static void ChangeCurrentInterval(float _currentInterval)
    {
        currentInterval = _currentInterval;
    }

    public void PauseResume()
    {
        if (!GameIsPaused)
        {
            GameIsPaused = true;
            Time.timeScale = 0f;
            FrameManager.ChangeInterval(1f);
            ToggleActive(this_button);
            ToggleActive(other_button);

        }
        else
        {
            GameIsPaused = false;
            Time.timeScale = 1f;
            FrameManager.ChangeInterval(1f);
            ToggleActive(this_button);
            ToggleActive(other_button);
        }
    }
}
