using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [Required("Must Assign an Audio Clip")]
    [SerializeField] private AudioClip _audioClip;

    private AudioSource _myAudioSource;
    
    protected virtual void Awake()
    {
        gameObject.AddComponent<AudioSource>();
        _myAudioSource = GetComponent<AudioSource>();
        _myAudioSource.playOnAwake = false;
        _myAudioSource.clip = _audioClip;

    }

    public virtual void Play(bool doPlay)
    {
        if (doPlay)
        {
            StopAllOthers();
            _myAudioSource.Play();
        }
        else
        {
            if (!_myAudioSource.isPlaying) return;
            _myAudioSource.Pause();
        }
    }

    public void Restart()
    {
        _myAudioSource.Stop();
    }
    
    private void StopAllOthers()
    {
        foreach (var audioSource in FindObjectsOfType<AudioSource>())
        {
            if(audioSource == _myAudioSource) continue;
            audioSource.Stop();
        }
    }
}
