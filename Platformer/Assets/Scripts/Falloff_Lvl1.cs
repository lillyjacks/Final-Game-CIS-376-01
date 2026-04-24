using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FallOff_Lvl1 : MonoBehaviour
{
    //[SerializeField] AudioSource fallSound;

    void OnTriggerEnter(Collider other)
    {
    //    fallSound.Play();
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(2);
    }
}