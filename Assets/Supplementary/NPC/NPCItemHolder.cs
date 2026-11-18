using UnityEngine;

public class NPCItemHolder : SneedSpace {

    //::::::::::::
    //:: Fields ::
    //::::::::::::
    #region fields

    [Space(20)]
    [Header("Hand References")]
    [SerializeField] private Transform handMountLeft;
    [SerializeField] private Vector3 heldItemRotationLeft;
    [SerializeField] private Transform handMountRight;
    [SerializeField] private Vector3 heldItemRotationRight;
    [SerializeField] private LayerMask itemMask;

    [Space]
    [Header("Item Search")]
    [SerializeField] private float searchRadius = 8f;
    [SerializeField] private float pickupDistance = 0.1f;

    private NPCBodyPartHandler dismember;
    private NPCAnimatorController animatorController;
    private NPCBehaviourController behaviour;
    private NPCMovement movement;
    private NPCItemPickup targetItem;
    private NPCItemPickup heldItem;
    private GameObject currentItem;
    private NpcController npc;

    private bool isPickingUp;
    private readonly Collider[] itemHits = new Collider[16];//item collider buffer
    private NPCWeaponHand weaponHand = NPCWeaponHand.Left;

    public NPCWeaponHand WeaponHand => weaponHand;
    public bool HasItem => heldItem != null;
    public bool HasItemTarget => targetItem != null;
    public Transform TargetItemTransform => targetItem != null ? targetItem.transform : null;
    public GameObject CurrentItem => currentItem;


    private void Awake() {
        dismember = GetComponent<NPCBodyPartHandler>();   
        npc = GetComponent<NpcController>();
        behaviour = GetComponent<NPCBehaviourController>();
        movement = GetComponent<NPCMovement>();
        animatorController = GetComponent<NPCAnimatorController>();
        //animatorController?.SetHasItem(false);
    }


    #endregion


    public bool TryHandleItem() {
        if(npc.State != LifeState.Alive) {
            return false;
        }


        if (isPickingUp) return true;
        if (HasItem) return false;
        if(!TryChooseWeaponHand()) return false;
        if (targetItem == null) FindNearestItem();
        if (targetItem == null) return false;

        movement.MoveToItem(targetItem.transform);

        //to - origin
        Vector3 difference = targetItem.transform.position - transform.position;

        if(difference.sqrMagnitude <= pickupDistance*pickupDistance) {
            PrintMessage("Beggining Pickup");
            BeginPickup();
        }

        return true;
    }


    private void FindNearestItem() {

        //fill item buffer
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            searchRadius,
            itemHits,
            itemMask
        );

        float bestDistance = float.PositiveInfinity;
        NPCItemPickup nearestItem = null;

        //go through each item, see if its a NPCItemPickup item
        for(int i = 0; i < count; i++) {

            Collider hit = itemHits[i];
            if(hit == null) continue;

            NPCItemPickup item = hit.GetComponent<NPCItemPickup>();
            if(item == null) continue;
            if(item.InUse) continue;

            //dist to item
            float distance = (item.transform.position - transform.position).sqrMagnitude;

            if(distance < bestDistance) {
                bestDistance = distance;
                nearestItem = item;
            }

        }

        if(nearestItem == null) return;

        //reserve item before this NPC begins walking toward it
        if(nearestItem.TryReserve(this)) {
            targetItem = nearestItem;
        }

    }


    private bool TryChooseWeaponHand() {

        if(dismember == null)
            return false;

        if(dismember.RightHand) {
            weaponHand = NPCWeaponHand.Right;
            return true;
        }

        if(dismember.LeftHand) {
            weaponHand = NPCWeaponHand.Left;
            return true;
        }

        return false;
    }


    private void BeginPickup() {
        if(isPickingUp)
            return;

        isPickingUp = true;
        movement.StopMovement();
        animatorController?.SetWeaponHand(weaponHand);
        animatorController?.PlayPickup();//CALLS OnPickupBegin

        PrintMessage("Picking...");
    }


    /// <summary>
    /// animation event fired when hand reaches the item
    /// </summary>
    public void OnPickupBegin() {
        PrintMessage("Entered animation");
        PickupTargetItem();
        isPickingUp = false;
    }


    private void PickupTargetItem() {
        if(targetItem == null || handMountLeft == null) {
            return;
        }

        // the target is now held
        heldItem = targetItem;
        targetItem = null;

        PrintMessage($"{heldItem.CurrentWeaponType}");
        currentItem = heldItem.gameObject;

        // set position in hand
        Transform handMount =
            weaponHand == NPCWeaponHand.Left
                ? handMountLeft
                : handMountRight;

        Vector3 handRotation =
            weaponHand == NPCWeaponHand.Left
                ? heldItemRotationLeft
                : heldItemRotationRight;


        Transform itemTransform = heldItem.transform;
        itemTransform.SetParent(handMount);
        itemTransform.localPosition = Vector3.zero;
        itemTransform.localRotation = Quaternion.Euler(handRotation);

        // set rb and colliders to not interfere with parent
        Rigidbody rb = heldItem.GetComponent<Rigidbody>();

        if(rb != null) {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Collider[] colliders = heldItem.GetComponentsInChildren<Collider>();

        foreach(Collider itemCollider in colliders) {
            itemCollider.enabled = false;
        }

        animatorController?.SetHasItem(true);
    }


    public void DropHeldItem() {
        if(heldItem == null) {
            return;
        }

        Transform itemTransform = heldItem.transform;

        // Keep its current world position when unparenting.
        itemTransform.SetParent(null, true);

        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        if(rb != null) {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Collider[] colliders = heldItem.GetComponentsInChildren<Collider>();
        foreach(Collider itemCollider in colliders) {
            itemCollider.enabled = true;
        }

        //IMPORTANT
        //available for pickup again
        heldItem.Release(this);
        heldItem = null;
        currentItem = null;
        animatorController?.SetHasItem(false);

    }



}
