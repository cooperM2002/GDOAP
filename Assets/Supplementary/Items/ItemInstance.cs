using UnityEngine;

/// <summary>
/// Represents a <b>PER COPY RUNTIME STATE OF AN ITEM</b>
/// One ItemInstance => One Physical in-world copy of an item 
/// Persists across pickup, equip, throw by slotting into a ItemInstanceCarrier
/// This is the base class for other item specific subclasses that need their own persistent data such as ammo, durability
/// </summary>
[System.Serializable] public class ItemInstance
{
    [SerializeField] private ItemDefinition definition;
    public ItemDefinition Definition => definition;

    public ItemInstance(ItemDefinition def) {
        definition = def;
    }
}
