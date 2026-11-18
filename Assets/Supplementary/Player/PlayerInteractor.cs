using UnityEngine;

public class PlayerInteractor : SneedSpace {

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 2.0f;

    private Camera aimCamera;
    private Crosshair crosshair;
    private ItemInteractable hovered;
    private PlayerControls config;
    private PlayerConfig controls;
    private PlayerItemGrabber grab;


    private void Awake() {
        config = GetComponentInParent<PlayerControls>();
        controls = config.Controls;
        grab = GetComponent<PlayerItemGrabber>();

        ResolveAimReferences();
    }


    void Update() {
        if (Input.GetKeyDown(controls.itemInteractKey)) 
            AttemptInteraction();
    }


    private bool ResolveAimReferences() {
        if (aimCamera == null) aimCamera = Camera.main;
        if (aimCamera == null) {
            PrintError("Interacting: no camera tagged MainCamera found.");
            return false;
        }

        if (crosshair == null) crosshair = aimCamera.GetComponent<Crosshair>();
        if (crosshair == null) {
            PrintError("Interacting: Main Camera has no Crosshair component.");
            return false;
        }

        return true;
    }


    private void AttemptInteraction() {
        RaycastHit hitInfo;
        bool hitSomething = TryGetHit(interactionRange, out hitInfo);

        // perform raycast
        if (hitSomething){
            InteractableObject interactable = hitInfo.collider.GetComponent<InteractableObject>();
            if (interactable != null) 
                interactable.Interact(gameObject); //it has been interacted with
        }
    }


    /// <summary>
    /// Lives on the Main Camera, performs check raycasts with a player mask. For interactions.
    /// </summary>
    /// <returns>If raycast was successful.</returns>
    public bool TryGetHit(float range, out RaycastHit hit) {
        hit = default;
        if (!ResolveAimReferences()) 
            return false;              

        return Physics.Raycast(
                aimCamera.ScreenPointToRay(crosshair.AimScreenPosition),
                out hit,    
                range,      
                Physics.DefaultRaycastLayers & ~(1 << LayerMask.NameToLayer("Player"))  // everything EXCEPT player
            ); ;
    }


}
