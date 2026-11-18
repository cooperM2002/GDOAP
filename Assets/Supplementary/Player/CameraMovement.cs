using System.Collections;
using UnityEngine;

public class CameraMovement : SneedSpace {

    [SerializeField] private bool useMouseDirection = false;
    [SerializeField] private Vector3 initialCameraPos;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private bool doDownwardBob = false;
    [SerializeField] private bool doWalkBob = false;

    [SerializeField] private float landBobAmount = 0.25f;
    [SerializeField] private float landBobDuration = 0.5f;

    [SerializeField] private float walkBobSpeed = 8f;
    [SerializeField] private float walkBobAmountY = 0.04f;
    [SerializeField] private float walkBobTiltAmount = 2.5f;
    [SerializeField] private float walkBobReturnSpeed = 8f;

    private float walkBobPhase;
    private float walkBobBlend;
    private float landYOffset;
    private PlayerMovement playerMotor;
    private MouseLook mouseLook;
    private Coroutine landBobRoutine;
    public float tiltDegrees = 10f;        //max roll when holding A/D
    public float tiltSmoothTime = 0.08f;   //lower = snappier, higher = floatier
    private float zVelocity;               //required by SmoothDampAngle
    private bool justLanded;


    void Start() {

        playerMotor = GetComponentInParent<PlayerMovement>();
        mouseLook = GetComponentInParent<MouseLook>();  

        if (playerMotor == null) 
            PrintError("CameraBehaviour: no player motor detected");
        if (mouseLook == null)
            PrintError("Mouselook null");

        initialCameraPos = transform.localPosition;

    }


    void Update() {

        PollMovement(
            out Vector3 MoveDir,
            out bool left,
            out bool right,
            out float input);

        ApplyLandBob();

        ApplyBobTilt(MoveDir, input);

    }

    private void PollMovement(out Vector3 MoveDir, out bool left, out bool right, out float input) {
        if (useMouseDirection) {
            MoveDir = mouseLook.MouseDelta;
            left = mouseLook != null ? MoveDir.x < 0 : Input.GetKey(KeyCode.A);
            right = mouseLook != null ? MoveDir.x > 0 : Input.GetKey(KeyCode.A);
        } else {
            MoveDir = playerMotor.MoveInput;
            left = playerMotor != null ? MoveDir.x < 0 : Input.GetKey(KeyCode.A);
            right = playerMotor != null ? MoveDir.x > 0 : Input.GetKey(KeyCode.A);
        }

        // inputdir
        //input = left ? -1f : 1f;
        input = 0f;
        if (left) {
            input = -1f;//left 
        } else if (right) {
            input = 1f;//right
        }
    }

    //downward bob
    private void ApplyLandBob() {
        if (playerMotor != null)
            justLanded = playerMotor.JustLanded;//&& PlayerMoto.IsGrounded

        if (justLanded && doDownwardBob) {
            PrintMessage($"{justLanded}");
            PlayLandBob();
        }

    }


    private void ApplyBobTilt(Vector3 MoveDir, float input) {
        bool isWalking = MoveDir.sqrMagnitude > 0.01f;

        if(doWalkBob) {
            ApplyWalkBob(isWalking, input);
        } else {
            ApplyTiltOld(input);
        }

    }


    private void ApplyTilt(float inputDir, float walkRoll) {
        float inputLean = -inputDir * tiltDegrees;
        float targetRotZ = inputLean + walkRoll;

        Vector3 tiltBy = transform.localEulerAngles;

        float newZ 
            = Mathf.SmoothDampAngle(
                tiltBy.z,
                targetRotZ,
                ref zVelocity,
                tiltSmoothTime
            );

        tiltBy.z = newZ;
        transform.localEulerAngles = tiltBy;

    }


    private void ApplyTiltOld(float inputDir) {
        float targetRotZ = -inputDir * tiltDegrees; 

        //A = +tilt, D = -tilt
        Vector3 tiltBy = transform.localEulerAngles; 

        float newZ = 
            Mathf.SmoothDampAngle( 
                tiltBy.z, 
                targetRotZ, 
                ref zVelocity, 
                tiltSmoothTime 
            ); 

        tiltBy.z = newZ; 
        transform.localEulerAngles = tiltBy; 
    
    }


    public void PlayLandBob() {
        if(landBobRoutine != null) {
            StopCoroutine(landBobRoutine);
        }

        landBobRoutine = StartCoroutine(LandBob());
    }


    private IEnumerator LandBob() {
        float timer = 0f;

        while(timer < landBobDuration) {
            timer += Time.deltaTime;

            float p = Mathf.Clamp01(timer / landBobDuration);
            float curve = Mathf.Sin(p * Mathf.PI);

            landYOffset = -curve * landBobAmount;

            yield return null;
        }

        landYOffset = 0f;
        landBobRoutine = null;
    }


    public void ApplyWalkBob(bool isWalking, float inputDir) {
        float targetBlend = isWalking && doWalkBob ? 1f : 0f;

        walkBobBlend = 
            Mathf.MoveTowards(
                walkBobBlend,
                targetBlend,
                Time.deltaTime * walkBobReturnSpeed
            );

        if(walkBobBlend > 0f) {
            walkBobPhase += Time.deltaTime * walkBobSpeed;
        }

        float wave = Mathf.Sin(walkBobPhase);
        float yBob = wave * walkBobAmountY * walkBobBlend; // up/down bob
        float walkRoll = -wave * walkBobTiltAmount * walkBobBlend; // sideways roll bob

        Vector3 finalPos = initialCameraPos;
        finalPos.y += yBob;
        finalPos.y += landYOffset;

        transform.localPosition = finalPos;
        ApplyTilt(inputDir, walkRoll);
    
    }



}
