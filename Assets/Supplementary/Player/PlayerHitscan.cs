using System.Collections;
using UnityEngine;


public class PlayerHitscan : PrimaryUseBehaviour {

    [Space(20)]
    [Header("Raycast")]


    [SerializeField] private float maxRange = 100f;
    [SerializeField] private LayerMask hitMask = ~0; //everything layer
    [SerializeField] private LayerMask npcRootMask;
    [SerializeField] private GameObject indicator;
    [SerializeField] private PlayerAudioManager playerAudio;
    //[SerializeField] private WeaponVisuals weaponVisuals;

    private Transform aimTransform;
    private Camera aimCamera;
    private Crosshair crosshair;
    //private WeaponController weapon;
    //private NPCHitManager previousTarget;


    private bool canFire = true;
    private int damage;

    private void Awake() {

        Camera cam = Camera.main;
        if (cam != null) 
            aimTransform = cam.transform;
         else 
            PrintError("No main cam found.");
        

        aimCamera = aimTransform.GetComponent<Camera>();
        if(aimCamera == null) aimCamera = Camera.main;
        if(aimCamera != null) crosshair = aimCamera.GetComponent<Crosshair>();
        if(crosshair == null) PrintError("HitscanBehaviour: no Crosshair found on the aim camera.");

        //weapon = GetComponent<WeaponController>();
        //weaponVisuals = GetComponentInParent<WeaponVisuals>();
        ////PrintMessage($"Has weapon: {weapon != null}, Has visuals: {weaponVisuals!=null}");

        //damage = (weapon != null && weapon.IsConfigured()) ? weapon.HitDamage : 26 ;
        damage = 26;

        //if(weapon != null) {
        //    if(weapon.IsConfigured()) damage = weapon.HitDamage;
        //} else {
        //    damage = 26;
        //}

    }


    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            PrimaryUse();
        }
    }

    public override void PrimaryUse() {
        //if(revolver && !canFire) return; 
        //if(revolver) canFire = false;

        //:::::::::::::
        //:: raycast ::
        //:::::::::::::
        RaycastHit hit;
        Ray ray;

        ray = aimCamera != null && crosshair != null 
            ? aimCamera.ScreenPointToRay(crosshair.AimScreenPosition)
            : new Ray(aimTransform.position, aimTransform.forward);

        bool hitFound = Physics.Raycast(ray, out hit, maxRange, hitMask);
        int fireCount = 0;

        //:: SFX/VFX ::

        playerAudio?.StartFirePlayback();
        //weaponVisuals?.PlayFire();

        //:: Apply Hit ::

        if(!hitFound) {
            return;
        }

        Vector3 hitDirection = ray.direction.normalized;
        DetectHit(hit, hitDirection);
        fireCount++;
        PrintMessage($"Shot #{fireCount}");
    }


    private void DetectHit(RaycastHit hit, Vector3 hitDirection) {
        NPCHitManager npc = hit.collider.GetComponentInParent<NPCHitManager>();
        if(npc != null) {
            npc.SendDamage(damage, hit, hitDirection);
            PrintMessage($"NPC hit: {hit.collider.name} @ root: ({npc.gameObject.name})");
            return;
        }



        Rigidbody hitRb = hit.collider.GetComponentInChildren<Rigidbody>();
        if(hitRb != null) {
            hitRb.AddForceAtPosition(hitDirection * 5f, hit.point, ForceMode.Impulse);
            return;
        }

        StartCoroutine(ImpactIndicator(hit.point));
    }


    public bool TryGetAimData(out Ray aimRay, out Vector3 aimPoint) {
        aimRay = default;
        aimPoint = Vector3.zero;

        if(!ResolveAimReferences()) return false;

        aimRay = aimCamera.ScreenPointToRay(crosshair.AimScreenPosition);
        aimPoint = Physics.Raycast(aimRay, out RaycastHit hit, maxRange, hitMask) 
            ? hit.point
            : aimRay.GetPoint(maxRange);


        return true;
    }


    private bool ResolveAimReferences() {
        if(aimCamera == null) aimCamera = Camera.main;
        if(aimCamera == null) {
            PrintError("HitscanBehaviour: no camera tagged MainCamera found.");
            return false;
        }

        aimTransform = aimCamera.transform;

        if(crosshair == null) crosshair = aimCamera.GetComponent<Crosshair>();
        if(crosshair == null) {
            PrintError("HitscanBehaviour: Main Camera has no Crosshair component.");
            return false;
        }

        return true;
    }


    public void OnRevolverHammerReady() {
        canFire = true;
        PrintMessage("Revolver Hammer Ready");
    }


    private IEnumerator ImpactIndicator(Vector3 pos) {
        Vector3 dir          = (pos - transform.position).normalized;
        Quaternion rotatePos = Quaternion.LookRotation(dir);
        GameObject arrow     = Instantiate(indicator,pos,rotatePos);

        yield return new WaitForSeconds(10);//pause at 1s
        Destroy(arrow);
    }


}
