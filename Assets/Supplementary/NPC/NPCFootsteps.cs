using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class NPCFootsteps : SneedSpace {

    [SerializeField] private AudioClip[] footstepAudioClips;
    [SerializeField] private float minimumPitch = 0.9f;
    [SerializeField] private float maximumPitch = 1.1f;

    [SerializeField] private AudioSource audioSource;


    private void Awake() {
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null) {
            PrintError("audio source is null");
        }
        PlayFootstep();
    }

    [ContextMenu("Test Footstep")]
    public void PlayFootstep() {
        PrintMessage("ENTERED FOOTSTEP");

        if(footstepAudioClips == null || footstepAudioClips.Length == 0) {
            PrintError("no clips");
            return;

        }

        AudioClip clip =
            footstepAudioClips[Random.Range(0, footstepAudioClips.Length)];

        audioSource.pitch =
            Random.Range(minimumPitch, maximumPitch);
        audioSource.clip = clip;
        audioSource.Play();
    }


}
