using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// used this for tutorial: https://www.youtube.com/watch?v=zc8ac_qUXQY&t=2s
public class MainMenu : MonoBehaviour
{
    //public void PlayGame()
    //{
    //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    //}
    public void GoToLvl1()
    {
        SceneManager.LoadScene("level1"); // replace "level1" with level name. Add scene to File -> Build Profiles -> Scene List
    }

    public void GoToLvl2()
    {
        SceneManager.LoadScene("level2"); // replace "level2" with level name. Add scene to File -> Build Profiles -> Scene List
    }

    public void GoToLvl3()
    {
        SceneManager.LoadScene("level3"); // replace "level3" with level name. Add scene to File -> Build Profiles -> Scene List
    }

    public void GoToLvls()
    {
        SceneManager.LoadScene("LevelsScene"); // replace "level3" with level name. Add scene to File -> Build Profiles -> Scene List
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
