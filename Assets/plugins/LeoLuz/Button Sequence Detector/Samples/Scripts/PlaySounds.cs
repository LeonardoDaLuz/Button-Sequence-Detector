using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlaySounds : MonoBehaviour {
    public AudioClip[] sounds; 
    private AudioSource audioSource;
    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void PlaySound(int index)
    {
        audioSource.clip = sounds[index];
        audioSource.Play();
    }
}
