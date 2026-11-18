using UnityEngine;

public class PlayerWeaponHandler : SneedSpace {

    [SerializeField] private ItemController itemController;
    //[SerializeField] private WeaponVisuals weaponVisuals;

    private GameObject cachedItemObject;
    //private WeaponController currentWeapon;
    private PrimaryUseBehaviour currentPrimaryUse;

    private void Awake() {
        //if(weaponVisuals==null) weaponVisuals = GetComponentInChildren<WeaponVisuals>();
        if(itemController == null) itemController = GetComponent<ItemController>();
        
    }


    void Update() {
        bool leftPressed = Input.GetMouseButtonDown(0);

        //holster
        if(Input.GetKeyDown(KeyCode.H)) {
            PrintMessage("Pressed H");
            ToggleHolsterVisual();
        }

        // && itemController != null && itemController.IsEquipped
        //if(leftPressed) {
        //    weaponVisuals.PlayFire();
        //}

    }

    //private void RefreshCurrentVisuals() {
    //    if(itemController == null)
    //        return;

    //    GameObject currentItem = itemController.CurrentItemObject;

    //    if(currentItem == cachedItemObject)
    //        return;

    //    cachedItemObject = currentItem;
    //    currentVisuals = null;

    //    if(currentItem != null) {
    //        currentVisuals = currentItem.GetComponentInChildren<WeaponVisuals>(true);

    //        if(currentVisuals != null)
    //            PrintMessage($"Bound WeaponVisuals to: {currentVisuals.name}");
    //        else
    //            PrintMessage("Current item has no WeaponVisuals.");
    //    }
    //}

    private void ToggleHolsterVisual() {
        if(itemController == null) return;
        if(!itemController.HasItem()) return;

        itemController.ToggleHolster();

        //if(weaponVisuals != null)
            //weaponVisuals.SetHolsteredVisual(itemController.IsHolstered);
    }

    //private void ToggleHolsterVisual() {
    //    if(itemController == null)
    //        return;

    //    if(!itemController.HasItem())
    //        return;

    //    itemController.ToggleHolster();

    //    RefreshCurrentVisuals();

    //    if(currentVisuals != null)
    //        currentVisuals.SetHolsteredVisual(itemController.IsHolstered);
    //}

    //private void TryPlayFireVisual() {
    //    if(itemController == null)
    //        return;

    //    if(!itemController.IsEquipped)
    //        return;

    //    RefreshCurrentVisuals();

    //    if(currentVisuals != null)
    //        currentVisuals.PlayFire();
    //}


}
