using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerAudioManager : MonoBehaviour {

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] footsteps;
    [SerializeField] private AudioClip other;

    private float originalPitch;
    private float pitchModifier = 0.2f;
    private Vector3 hitPoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    /// <summary>
    /// Initializes the AudioSource and ensures it doesn't auto-play.
    /// </summary>
    private void Awake() {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
        _audioSource.Stop();

        originalPitch = _audioSource.pitch;


    }

    /// <summary>
    /// Starts playback of the primary audio clip and resets reflection timer.
    /// </summary>
    public void StartPlayback() {
        if(_audioSource.isPlaying)
            return;
        int index = Random.Range(0, footsteps.Length);
        _audioSource.PlayOneShot(footsteps[index]);
        //_audioSource.Play();
        //_audioSource.volume = 0.1f;
        //_nextReflectionTime = Time.time;
    }
    public void StartFirePlayback() {
        //if(_audioSource.isPlaying)
        //    return;
        _audioSource.pitch = originalPitch;
        int index = Random.Range(0, footsteps.Length);
        _audioSource.pitch = Random.Range(originalPitch-pitchModifier, originalPitch+pitchModifier);
        _audioSource.PlayOneShot(footsteps[index]);
        //_audioSource.Play();
        //_audioSource.volume = 0.1f;
        //_nextReflectionTime = Time.time;
    }

    public void StartHammerPlayBack() {
        //if(_audioSource.isPlaying)
        //    return;
        _audioSource.pitch = originalPitch;
        _audioSource.pitch = Random.Range(originalPitch - pitchModifier, originalPitch + pitchModifier);
        _audioSource.PlayOneShot(other);
        //_audioSource.Play();
        //_audioSource.volume = 0.1f;
        //_nextReflectionTime = Time.time;
    }

    public void StartHitPlayback(Vector3 hitPoint) {
        if(footsteps == null || footsteps.Length == 0)
            return;

        int index = Random.Range(0, footsteps.Length);
        AudioClip clip = footsteps[index];

        GameObject audioObject = new GameObject("Hit Impact Audio");
        audioObject.transform.position = hitPoint;

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 1f; // 3D
        source.pitch = Random.Range(originalPitch - pitchModifier, originalPitch + pitchModifier);
        source.volume = _audioSource.volume;
        source.outputAudioMixerGroup = _audioSource.outputAudioMixerGroup;

        source.Play();

        Destroy(audioObject, clip.length / Mathf.Abs(source.pitch));
    }
}
