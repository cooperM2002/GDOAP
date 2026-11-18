using UnityEngine;

public class NPCCombat : SneedSpace
{
    [Header("Refs")]
    [SerializeField] private NPCVision vision;
    [SerializeField] private NPCItemHolder itemHolder;
    [SerializeField] private NPCBehaviourController behaviour;
    private bool weaponReady;

    [Header("Firing")]
    [SerializeField] private float fireInterval = 0.6f;

    private GameObject currentItem;
    private NPCHitscan hitscan;
    private float nextFireTime;
    NpcController npc;


    //state == combat, and hasItem == true

    private void Awake() {
        npc = GetComponent<NpcController>();
        vision = GetComponent<NPCVision>();
        itemHolder = GetComponent<NPCItemHolder>();
        behaviour = GetComponent<NPCBehaviourController>();
        weaponReady = false;

    }

    private void Update() {
        TryFire();
    }

    private void TryFire() {
        if(behaviour == null || behaviour.State != BehaviourState.Combat) { return; }
        if(vision == null || !vision.HasTarget || !vision.IsTargetVisible) { return; }
        if(itemHolder == null || !itemHolder.HasItem || itemHolder.CurrentItem == null) {
            ClearWeapon();
            return;
        }

        RefreshWeaponReference();
        if(hitscan == null) return;
        if(Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireInterval;
        //if(weaponReady==true) 
        hitscan.FireWeapon(vision.CurrentAimPoint);
    }

    private void RefreshWeaponReference() {
        GameObject heldItem = itemHolder.CurrentItem;

        if(heldItem == currentItem)
            return;

        currentItem = heldItem;
        hitscan = currentItem.GetComponent<NPCHitscan>();

        if(hitscan == null) {
            PrintError($"{currentItem.name} has no NPCHitscanBehaviour.");
        }

    }

    private void ClearWeapon() {
        currentItem = null;
        hitscan = null;
        weaponReady = false;
    }

    public void BeginCanFire() {
        weaponReady = true;
        PrintMessage("BeginCanFire");
    }

    //private void Update() {
    //    GetCurrentWeapon();
    //}

    //private void GetCurrentWeapon() {
    //    //if(itemHolder.HasItem==false) {
    //    //    return;
    //    //}
    //    //currentItem = itemHolder.CurrentItem;
    //    //if(currentItem == null) {
    //    //    PrintError("no item");
    //    //    return;
    //    //}
    //    //hitscan = currentItem.GetComponent<NPCHitscanBehaviour>();
    //    //if(hitscan == null) {
    //    //    PrintError("no hitscan");
    //    //}

    //    //hitscan.FireWeapon(pistolAimTransform);
    //}

}
