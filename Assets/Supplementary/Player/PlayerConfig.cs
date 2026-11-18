using UnityEngine;

[CreateAssetMenu(fileName = "InputConfig", menuName = "Configs/InputConfig")]
public class PlayerConfig : ScriptableObject
{
    //[field: SerializeField] public KeyCode ...Key { get; private set; } = KeyCode.;
    [Header("Movement")]
    //[field: SerializeField] public KeyCode interactKey { get; private set; } = KeyCode.E;
    [field: SerializeField] public KeyCode crouchKey { get; private set; } = KeyCode.LeftControl;
    [field: SerializeField] public KeyCode sprintKey { get; private set; } = KeyCode.LeftShift;


    [Header("Interacting")]
    [field: SerializeField] public KeyCode interactKey { get; private set; } = KeyCode.E;
    [field: SerializeField] public KeyCode throwKey { get; private set; } = KeyCode.Mouse0;

    [Header("Items")]
    [field: SerializeField] public KeyCode holsterKey { get; private set; } = KeyCode.H;
    [field: SerializeField] public KeyCode dropKey { get; private set; } = KeyCode.G;
    [field: SerializeField] public KeyCode itemInteractKey { get; private set; } = KeyCode.F;

}

