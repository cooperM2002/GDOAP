using UnityEngine;

public class Crosshair : SneedSpace {

    [Space]
    [Header("References")]
    [SerializeField] private MouseFreeAim freeAim;

    [Space]
    [Header("Crosshair")]
    [SerializeField] private Texture2D crosshairTexture;
    [SerializeField] private float crosshairSize = 18f;
    [SerializeField] private Color crosshairColor = Color.white;

    private Camera cam;

    /// <summary>
    /// The exact crosshair position using freeaim.
    /// </summary>
    public Vector2 AimScreenPosition {
        get {
            Vector2 centre = new Vector2(
                cam.pixelWidth * 0.5f,
                cam.pixelHeight * 0.5f
            );

            if (freeAim == null)
                return centre;

            return centre + freeAim.AimOffsetPixels;
        }
    }


    private void Start() {
        cam = GetComponent<Camera>();

        if (freeAim == null)
            freeAim = GetComponentInParent<MouseFreeAim>();

        if (cam == null)
            PrintError("Crosshair requires a Camera component.");
    }


    private void OnGUI() {
        if (cam == null)
            return;

        //int size = 18;
        //float posX = cam.pixelWidth / 2 - size / 4;
        //float posY = cam.pixelHeight / 2 - size / 2;
        //GUI.Label(new Rect(posX, posY, size, size), "•");

        Vector2 aimPosition = AimScreenPosition;

        // OnGUI measures Y downward from the top.
        float guiX =
            aimPosition.x - crosshairSize * 0.5f;

        float guiY =
            cam.pixelHeight
            - aimPosition.y
            - crosshairSize * 0.5f;

        Rect crosshairRect = new Rect(
            guiX,
            guiY,
            crosshairSize,
            crosshairSize
        );

        Color previousColor = GUI.color;
        GUI.color = crosshairColor;

        if (crosshairTexture != null) {
            GUI.DrawTexture(crosshairRect, crosshairTexture, ScaleMode.ScaleToFit, true);
        } else {
            GUIStyle style = 
                new GUIStyle(GUI.skin.label) {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.RoundToInt(crosshairSize)
                };

            GUI.Label(crosshairRect, "•", style);
        }

        GUI.color = previousColor;
    }


}
