using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightOperable : OperableObject {

    [Header("Light Settings")]
    [SerializeField] private Light myLight;
    [SerializeField] private Vector3 directionalRotationDelta = new Vector3(0, 45, 0);


    void Awake() {
        if (myLight==null) 
            myLight = GetComponentInChildren<Light>();

        identifierExtra = "LightOperable";
    }


    public override void Operate() {
        if (myLight.type == LightType.Directional) {
            transform.Rotate(directionalRotationDelta, Space.World);
            PrintMessage("Operated....");
        } else {
            myLight.enabled = !myLight.enabled;
            PrintMessage("Operated....");
        }

        // put whatever here, change color, intensity, etc
    }


}
