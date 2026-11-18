using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static lookup from <b>NPC ID -> live NPCIdentity component</b>
/// register in Awake, unregister in OnDestroy
/// Anything holding a saved ID can resolve to the live NPC via <see cref="Get"/>,
/// or null if that NPC is unloaded / destroyed
/// </summary>
public class NPCRegistry{
    /*-------------------------------------------------------------------------------------------------*/
    //debug
    private const string Identifier = "<color=#b38100><b>[NPCRegistry]</b></color>";
    private static void PrintMessage(string message) { Debug.Log($"{Identifier}:\n{message}"); }
    /*-------------------------------------------------------------------------------------------------*/

    private static readonly Dictionary<string, NPCIdentity> identityDict = new();


    public static void Register(NPCIdentity npc) {
        if(npc == null || string.IsNullOrEmpty(npc.InstanceId)) return;
        if(identityDict.ContainsKey(npc.InstanceId)) {
            PrintMessage($"WARNING, duplicate ID: {npc.InstanceId}, short ID: {npc.ShortId}");
        }
        identityDict[npc.InstanceId] = npc; 
    }

    public static void Unregister(string Id) {
        if(string.IsNullOrEmpty(Id)) return;
        identityDict.Remove(Id);
    }

    public static NPCIdentity Get(string Id) {
        if(string.IsNullOrEmpty(Id)) return null;
        identityDict.TryGetValue(Id, out NPCIdentity npc);
        return npc;
    }

    public static bool Exists(string Id) {
        return string.IsNullOrEmpty(Id) && identityDict.ContainsKey(Id);
    }

    /////////////////////////////////////???????????????????????????????????????????
    /// <summary>
    /// Clears the registry. Called automatically on play-mode start to prevent
    /// stale entries surviving across domain reloads in the editor.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearOnPlayModeStart() {
        identityDict.Clear();
    }

}
