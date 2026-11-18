using UnityEngine;

/// <summary>
/// Manages single equipped/held item slot
/// </summary>
public class ItemController : SneedSpace {

    //========//
    // fields //
    //========//

    #region fields

    [Space(20)]
    [Header("Hold / Aim")]
    [SerializeField] private Transform itemHoldRoot; // position on player of held item
    [SerializeField] private Transform itemThrowRoot; // throw position of held item
    [SerializeField] private Transform aimTransform; // main camera, inherits both pitch and yaw

    [Header("Drop / Throw")]
    [SerializeField] private float throwForce = 300f;
    [SerializeField] float minSpin = 1f;
    [SerializeField] float maxSpin = 3f;

    [Header("Holster")]
    [SerializeField] private bool startHolstered = false; // holster start state
    
    public LayerMask rayMask = ~0;
    private bool isHolstered = false;

    private PlayerControls config;
    private PlayerConfig controls;
    private PlayerMovement motor;
    private ItemDefinition ItemDef;
    private ItemInstance currentInstance;
    private GameObject itemCurrent;

    public ItemDefinition CurrentItemDef => ItemDef;
    public GameObject CurrentItemObject => itemCurrent;
    public ItemInstance CurrentItemInstance => currentInstance;
    public bool IsHolstered =>  isHolstered; // && HasItem()
    public bool IsEquipped => HasItem() && !IsHolstered;



    ///////////
    // item use 

    private PlayerMovement playerMotor;
    private bool isAutomatic;
    private float rateOfFire;
    private float nextFireTime = 0f;


    #endregion


    //===========//
    // main loop //
    //===========//

    #region mainloop

    private void Awake() {
        config = GetComponentInParent<PlayerControls>();
        controls = config.Controls;

        if(playerMotor == null) playerMotor = GetComponentInParent<PlayerMovement>();
        if(itemHoldRoot == null) PrintMessage("itemHoldRoot must be assigned");
        if (itemThrowRoot == null) PrintMessage("itemThrowRoot must be assigned");
        if (aimTransform == null) {
            Camera cam = Camera.main;
            if (cam != null) aimTransform = cam.transform;
        }

        motor = GetComponent<PlayerMovement>();
        if (motor == null) PrintMessage("motor is null");
        
        isHolstered = startHolstered;
    }


    private void Update() {

        //if(isWeapon) {
        if (isAutomatic) {
            if (Input.GetMouseButton(0)) {
                if (Time.time >= nextFireTime) {
                    TryPrimaryUse();
                    nextFireTime = Time.time + rateOfFire;
                }
            }

        } else {
            if (Input.GetMouseButtonDown(0)) {
                if (Time.time >= nextFireTime) {
                    TryPrimaryUse();
                    nextFireTime = Time.time + rateOfFire;
                }
            }

        }

        // throw
        if (Input.GetKeyDown(controls.dropKey) && HasItem())
            ThrowItem();
    }


    #endregion

    //============//
    // controller //
    //============//

    #region controller

    private void TryPrimaryUse() {
        //if(!IsEquipped) {
        //    PrintMessage($"isEquiped: {IsEquipped}, click ignored, nothing equipped");
        //    return;
        //}

    }

    #endregion

    //=================//
    // item operations //
    //=================//

    #region item ops

    public bool HasItem() {
        if (ItemDef != null) return true;
        return false;
    }


    public bool TryPickupItem(ItemDefinition source, ItemInstance incomingInstance = null) {
        if (source == null) return false;
        if (HasItem()) ThrowItem();

        currentInstance = incomingInstance;
        EquipItem(source, incomingInstance);

        return true;
    }


    public void EquipItem(ItemDefinition data, ItemInstance incomingInstance = null) {
        if (data == null) return;

        if (itemHoldRoot == null) {
            PrintMessage("cannot equip, itemHoldRoot is null");
            return;
        }

        if (data.PlayerPrefab == null) {
            PrintMessage("cannot equip, PlayerPrefab is null on ItemDefinition");
            return;
        }

        ClearItem();
        ItemDef = data;

        //create fresh instance
        currentInstance = incomingInstance;
        if(currentInstance == null) {
            currentInstance = data.CreateDefaultInstance();
            PrintMessage($"Created default instance for {data.ItemName}");
        }

        itemCurrent = Instantiate(ItemDef.PlayerPrefab, itemHoldRoot);

        ItemInstanceCarrier carrier = itemCurrent.GetComponent<ItemInstanceCarrier>();
        if(carrier == null) {
            PrintMessage($"No InstanceCarrier for this instance: {data.ItemName}");
        } else {
            carrier.Instance = currentInstance;
        }

        isHolstered = startHolstered;//set holster state
    }


    #endregion


    //============//
    // Drop/Throw //
    //============//
    #region drop/throw

    void ThrowItem() {
        if (ItemDef == null) return;

        Vector3 rootPos = itemThrowRoot.position;
        GameObject worldObject = Instantiate(ItemDef.WorldPrefab, rootPos, Quaternion.identity);
        ItemInstanceCarrier worldCarrier = worldObject.GetComponent<ItemInstanceCarrier>();

        if (worldCarrier != null) {
            worldCarrier.Instance = currentInstance;
        } else {
            PrintMessage($"Carrier null on {ItemDef.ItemName}");
        }

        Rigidbody thrownBody = worldObject.GetComponent<Rigidbody>();

        if (thrownBody == null){ 
            ClearItem(); 
            return; 
        }

        //force,rotation when dropped
        MoveItem(thrownBody);
        DropItem(worldObject);
    }

    void MoveItem(Rigidbody thrownBody) {

        Vector3 aimForward  = aimTransform != null ? aimTransform.forward  : transform.forward;
        Vector3 aimPos      = aimTransform != null ? aimTransform.position : transform.position;
        Vector3 throwOrigin = itemHoldRoot.position;
        Vector3 randomAxis  = Random.onUnitSphere;

        float randomTorque = Random.Range(minSpin, maxSpin);
        float aimForce = motor.Velocity.z * .2f;

        //forward ray
        Ray ray =  new Ray(aimPos, aimForward);
        bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, fbd, rayMask);

        Vector3 aimPoint = hit ? hitInfo.point : (aimTransform.position + aimTransform.forward * fbd);
        Vector3 throwDir = (aimPoint - throwOrigin).normalized;
        Vector3 force    = throwDir * throwForce;//forward * throwForce
        Vector3 torque   = aimForward*randomTorque;//randomAxis *
                                                 
        thrownBody.AddForce(force, ForceMode.Impulse);
        thrownBody.AddTorque(torque, ForceMode.Impulse);

    }


    void DropItem(GameObject worldObject) {
        if (ItemDef == null) return;
        // spawn world object (no force)
        //GameObject worldObject = Instantiate(ItemDef.WorldPrefab, itemHoldRoot.position, Quaternion.identity);

        ////re - enable collisions(same idea as your Grabber)
        //Collider worldCol = worldObject.GetComponent<Collider>();
        //if(worldCol != null) {
        //    foreach(Collider c in GetComponentsInParent<Collider>()) {
        //        Physics.IgnoreCollision(c, worldCol, false);
        //    }
        //}
        ClearItem();
    }


    void ClearItem() {
        if (itemCurrent != null) {
            Destroy(itemCurrent);
            itemCurrent = null;
        }

        ItemDef = null;
        currentInstance = null; 
    }


    #endregion


    //=========//
    // Holster //
    //=========//
    #region holster


    public void ToggleHolster() {
        if (!HasItem()) {
            PrintMessage("Holster toggle ignored (no item equipped)");
            return;
        }

        isHolstered = !isHolstered;
        PrintMessage($"Holster is now: {isHolstered}");
    }


    public void SetHolstered(bool holstered) {
        if(!HasItem()) {
            isHolstered = false; // safe default
            return;
        }

        isHolstered = holstered;
    }


    #endregion


    //=======//
    // debug //
    //=======//
    #region debug

    public float hitSphereRadius = 0.08f;
    public float fbd = 200f;

    void OnDrawGizmosSelected()
    {
        Vector3 aimPoint;
        Transform aim = aimTransform.transform;
        Vector3 throwOrigin = itemHoldRoot.position;

        Ray ray = new Ray(aim.position, aim.forward);
        bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, fbd, rayMask);

        if (hit) {
            aimPoint = hitInfo.point;
        } else {
            aimPoint = aim.position + aim.forward * fbd;
        }

        //aim ray, where player is looking
        Gizmos.color = hit ? Color.red : Color.yellow;  
        Gizmos.DrawLine(aim.position, aimPoint);
        Gizmos.DrawWireSphere(aimPoint, hitSphereRadius);   //hit or fallback point

        //throw direction from origin to aim point
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(throwOrigin, aimPoint);
        Gizmos.DrawWireSphere(throwOrigin, hitSphereRadius);
    }


    #endregion


}
