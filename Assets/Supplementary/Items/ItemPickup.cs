using UnityEngine;

/// <summary>
/// Represents a <b>physical representation of a pickup</b> 
/// that can be acquired by the player.
/// Connects an object to a data definition.
/// </summary>
public class ItemInteractable : InteractableObject
{
    /*-------------------------------------------------------------------------------------------------*/
    //debug
    private const string Identifier = "<color=#1d60c4>[ItemInteractable]</color>";
    private void PrintMessage(string message) { Debug.Log($"{Identifier}: {message}"); }
    /*-------------------------------------------------------------------------------------------------*/

    /*
    * Methods to implement later
    *   - a method to provide the definition
    *   - a method to "consume" or remove this object when picked up
    */

    [SerializeField] private ItemDefinition itemDef;    //reference def
    [SerializeField] private bool available = true;     //bool, can be used
    [SerializeField] private ItemController controller;

    private void Awake()
    {

    }

    /// <summary>
    /// override Interact in ItemPickup class
    /// </summary>
    /// <param name="fromObject"></param>
    override public void Interact(GameObject fromObject)
    {
        //no controller
        if (controller == null) {
            //PrintMessage("could not find ItemController, auto assigning");
            //Debug.Log("could not find ItemController, auto assigning");
            controller = fromObject.GetComponentInParent<ItemController>();
            //fromObject.transform.root.GetComponent<ItemController>()
        }

        //no itemcontroller or no item def
        if (controller == null || itemDef == null) return;

        ItemInstanceCarrier carrier = GetComponent<ItemInstanceCarrier>();
        ItemInstance incoming = (carrier!=null) ? carrier.Instance : null;

        bool success = controller.TryPickupItem(itemDef, incoming);

        //   [FIX THIS]
        if (success) {
            ConsumeWorldPickup();
        } else {
            PrintMessage("cannot consume");
            //Debug.Log("cannot consume");
            return;
        }
    }


    /// <summary>
    /// returns itemDef object
    /// </summary>
    /// <returns></returns>
    public ItemDefinition GetItemDef()
    {
        //return the data ref
        if (itemDef == null) {
            PrintMessage("No ItemDef found");
            //Debug.Log("No ItemDef"); 
            return null; 
        }

        return itemDef;
    }


    /// <summary>
    /// Checks if item is <i>bool available</i>, if True, get collider, disable, then destroy gameobject 
    /// </summary>
    public void ConsumeWorldPickup()
    {
        if (!available) return; //already used
        available = false;      //now in use

        Collider itemCollider = GetComponent<Collider>();
        if(itemCollider != null) {
            itemCollider.enabled = false; //disable collider
        }

        Destroy(gameObject);
    }


}
