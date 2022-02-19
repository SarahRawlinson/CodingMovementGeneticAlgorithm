using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private int soundsPlayingAtOnce = 10;
    private int activeSounds = 0;

    IEnumerator AddToSounds(float t)
    {
        yield return new WaitForSeconds(t);
        activeSounds--;
    }

    private void Awake()
    {
        if (FindObjectsOfType<SoundManager>().Length > 1) Destroy(this);
    }

    public bool CanPlay(float length)
    {
        if (activeSounds >= soundsPlayingAtOnce) return false;
        activeSounds++;
        StartCoroutine(AddToSounds(length));
        return true;
    }
}
