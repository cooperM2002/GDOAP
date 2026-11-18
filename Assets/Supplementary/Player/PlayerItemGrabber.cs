using UnityEngine;
using UnityEngine.Animations;

//TODO: if object is ItemPickup/ItemDefinition item => hold E -> grab
//TODO: if object is static/prop prefab => ie normal rigidbody -> normal logic


public class PlayerItemGrabber : SneedSpace {

    //========//
    // fields //
    //========//

    #region fields

    [Space(20)]
    [Header ("Object References")]
    [SerializeField] private PlayerInteractor interacting; // interaction ray
    [SerializeField] private Transform referenceHoldPoint; //item sits here
    [SerializeField] private Rigidbody physicsHoldRb; // unparented, kinematic

    [Space]
    [Header("Interaction")]
    [SerializeField] private float grabRange = 3f;
    [SerializeField] private float pullRange = 20f;

    [Space]
    [Header("Holding")]
    [SerializeField] private float throwForce = 200f;
    [SerializeField] private float pullForce = 50f;
    [SerializeField] private float grabBreakForce = 100f;
    [SerializeField] private float grabBreakTorque = 100f;

    private PlayerControls config;
    private PlayerConfig controls;
    private FixedJoint connectionJoint;
    private ConfigurableJoint attachJoint;
    private Rigidbody grabbedBody;      
    private bool isHolding;
    private bool grabPressed;
    private bool throwPressed;

    public bool IsGrabbing => isHolding;

    #endregion


    //===========//
    // main loop //
    //===========//

    #region mainloop

    private void Awake() {
        config = GetComponentInParent<PlayerControls>();
        controls = config.Controls;
        
        if (controls == null) PrintMessage("No control config found");

        if (referenceHoldPoint == null) PrintMessage("Grab hold point must not be null!");

        if (physicsHoldRb != null) {
            physicsHoldRb.transform.SetParent(null, true);
            physicsHoldRb.isKinematic = true;
            physicsHoldRb.useGravity = false;
            physicsHoldRb.detectCollisions = false;
        }

        //not attatched
        if (referenceHoldPoint.IsChildOf(transform) == false) 
            PrintMessage("Grab hold point must be a child of this object");
        
        Collider playerCollider = GetComponentInParent<Collider>();//players collider
        playerCollider.gameObject.layer = LayerMask.NameToLayer("Player");//Debug.Log(playerCollider.gameObject.name+"");

        isHolding = false;
    }


    private void Update() {
        //if(Input.GetKeyDown(grabKey))
        //    grabPressed = true;
        if (Input.GetKeyDown(controls.interactKey))
            grabPressed = true;

        if (Input.GetKeyDown(controls.throwKey))
            throwPressed = true;
    }


    private void FixedUpdate() {
        FollowReferenceHoldPoint();

        //keep holding state based on the actual grabbed body
        isHolding = grabbedBody != null;

        if (grabPressed) {
            grabPressed = false;
            
            if(!isHolding) {
                AttemptPull();
            } else {
                Drop();
            }
            
            isHolding = grabbedBody != null;
        }
        if (throwPressed) {
            throwPressed = false;

            if (isHolding) 
                Throw();
            
            isHolding = grabbedBody != null;
        }
    }

    #endregion


    //=================//
    // item hold logic //
    //=================//

    #region item hold logic

    private void FollowReferenceHoldPoint() {
        if (referenceHoldPoint == null || physicsHoldRb == null) return;

        physicsHoldRb.MovePosition(referenceHoldPoint.position);

        // follow only world Y rotation ignore camera pitch/roll
        float yaw = referenceHoldPoint.eulerAngles.y;
        float yaw_z = referenceHoldPoint.eulerAngles.normalized.z;

        Quaternion flatRotation = Quaternion.Euler(0f, yaw, yaw_z);
        physicsHoldRb.MoveRotation(flatRotation);
    }
    /// <summary>
    /// attempts to pull not held object 
    /// performs raycast through middle of camera,
    /// if the raycast hits a rigidbody that is not kinematic, pick its up
    /// </summary>
    void AttemptPull() {
        RaycastHit hitInfo;
        bool hitSomething = interacting.TryGetHit(grabRange, out hitInfo);
        if(hitSomething == false) return;//hit empty

        Rigidbody rb = hitInfo.rigidbody;
        if(rb == null) return;
        if(rb.isKinematic) return;
        grabbedBody = rb;

        //========================//
        // GRAB logic starts HERE //
        //========================//

        //within pull range
        if(hitInfo.distance < pullRange) {
            if(attachJoint != null) return;

            //kill motion
            grabbedBody.linearVelocity = Vector3.zero;
            grabbedBody.angularVelocity = Vector3.zero;

            //configurable joint on grabbed object
            attachJoint = grabbedBody.gameObject.AddComponent<ConfigurableJoint>();
            attachJoint.connectedBody = physicsHoldRb;

            //world space target behavior
            attachJoint.configuredInWorldSpace = true;

            grabbedBody.constraints =
                RigidbodyConstraints.FreezeRotationX
                | RigidbodyConstraints.FreezeRotationZ;

            //free linear motion, drive it toward connected body
            attachJoint.xMotion = ConfigurableJointMotion.Free;
            attachJoint.yMotion = ConfigurableJointMotion.Free;
            attachJoint.zMotion = ConfigurableJointMotion.Free;

            //angular behavior
            attachJoint.angularXMotion = ConfigurableJointMotion.Free;
            attachJoint.angularYMotion = ConfigurableJointMotion.Free;
            attachJoint.angularZMotion = ConfigurableJointMotion.Free;

            //SoftJointLimit softLimit = new SoftJointLimit();
            //softLimit.limit = 15f;
            //softLimit.bounciness = 0.0f; // Prevent bouncing off the limit walls


            //position drive
            JointDrive drive = new JointDrive();
            drive.positionSpring = 500f;
            drive.positionDamper = 100f;
            drive.maximumForce = 1000f;

            attachJoint.xDrive = drive;
            attachJoint.yDrive = drive;
            attachJoint.zDrive = drive;

            //rotation drive
            JointDrive angularDrive = new JointDrive();
            angularDrive.positionSpring = 3000f;
            angularDrive.positionDamper = 5000f;
            angularDrive.maximumForce = 3000f;

            attachJoint.rotationDriveMode = RotationDriveMode.Slerp;
            attachJoint.slerpDrive = angularDrive;

            //anchor setup
            attachJoint.autoConfigureConnectedAnchor = false;

            attachJoint.anchor = grabbedBody.transform.InverseTransformPoint(grabbedBody.worldCenterOfMass);
            //attachJoint.anchor = Vector3.zero;

            attachJoint.connectedAnchor = Vector3.zero;

            attachJoint.enableCollision = false;
            attachJoint.breakForce = grabBreakForce;
            attachJoint.breakTorque = grabBreakTorque;

            SetCollidersOnGrab(true);
        } else {
            Vector3 pull = transform.forward * this.pullForce;
            //grabbedBody.AddForce(pull, ForceMode.Force);
        }
    }

    ///// <summary>
    ///// attempts to pull not held object 
    ///// performs raycast through middle of camera,
    ///// if the raycast hits a rigidbody that is not kinematic, pick its up
    ///// </summary>
    //void AttemptPull() {
    //    RaycastHit hitInfo;
    //    bool hitSomething = interacting.TryGetHit(grabRange, out hitInfo);

    //    // hit empty
    //    if (hitSomething == false) 
    //        return; 

    //    Rigidbody rb = hitInfo.rigidbody;
    //    if (rb == null) return;
    //    if (rb.isKinematic) return;
    //    grabbedBody = rb;

    //    //========================//
    //    // GRAB logic starts HERE //
    //    //========================//

    //    // within pull range
    //    if (hitInfo.distance < pullRange) {
    //        if (attachJoint != null) return;

    //        // kill motion
    //        grabbedBody.linearVelocity = Vector3.zero;
    //        grabbedBody.angularVelocity = Vector3.zero;

    //        // configurable joint on grabbed object
    //        attachJoint = grabbedBody.gameObject.AddComponent<ConfigurableJoint>();
    //        attachJoint.connectedBody = physicsHoldRb;

    //        // world space target behavior
    //        attachJoint.configuredInWorldSpace = true;

    //        grabbedBody.constraints =
    //            RigidbodyConstraints.FreezeRotationX | 
    //            RigidbodyConstraints.FreezeRotationZ;

    //        // free linear motion, drive it toward connected body
    //        attachJoint.xMotion = ConfigurableJointMotion.Free;
    //        attachJoint.yMotion = ConfigurableJointMotion.Free;
    //        attachJoint.zMotion = ConfigurableJointMotion.Free;

    //        // angular behavior
    //        attachJoint.angularXMotion = ConfigurableJointMotion.Free;
    //        attachJoint.angularYMotion = ConfigurableJointMotion.Free;
    //        attachJoint.angularZMotion = ConfigurableJointMotion.Free;

    //        //SoftJointLimit softLimit = new SoftJointLimit();
    //        //softLimit.limit = 15f;
    //        //softLimit.bounciness = 0.0f; // Prevent bouncing off the limit walls


    //        // position drive
    //        JointDrive drive = new JointDrive {
    //            positionSpring = 500f,
    //            positionDamper = 100f,
    //            maximumForce = 1000f
    //        };

    //        attachJoint.xDrive = drive;
    //        attachJoint.yDrive = drive;
    //        attachJoint.zDrive = drive;

    //        // rotation drive
    //        JointDrive angularDrive = new JointDrive {
    //            positionSpring = 3000f,
    //            positionDamper = 5000f,
    //            maximumForce = 3000f
    //        };

    //        attachJoint.rotationDriveMode = RotationDriveMode.Slerp;
    //        attachJoint.slerpDrive = angularDrive;
    //        // anchor setup
    //        attachJoint.autoConfigureConnectedAnchor = false;
    //        attachJoint.anchor = grabbedBody.transform.InverseTransformPoint(grabbedBody.worldCenterOfMass);
    //        // attachJoint.anchor = Vector3.zero;
    //        attachJoint.connectedAnchor = Vector3.zero;
    //        attachJoint.enableCollision = false;
    //        attachJoint.breakForce = grabBreakForce;
    //        attachJoint.breakTorque = grabBreakTorque;

    //        SetCollidersOnGrab(true);
    //    } else {
    //        Vector3 pull = transform.forward * this.pullForce;
    //        //grabbedBody.AddForce(pull, ForceMode.Force);
    //    }
    //}


    /// <summary>
    /// throws grabbed rigidbody w/ throwforce
    /// </summary>
    void Throw() {
        if (grabbedBody == null) 
            return;
        
        // apply force
        Rigidbody thrownBody = grabbedBody;             // object to be thrown
        Vector3 force = transform.forward * throwForce; // thrown force
        thrownBody.AddForce(force, ForceMode.Impulse);                     // adds force @ direction * magnitude

        Drop();
    }


    /// <summary>
    /// drop
    /// </summary>
    void Drop() {
        if (connectionJoint != null) Destroy(connectionJoint);
        if (attachJoint != null) Destroy(attachJoint);
        if (grabbedBody == null) return;

        grabbedBody.constraints = RigidbodyConstraints.None;
        grabbedBody.WakeUp();
        SetCollidersOnGrab(false);

        grabbedBody = null;
    }


    /// <summary>
    /// enable: ingore=false, disable: ignore=true
    /// </summary>
    private void SetCollidersOnGrab(bool ignore) {
        if (grabbedBody == null) 
            return;

        Collider[] playerColliders = GetComponentsInParent<Collider>();
        Collider[] grabbedColliders = grabbedBody.GetComponentsInChildren<Collider>();

        foreach(Collider pc in playerColliders) {
            foreach(Collider gc in grabbedColliders) {
                Physics.IgnoreCollision (gc, pc, ignore);   
            }
        }
    }


    // called when the joint of this components' gameobject is on has broken
    private void OnJointBreak(float breakForce) {
        // When our joint breaks, call Drop to ensure that we clean up
        // after ourselves.
        Drop();
        //grabbedBody.useGravity = true;
    }


    private void OnDrawGizmos() {
        if (referenceHoldPoint == null)
            return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(referenceHoldPoint.position, .5f);

        if (physicsHoldRb == null) 
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawCube(physicsHoldRb.position, new Vector3(.5f, .5f, .5f));
    }

    #endregion


}
