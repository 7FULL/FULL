using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //Singleton
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    public void PlaySound(AudioClip audioSource, Vector3 audioClip)
    {
        AudioSource.PlayClipAtPoint(audioSource, audioClip);
    }
}
