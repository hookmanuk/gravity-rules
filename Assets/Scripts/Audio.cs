using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    public AudioClip MusicForwards;
    public AudioClip MusicBackwards;
    private AudioSource _audioSource;
    private int _totalTimeSamples;

    // Start is called before the first frame update
    void Start()
    {        
        _totalTimeSamples = MusicForwards.samples;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Forwards(bool start)
    {
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }
        int backwardSamples = _audioSource.timeSamples;

        _audioSource.clip = MusicForwards;
        if (!start)
        {
            _audioSource.timeSamples = MusicForwards.samples - backwardSamples;
        }
        _audioSource.Play();
    }

    public void Reverse()
    {
        int forwardSamples = _audioSource.timeSamples;
        
        _audioSource.clip = MusicBackwards;
        _audioSource.timeSamples = MusicBackwards.samples - forwardSamples;
        _audioSource.Play();
    }
}
