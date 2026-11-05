using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AnimationController))]
public class GoapAgent : MonoBehaviour {


    [Header("Sensors")]
    [SerializeField] Sensor chaseSensor;
    [SerializeField] Sensor attackSensor;


    [Header("Known Locations")]
    [SerializeField] Transform restingPosition;
    [SerializeField] Transform foodShack;
    [SerializeField] Transform doorOnePosition;
    [SerializeField] Transform doorTwoPosition;


    [Header("States")]
    public float health = 100;
    public float stamina = 100;

    NavMeshAgent navMeshAgent;
    AnimationController animations;
    Rigidbody rb;

    CountdownTimer statsTimer;
    GameObject target;
    Vector3 destination;

    AgentGoal lastGoal;
    public AgentGoal currentGoal;
    public ActionPlan actionPlan;
    public AgentAction currentAction;

    public Dictionary<string, AgentState> states;
    public HashSet<AgentAction> actions;
    public HashSet<AgentGoal> goals;

    IGoapPlanner gPlanner;


    private void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animations = GetComponent<AnimationController>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        gPlanner = new GoapPlanner();

    }


    private void Start() {
        SetupTimers();
        SetupStates();
        SetupActions();
        SetupGoals();
    }


    void Update() {
        statsTimer.Tick(Time.deltaTime);
        animations.SetSpeed(navMeshAgent.velocity.magnitude);

        // Update the plan and current action if there is one
        if (currentAction == null) {
            Debug.Log("Calculating any potential new plan");
            CalculatePlan();

            if(actionPlan != null && actionPlan.Actions.Count > 0) {
                navMeshAgent.ResetPath();

                currentGoal = actionPlan.AgentGoal;
                Debug.Log($"Goal: {currentGoal.Name} with {actionPlan.Actions.Count} actions in Plan");

                currentAction = actionPlan.Actions.Pop();
                Debug.Log($"Popped Action: {currentAction.Name}");

                currentAction.Start();


            }
        }

        // If we have a current action, execute it
        if (actionPlan != null && currentAction != null) {
            currentAction.Update(Time.deltaTime);
            currentAction = null;

            if (currentAction.Complete) {
                Debug.Log($"{currentAction.Name} complete");
            }

            // Completed the Plan
            if (actionPlan.Actions.Count == 0) {
                Debug.Log("Plan complete");
                lastGoal = currentGoal;
                currentGoal = null;
            }
        }

    }


    void CalculatePlan() {
        float priorityLevel = currentGoal?.Priority ?? 0;

        HashSet<AgentGoal> goalsToCheck = goals;

        // If we have a current goal, we only want to check goals with higher priority
        if (currentGoal != null) {
            Debug.Log("Current goal exists, checking goals with higher priority");
            goalsToCheck = new HashSet<AgentGoal>(goals.Where(goal => goal.Priority > priorityLevel));
        }

        ActionPlan potetionalPlan = gPlanner.Plan(this, goalsToCheck, lastGoal);
        if (potetionalPlan != null) 
            actionPlan = potetionalPlan;
    }


    void SetupTimers() {
        statsTimer = new CountdownTimer(2f);
        statsTimer.OnTimerStop += () => {
            UpdateStats();
            statsTimer.Start();
        };
        statsTimer.Start();
    }


    // ================== MOVE ALL OF THIS SUCH THAT YOU CAN ADD THIS AS A LIST OF STATES, ACTIONS, AND GOALS !IN THE INSPECTOR! OR AS SIMPLE AddAction(...), AddState(...), AddGoal(...)
    void SetupStates() {
        states = new Dictionary<string, AgentState>();
        StateFactory factory = new StateFactory(this, states);

        factory.AddState("Nothing", () => false);
        factory.AddState("AgentIdle", () => !navMeshAgent.hasPath);
        factory.AddState("AgentMoving", () => navMeshAgent.hasPath);
        factory.AddState("AgentHealthLow", () => health < 30);
        factory.AddState("AgentHealthy", () => health >= 30);
        factory.AddState("AgentStaminaLow", () => stamina < 10);
        factory.AddState("AgentRested", () => stamina >= 50);


        factory.AddLocationState("AgentAtDoorOne", 3f, doorOnePosition);
        factory.AddLocationState("AgentAtDoorTwo", 3f, doorTwoPosition);
        factory.AddLocationState("AgentAtRestingPosition", 3f, restingPosition);
        factory.AddLocationState("AgentAtFoodShack", 3f, foodShack);


        factory.AddSensorState("PlayerInChaseRange", chaseSensor);
        factory.AddSensorState("PlayerInAttackRange", attackSensor);
        factory.AddState("AttackingPlayer", () => false); // Player can always be attacked, this will never become true





    }

    void SetupActions() {
        actions = new HashSet<AgentAction>();

        actions.Add(new AgentAction.Builder("Relax")
            .WithStrategy(new IdleStrategy(5))
            .AddEffect(states["Nothing"])
            .Build()
            );

        actions.Add(new AgentAction.Builder("Wander")
            .WithStrategy(new WanderStrategy(navMeshAgent, 10))
            .AddEffect(states["AgentMoving"])
            .Build()
            );

        actions.Add(new AgentAction.Builder("MoveToEatingPosition")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => foodShack.position))
            .AddEffect(states["AgentAtFoodShack"])
            .Build()
            );

        actions.Add(new AgentAction.Builder("Eat")
            .WithStrategy(new IdleStrategy(5)) // Later replace w command
            .AddPrecondition(states["AgentAtFoodShack"])
            .AddEffect(states["AgentAtFoodShack"])
            .Build()
            );

        actions.Add(new AgentAction.Builder("MoveToDoorOne")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => doorOnePosition.position))
            .AddEffect(states["AgentAtDoorOne"])
            .Build()
            );

        actions.Add(new AgentAction.Builder("MoveToDoorTwo")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => doorTwoPosition.position))
            .AddEffect(states["AgentAtDoorTwo"])
            .Build()
            );

        actions.Add(new AgentAction.Builder("MoveFromD1ToRestArea")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position))
            .AddPrecondition(states["AgentAtDoorOne"])
            .AddEffect(states["AgentAtRestingPosition"])
            .Build()
            );

        actions.Add(new AgentAction.Builder("MoveFromD2ToRestArea")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position))
            .WithCost(2)
            .AddPrecondition(states["AgentAtDoorTwo"])
            .AddEffect(states["AgentAtRestingPosition"])
            .Build()
            );

        actions.Add(new AgentAction.Builder("Rest")
            .WithStrategy(new IdleStrategy(5))
            .AddPrecondition(states["AgentAtRestingPosition"])
            .AddEffect(states["AgentIsRested"])
            .Build()
            );

        actions.Add(new AgentAction.Builder("ChasePlayer")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => states["PlayerInChaseRange"].Location))
            .AddPrecondition(states["PlayerInChaseRange"])
            .AddEffect(states["PlayerInAttackRange"])
            .Build()
            );

        actions.Add(new AgentAction.Builder("AttackPlayer")
            .WithStrategy(new AttackStrategy(animations))
            .AddPrecondition(states["PlayerInAttackRange"])
            .AddEffect(states["AttackingPlayer"])
            .Build()
            );

    }



    void SetupGoals() {
        goals = new HashSet<AgentGoal>();

        goals.Add(new AgentGoal.Builder("ChillOut")
            .WithPriority(1)
            .WithDesiredEffects(states["Nothing"])
            .Build()
            );

        goals.Add(new AgentGoal.Builder("Wander")
            .WithPriority(1)
            .WithDesiredEffects(states["AgentMoving"])
            .Build()
            );

        goals.Add(new AgentGoal.Builder("KeepHealthUp")
            .WithPriority(2)
            .WithDesiredEffects(states["AgentIsHealthy"])
            .Build()
            );

        goals.Add(new AgentGoal.Builder("KeepStaminaUp")
            .WithPriority(2)
            .WithDesiredEffects(states["AgentIsRested"])
            .Build()
            );

        goals.Add(new AgentGoal.Builder("SeekAndDestroy")
            .WithPriority(2)
            .WithDesiredEffects(states["AttackingPlayer"])
            .Build()
            );

    }

    // ===================================================================================================

    // TODO move into stats system
    void UpdateStats() {
        stamina += InRangeOf(restingPosition.position, 3f) ? 20 : -10;
        health += InRangeOf(foodShack.position, 3f) ? 20 : -5;
        stamina = Mathf.Clamp(stamina, 0, 100);
        health = Mathf.Clamp(health, 0, 100);
    }

    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(transform.position, pos) < range;


    void OnEnable() => chaseSensor.OnTargetChanged += HandleTargetChanged;
    void OnDisable() => chaseSensor.OnTargetChanged -= HandleTargetChanged;

    void HandleTargetChanged() {
        Debug.Log("Target changed, clearing current action and goal");
        // Force planner to re evaluate plan
        currentAction = null;
        currentGoal = null;
    }


}
