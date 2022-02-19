using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] public List<AudioClip> deathClips;
    [SerializeField] private AudioSource source;
    private SoundManager _manager;

    private void Start()
    {
        _manager = FindObjectOfType<SoundManager>();
        GetComponent<Brain>().OnDead += PlayDeath;
    }

    public void PlayDeath()
    {
        PlaySound(Random.Range(0, deathClips.Count), deathClips);
    }

    public void PlaySound(int index, List<AudioClip> clips)
    {
        if (_manager.CanPlay(clips[index].length))
        {
            source.clip = clips[index];
            source.Play();
        }
    }
}
