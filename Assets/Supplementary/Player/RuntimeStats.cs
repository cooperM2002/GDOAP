using UnityEngine;

public class RuntimeStatsWindow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement player;

    [Header("Window")]
    [SerializeField] private bool showWindow = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    [SerializeField] private float width = 260f;
    [SerializeField] private float padding = 10f;
    [SerializeField] private Font debugFont;

    private GUIStyle labelStyle;
    private GUIStyle valueStyle;
    private GUIStyle boxStyle;

    private void Awake() {
        if(player == null)
            player = FindFirstObjectByType<PlayerMovement>();
    }

    private void Update() {
        if(Input.GetKeyDown(toggleKey))
            showWindow = !showWindow;
    }

    private void OnGUI() {
        if(!showWindow || player == null) return;

        InitStyles();

        //float x = Screen.width - width - padding;
        float x = padding;

        float y = padding;

        GUILayout.BeginArea(new Rect(x, y, width, 350f), boxStyle);

        GUILayout.Label("PLAYER STATS", valueStyle);
        Space();

        Row("Speed", HorizontalSpeed(player.Velocity).ToString("0.00"));
        Row("Velocity Y", player.Velocity.y.ToString("0.00"));
        Row("Grounded", player.isGrounded.ToString());
        Row("Moving", player.IsMoving.ToString());
        Row("Sprinting", player.isSprinting.ToString());
        Row("Crouching", player.isCrouching.ToString());
        Row("Move Input", player.MoveInput.ToString());

        GUILayout.EndArea();
    }

    private void Row(string label, string value) {
        GUILayout.BeginHorizontal();

        GUILayout.Label(label, labelStyle, GUILayout.Width(120f));
        GUILayout.Label(value, valueStyle);

        GUILayout.EndHorizontal();
    }

    private void Space() {
        GUILayout.Space(6f);
    }

    private float HorizontalSpeed(Vector3 v) {
        v.y = 0f;
        return v.magnitude;
    }

    private void InitStyles() {
        if(labelStyle != null) return;

        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.padding = new RectOffset(10, 10, 10, 10);

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.alignment = TextAnchor.MiddleLeft;
        labelStyle.font = debugFont;
        labelStyle.fontSize = 14;

        valueStyle = new GUIStyle(GUI.skin.label);
        valueStyle.alignment = TextAnchor.MiddleLeft;
        valueStyle.font = debugFont;
        valueStyle.fontSize = 14;
        valueStyle.fontStyle = FontStyle.Bold;
    }
}
