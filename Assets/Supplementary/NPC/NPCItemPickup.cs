using UnityEngine;

public class NPCItemPickup : MonoBehaviour {

    [SerializeField] private NPCWeaponType currentWeaponType;

    public NPCWeaponType CurrentWeaponType => currentWeaponType;
    private NPCItemHolder reservedBy;
    public bool InUse => reservedBy != null;


    public bool IsReservedBy(NPCItemHolder holder) {
        return reservedBy == holder;
    }


    public bool TryReserve(NPCItemHolder holder) {
        if (holder == null)
            return false;

        // Someone else already owns/reserved it.
        if (reservedBy != null && reservedBy != holder)
            return false;

        reservedBy = holder;
        return true;
    }


    public void Release(NPCItemHolder holder) {
        // Only the NPC that reserved it can release it.
        if (reservedBy == holder)
            reservedBy = null;
    }


    private void GetCurrentWeaponType() {
        
    }

}
