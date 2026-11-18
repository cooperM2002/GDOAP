using UnityEngine;

[DefaultExecutionOrder(-100)]
public class MouseFreeAim : SneedSpace {

    [Space(20)]
    [Header("Free Aim Region")]
    [SerializeField, Range(0.05f, 1f)] private float regionWidth = 0.35f; // Width of the region as a fraction of screen width.
    [SerializeField, Range(0.05f, 1f)] private float regionHeight = 0.30f; // Height of the region as a fraction of screen height.

    [Space]
    [Header("Camera Input Blend")]
    [Tooltip("Minimum mouse movement sent to camera, 0 = free aim, 1 = camera aiming")]
    [SerializeField, Range(0f, 1f)] private float cameraInputShare = 0.25f;
    [Tooltip("Distance from camera center that share begins increasing.")]
    [SerializeField, Range(0f, 0.95f)] private float cameraBlendStart = 0.15f;

    [Space]
    [Header("Aim Movement")]
    [SerializeField] private float aimSensitivity = 25f;

    [Space]
    [Header("Debug")]
    [SerializeField] private bool drawDebugRegion = true;
    [SerializeField] private Color boundsColor = new Color(0f, 1f, 0f, 0.8f);
    [SerializeField] private Color aimPointColor = new Color(1f, 0f, 0f, 0.9f);
    [SerializeField] private float borderThickness = 2f;
    [SerializeField] private float aimPointSize = 8f;


    /// <summary>
    /// Aim position measured in pixels from screen centre.
    /// </summary>
    public Vector2 AimOffsetPixels { get; private set; }

    /// <summary>
    /// Complete mouse movement this frame.
    /// </summary>
    public Vector2 MouseDelta { get; private set; }

    /// <summary>
    /// The mouse movement that pushed beyond the region.
    /// </summary>
    public Vector2 CameraMouseDelta { get; private set; }

    /// <summary>
    /// The size of half of the free aim region.
    /// </summary>
    public Vector2 RegionHalfSizePixels =>
        new Vector2(
            Screen.width * regionWidth * 0.5f,
            Screen.height * regionHeight * 0.5f
        );

    /// <summary>
    /// Mouse offset from the center, 0 means fully centered, +- 1 means at the edge of the region.
    /// </summary>
    public Vector2 AimOffsetNormalized {
        get {
            Vector2 limits = RegionHalfSizePixels;
            return new Vector2(
                limits.x > 0f ? AimOffsetPixels.x / limits.x : 0f,
                limits.y > 0f ? AimOffsetPixels.y / limits.y : 0f
            );
        }
    }


    private void Update() {
        ReadMouseInput();
        UpdateFreeAim();
    }


    private void ReadMouseInput() {
        MouseDelta = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        );
    }


    private void UpdateFreeAim() {
        Vector2 limits = RegionHalfSizePixels;

        // At 100%, behave like ordinary centred camera aiming.
        if(cameraInputShare >= 0.999f) {
            AimOffsetPixels = Vector2.zero;
            CameraMouseDelta = MouseDelta;
            return;
        }

        Vector2 normalizedAim = new Vector2(
            limits.x > 0f ? AimOffsetPixels.x / limits.x : 0f,
            limits.y > 0f ? AimOffsetPixels.y / limits.y : 0f
        );

        Vector2 cameraShare = new Vector2(
            CalculateAxisCameraShare(normalizedAim.x, MouseDelta.x),
            CalculateAxisCameraShare(normalizedAim.y, MouseDelta.y)
        );

        // Divide the original mouse input between free aim
        // and camera rotation.
        Vector2 cameraInput = new Vector2(
            MouseDelta.x * cameraShare.x,
            MouseDelta.y * cameraShare.y
        );

        Vector2 freeAimInput = new Vector2(
            MouseDelta.x * (1f - cameraShare.x),
            MouseDelta.y * (1f - cameraShare.y)
        );

        Vector2 desiredAimOffset = AimOffsetPixels + freeAimInput * aimSensitivity;

        Vector2 clampedAimOffset = new Vector2(
            Mathf.Clamp(desiredAimOffset.x, -limits.x, limits.x),
            Mathf.Clamp(desiredAimOffset.y, -limits.y, limits.y)
        );

        // If any free-aim input still crossed the boundary,
        // pass that remaining portion to the camera.
        Vector2 overflowPixels = desiredAimOffset - clampedAimOffset;
        Vector2 overflowInput = overflowPixels / Mathf.Max(aimSensitivity, 0.001f);

        CameraMouseDelta = cameraInput + overflowInput;
        AimOffsetPixels = clampedAimOffset;
    }


    private float CalculateAxisCameraShare(float normalizedAimPosition, float mouseInput) {
        bool movingOutward =
            Mathf.Abs(normalizedAimPosition) > 0.001f && 
            Mathf.Sign(mouseInput) == 
            Mathf.Sign(normalizedAimPosition);

        // Moving inward should pull the crosshair back toward
        // the centre instead of keeping it stuck at the edge.
        if(!movingOutward) return cameraInputShare;

        float distanceFromCentre = Mathf.Abs(normalizedAimPosition);
        float blendAmount = Mathf.InverseLerp(cameraBlendStart, 1f, distanceFromCentre);
        blendAmount = Mathf.SmoothStep(0f, 1f, blendAmount);

        return Mathf.Lerp(cameraInputShare, 1f, blendAmount );
    }


    private void OnGUI() {
        if(!drawDebugRegion) return;

        Vector2 limits = RegionHalfSizePixels;

        Rect regionRect = new Rect(
            Screen.width * 0.5f - limits.x,
            Screen.height * 0.5f - limits.y,
            limits.x * 2f,
            limits.y * 2f
        );

        DrawRectangleBorder(
            regionRect,
            boundsColor,
            borderThickness
        );

        Rect aimPointRect = new Rect(
            Screen.width * 0.5f + AimOffsetPixels.x - aimPointSize * 0.5f,
            Screen.height * 0.5f - AimOffsetPixels.y - aimPointSize * 0.5f,
            aimPointSize,
            aimPointSize
        );

        Color previousColor = GUI.color;

        GUI.color = aimPointColor;
        GUI.DrawTexture(aimPointRect, Texture2D.whiteTexture);
        GUI.color = previousColor;
    
    }


    private static void DrawRectangleBorder(Rect rect, Color color,float thickness) {
        Color previousColor = GUI.color;
        GUI.color = color;

        GUI.DrawTexture(
            new Rect(rect.x, rect.y, rect.width, thickness),
            Texture2D.whiteTexture);

        GUI.DrawTexture(
            new Rect(rect.x, rect.yMax - thickness, rect.width, thickness),
            Texture2D.whiteTexture);

        GUI.DrawTexture(
            new Rect(rect.x, rect.y, thickness, rect.height),
            Texture2D.whiteTexture);

        GUI.DrawTexture(
            new Rect(rect.xMax - thickness, rect.y, thickness, rect.height),
            Texture2D.whiteTexture);

        GUI.color = previousColor;
    }


}
