using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// used this for tutorial: https://www.youtube.com/watch?v=zc8ac_qUXQY&t=2s
public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // tutorial https://www.youtube.com/watch?v=3SdMFPdSi7M
    public void GoToOptions()
    {
        SceneManager.LoadScene("OptionsMenu");
    }
    
    public void GoToMain()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
