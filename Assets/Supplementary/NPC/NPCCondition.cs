//using FIMSpace.FProceduralAnimation;
using UnityEngine;

public class NPCCondition : SneedSpace  {

    [Space(20)]
    [Header("Current Condition")]
    [SerializeField] private MovementCondition movementCondition = MovementCondition.Normal;
    [SerializeField] private float movementSpeedMultiplier = 1f;

    [Space]
    [Header("Injuries")]
    [SerializeField] private bool leftLegHit;
    [SerializeField] private bool rightLegHit;

    [Space]
    [Header("Injury Speeds")]
    [SerializeField] private float crawlSpeedMultiplier = 0.25f;

    //private LegsAnimator legs;
    private NPCAnimatorController animatorController;
    private NPCItemHolder itemHolder;
    private NPCBodyPartHandler dismember;


    public MovementCondition MovementCondition => movementCondition;
    public float MovementSpeedMultiplier => movementSpeedMultiplier;
    public int LocomotionStyle => (int)movementCondition;
    public bool BlocksMovement => movementCondition == MovementCondition.Stunned;

    public bool LeftLegHit => leftLegHit;
    public bool RightLegHit => rightLegHit;
    public bool BothLegsHit => leftLegHit && rightLegHit;

    private void Awake() {
        //legs = GetComponent<LegsAnimator>();
        animatorController = GetComponent<NPCAnimatorController>();
        itemHolder = GetComponent<NPCItemHolder>();
    }

    public void ApplyBodyPartCondition(BodyPart part, BodyPartProfile profile) {
        if(profile == null) {
            return;
        }

        if(part == BodyPart.LegLeft) {
            leftLegHit = true;
        }

        if(part == BodyPart.LegRight) {
            rightLegHit = true;
        }

        if(BothLegsHit) {
            //if(legs != null) legs.enabled = false;

            itemHolder?.DropHeldItem();
            movementCondition = MovementCondition.Crawl;
            movementSpeedMultiplier = crawlSpeedMultiplier;
            PrintMessage($"both legs hit: {BothLegsHit}");
            return;
        }

        movementCondition = profile.conditionOnHit;
        movementSpeedMultiplier = profile.movementSpeedMultiplier;
        PrintMessage($"reached profile {profile.name}, condition: {movementCondition.ToString()}, speedmult = {movementSpeedMultiplier}");
        




        //::::::::::::::::::::::::::::::::::::::::::::::::::
        //:: todo:                                        ::
        //:: - start duration timer                       ::
        //:: - stack injuries                             ::
        //:: - ignore weaker conditions                   ::
        //:: - apply left/right-specific animation style  ::
        //::::::::::::::::::::::::::::::::::::::::::::::::::
    }

    public void ClearBodyPartCondition() {
        movementCondition = MovementCondition.Normal;
        movementSpeedMultiplier = 1f;

        leftLegHit = false;
        rightLegHit = false;
    }
}
