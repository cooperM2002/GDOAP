using UnityEngine;

public class StopSavePlaymode : MonoBehaviour
{
    void OnGUI() {
        Event e = Event.current;
        if(e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.S) {
            e.Use(); // Consumes the event
            Debug.Log("Ctrl+S Intercepted!");
        }
    }
}
