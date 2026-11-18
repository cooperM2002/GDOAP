#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NPCVision))]
public class VisionSensorEditor : Editor
{
    SerializedProperty _viewRadius;
    SerializedProperty _viewAngle;
    SerializedProperty _eyePoint;

    private void OnEnable() {
        _viewRadius = serializedObject.FindProperty("viewRadius");
        _viewAngle = serializedObject.FindProperty("viewAngle");
        _eyePoint = serializedObject.FindProperty("eyePoint");
    }

    //private void OnSceneGUI() {
    //    serializedObject.Update();

    //    var sensor = (NPCVision)target;
    //    Transform t = sensor.transform;

    //    float radius = Mathf.Max(0f, _viewRadius.floatValue);
    //    float angle = Mathf.Clamp(_viewAngle.floatValue, 0f, 360f);

    //    Transform eye = _eyePoint.objectReferenceValue as Transform;
    //    Vector3 origin = (eye != null) ? eye.position : t.position;

    //    // --- Draw the FOV arc (solid + wire) ---
    //    Vector3 forwardFlat = t.forward;
    //    forwardFlat.y = 0f;
    //    if(forwardFlat.sqrMagnitude < 0.0001f) forwardFlat = Vector3.forward;
    //    forwardFlat.Normalize();

    //    Vector3 leftDir = Quaternion.Euler(0f, -angle * 0.5f, 0f) * forwardFlat;
    //    Vector3 rightDir = Quaternion.Euler(0f, angle * 0.5f, 0f) * forwardFlat;

    //    // solid fill
    //    Handles.color = new Color(1f, 1f, 1f, 0.10f);
    //    Handles.DrawSolidArc(origin, Vector3.up, leftDir, angle, radius);

    //    // wire outline + edge lines
    //    Handles.color = new Color(1f, 1f, 1f, 0.50f);
    //    Handles.DrawWireArc(origin, Vector3.up, leftDir, angle, radius);
    //    Handles.DrawLine(origin, origin + leftDir * radius);
    //    Handles.DrawLine(origin, origin + rightDir * radius);

    //    // --- Radius handle (drag to change viewRadius) ---
    //    Handles.color = Color.white;

    //    Vector3 handlePos = origin + forwardFlat * radius;

    //    EditorGUI.BeginChangeCheck();
    //    float newRadius = Handles.ScaleValueHandle(
    //        radius,              // current value
    //        handlePos,           // handle position
    //        Quaternion.LookRotation(forwardFlat, Vector3.up),
    //        1f,                  // handle size
    //        Handles.ConeHandleCap,
    //        0.25f                // snap
    //    );

    //    if(EditorGUI.EndChangeCheck()) {
    //        Undo.RecordObject(sensor, "Change View Radius");
    //        _viewRadius.floatValue = Mathf.Max(0f, newRadius);
    //        serializedObject.ApplyModifiedProperties();
    //        EditorUtility.SetDirty(sensor);
    //    }
    //}
}
#endif