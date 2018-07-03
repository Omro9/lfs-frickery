using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is responsible for playing audio by varying the volume of the audio sources
// based on the canoe's movements
//
// Also, this camera object should have two audio sources for white noise wave sounds
// and variable wind sounds
public class SoundController : MonoBehaviour {

    // Audio Sources
    private AudioSource[] m_audioSources;
    public AudioClip m_calmWaves;
    public AudioClip m_wind;

    // Canoe has to have a rigidbody for velocity calculations
    public Transform m_player;
    public Transform m_canoe;
    private Rigidbody m_rigidbody;

    // Volume bounds
    private float m_windMin = 0f;
    private float m_windMax = .8f;
    private float m_waveMin = .1f;
    private float m_waveMax = .7f;

    private float m_windChangeAcceleration = 1f;
    private float m_waveChangeAcceleration = 2f;

    void Start()
    {
        m_audioSources = transform.GetComponents<AudioSource>();
        m_rigidbody = m_player.GetComponent<Rigidbody>();

        PlaySource(m_audioSources[0], m_calmWaves, true, m_waveMin);
        PlaySource(m_audioSources[1], m_wind, true, m_windMin);
    }

    void Update()
    {
        UpdateWaveSource();
        UpdateWindSource();
    }

    private void PlaySource(AudioSource source, AudioClip clip, bool looping, float volume)
    {
        source.clip = clip;
        source.loop = looping;
        source.volume = volume;
        source.Play();
    }    

    private void UpdateWaveSource()
    {
        Debug.Log(m_canoe.rotation.eulerAngles.x);
        if (m_canoe.rotation.eulerAngles.x > 300f && m_canoe.rotation.eulerAngles.x < 358f)
            m_audioSources[0].volume += m_waveChangeAcceleration * Time.deltaTime;
        else
            m_audioSources[0].volume -= m_waveChangeAcceleration * Time.deltaTime / 2;

        m_audioSources[0].volume = Mathf.Clamp(m_audioSources[0].volume, m_waveMin, m_waveMax);
    }

    private void UpdateWindSource()
    {
        if(m_rigidbody.velocity.magnitude > 0f)
            m_audioSources[1].volume += m_windChangeAcceleration * Time.deltaTime;
        else
            m_audioSources[1].volume -= m_windChangeAcceleration * Time.deltaTime;

        m_audioSources[1].volume = Mathf.Clamp(m_audioSources[1].volume, m_windMin, m_windMax);
    }
}
