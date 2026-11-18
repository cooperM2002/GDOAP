using UnityEngine;

/// <summary>
/// Runtime identity for a NPC
/// Systems that refer to a specific NPC store its <see cref="InstanceID"/> using <see cref="NPCRegistry"/>
/// 
/// <list type="table">
/// Workflow:
///  <item>- <b>Unique premade NPCs:</b> assign an ID in the inspector (e.g. "shopkeeper_bob").</item>
///  <item>- <b>Generic spawned NPCs:</b> leave the field empty; a GUID is generated at Awake.</item>
///  <item>- <b>NEVER</b> assign an ID to a prefab asset (the OnValidate guard clears it).</item>
///  </list>
/// </summary>
public class NPCIdentity : SneedSpace{

    [Space (20)]

    [SerializeField] private string instanceId;
    [SerializeField] private int shortenedLength;

    private string shortId;

    public string InstanceId => instanceId;
    public string ShortId => shortId;

    private void Awake() {

        //generate new ID if empty (else pre authored otherwise)
        if(string.IsNullOrEmpty(instanceId)) {

            //globally unique identifer, 128bit number ex "3f2504e0-4f89-41d3-9a0c-0305e82c3301"
            instanceId = System.Guid.NewGuid().ToString();
            shortId = ShortenId(instanceId);
        } else {
            //PrintMessage($"Pre-authored ID: {instanceId}, shortId: {shortId}");
        }

        NPCRegistry.Register(this);
    }

    private void OnDestroy() {
        NPCRegistry.Unregister(instanceId);
    }

    /// <summary>
    /// Returns 8 character shorthand of full 128bit GUID
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    private string ShortenId(string Id) {

        string shortened;
        if(Id.Length == 0) return "Invalid Id (length 0)";
        if(Id.Length <= shortenedLength-1) return Id;

        Id = Id.Replace("-","");
        shortened = Id.Substring(0, shortenedLength-1);

        return shortened;
    }

////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
    private new void OnValidate() {
        // Guard: if this is a prefab asset (not a scene instance),
        // force-clear the ID so copies spawned at runtime always auto-generate.
        if(UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this))
            instanceId = "";
    }
#endif
////////////////////////////////////////////////////////////////////////////////////


}
