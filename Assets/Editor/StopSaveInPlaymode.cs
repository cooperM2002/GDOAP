using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class StopSaveInPlaymode
{
    static StopSaveInPlaymode() {
        FieldInfo info = typeof(EditorApplication).GetField(
            "globalEventHandler",
            BindingFlags.Static | BindingFlags.NonPublic);

        var value = (EditorApplication.CallbackFunction)info.GetValue(null);
        value += OnEvent;
        info.SetValue(null, value);
    }

    static void OnEvent() {
        Event e = Event.current;
        if(e == null) return;

        // Block Ctrl+S (Windows/Linux) and Cmd+S (macOS) during play mode
        if(EditorApplication.isPlaying
            && e.type == EventType.KeyDown
            && (e.control || e.command)
            && e.keyCode == KeyCode.S) {
            e.Use();
            Debug.Log("Ctrl+S blocked during play mode");
        }
    }
}
