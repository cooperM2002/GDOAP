using UnityEngine;

public class NPCAnimatorController : SneedSpace {

    [Space(20)]
    [Header("References")]
    [SerializeField] private Animator animator;

    [Space]
    [Header("Damping")]
    [SerializeField] private float speedDampTime = 0.15f;
    [SerializeField] private float directionDampTime = 0.15f;

    [Space(5)][Header("Overrides")][Space(5)]

    [Header("Flee")]
    [SerializeField] private AnimationClip normalIdle;
    [SerializeField] private AnimationClip terrifiedIdle;

    private AnimatorOverrideController overrideController;
    private Animator[] animators;

    private int pickupHash;
    private int hasItemHash;
    private int speedHash;
    private int moveXHash;
    private int moveYHash;
    private int inCombatHash;
    private int locomotionHash;
    private int ragdollHoldIndexHash;
    private int dazedHash;
    private int weaponHandHash;

    private bool isTerrified;



    private void Awake() {
        if(animator == null) {
            animator = GetComponent<Animator>();
        }

        animators = GetComponentsInChildren<Animator>(true);

        speedHash = Animator.StringToHash("Speed");
        moveXHash = Animator.StringToHash("MoveX");
        moveYHash = Animator.StringToHash("MoveY");
        dazedHash = Animator.StringToHash("Dazed");
        pickupHash = Animator.StringToHash("Pickup");
        hasItemHash = Animator.StringToHash("HasItem");
        inCombatHash = Animator.StringToHash("InCombat");
        weaponHandHash = Animator.StringToHash("WeaponHand");
        locomotionHash = Animator.StringToHash("LocomotionStyle");
        ragdollHoldIndexHash = Animator.StringToHash("RagdollHoldIndex");

        InitializeOverrides();
    }


    //==========//
    // movement //
    //==========//
    #region movement


    public void UpdateMovement(Vector3 worldVelocity, float movementRefSpeed, int locomotionStyle, bool inCombat) {
        if(animator == null) 
            return;

        float speed = worldVelocity.magnitude;
        animator.SetFloat(
            speedHash,
            speed,
            speedDampTime,
            Time.deltaTime
        );

        animator.SetBool(inCombatHash, inCombat);
        animator.SetInteger(locomotionHash, locomotionStyle);

        UpdateDirectionalMovement(worldVelocity);

    }

    private void UpdateDirectionalMovement(Vector3 worldVelocity) {
        

        Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);

        Vector3 localDirection = 
            localVelocity.sqrMagnitude > 0.001f
                ? localVelocity.normalized
                : Vector3.zero;
        

        //moveX = Mathf.Clamp(localVelocity.x/divisor, -1f, 1f);
        //moveY = Mathf.Clamp(localVelocity.z/divisor, -1f, 1f);   

        animator.SetFloat(moveXHash, localDirection.x, directionDampTime, Time.deltaTime);
        animator.SetFloat(moveYHash, localDirection.z, directionDampTime, Time.deltaTime);

    }

    public void StopMovement(float movementRefSpeed, int locomotionStyle, bool inCombat) {
        
        UpdateMovement(
            Vector3.zero,
            movementRefSpeed,
            locomotionStyle,
            inCombat
        );

    }


    #endregion


    //=======//
    // items //
    //=======//
    #region items


    public void SetWeaponHand(NPCWeaponHand hand) {
        if(animator == null) return;
        animator.SetInteger(weaponHandHash, (int)hand);
    }


    public void PlayPickup() {
        if(animator == null)
            return;

        animator.SetTrigger(pickupHash);
    }

    public void SetHasItem(bool hasItem) {
        if(animator == null)
            return;

        animator.SetBool(hasItemHash, hasItem);
    }


    #endregion


    //===================//
    // reactions/ragdoll //
    //===================//
    #region reactions/ragdoll

    public void SetRagdollHoldIndex(int index) {
        if(animator == null) 
            return;

        animator.SetInteger(ragdollHoldIndexHash, index);
    }

    public void SetDazed(bool dazed) {
        if(animator == null) 
            return;

        animator.SetBool(dazedHash, dazed);
    }

    public void SetAnimatorsEnabled(bool enabled) {
        if(animators == null) return;

        foreach(Animator anim in animators) {
            if(anim == null) 
                continue;

            anim.enabled = enabled;

            if(enabled) {
                anim.Rebind();
                anim.Update(0f);
            }
        }

    }

    #endregion


    //===========//
    // Overrides //
    //===========//
    #region overrides


    private void InitializeOverrides() {
        if(animator == null)
            return;

        overrideController = new AnimatorOverrideController(
            animator.runtimeAnimatorController
        );

        animator.runtimeAnimatorController = overrideController;
    }

    public void SetTerrifiedIdle(bool terrified) {
        if(overrideController == null)
            return;

        if(normalIdle == null || terrifiedIdle == null)
            return;

        if(isTerrified == terrified) 
            return;

        isTerrified = terrified;

        overrideController[normalIdle] = 
            terrified 
                ? terrifiedIdle 
                : normalIdle;
    }

    #endregion

}




//public class NPCAnimOverrides : MonoBehaviour
//{

//    [Header("References")]
//    [SerializeField] private Animator animator;

//    [Header("Idle Clips")]
//    [SerializeField] private AnimationClip normalIdle;
//    [SerializeField] private AnimationClip terrifiedIdle;

//    private AnimatorOverrideController overrideController;
//    private bool isTerrified;


//    private void Awake() {
//        if(animator == null)
//            animator = GetComponent<Animator>();

//        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
//        animator.runtimeAnimatorController = overrideController;
//    }

//    public void SetTerrifiedIdle(bool terrified) {
//        if(isTerrified == terrified) return;

//        isTerrified = terrified;
//        overrideController[normalIdle] = terrified ? terrifiedIdle : normalIdle;
//    }


//}
