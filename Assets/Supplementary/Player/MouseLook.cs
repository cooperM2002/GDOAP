using UnityEngine;


public class MouseLook : SneedSpace {

    [SerializeField] private RotationAxes axes = RotationAxes.MouseXandY;
    [SerializeField] private float defaultSensitivity = 5.0f;
    [SerializeField] private float minRotationY = -45.0f;
    [SerializeField] private float maxRotationY = 45.0f;

    private enum RotationAxes { MouseXandY = 0, MouseX = 1, MouseY = 2 }
    private MouseFreeAim freeAim;
    //private PlayerRecoil recoil;
    private Camera cam;

    private float verticalRot = 0;
    private float horizontalRot = 0;
    private float baseFov;
    private float zoomFov;
    private float sensitivityX;
    private float sensitivityY;
    private float zoomSensitivity;
    private float previousRecoilPitch;

    public Vector2 MouseDelta { get; private set; }


    private void Awake() {
        Rigidbody body = GetComponent<Rigidbody>();
        if(body != null)
            body.freezeRotation = true;

        cam = GetComponentInChildren<Camera>();
        //recoil = GetComponentInParent<PlayerRecoil>();
        freeAim = GetComponentInParent<MouseFreeAim>();
    }


    void Start() {
        sensitivityX = defaultSensitivity;
        sensitivityY = defaultSensitivity;
        zoomSensitivity = defaultSensitivity * 0.75f;
        horizontalRot = transform.localEulerAngles.y;
        baseFov = cam.fieldOfView;
        zoomFov = baseFov * .75f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update() {
        PollMouse(
            out float mouseHorizontal, 
            out float mouseVertical);

        MoveCamera(
            mouseHorizontal, 
            mouseVertical);

        ZoomFov();
    }


    private void PollMouse(out float mouseH, out float mouseV) {
        if (freeAim != null) {
            Vector2 cameraMouseDelta =freeAim.CameraMouseDelta;
            mouseV = cameraMouseDelta.x;
            mouseH = cameraMouseDelta.y;
        } else {
            // normal mouse look if FreeAimController is missing
            mouseV = Input.GetAxis("Mouse X");
            mouseH = Input.GetAxis("Mouse Y");
        }

        MouseDelta = new Vector2(mouseV, mouseH);
    }


    private void MoveCamera(float mouseHorizontal, float mouseVertical) {
        if (axes == RotationAxes.MouseX) {

            transform.Rotate(0, mouseVertical * sensitivityX, 0); // vertical

        } else if (axes == RotationAxes.MouseY) {

            //float recoilPitchDelta = recoil.RecoilOffset.x - previousRecoilPitch; // only get recoil added since the previous frame
            //previousRecoilPitch = recoil.RecoilOffset.x;
            verticalRot -= mouseHorizontal * sensitivityY;      // normal mouse pitch
            //verticalRot += recoilPitchDelta;                    // permanently move the player's current look pitch
            verticalRot = Mathf.Clamp(verticalRot, minRotationY, maxRotationY); // recoil can never push past your normal look limits

            transform.localEulerAngles =
                new Vector3(
                    verticalRot,
                    horizontalRot,
                    0f);

        } else {

            // horizontal & vertical
            float delta = mouseVertical * sensitivityX;
            verticalRot -= mouseHorizontal * sensitivityY;
            verticalRot = Mathf.Clamp(verticalRot, minRotationY, maxRotationY);
            horizontalRot = transform.localEulerAngles.y + delta;            //increment rotation angle by delta

            transform.localEulerAngles = 
                new Vector3(
                    verticalRot,
                    horizontalRot,
                    0);
        }
    }


    private void ZoomFov() {
        if(Input.GetMouseButton(1)) {
            cam.fieldOfView = zoomFov;
            sensitivityX = zoomSensitivity;
            sensitivityY = zoomSensitivity;
        } else {
            cam.fieldOfView = baseFov;
            sensitivityX = defaultSensitivity;
            sensitivityY = defaultSensitivity;
        }
    }


}
