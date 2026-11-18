using UnityEngine;

[CreateAssetMenu(fileName = "BodyPartProfile", menuName = "Scriptable Objects/BodyPartProfile")]
public class BodyPartProfile : ScriptableObject
{
    public BodyPart part;

    [Header("Condition")]
    public MovementCondition conditionOnHit = MovementCondition.Normal;
    public float movementSpeedMultiplier = 1.0f;
    public bool conditionPersists = true;
    public float conditionDuration = 3.0f;


    [Header("Damage")]
    public float damageMultiplier = 1f;
    public bool alwaysLethal = false;
    public bool reverseDirection = false;

    [Header("Physics")]
    public float forceMultiplier =1f;
    public float musclePower = 1f;

    [Header("Hit effects")]
    public GameObject bulletHitVfxPrefab;
    public GameObject secondaryBloodEffect;

    [Header("Hit Sounds")]
    public AudioClip primaryHitSound;
    public AudioClip secondaryHitSound;

    [Header("Collided Sounds")]
    public AudioClip primaryCollidedSound;
    //public AudioClip secondaryHitSound;


    //audio later
}
