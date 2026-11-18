using System.Collections;
using UnityEngine;

public class NPCHitscan : SneedSpace {

    [Space(20)]
    [Header("Raycast")][Space]
    [SerializeField] private Transform aimTransform; // either end of barrel or something like that
    [SerializeField] private float maxRange = 100f;
    [SerializeField] private LayerMask hitMask = ~0; // should be player
    [SerializeField] private GameObject indicator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem muzzleFlash;
    private float pitchModifier = 0.1f;
    private float originalPitch;

    private Ray testRay;

    //retrofit for npc
    //[SerializeField] private PlayerAudioManager playerAudio;
    //[SerializeField] private WeaponVisuals weaponVisuals;
    //private NPCHitManager target;//playerhitmanager

    private int damage;

    private void Update() {
        //TestCanFire();
    }

    private void Awake() {

        //audioSource = GetComponentInChildren<AudioSource>();

        originalPitch = audioSource.pitch;
        //muzzleFlash = GetComponentInChildren<ParticleSystem>();

        //weapon = GetComponent<WeaponController>();
        //weaponVisuals = GetComponentInParent<WeaponVisuals>();
        //PrintMessage($"Has weapon: {weapon != null}, Has visuals: {weaponVisuals != null}");

        //if(weapon != null) {
        //    if(weapon.IsConfigured())
        //        damage = weapon.HitDamage;

        //} else {
        //    damage = 26;
        //}
        damage = 26;

    }

    public void TestCanFire() {
        RaycastHit hitInfo;

        testRay = new Ray(aimTransform.position, aimTransform.forward);
        bool hitFound = Physics.Raycast(testRay, out hitInfo, maxRange, hitMask);
        if(!hitFound) return;
        Vector3 hitDirection = aimTransform.forward.normalized;

        PlayerHitManager player = hitInfo.collider.GetComponent<PlayerHitManager>();


        if(player != null) {
            PrintMessage("CAN FIRE");
            return;
        }

    }

    public void FireWeapon(Vector3 targetPoint) {
        if(aimTransform == null) {
            PrintError("NPC weapon has no aim/muzzle transform.");
            return;
        }
        PrintMessage("entered firweapon");

        Vector3 toTarget = targetPoint - aimTransform.position;
        if(toTarget.sqrMagnitude < 0.001f) return;
        Vector3 hitDirection = toTarget.normalized;


        RaycastHit hitInfo;
        Ray ray = new Ray(
            aimTransform.position, 
            hitDirection
        );

        bool hitFound = Physics.Raycast(ray, out hitInfo, maxRange, hitMask);

        //playerAudio?.StartFirePlayback();
        //weaponVisuals?.PlayFire();
        StartFirePlayback();

        if(!hitFound) {
            PrintMessage("no hit found");

            return;

        }

        PlayerHitManager player = hitInfo.collider.GetComponent<PlayerHitManager>();

        if(player != null) {
            player.SendDamage(damage, hitInfo, hitDirection, transform.position);
            //PrintMessage($"player hit: {hitInfo.collider.name} @ root: ({player.gameObject.name})");
            return;
        } else {
            PrintError("Player is null");
        }


            //Rigidbody hitRb = hitInfo.collider.GetComponentInChildren<Rigidbody>();
            //if(hitRb != null) {
            //    hitRb.AddForceAtPosition(hitDirection * 5f, hitInfo.point, ForceMode.Impulse);
            //    return;
            //}


        StartCoroutine(ImpactIndicator(hitInfo.point));
    }
    private void StartFirePlayback() {
        //if(_audioSource.isPlaying)
        //    return;
        audioSource.pitch = originalPitch;
        audioSource.pitch = Random.Range(originalPitch - pitchModifier, originalPitch + pitchModifier);
        audioSource.PlayOneShot(audioSource.clip);
        muzzleFlash.Play(true);


        //_audioSource.Play();
        //_audioSource.volume = 0.1f;
        //_nextReflectionTime = Time.time;
    }




    private IEnumerator ImpactIndicator(Vector3 pos) {
        //face relative to players position
        Vector3 dir = (pos - transform.position).normalized;
        Quaternion rotatePos = Quaternion.LookRotation(dir);
        GameObject arrow = Instantiate(indicator, pos, rotatePos);

        yield return new WaitForSeconds(10);//pause at 1s
        Destroy(arrow);
    }
}
