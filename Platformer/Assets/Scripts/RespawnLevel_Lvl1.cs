using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnLevel : MonoBehaviour
{

    private void Start()
    {
        SceneManager.LoadScene(3);
    }

}
