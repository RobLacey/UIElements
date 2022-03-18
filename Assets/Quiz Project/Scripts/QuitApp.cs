using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitApp : MonoBehaviour
{
    public void Quit()
    {
        foreach (var audioSource in FindObjectsOfType<AudioSource>())
        {
            audioSource.Stop();
        }

        Application.Quit();
    }
}
