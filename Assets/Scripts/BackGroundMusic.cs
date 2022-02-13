using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundMusic : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;
    private int _currentClip = -1;
    private float volumn = 1;
    private bool volumnOn = true;

    public void TurnVolumeDown()
    {
        if (volumnOn) audioSource.volume -= 0.1f;
            
    }
    public void TurnVolumeUp()
    {
        if (volumnOn) audioSource.volume += 0.1f;
    }
    public void TurnVolumeOnOff(bool on)
    {
        if (on)
        {
            audioSource.volume = volumn;
        }
        else
        {
            volumn = audioSource.volume;
            audioSource.volume = 0f;
        }
        volumnOn = on;
    }
            
        
    private void Update()
    {
        if (!audioSource.isPlaying)
        {
            PlayNextClip();
        }
    }

    private void PlayNextClip()
    {
        _currentClip += 1;
        if (_currentClip > audioClips.Length -1 || _currentClip < 0)
        {
            _currentClip = 0;
        }
        audioSource.clip = audioClips[_currentClip];
        audioSource.Play();
    }
}
