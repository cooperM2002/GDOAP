using UnityEngine;

public class NPCArmAimIK : SneedSpace
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NPCVision vision;
    [SerializeField] private NPCItemHolder itemHolder;
    [SerializeField] private NPCBehaviourController behaviour;

    [Header("Aim Rig")]
    [SerializeField] private Transform aimRigRoot;

    [Header("Left Arm")]
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform leftElbowHint;
    [SerializeField] private Vector3 leftHandRotationOffset;
    [SerializeField, Range(0f, 1f)] private float leftHandMaximumWeight = 1f;

    [Header("Right Arm")]
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private Transform rightElbowHint;
    [SerializeField] private Vector3 rightHandRotationOffset;
    [SerializeField, Range(0f, 1f)] private float rightHandMaximumWeight = 1f;

    [Header("Aiming")]
    [SerializeField] private float rotationSmoothSpeed = 15f;

    [Header("Blending")]
    [SerializeField] private float weightBlendSpeed = 8f;

    [Tooltip("UpperBody is normally layer 1.")]
    [SerializeField] private int ikLayerIndex = 1;

    private float currentLeftWeight;
    private float currentRightWeight;

    private Quaternion smoothedAimRotation;

    private void Awake() {
        if(animator == null)
            animator = GetComponent<Animator>();

        if(vision == null)
            vision = GetComponent<NPCVision>();

        if(itemHolder == null)
            itemHolder = GetComponent<NPCItemHolder>();

        if(behaviour == null)
            behaviour = GetComponent<NPCBehaviourController>();

        if(aimRigRoot != null)
            smoothedAimRotation = aimRigRoot.rotation;
        else
            smoothedAimRotation = transform.rotation;
    }

    private void OnAnimatorIK(int layerIndex) {
        if(layerIndex != ikLayerIndex)
            return;

        if(animator == null)
            return;

        bool shouldAim =
            vision != null &&
            vision.HasTarget &&
            vision.IsTargetVisible &&
            itemHolder != null &&
            itemHolder.HasItem &&
            behaviour != null &&
            behaviour.State == BehaviourState.Combat;

        UpdateWeights(shouldAim);
        ApplyIKWeights();

        if(!shouldAim)
            return;

        if(aimRigRoot == null)
            return;

        RotateAimRig();

        ApplyLeftArmIK();
        ApplyRightArmIK();
    }

    private void UpdateWeights(bool shouldAim) {

        bool useLeft =
            shouldAim && itemHolder.WeaponHand == NPCWeaponHand.Left;

        bool useRight =
            shouldAim && itemHolder.WeaponHand == NPCWeaponHand.Right;

        float targetLeftWeight =
            shouldAim ? leftHandMaximumWeight : 0f;

        float targetRightWeight =
            shouldAim ? rightHandMaximumWeight : 0f;

        currentLeftWeight = Mathf.MoveTowards(
            currentLeftWeight,
            targetLeftWeight,
            weightBlendSpeed * Time.deltaTime
        );

        currentRightWeight = Mathf.MoveTowards(
            currentRightWeight,
            targetRightWeight,
            weightBlendSpeed * Time.deltaTime
        );
    }

    private void ApplyIKWeights() {
        // Left arm
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, currentLeftWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, currentLeftWeight);
        animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, currentLeftWeight);

        // Right arm
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, currentRightWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, currentRightWeight);
        animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, currentRightWeight);
    }

    private void RotateAimRig() {
        Vector3 aimDirection =
            vision.CurrentAimPoint - aimRigRoot.position;

        if(aimDirection.sqrMagnitude < 0.0001f)
            return;

        Quaternion desiredRotation = Quaternion.LookRotation(
            aimDirection.normalized,
            transform.up
        );

        float smoothing =
            1f - Mathf.Exp(-rotationSmoothSpeed * Time.deltaTime);

        smoothedAimRotation = Quaternion.Slerp(
            smoothedAimRotation,
            desiredRotation,
            smoothing
        );

        aimRigRoot.rotation = smoothedAimRotation;
    }

    private void ApplyLeftArmIK() {
        if(currentLeftWeight <= 0.001f)
            return;

        if(leftHandTarget != null) {
            animator.SetIKPosition(AvatarIKGoal.LeftHand,leftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand,leftHandTarget.rotation *Quaternion.Euler(leftHandRotationOffset));
        }

        if(leftElbowHint != null) {
            animator.SetIKHintPosition(AvatarIKHint.LeftElbow,leftElbowHint.position);
        }
    }

    private void ApplyRightArmIK() {
        if(currentRightWeight <= 0.001f)
            return;

        if(rightHandTarget != null) {
            animator.SetIKPosition(AvatarIKGoal.RightHand,rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand,rightHandTarget.rotation *Quaternion.Euler(rightHandRotationOffset));
        }

        if(rightElbowHint != null) {
            animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowHint.position);
        }
    }
}