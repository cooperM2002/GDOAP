using UnityEngine;

public class NPCVision : SneedSpace 
{

    //========//
    // Vision //
    //========//
    #region fields/default


    [Space(20)]
    [Header("Vision Settings")]
    [SerializeField, Range(0f, 360f)] private float viewAngle = 90f;        //total angle of view, 90deg
    [SerializeField, Range(0f, 360f)] private float combatViewAngle = 90f;  //total angle of view, 90deg
    [SerializeField] private float viewRadius = 12f;
    [SerializeField] private bool flattenY = false;
    [SerializeField] private Transform viewPoint;

    [Space]
    [Header("Object Detection Settings")]
    [SerializeField] private float targetMemoryTime = 4.0f;
    [SerializeField] private LayerMask targetMask;   //target, player layer
    [SerializeField] private LayerMask obstacleMask; //obstacle, building layer

    private readonly Collider[] _hits = new Collider[16];  //fixed amount of stored colliders
    private Transform currentTarget;
    private bool isTargetVisible;
    private float lastSeenTime;
    private Vector3 currentAimPoint;


    //:::::: for WanderingAI :::::::::::::::
    public Vector3 CurrentAimPoint => currentAimPoint;
    public Transform CurrentTarget => currentTarget;
    public bool IsTargetVisible => isTargetVisible;
    public float TotalViewAngle => viewAngle;
    public bool HasTarget => CurrentTarget != null;



    private void Update() {
        ScanForTarget();
    }


    #endregion


    //========//
    // vision //
    //========//
    #region vision
    public void ScanForTarget() {

        // check (radius)
        // then (is in FOV)
        // then (is in LOS)
        // finally pick nearest target

        //:::::::: setup ::::::::::::

        Vector3 origin = (viewPoint!=null) 
            ? viewPoint.position 
            : transform.position;
        
        // target candidates buffer
        int targetCount 
           = Physics.OverlapSphereNonAlloc( origin, viewRadius, _hits, targetMask);

        if(targetCount <= 0) {
            if(Time.time - lastSeenTime > targetMemoryTime) {
                isTargetVisible = false;
                currentTarget = null;
                currentAimPoint = default;
            }
            return;
        }

        //:::::::: pick candidate ::::::::::::::

        // pick best candidate
        Transform bestTarget = null;
        Vector3 bestPoint = default;
        float bestDist = float.PositiveInfinity; // any target closer to origin than this will be a better target

        for(int i = 0; i < targetCount; i++) {

            Collider c = _hits[i];
            Vector3 point = c.bounds.center;
            Transform candidate = c.transform.root;

            if(!TargetInFov(origin, point)) 
                continue;

            float sqrDist = (point - origin).sqrMagnitude; // dist to target position

            // this is our new closest target
            if(sqrDist < bestDist) {
                bestDist = sqrDist;
                bestTarget = candidate;
                bestPoint = point;
            }

        }

        //::::::: no target :::::::::

        if(bestTarget == null) {
            if(Time.time - lastSeenTime > targetMemoryTime) {
                isTargetVisible = false;
                currentTarget = null;
                currentAimPoint = bestPoint;
            }
            return;
        }

        // no line of sight, obstructed
        if(!HasLineOfSight(origin, bestPoint)) {
            if(Time.time - lastSeenTime > targetMemoryTime) {
                isTargetVisible = false;
                currentTarget = null;
                currentAimPoint = default;
            }
            return;
        }

        //::::::::: set target based on candidates :::::::::::

        isTargetVisible = true;
        currentTarget = bestTarget;
        currentAimPoint = bestPoint;
        lastSeenTime = Time.time;

    }

    /// <summary>
    /// takes in origin point and candidate, check if candidate inside view cone (angle test)
    /// </summary>
    private bool TargetInFov(Vector3 origin, Vector3 targetPoint) {

        // vec from this to target
        Vector3 toTarget = (targetPoint - origin);
        Vector3 forward = transform.forward;

        if(flattenY) {
            toTarget.y = 0f;
            forward.y = 0f;
        }

        // basically touching
        if(toTarget.sqrMagnitude < 0.0001f) return true;

        toTarget.Normalize();
        forward.Normalize();

        float currentViewAngle = HasTarget ? 180f : viewAngle;
        float halfAngleRad = (currentViewAngle * 0.5f) * Mathf.Deg2Rad;
        float cosThreshold = Mathf.Cos(halfAngleRad);
        float dot = Vector3.Dot(forward, toTarget);

        bool inFov = dot >= cosThreshold;
        return inFov;
    }

    /// <summary>
    /// raycasts to target and checks for rays hit on any obstacle to determine blocked vision
    /// </summary>
    private bool HasLineOfSight(Vector3 origin, Vector3 targetPoint) {

        Vector3 dir = (targetPoint - origin);
        float dist = dir.magnitude;

        if(dist < 0.00001f) return true; 

        dir /= dist; //normalize

        //obstacle ray, if something is obstructing the target, block
        //bool blocked = Physics.Raycast(origin, dir, dist, obstacleMask);
        return !Physics.Raycast(origin, dir, dist, obstacleMask);
    }


    #endregion


}
