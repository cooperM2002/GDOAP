using UnityEngine;


public class PlayerHitManager : SneedSpace {

    [Space(20)]
    //[SerializeField] private BloodEffects bloodEffects;//player hit effects (overlay png?)
    [SerializeField] private PlayerAudioManager playerAudio;
    //[SerializeField] private CameraImpactEffects cameraImpact;
    [SerializeField] private AudioClip[] impactClips;
    [SerializeField] private AudioClip[] collidedClips;
    [SerializeField][Range(0, 1)] private float impactVolume = 1f;
    [SerializeField][Range(0, 1)] private float primaryImpactVolume = 1f;
    [SerializeField] private float pitchModifier = 0.1f;
    //[SerializeField] private float volumeModifier;
    //[SerializeField][Range(-3, 3)] private float impactPitch = 1f;

    private bool canDie;
    private float health;
    private PlayerController player;
    private PlayerOverlayManager overlay;


    private void Awake() {
        player = GetComponent<PlayerController>();
        overlay = GetComponentInChildren<PlayerOverlayManager>();
        health = player.Health;
        PrintMessage($"{health}");
    }


    //==================//
    // DAMAGE BEHAVIOUR //
    //==================//

    #region damage

    public void SendDamage(int damage, RaycastHit hit, Vector3 npcForward, Vector3 npcPosition) {


        //:::::: damage/health shit :::::::


        //int finalDamage = Mathf.RoundToInt(profile.damageMultiplier * damage);
        //npc.damageCount += finalDamage;
        //npc.shouldFlea = npc.damageCount >= npc.Health * 0.66f;

        //!!!!!!!!!!!//
        player.damageCount += damage;
        float currentHealth = health - player.damageCount;
        float maxHealth = health;
        bool lethal = (player.damageCount >= player.Health);
        PrintMessage($"Player +damage({damage}), currentHealth: {currentHealth} maxHealth: {maxHealth}");
        canDie = player.CanDie;
        overlay.SetHealth(
            currentHealth,
            maxHealth
        );
        //!!!!!!!!!!!//i


        //:::::: vfx/sfx :::::::::::::::::::::

        //PlayHitEffects(hit, profile);


        //:::::: apply effects/death :::::::


        TriggerReact(
            hit.transform,
            hit.point,
            hit.normal,
            npcPosition
            );
        

        if(lethal) {
            //TriggerDeath(hit.transform, hit.point, hit.normal);
            TriggerDeath(
                hit.transform,
                hit.point,
                npcForward
                );

        } 
        //else {
        //    TriggerReact(
        //        hit.transform,
        //        hit.point,
        //        hit.normal,
        //        npcPosition
        //        );
        //}

    }

    //private void PlayHitEffects(RaycastHit hit, BodyPartProfile profile) {
    //    StartHitPlayback(profile.primaryHitSound, hit.point, 0.75f, 0.75f);
    //    bloodEffects.SpawnBloodImpact(hit, ragdoll, profile.bulletHitVfxPrefab);
    //    bloodEffects.SpawnBloodImpact(hit, ragdoll, profile.secondaryBloodEffect);
    //}


    #endregion


    //===========================//
    // TRIGGER DEATH/REACT/KNOCK //
    //===========================//
    #region trigger_behaviour


    private void TriggerDeath(Transform hitBone, Vector3 hitPoint, Vector3 hitNormal) {

        player.ApplyDeath(hitBone, hitPoint, hitNormal);

        //Vector3 dir = -hitNormal;
        //ragdoll.ExecuteReaction(ReactionType.Death, hitBone, hitPoint, dir, npc.HitSpeed, profile);
    }


    private void TriggerReact(Transform hitBone, Vector3 hitPoint, Vector3 hitNormal, Vector3 npcPosition) {
        //PrintMessage("Triggered Player React");
        Vector3 toNpc = npcPosition - transform.position;

        //cameraImpact.AddImpact(toNpc);

        //player.ApplyReact(hitBone, hitPoint, hitNormal);
        //Vector3 dir = -hitNormal;
        //ragdoll.ExecuteReaction(ReactionType.Flinch, hitBone, hitPoint, dir, npc.HitSpeed, profile);



    }


    #endregion


    //==================//
    // hit impact audio //
    //==================//
    #region audio


    //public void StartHitPlayback(AudioClip clip, Vector3 hitPoint, float pitch, float volume) {
    //    if(impactClips == null || impactClips.Length == 0) return;

    //    // ::::: BACKGROUND CLIP ::::: consistent background clip array, should be low pitch

    //    GameObject hitObject = new GameObject("Background Hit Impact Audio");
    //    AudioSource hitSource = hitObject.AddComponent<AudioSource>();
    //    AudioClip hitClip = impactClips[Random.Range(0, impactClips.Length)];

    //    hitObject.transform.position = hitPoint;
    //    hitSource.clip = hitClip;
    //    hitSource.spatialBlend = 1f;
    //    hitSource.pitch = Random.Range(pitch - pitchModifier, pitch + pitchModifier);
    //    hitSource.volume = impactVolume;
    //    //source.outputAudioMixerGroup = _audioSource.outputAudioMixerGroup;


    //    // ::::: PRIMARY CLIP ::::::

    //    if(clip != null) {
    //        GameObject primaryObject = new GameObject("Primary Hit Impact Audio");
    //        AudioSource primarySource = primaryObject.AddComponent<AudioSource>();

    //        primaryObject.transform.position = hitPoint;
    //        primarySource.clip = clip;
    //        primarySource.spatialBlend = 1f;
    //        primarySource.pitch = Random.Range(0.75f - pitchModifier, 0.85f + pitchModifier);
    //        primarySource.volume = volume;
    //        //source.outputAudioMixerGroup = _audioSource.outputAudioMixerGroup;

    //        primarySource.Play();
    //        Destroy(primaryObject, clip.length / Mathf.Abs(primarySource.pitch));

    //    } else {
    //        PrintError("Primary source not found");
    //    }

    //    hitSource.Play();
    //    Destroy(hitObject, clip.length / Mathf.Abs(hitSource.pitch));
    //}

    //public void StartCollidedPlayback(Vector3 hitPoint, float pitch, float volume) {
    //    if(impactClips == null || impactClips.Length == 0) return;

    //    // ::::: PRIMARY CLIP ::::::

    //    GameObject primaryObject = new GameObject("Primary Hit Impact Audio");
    //    AudioSource primarySource = primaryObject.AddComponent<AudioSource>();
    //    AudioClip collidedClip = collidedClips[0];

    //    primaryObject.transform.position = hitPoint;
    //    primarySource.clip = collidedClip;
    //    primarySource.spatialBlend = 1f;
    //    primarySource.pitch = Random.Range(pitch - pitchModifier, pitch + pitchModifier);
    //    primarySource.volume = volume;
    //    //source.outputAudioMixerGroup = _audioSource.outputAudioMixerGroup;

    //    if(!primarySource.isPlaying) primarySource.Play();
    //    Destroy(primaryObject, collidedClip.length / Mathf.Abs(primarySource.pitch));

    //}


    #endregion

}
