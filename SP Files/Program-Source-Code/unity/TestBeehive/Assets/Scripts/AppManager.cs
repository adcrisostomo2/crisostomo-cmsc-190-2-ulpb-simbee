using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppManager : MonoBehaviour
{
    public static int BuildIndex = 0;

    public void PlayAutomaticBeekeeping()
    {
        FrameManager.automatic = true;
        FrameManager.isReset = true;

        SceneManager.LoadScene(1);
    }

    public void PlayManualBeekeeping()
    {
        FrameManager.manual = true;
        FrameManager.isReset = true;

        SceneManager.LoadScene(2);
    }

    public void PlayQuiz()
    {
        QuizManager.isReset = true;

        SceneManager.LoadScene(3);
    }

    public void GoToMainMenu()
    {
        FrameManager.automatic = false;
        FrameManager.manual = false;

        SceneManager.LoadScene(0);
    }

    public void QuitApp()
    {
        Debug.Log("QUITTED!");
        Application.Quit();
    }
}
