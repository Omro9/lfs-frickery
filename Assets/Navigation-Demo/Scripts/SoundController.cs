using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is responsible for playing audio
// This script is attached to a camera object with an audio source
//
// Also, this camera object should have two audio sources for white noise wave sounds
// and variable wind sounds
public class SoundController : MonoBehaviour {

    private AudioSource[] m_audioSources;
    public AudioClip m_calmWaves;
    public AudioClip m_wind;

    // Canoe has to have a rigidbody for velocity calculations
    public Transform m_canoe;

    void Start()
    {
        m_audioSources = transform.GetComponents<AudioSource>();

        PlaySource(m_audioSources[0], m_calmWaves, true, .3f);
        PlaySource(m_audioSources[1], m_wind, true, .2f);
    }

    void Update()
    {
        
    }

    private void PlaySource(AudioSource source, AudioClip clip, bool looping, float volume)
    {
        source.clip = clip;
        source.loop = looping;
        source.volume = volume;
        source.Play();
    }

    // The following compares a changable source (like a velocity) and correlates it to a linear funciton from min to max
    // Ex: as the canoe's velocity increases, the wind and pitch increase
    // Ex: As the canoe dips down, the calming waves get louder to sound more realistic to a splash sound
    private void VariableSource(AudioSource source, float changingVariable, float maxChangingVariable, float minVolume, float maxVolume, float minPitch, float maxPitch)
    {

    }
    
}
