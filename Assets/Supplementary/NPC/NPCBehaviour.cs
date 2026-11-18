using UnityEngine;

public class NPCBehaviourController : SneedSpace {

    [SerializeField] private NpcController npc;
    [SerializeField] private NPCMovement movement;
    [SerializeField] private NPCVision vision;
    [SerializeField] private NPCCondition condition;
    [SerializeField] private NPCItemHolder itemHolder;

    [SerializeField] private BehaviourState state;
    [SerializeField] private float combatDistance = 3f;

    public BehaviourState State => state;

    private void Awake() {
        if(npc == null) npc = GetComponent<NpcController>();
        if(vision == null) vision = GetComponent<NPCVision>();
        if(movement == null) movement = GetComponent<NPCMovement>();
        if(itemHolder == null) itemHolder = GetComponent<NPCItemHolder>();
        if(condition == null) condition = GetComponent<NPCCondition>();

    }

    private void Update() {
        UpdateState();
        RunState();
    }

    private void SetState(bool condition, BehaviourState newState) {
        if(condition) {
            state = newState;
        } else {
            return;
        }
    }

    private void UpdateState() {

        if(npc.State == LifeState.Dead)                     { state = BehaviourState.Dead;    return; }
        if(npc.State == LifeState.Knocked)                  { state = BehaviourState.Knocked; return; }
        if(condition != null && condition.BlocksMovement)   { state = BehaviourState.Knocked; return; }
        if(npc.shouldFlea)                                  { state = BehaviourState.Flee;    return; }

        if(vision!=null && vision.HasTarget) {

            float distance = Vector3.Distance(transform.position, vision.CurrentTarget.position);
            if(distance < combatDistance) {
                PrintMessage($"entered combat");
                state = BehaviourState.Combat;

            } else {
                PrintMessage($"entered chase");
                state = BehaviourState.Chase;
            
            }

            return;
        }
        state = BehaviourState.Patrol;

    }

    private void RunState() {

        if(movement==null) return;

        bool engaged = 
            state == BehaviourState.Chase || 
            state == BehaviourState.Combat ||
            npc.State != LifeState.Knocked;

        //if(engaged && itemHolder != null && itemHolder.TryHandleItem()) {
        //    return;
        //}

        switch(state) {
            case BehaviourState.Dead:
                movement.StopMovement();
                break;

            case BehaviourState.Knocked:
                movement.StopMovement();
                break;

            case BehaviourState.Flee:
                movement.Flee();
                break;

            case BehaviourState.Chase:
                movement.Chase(vision.CurrentTarget);
                break;

            case BehaviourState.Patrol:
                movement.Patrol();
                break;

            case BehaviourState.Combat:
                movement.Combat360(vision.CurrentTarget);
                break;
        }
    }




}
