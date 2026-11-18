using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent (typeof(NavMeshLink))]
public class DoorNavLink : SneedSpace {

    [SerializeField] DoorOperable door;
    public DoorOperable Door => door;


    public void AgentOpenDoor() {

        if(door == null) {
            PrintError("No Door Detected");
            return;
        } 

        door.Operate();
    
    }

}
