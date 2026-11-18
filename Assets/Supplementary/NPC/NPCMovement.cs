using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCMovement : SneedSpace {
    //========//
    // fields //
    //========//
    #region fields


    [Space(20)]
    [Header("Combat")][Space]
    [SerializeField] private float combatRadius = 5f;
    [SerializeField] private float combatRadiusVariance = 0.75f;
    [SerializeField] private float combatRepathTime = 0.5f;


    [Space]
    [Header("Patrol")][Space]
    [SerializeField] private float wanderRadius = 100f;
    [SerializeField] private float arriveDistance = 0.6f;
    [SerializeField] private float repathCooldown = 4f;

    [Space]
    [Header("Chase")][Space]
    [SerializeField] private float chaseStoppingDistance = 1.5f;
    [SerializeField] private float chaseRepathCooldown = 0.15f;
    [SerializeField] private float nextChaseRepathTime;

    [Space]
    [Header("Animator")][Space]
    [SerializeField] private NPCAnimatorController animatorController;
    [SerializeField] private NPCCondition condition;

    [Space]
    [Header("Path Settings")][Space]
    [SerializeField] private GameObject path;
    [SerializeField] private GameObject fleaPoint;
    [SerializeField] private bool ignoreHeight = false;

    [Space]
    [Header("Footsteps")][Space]
    [SerializeField] private AudioClip[] footstepAudioClips;
    [SerializeField] private float minimumPitch = 0.9f;
    [SerializeField] private float maximumPitch = 1.1f;




    // components
    private PlayerMovement player;
    private NavMeshAgent agent;
    private NpcController npc;
    private NPCVision vision;

    // animator
    private float walkSpeed;
    private float runSpeed;

    // combat
    private Vector3 currentCombatPoint;
    private float nextCombatRepath;
    private bool combat;

    // path
    private List<Transform> pathList = new List<Transform>();
    private float nextRepathTime;

    // vision
    private float viewRange = 5.0f;
    private float viewRadius = 0.75f;

    // footstep
    private AudioSource audioSource;



    #endregion


    //===========//
    // main loop //
    //===========//
    #region mainloop


    private void Awake() {

        player = FindFirstObjectByType<PlayerMovement>();
        npc = GetComponent<NpcController>();
        agent = GetComponent<NavMeshAgent>();
        vision = GetComponent<NPCVision>();
        condition = GetComponent<NPCCondition>();
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null) {
            PrintError("audio source is null");
        }

        if(animatorController == null) 
            animatorController = GetComponent<NPCAnimatorController>();

        if(path == null) 
            path = GameObject.FindGameObjectWithTag("NPCPath");

        if(fleaPoint == null) 
            fleaPoint = GameObject.FindGameObjectWithTag("FleaPoint");

        walkSpeed = npc != null ? npc.NpcWalkSpeed : agent.speed;
        runSpeed  = npc != null ? npc.NpcRunSpeed  : agent.speed * 1.5f;

        combat = false;

        BuildPath(path);
        TryPickNewDestination(force: true);

    }

    private void Update() {
        //if(Input.GetKeyDown(KeyCode.T))
        //    animator.SetBool("InCombat", true);

        //if(Input.GetKeyDown(KeyCode.Y))
        //    animator.SetBool("InCombat", false);

        //animator.SetFloat("MoveX", Input.GetAxis("Horizontal"));
        //animator.SetFloat("MoveY", Input.GetAxis("Vertical"));

        /*
        //====================//
        // preliminary checks //
        //====================//

        //if(npc == null || agent == null || vision == null) return;

        ////dead
        //if(!npc.IsAlive) {
        //    StopAgent();//stop agent
        //    SetAnimSpeed(0f);//stop animation
        //    return;
        //}else if(npc.State == LifeState.Knocked) {
        //    StopAgent();
        //    SetAnimSpeed(0f);
        //    return;
        //}

        //agent is usable after ragdoll enable-disable
        if(!agent.enabled) agent.enabled = true;


        //=================//
        // condition check //
        //=================//

        ApplyMovementCondition();

        if(condition != null && condition.BlocksMovement) {
            StopAgent();
            SetAnimSpeed(0f);
            return;
        }

        WarpBackToPath();//resync to navmesh
        agent.isStopped = false;//prelims passed

        //......after safety, before patrol logic.........//

        //===============//
        // chasing logic //
        //===============//
        if(npc.shouldFlea) {
            if(animatorOverrides != null)
                animatorOverrides.SetTerrifiedIdle(true);

            agent.SetDestination(fleaPoint.transform.position);
            SetAnimSpeed(agent.velocity.magnitude);
            return;
        } else {
            if(animatorOverrides != null)
                animatorOverrides.SetTerrifiedIdle(false);
        }

        if(vision.HasTarget) {
            agent.stoppingDistance = chaseStoppingDistance;      //optional
            agent.SetDestination(vision.CurrentTarget.position); //chase target
            SetAnimSpeed(agent.velocity.magnitude);
            return;//IMPORTANT! skip wander path selection this frame
        }

        agent.stoppingDistance = 0f;//if not chasing, restore patrol stopping distance

        //==========================//
        // Patrol / wandering logic //
        //==========================//

        //new destination
        bool arrived = agent.hasPath && agent.remainingDistance <= arriveDistance;//arrived
        bool needsPath = !agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete;//no valid path

        if(arrived || needsPath) TryPickNewDestination(force: true);

        //BlendTree speed from velocity (m/s)
        SetAnimSpeed(agent.velocity.magnitude);

        //.........................................//
        //old obstacle hook, DONT rotate manually
        sightRay = new Ray(transform.position, transform.forward);
        DetectObstacle(sightRay);
        //.........................................//
         
         */
    }


    #endregion


    //=================//
    // movement states //
    //=================//
    #region movement states


    public void Patrol() {
        if(!PrepareMovement()) { UpdateMovementAnimation(); return; }

        SetCombat(false);
        SetTerrifiedIdle(false);
        agent.stoppingDistance = 0f;

        bool arrived = agent.hasPath && agent.remainingDistance <= arriveDistance;
        bool needsPath = !agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete;

        if(arrived || needsPath) {
            TryPickNewDestination(force: true);
        }

        UpdateMovementAnimation();
    }


    public void Chase(Transform target) {
        if(target == null)      { Patrol(); return; }
        if(!PrepareMovement())  { UpdateMovementAnimation(); return; }

        SetCombat(false);
        SetTerrifiedIdle(false);
        agent.stoppingDistance = chaseStoppingDistance;

        if(Time.time >= nextChaseRepathTime) {
            nextChaseRepathTime = Time.time + chaseRepathCooldown;

            if(agent.isOnNavMesh) {
                agent.SetDestination(target.position);
            }
        }

        UpdateMovementAnimation();
    }


    public void Flee() {
        if(!PrepareMovement()) { UpdateMovementAnimation(); return; }

        SetCombat(false);
        SetTerrifiedIdle(true);

        if(fleaPoint == null) { StopMovement(); return; }

        agent.updateRotation = true;
        agent.stoppingDistance = 0f;

        if(agent.isOnNavMesh) {
            agent.SetDestination(fleaPoint.transform.position);
        }

        UpdateMovementAnimation();
    }


    public void Combat360(Transform player) {
        if(!PrepareMovement()) { UpdateMovementAnimation(); return; }

        SetCombat(true);
        agent.stoppingDistance = 0f;

        bool arrived =
            agent.hasPath &&
            !agent.pathPending &&
            agent.remainingDistance <= 0.1f;

        bool needsPath =
            !agent.hasPath ||
            agent.pathStatus != NavMeshPathStatus.PathComplete;

        //bool timerExpired = Time.time >= nextCombatRepath;
        if(arrived || needsPath) {// || timerExpired
            //nextCombatRepath = Time.time + combatRepathTime;
            currentCombatPoint = GetCombatPoint(player);
            agent.SetDestination(currentCombatPoint);

        }

        FacePlayer(player);
        UpdateMovementAnimation();
    }


    public void StopMovement() {
        if(agent != null && agent.enabled && agent.isOnNavMesh) {
            StopAgent();
        }

        int locomotionStlye = condition != null
            ? condition.LocomotionStyle
            : 0;

        animatorController?.StopMovement(walkSpeed, locomotionStlye, combat);
    }


    private void SetCombat(bool enabled) {
        combat = enabled;

        if(agent != null)
            agent.updateRotation = !enabled;
    }

    private bool ShouldChase() {
        return vision != null && vision.IsTargetVisible && vision.CurrentTarget != null;
    }

    private bool InCombat() {
        return vision != null && vision.IsTargetVisible && vision.CurrentTarget != null && combat == true;
    }

    private void DoChase(Transform target) {
        // optional but typical for chase
        agent.stoppingDistance = chaseStoppingDistance;

        // minimal throttle so you don't recompute paths every frame
        if(Time.time < nextChaseRepathTime) return;
        nextChaseRepathTime = Time.time + chaseRepathCooldown;

        if(agent.isOnNavMesh)
            agent.SetDestination(target.position);
    }


    #endregion


    //========//
    // combat //
    //========//
    #region combat


    private void FacePlayer(Transform player) {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if(dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            540f * Time.deltaTime);//mess with this multiplier
    }


    private Vector3 GetCombatPoint(Transform target) {
        float angle  = Random.Range(-180f, 180f) * Mathf.Deg2Rad; 
        float radius = Random.Range(-combatRadiusVariance, combatRadiusVariance) + combatRadius;

        Vector3 desired = target.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        if(NavMesh.SamplePosition(desired, out NavMeshHit hit, 2f, NavMesh.AllAreas)) {
            return hit.position;
        }

        return transform.position;
    }


    public void MoveToItem(Transform item) {
        if(item == null) return;
        if(npc.State != LifeState.Alive) return;
        
        if(!PrepareMovement()) { UpdateMovementAnimation(); return; }

        SetCombat(true);
        agent.updateRotation = true;
        agent.stoppingDistance = 0f;

        Vector3 destination = item.position;
        if(NavMesh.SamplePosition(item.position, out NavMeshHit navHit, 1.5f, agent.areaMask)) {
            destination = navHit.position;
        }

        agent.SetDestination(destination);

        //Slow down close to the item.
        if(!agent.pathPending && agent.remainingDistance <= 3f) {
            agent.speed = walkSpeed;
            if(agent.remainingDistance <= 0.1f) 
                agent.speed = walkSpeed * 0.5f;
            
        } else {
            agent.speed = runSpeed;
        }

        UpdateMovementAnimation();
    }


    #endregion


    //==========//
    // animator //
    //==========//
    #region animator

    private void UpdateMovementAnimation() {
        if(animatorController == null) return;

        Vector3 velocity = 
            (agent != null && agent.enabled)
                ? agent.velocity
                : Vector3.zero;

        int locomotionStyle =
            (condition != null)
                ? condition.LocomotionStyle
                : 0;

        animatorController.UpdateMovement(velocity, walkSpeed, locomotionStyle, combat);
    }


    private void SetTerrifiedIdle(bool terrified) {
        animatorController?.SetTerrifiedIdle(terrified);
    }

    private bool PrepareMovement() {

        // Never allow navigation while knocked, dead, or getting up.
        if(npc.State != LifeState.Alive) return false;
        if(agent == null) return false;

        // Important: do not force-enable the agent here.
        // Knock/ragdoll/death systems may intentionally disable it.
        if(!agent.enabled) return false;

        ApplyMovementCondition();
        WarpBackToPath();

        if(!agent.isOnNavMesh) return false;

        agent.isStopped = false;
        return true;
    }

    private void ApplyMovementCondition() {
        if(condition == null) return;

        float speed = 
            ShouldChase()
                ? runSpeed
                : walkSpeed;

        if(agent == null) return;

        if(combat) {
            agent.speed = walkSpeed * condition.MovementSpeedMultiplier;

        } else {
            agent.speed = speed * condition.MovementSpeedMultiplier;

        }

    }


    #endregion


    //===============//
    // navmesh agent //
    //===============//
    #region navmeshagent


    private void StopAgent() {
        if(agent == null) return;
        agent.isStopped = true;
        agent.ResetPath();
    }

    private void WarpBackToPath() {
        if(!agent.isOnNavMesh) {
            NavMeshHit hit;
            bool pathReachable = NavMesh.SamplePosition(transform.position, out hit, 2f, agent.areaMask);

            if(pathReachable) 
                agent.Warp(hit.position);
        }
    }

    private bool TryPickNewDestination(bool force) {

        if(agent == null) return false;
        if(!force && Time.time < nextRepathTime) return false; //still on path or not forced

        nextRepathTime = Time.time + repathCooldown;

        if(!agent.isOnNavMesh) return false;
        //Vector3 random = Random.insideUnitSphere * wanderRadius;
        //random.y = 0f;

        //===============//
        // Path follower //
        //===============//

        /*
        //System.Random rand = new System.Random();
        //int idx = rand.Next(pathList.Count);      //random.Next(list.Count) generates a number from 0 (inclusive) up to list.Count (exclusive)
        //Transform node = pathList[idx];           //Access the item using the random index
        //Vector3 randomNode = node.position;
        ////randomNode.y = 0f;
        */
        
        Vector3 candidate = RandomNode(ignoreHeight);
        NavMeshHit hit;

        if(NavMesh.SamplePosition(candidate, out hit, wanderRadius, agent.areaMask)) {
            agent.SetDestination(hit.position);
            return true;
        }

        return false;
    }

    private void BuildPath(GameObject path) {
        foreach(Transform node in path.transform) 
            pathList.Add(node.transform);
        
        if(pathList == null)
            PrintError("Path is empty");
    }

    private Vector3 RandomNode(bool ignoreHeight) {
        System.Random rand = new System.Random();

        //idx= 0 =< idx < list.Count
        Transform node = pathList[rand.Next(pathList.Count)];//Access the item using the random index
        Vector3 randomNode = node.position;
        randomNode.y = ignoreHeight ? 0f : randomNode.y;

        return randomNode;
    }


    #endregion


    //===========//
    // Footsteps //
    //===========//
    #region footsteps


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
    #endregion



    void OnDrawGizmos() {
        if (!Camera.current.CompareTag("MainCamera")) 
            return; //render only on main camera

        Ray ray = new Ray(transform.position, transform.forward);

        Vector3 start = new Vector3(
            ray.origin.x,
            ray.origin.y + 2f,
            ray.origin.z
            );

        float radius = viewRadius;
        float range = viewRange;

        Vector3 dir = ray.direction.normalized; //direction of line
        Vector3 end = start + dir * range;      //endpoint of line
        Gizmos.color = Color.gray;

        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(end, radius);
    }


}
