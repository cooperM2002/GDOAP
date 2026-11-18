using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractableObject : SneedSpace {

    /// <summary>
    /// Overridable generic interact method, called on ray hit of Interactables collider.
    /// </summary>
    /// <param name="fromObject"></param>
    public virtual void Interact(GameObject fromObject) {
        PrintMessage($"interacted with by {fromObject.name}");
    }

}
