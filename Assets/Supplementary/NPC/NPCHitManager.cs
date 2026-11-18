//using FIMSpace.FProceduralAnimation;
using System.Collections;
using UnityEngine;

/// <summary>
/// Drives Targets' reactions to different things like hitscan hits, TODO: rigidbody/projectile hits
/// called within RayShooter, handles ragdoll on/off when target is hit
/// </summary>
public class NPCHitManager : SneedSpace  {

    //=======//
    // Setup //
    //=======//
    #region setup


    //::::::::::::::::
    //:: components ::
    //::::::::::::::::

    [Space(20)]
    [Header("References")]
    [SerializeField] private NpcController npc;
    [SerializeField] private NPCRagdollHandler ragdoll;
    [SerializeField] private NPCCondition condition;
    [SerializeField] private NPCAnimatorController animatorController;
    [SerializeField] private NPCItemHolder itemHolder;
    [SerializeField] private BloodEffects bloodEffects;
    [SerializeField] private PlayerAudioManager playerAudio;
    [SerializeField] private NPCBodyPartHandler dismember;

    //:::::::::::
    //:: Audio ::
    //:::::::::::

    [Space]
    [Header("Audio")]
    [SerializeField] private AudioClip[] impactClips;
    [SerializeField] private AudioClip[] collidedClips;
    [SerializeField][Range(0, 1)] private float impactVolume = 1f;
    [SerializeField][Range(0, 1)] private float primaryImpactVolume = 1f;
    [SerializeField]private float pitchModifier = 0.1f;


    private Coroutine knockCoroutine;
    private bool reacted = false;
    private bool canDie;


    public void ResetReactionGate() => reacted = false;


    private void Awake(){
        if (npc==null) npc = GetComponentInParent<NpcController>();
        if (ragdoll == null) ragdoll = GetComponentInParent<NPCRagdollHandler>();
        if (condition == null) condition = GetComponent<NPCCondition>();
        if (bloodEffects == null) bloodEffects = GetComponent<BloodEffects>();
        if (itemHolder == null) itemHolder = GetComponent<NPCItemHolder>();
        if (animatorController == null) animatorController = GetComponent<NPCAnimatorController>();

    }


    #endregion


    //==================//
    // DAMAGE BEHAVIOUR //
    //==================//
    #region damage


    public void SendDamage(int damage, RaycastHit hit, Vector3 playerForward) {
        if(npc == null) return;

        Rigidbody hitRb = hit.rigidbody;

        //if(!npc.IsAlive && hitRb != null) {
        //    hitRb.AddForceAtPosition(
        //        playerForward.normalized * 2f,
        //        hit.point,
        //        ForceMode.Impulse
        //    );
        //}

        //:::::::::::::::::::::::::::
        //:: Get hit BodyPart info ::
        //:::::::::::::::::::::::::::

        bool ragdollHit = ragdoll.ResolveHit(
            hit.transform,
            out BodyPart part,
            out Transform sourceBone);


        if(!ragdollHit) {
            PrintError($"warning, could not resolve hit on {hit.transform?.name}"); return;
        } else {
            PrintError($"hit on {hit.transform?.name}");
        }

        // // // // Transform sourceBone = ragdoll.DummyBoneToSourceBone(hit.transform);


        //:::::@TODO::::::  IsHandHit, Is...Hit
        //if(ragdoll.IsHandHit(hit.transform)) {
        //    itemHolder?.DropHeldItem();
        //}

        //drop the held item
        if(hit.transform.name.ToLower().Contains("lowerarm.l") || hit.transform.name.ToLower().Contains("hand.l")) {
            itemHolder?.DropHeldItem();
        }

        BodyPartProfile profile = ragdoll.GetProfile(part);

        if(npc.damageCount >= npc.Health && reacted) {
            ////TriggerReact(hit.transform, hit.point, hit.normal, profile);
            ////simple corpse hit impulse, completely independent of RA2
            //if(dummyRb != null) {
            //    dummyRb.AddForceAtPosition(
            //        playerForward.normalized * 2f,
            //        hit.point,
            //        ForceMode.Impulse
            //    );
            //}

            PlayHitEffects(hit, profile);


            bool sever_again = dismember.ApplyLimbDamage(sourceBone, damage);
            if(sever_again) {
                ragdoll.ExecuteReaction(
                    ReactionType.Dismember,
                    hit.transform,
                    hit.point,
                    hit.normal,
                    npc.HitSpeed,
                    profile,
                    tryDismember: true);//was true

            }
        }

        //::::::::::::::::::::::::
        //:: damage/health shit ::
        //::::::::::::::::::::::::

        int finalDamage = Mathf.RoundToInt(profile.damageMultiplier * damage);
        npc.damageCount += finalDamage;
        npc.shouldFlea = npc.damageCount >= npc.Health * 0.66f;

        //Lethal, health<=0 or alwayslethal
        bool lethal = (npc.damageCount >= npc.Health) || (profile.alwaysLethal);
        PrintMessage($"{npc.gameObject.name} +damage({damage}), health: {npc.Health - npc.damageCount}, {npc.CanDie}, Parent:'{gameObject.transform.name}'");
        canDie = npc.CanDie;

        //:::::::::::
        //:: sever ::
        //:::::::::::
        bool sever = dismember.ApplyLimbDamage(sourceBone, damage);
        if(sever) {
            ragdoll.ExecuteReaction(
                ReactionType.Dismember,
                hit.transform,
                hit.point,
                hit.normal,
                npc.HitSpeed,
                profile,
                tryDismember: true);

        }

        //:::::::::::::
        //:: vfx/sfx ::
        //:::::::::::::

        PlayHitEffects(hit, profile);
        bloodEffects.SpawnBloodOneShot(hit, playerForward);


        //:::::::::::::::::::::::::::
        //:: apply bodypart effect ::
        //:::::::::::::::::::::::::::

        if(lethal) {
            //TriggerDeath(hit.transform, hit.point, hit.normal);
            TriggerDeath(hit.transform, hit.point, playerForward, profile);
        } else {
            condition?.ApplyBodyPartCondition(part, profile);
            TriggerReact(hit.transform, hit.point, hit.normal, profile);
        }


    }


    public void SendExplosion(int damage, float force, RaycastHit hit) {
        //if(npc == null)return;

        ////:::::::::::::::::::::::::::
        ////:: Get hit BodyPart info ::
        ////:::::::::::::::::::::::::::

        //bool ragdollHit = ragdoll.ResolveHit(
        //    hit.transform,
        //    out BodyPart part,
        //    out RagdollChainBone ra2Bone,
        //    out Rigidbody dummyRb);

        //if(!ragdollHit) {
        //    PrintError($"Could not resolve explosion hit on {hit.transform?.name}");
        //    return;
        //}

        //BodyPartProfile profile =
        //    ragdoll.GetProfile(part);

        ////::::::::::::
        ////:: Damage ::
        ////::::::::::::

        //npc.damageCount += damage;
        //PrintMessage($"EXPLODED {npc.gameObject.name} on {hit.transform.name}");
        //PlayHitEffects(hit, profile);//::::@TODO:::::: make custom effects handling
        //TriggerExplosion(hit.transform,hit.point,Vector3.up,force,profile);
    }


    //    public void SendDamage(int damage, RaycastHit hit, Vector3 playerForward) {

    //    if(npc == null || ragdoll == null)
    //        return;

    //    bool resolved = ragdoll.ResolveHit(
    //        hit.transform,
    //        out BodyPart part,
    //        out RagdollChainBone ra2Bone,
    //        out Rigidbody dummyRb
    //    );

    //    if(!resolved || ra2Bone == null) {
    //        PrintError($"Could not resolve hit on {hit.transform?.name}");
    //        return;
    //    }

    //    BodyPartProfile profile = ragdoll.GetProfile(part);

    //    Transform sourceBone =
    //        ragdoll.DummyBoneToSourceBone(hit.transform);

    //    bool alreadyDead = !npc.IsAlive;

    //    // Allow additional limbs to be severed after death.
    //    if(alreadyDead) {
    //        PlayHitEffects(hit, profile);

    //        dismemberment?.TryDismember(
    //            sourceBone,
    //            ra2Bone,
    //            hit.point,
    //            playerForward,
    //            damage,
    //            lethalOrAlreadyDead: true
    //        );

    //        return;
    //    }

    //    // Normal reaction gate applies only to the living NPC flow.
    //    if(reacted)
    //        return;

    //    int finalDamage =
    //        Mathf.RoundToInt(profile.damageMultiplier * damage);

    //    npc.damageCount += finalDamage;
    //    npc.shouldFlea = npc.damageCount >= npc.Health * 0.66f;

    //    bool lethal =
    //        npc.damageCount >= npc.Health ||
    //        profile.alwaysLethal;

    //    PlayHitEffects(hit, profile);

    //    bool severed = dismemberment?.TryDismember(
    //        sourceBone,
    //        ra2Bone,
    //        hit.point,
    //        playerForward,
    //        finalDamage,
    //        lethalOrAlreadyDead: lethal
    //    ) ?? false;

    //    if(lethal) {
    //        TriggerDeath(
    //            hit.transform,
    //            hit.point,
    //            playerForward,
    //            profile
    //        );
    //    } else {
    //        condition?.ApplyBodyPartCondition(part, profile);

    //        TriggerReact(
    //            hit.transform,
    //            hit.point,
    //            hit.normal,
    //            profile
    //        );
    //    }
    //}


    #endregion


    //===========================//
    // TRIGGER DEATH/REACT/KNOCK //
    //===========================//
    #region trigger_behaviour


    private void TriggerExplosion(Transform hitBone, Vector3 hitPoint, Vector3 hitDirection, float force, BodyPartProfile profile) {
        if(ragdoll == null) return;

        reacted = true;
        npc.ApplyDeath();

        ragdoll.ExecuteReaction(
            ReactionType.explode,
            hitBone,
            hitPoint,
            hitDirection,
            force,
            profile,
            tryDismember: false);

    }


    private void TriggerDeath(Transform hitBone, Vector3 hitPoint, Vector3 hitNormal, BodyPartProfile profile) {
        if(reacted || ragdoll == null) return;
        
        reacted = true;
        npc.ApplyDeath();

        Vector3 dir = hitNormal;
        ragdoll.ExecuteReaction(
            ReactionType.Death, 
            hitBone, 
            hitPoint, 
            dir, 
            npc.HitSpeed, 
            profile, 
            tryDismember: true);
    }


    private void TriggerReact(Transform hitBone, Vector3 hitPoint, Vector3 hitNormal, BodyPartProfile profile) {
        if(reacted || ragdoll == null) return;

        Vector3 dir = -hitNormal;

        ragdoll.ExecuteReaction(
            ReactionType.Flinch, 
            hitBone, 
            hitPoint, 
            dir, 
            npc.HitSpeed, 
            profile);
    }


    private void TriggerKnock(Transform hitBone, Vector3 hitPoint, Vector3 hitNormal, BodyPartProfile profile) {

        //::::::::::::::::::::::::::
        //:: instaknock bodyparts ::
        //::::::::::::::::::::::::::

        //later add INSTAKNOCK instead of alwaysLethal
        if(!profile.alwaysLethal) {
            ragdoll.ExecuteReaction(ReactionType.Flinch, hitBone, hitPoint, hitNormal, npc.HitSpeed, profile);
            return;
        }

        //:::::::::::::::::::::::::::::
        //:: non knockable bodyparts ::
        //:::::::::::::::::::::::::::::

        ragdoll.ExecuteReaction(ReactionType.Flinch,hitBone, hitPoint, hitNormal, npc.HitSpeed, profile);

        //::::::::::::::::::::::::
        //:: knock during dazed ::
        //::::::::::::::::::::::::

        if(reacted) {
            reacted = true;

            npc.ApplyKnock();
            if(ragdoll == null) return;
            Vector3 dir = -hitNormal;

            ragdoll.ExecuteReaction(ReactionType.Knock, hitBone, hitPoint, hitNormal, npc.HitSpeed, profile);
            return;
        }

        //::::::::::::::::::::
        //:: initiate dazed ::
        //::::::::::::::::::::

        if(knockCoroutine != null) StopCoroutine(knockCoroutine);
        knockCoroutine = StartCoroutine(
            KnockAfterDazed(hitBone, hitPoint, hitNormal, profile, 3f));

    }


    private IEnumerator KnockAfterDazed(Transform hitBone, Vector3 hitPoint, Vector3 hitNormal, BodyPartProfile profile, float duration) {
        reacted = true;

        //animator.SetBool("Dazed", true);
        animatorController?.SetDazed(true);
        yield return new WaitForSeconds(duration);
        animatorController?.SetDazed(false);
        //reacted = true;

        yield return new WaitForSeconds(duration);

        npc.ApplyKnock();
        Vector3 dir = -hitNormal;

        ragdoll.ExecuteReaction(ReactionType.Knock, hitBone, hitPoint, hitNormal, npc.HitSpeed, profile);
        knockCoroutine = null;
    }


    public void SendCollisionKnock(Transform hitBone, Vector3 hitPoint, Vector3 hitNormal, float closingSpeed, BodyPartProfile profile) {
        StartCollidedPlayback(hitPoint, 0.55f, 0.5f);
        TriggerKnock(hitBone, hitPoint, hitNormal, profile);
    }


    #endregion


    //=========//
    // SFX/VFX //
    //=========//
    #region audio


    private void PlayHitEffects(RaycastHit hit, BodyPartProfile profile) {
        StartHitPlayback(profile.primaryHitSound, hit.point, 0.75f, 0.75f);
        bloodEffects.SpawnBloodImpact(hit, ragdoll, profile.bulletHitVfxPrefab);
        bloodEffects.SpawnBloodImpact(hit, ragdoll, profile.secondaryBloodEffect);

    }


    public void StartHitPlayback(AudioClip clip, Vector3 hitPoint, float pitch, float volume) {
        if(impactClips == null || impactClips.Length == 0) return;

        //:::::::::::::::::::::
        //:: BACKGROUND CLIP :: consistent background clip array, should be low pitch
        //:::::::::::::::::::::

        GameObject  hitObject = new GameObject("Background Hit Impact Audio");
        AudioSource hitSource = hitObject.AddComponent<AudioSource>();
        AudioClip   hitClip = impactClips[Random.Range(0, impactClips.Length)];

        hitObject.transform.position = hitPoint;
        hitSource.clip = hitClip;
        hitSource.spatialBlend = 1f;
        hitSource.pitch = Random.Range(pitch - pitchModifier, pitch + pitchModifier);
        hitSource.volume = impactVolume;
        //source.outputAudioMixerGroup = _audioSource.outputAudioMixerGroup;

        //::::::::::::::::::
        //:: PRIMARY CLIP ::
        //::::::::::::::::::

        if(clip != null) { 
            GameObject  primaryObject = new GameObject("Primary Hit Impact Audio");
            AudioSource primarySource = primaryObject.AddComponent<AudioSource>();

            primaryObject.transform.position = hitPoint;
            primarySource.clip = clip;
            primarySource.spatialBlend = 1f;
            primarySource.pitch = Random.Range(0.75f - pitchModifier, 0.85f + pitchModifier);
            primarySource.volume = volume;
            //source.outputAudioMixerGroup = _audioSource.outputAudioMixerGroup;
            
            primarySource.Play();
            Destroy(primaryObject, clip.length / Mathf.Abs(primarySource.pitch));

        } else {
            PrintError("Primary source not found");
        }

        hitSource.Play();
        Destroy(hitObject, clip.length / Mathf.Abs(hitSource.pitch));
    }


    public void StartCollidedPlayback(Vector3 hitPoint, float pitch, float volume) {
        if(impactClips == null || impactClips.Length == 0) return;

        //::::::::::::::::::
        //:: PRIMARY CLIP ::
        //::::::::::::::::::

        GameObject primaryObject = new GameObject("Primary Hit Impact Audio");
        AudioSource primarySource = primaryObject.AddComponent<AudioSource>();
        AudioClip collidedClip = collidedClips[0];

        primaryObject.transform.position = hitPoint;
        primarySource.clip = collidedClip;
        primarySource.spatialBlend = 1f;
        primarySource.pitch = Random.Range(pitch - pitchModifier, pitch + pitchModifier);
        primarySource.volume = volume;
        //source.outputAudioMixerGroup = _audioSource.outputAudioMixerGroup;

        if(!primarySource.isPlaying) primarySource.Play();
        Destroy(primaryObject, collidedClip.length / Mathf.Abs(primarySource.pitch));

    }


    #endregion


}
