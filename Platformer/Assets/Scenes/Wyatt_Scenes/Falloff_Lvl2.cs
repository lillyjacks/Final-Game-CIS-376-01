using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FallOff_Lvl2 : MonoBehaviour
{
    [SerializeField] AudioSource Spoon_Crash;

    void OnTriggerEnter(Collider other)
    {
        Spoon_Crash.Play();
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(3);
    }
}