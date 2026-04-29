using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Back_To_Title : MonoBehaviour
{
    [SerializeField] AudioSource start_up_noise;

    void OnTriggerEnter(Collider other)
    {
        start_up_noise.Play();
        SceneManager.LoadScene(8);
    }

   
}