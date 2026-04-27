using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Next_Lvl : MonoBehaviour
{
    [SerializeField] AudioSource start_up_noise;

    void OnTriggerEnter(Collider other)
    {
        start_up_noise.Play();
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(3);
    }
}