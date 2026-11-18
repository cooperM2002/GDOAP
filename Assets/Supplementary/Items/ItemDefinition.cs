using UnityEngine;



/// <summary>
/// Represents a reusable <b>description of an item</b> that can be in some way <b>used by the player</b>, ie can hold, consume, etc...
/// </summary>
[CreateAssetMenu(fileName = "ItemDefinition", menuName = "Scriptable Objects/ItemDefinition")]
public class ItemDefinition : ScriptableObject
{
    /*
    * REQUIREMENTS
    * 
    * Variables
    * 
    *   - identifier (string)
    *   - display label (string)
    *   - reference A (example: a prefab reference)
    *   - reference B (example: another prefab reference)
    *
    * Members to implement later
    * 
    *   - explicit properties (get/set blocks, no shorthand)
    *   - optional validation hook (OnValidate)
    */

    ////////////////
    ///  FIELDS  ///
    ////////////////
    //- identifier(string)
    //- display label(string)
    //- reference A(example: a prefab reference)
    //- reference B(example: another prefab reference)

    [SerializeField] private string itemName;
    [SerializeField] private string itemLabel;


    [SerializeField] private GameObject worldPrefab;
    [SerializeField] private GameObject playerPrefab;

    public string ItemName => itemName;
    public string ItemLabel => itemLabel;
    public GameObject WorldPrefab => worldPrefab;
    public GameObject PlayerPrefab => playerPrefab;


    //[SerializeField] private bool isConsumable;
    public enum ItemType
    {
        weapon, ammo, consumable, usuable, prop
    }


    /// <summary>
    /// Creates a fresh runtime instance for a new copy of this item.
    /// Override in subclasses to return a specialized ItemInstance subclass
    /// (e.g. a weapon definition returns a WeaponInstance with a full magazine).
    /// </summary>
    public virtual ItemInstance CreateDefaultInstance() {
        return new ItemInstance(this);
    }


    /////////////////
    ///  METHODS  ///
    /////////////////
    //- explicit properties(get/set blocks, no shorthand)
    //- optional validation hook(OnValidate)



    private void OnValidate()
    {
        // TODO: validation (no implementation yet)
    }
}
