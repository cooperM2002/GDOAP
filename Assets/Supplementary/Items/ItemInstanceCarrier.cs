using UnityEngine;

/// <summary>
/// Holds a reference to an <b>ItemInstance</b> on a specific prefab copy.
/// Attach to both the <i>world pickup prefab</i> and the <i>held (player) prefab</i>
/// of any item that needs per-copy runtime state.
/// The ItemController reads/writes this at the pickup / equip / throw seams
/// so the instance survives the cycle.
/// </summary>
public class ItemInstanceCarrier : MonoBehaviour
{
    /*-------------------------------------------------------------------------------------------------*/
    //debug
    private const string Identifier = "<color=#4caf50><b>[ItemInstanceCarrier]</b></color>";
    private void PrintMessage(string message) { Debug.Log($"{Identifier}:\n{message}", this); }
    /*-------------------------------------------------------------------------------------------------*/

    private ItemInstance instance;
    public ItemInstance Instance {
        get { return instance; }
        set {
            instance = value;
            PrintMessage(
                $"Instance set to: {(value == null ? "null" : value.GetType().Name + " (hash " + value.GetHashCode() + ")")}"
                );

        }
    }

    public bool HasInstance => instance != null;

}
