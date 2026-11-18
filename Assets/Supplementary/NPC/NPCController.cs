//using FIMSpace.FProceduralAnimation;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This class contains mostly things related to <b>STATE</b> <i>e.g</i> health, speed, toggle anims, die, etc...
/// </summary>
public class NpcController : SneedSpace  {

    //========//
    // Fields //
    //========//
    #region FIELDS


    [Space(20)]
    [Header("Stats")]
    [SerializeField] private LifeState state = LifeState.Alive;
    [SerializeField] private int startHealth = 100;
    [SerializeField] private float hitSpeed = 0.5f;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    private NPCAnimatorController animatorController;
    private NPCCondition condition;
    private NPCRagdollHandler ragdoll;
    private NPCIdentity identity;
    private NavMeshAgent agent;         
    //private LegsAnimator legs;
    private NPCMovement movement;
    private Transform NPCRoot;
    private NPCItemHolder itemHolder;

    private Coroutine standRoutine;
    private Coroutine dazedCoroutine;

    private Rigidbody[] ragdollBodies;

    private bool inCombat;
    private bool isAlive = true;
    private bool canDie;
    private string id;


    public int damageCount;
    public bool shouldFlea;
    //private int ragdolledHash;



    public NPCCondition Condition => condition;
    public LifeState State => state;
    public bool IsAlive => isAlive;
    public int  Health => startHealth;
    public bool CanDie => canDie;
    public float NpcWalkSpeed => walkSpeed;
    public float NpcRunSpeed => runSpeed;
    public float HitSpeed => hitSpeed;
    public string ID => id;
    //public enum LifeState { Alive, Knocked, Dead }


    #endregion


    //=================//
    // DEFAULT METHODS //
    //=================//
    #region DEFAULT METHODS


    private void Awake() {

        NPCRoot = transform.parent;

        //legs = GetComponent<LegsAnimator>();
        movement = GetComponent<NPCMovement>();
        agent = GetComponent<NavMeshAgent>();
        //ragdoll = GetComponent<RagdollManager>();
        identity = GetComponent<NPCIdentity>();
        condition = GetComponent<NPCCondition>();
        itemHolder = GetComponent<NPCItemHolder>();
        animatorController = GetComponent<NPCAnimatorController>();
        ragdollBodies = GetComponentsInChildren<Rigidbody>();

        //foreach(Rigidbody rb in ragdollBodies) {
        //    rb.useGravity = false;
        //    rb.isKinematic = true;
        //}
        EnableRagdoll(false);

    }


    void Start() {
        isAlive = true;
        startHealth = 100;

        if(identity != null) {
            id = identity.ShortId;
            identifierExtra = $"(#{id.ToUpper()})";
        }

        StartInfo();

    }


    private void StartInfo() {
        ClearPrintInfo();
        AddInfoHeader("References");
        AddInfoRefCheck("Wandering AI", movement);
        AddInfoRefCheck("NavMeshAgent", agent);
        AddInfoRefCheck("Transform", NPCRoot);
        AddInfoRefCheck("RagdollManager", ragdoll);
        AddInfoRefCheck("NPCIdentity", identity);
        AddInfoHeader("Attributes");
        AddInfoValue("Health", startHealth);
        AddInfoValue("Alive", isAlive);
        AddInfoValue("Speed", walkSpeed);
        AddInfoHeader("Conditions");
        AddInfoCheck("Health above zero", startHealth > 0);
        AddInfoCheck("Move speed positive", walkSpeed > 0);
        PrintStartInfo();
    }


    private void Update() {
        canDie = (startHealth <= damageCount);
    }


    #endregion

    //================//
    // alive/dead/etc //
    //================//
    #region LIFECYCLE METHODS

    public void SetAlive(bool alive) {
        isAlive = alive;
        //state = LifeState.Alive;
    }


    public void ApplyDeath() {
        if(state == LifeState.Dead) return;

        //
        // Cancel any recovery started before the lethal hit.
        if(standRoutine != null) {
            StopCoroutine(standRoutine);
            standRoutine = null;
        }
        if(dazedCoroutine != null) {
            StopCoroutine(dazedCoroutine);
            dazedCoroutine = null;
        }
        animatorController?.SetDazed(false);
        //

        itemHolder?.DropHeldItem();

        SetAlive(false); //set dead state
        state = LifeState.Dead;

        //
        ////stop navmesh movement
        //if(agent != null) {
        //    agent.isStopped = true;
        //    agent.ResetPath();
        //    // If you're going full ragdoll and never standing back up, you can disable the agent:
        //    // agent.enabled = false;
        //}
        //
        if(agent != null) {
            if(agent.enabled && agent.isOnNavMesh) {
                agent.isStopped = true;
                agent.ResetPath();
            }

            agent.enabled = false;
        }
        //

        //if(legs != null) {
        //    legs.enabled = false;
        //}

        EnableRagdoll(true);

        if(movement != null)
            movement.enabled = false; //disable Wandering behaviour
        SetAnims(false);


        StartCoroutine(DestroyAfterDelay(30f));
        //StartCoroutine(DestroyAfterDelay(5f)); //get rid of instance
    }

    private IEnumerator DestroyAfterDelay(float seconds) {
        yield return new WaitForSeconds(seconds);
        Destroy(NPCRoot.gameObject);
    }

    #endregion

    //===============//
    // knocked state //
    //===============//
    #region knocked

    public void ApplyKnock() {

        //if(state == LifeState.Knocked) return;//block reknocking
        if(state != LifeState.Alive)
            return;

        itemHolder?.DropHeldItem();
        state = LifeState.Knocked;

        //if(legs != null) {
        //    legs.enabled = false;
        //}

        //stop navmesh movement immediately
        if(agent != null) {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        if(movement != null) movement.enabled = false;

        int holdIndex = Random.Range(0, 4);
        animatorController?.SetRagdollHoldIndex(holdIndex);
    }

    public void OnGetUpRagdollComplete() {
        PrintMessage("Animation Event: Stand complete fired");
        //SetAnims(true);

        // restore muscle power — was dropped to 0.1f on knock and never restored

        //if(isFalling) return;//ignore, applyknock handles this

        //exiting fall = standing up complete

        //NPCHitManager hitManager = GetComponent<NPCHitManager>();
        //if(hitManager != null) hitManager.ResetReactionGate();
        ////SetAnims(true);
    }


    public void OnGetUpAnimComplete() {
        if(state == LifeState.Dead || !isAlive) {
            PrintMessage("Ignoring get-up event because NPC is dead.");
            return;
        }


        PrintMessage(
            $"GETUP EVENT frame={Time.frameCount}, " +
            $"time={Time.time:F2}, " +
            $"agentEnabled={agent != null && agent.enabled}, " +
            $"movementEnabled={movement != null && movement.enabled}"
        );

        //state = LifeState.Alive;
        state = LifeState.Dazed;

        if(agent != null) {
            agent.enabled = true;

            if(!agent.isOnNavMesh) {
                if(NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, agent.areaMask))
                    agent.Warp(hit.position);
            }
            agent.isStopped = false;
        }


        if(movement != null) movement.enabled = true;
        //if(legs != null) {
        //    legs.enabled = true;
        //}


        NPCHitManager hitManager = GetComponent<NPCHitManager>();
        if(hitManager != null) hitManager.ResetReactionGate();

        StartDazedWalk(5f);
    }


    public void StartDazedWalk(float duration) {
        if(dazedCoroutine != null) {
            StopCoroutine(dazedCoroutine);
        }

        dazedCoroutine = StartCoroutine(DazedWalkRoutine(duration));
    }


    private IEnumerator DazedWalkRoutine(float duration) {

        //
        if(state == LifeState.Dead || !isAlive) {
            dazedCoroutine = null;
            yield break;
        }
        //

        animatorController?.SetDazed(true);
        yield return new WaitForSeconds(duration);

        //
        if(state == LifeState.Dead || !isAlive) {
            animatorController?.SetDazed(false);
            dazedCoroutine = null;
            yield break;
        }
        //

        animatorController?.SetDazed(false);


        //if(state != LifeState.Alive) state = LifeState.Alive;
        if(state == LifeState.Dazed)
            state = LifeState.Alive;


        dazedCoroutine = null;
    }


    #endregion

    //=================//
    // Ragdoll helpers //
    //=================//
    #region RAGDOLL METHODS


    public void SetAnims(bool enabled) {

        animatorController?.SetAnimatorsEnabled(enabled);

    }

    public void EnableRagdoll(bool doEnable) {
        foreach(Rigidbody rb in ragdollBodies) {
            rb.useGravity = doEnable; 
            rb.isKinematic = !doEnable;
        }

    }


    #endregion


}
