using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class NPCRagdollHandler : SneedSpace {

    //=======//
    // Setup //
    //=======//
    #region setup


    [Space(20)]
    [Header("Body Part Profiles")]
    [SerializeField] private BodyPartProfile[] profiles;//7
    [SerializeField] private BodyPartProfile defaultProfile;//fallback
    
    [Space]
    [Header("Ragdoll References")]
    [SerializeField] private int ragdollDummyLayer = 0;
    [SerializeField] private GameObject dummyRagdoll;
    [SerializeField] private Transform armatureRoot;

    private NpcController npc;
    private NPCHitManager manager;
    private NPCBodyPartHandler dismember;


    #region dicts
    private readonly Dictionary<string,     Transform> SourceBoneDict = new();
    private readonly Dictionary<Transform,  BodyPart> BodyPartDict = new();
    //private readonly Dictionary<Transform,  RagdollChainBone> RA2BoneDict = new();
    private readonly Dictionary<Transform,  Rigidbody> DummyRbDict = new();
    private readonly Dictionary<BodyPart,   BodyPartProfile> profileDict = new();
    #endregion

    #region public
    public Dictionary<Transform, BodyPart> PartDict => BodyPartDict;
    //public RagdollAnimator2 Rag => rag;
    //public RagdollHandler RagHandler => ragHandler;
    #endregion


    private void Awake(){
        manager = GetComponent<NPCHitManager>();
        npc = GetComponent<NpcController>();
        dismember = GetComponent<NPCBodyPartHandler>();

        BuildSourceBoneDict();
    }


    private void Start() {
        //identifierExtra = $"(#{npc.ID.ToUpper()})";

        BuildDerivedDicts();
        BuildProfileDict();
        StartInfo();
    }


    private void StartInfo() {
        ClearPrintInfo();

        AddInfoHeader("References");
        AddInfoRefCheck("NPCContoller", npc);
        AddInfoRefCheck("HitManager", manager);
        //AddInfoRefCheck("RagdollAnimator2", rag);
        //AddInfoCheck("RagdollHandler", ragHandler != null);
        AddInfoRefCheck("Armature", armatureRoot);

        AddInfoHeader("Dictionaries");
        AddInfoDict("BodyPartDict:", BodyPartDict);
        //AddInfoDict("RA2Dict", RA2BoneDict,
        //    ra2Bone => ra2Bone.PhysicalDummyBone != null
        //        ? ra2Bone.PhysicalDummyBone.name : "NULL PhysicalDummyBone");

        PrintStartInfo();
    }


    #endregion


    //=================//
    // Apply Reactions //
    //=================//
    #region


    /// <summary>
    /// Public wrapper for RA2 reaction LateUpdate coroutine
    /// </summary>
    public void ExecuteReaction(ReactionType reaction, Transform hitBone, Vector3 hitPoint, Vector3 hitDirection, float force, BodyPartProfile profile, bool tryDismember = false) {
        //if(rag == null) { PrintError("Rag is null"); return; }

        hitDirection.Normalize();
        StartCoroutine(ReactionRoutine(reaction, hitBone, hitPoint, hitDirection, force, profile, tryDismember));
    }


    /// <summary>
    /// Applies specific type of reaction, Flinch, Knock, Death
    /// </summary>
    private IEnumerator ReactionRoutine(ReactionType reaction, Transform hitBone, Vector3 hitPoint, Vector3 hitDirection, float force, BodyPartProfile profile, bool tryDismember) {
        yield return new WaitForFixedUpdate();

        bool hitResolved = ResolveHit(
            hitBone, 
            out BodyPart part, 
            out Transform sourceBone);
        
        if(!hitResolved)    { PrintError($"warning, could not resolve hit on {hitBone?.name}"); yield break; } 
        //if(ra2Bone == null) { PrintError($"warning, no ra2bone on {hitBone?.name}");            yield break; }

        PrintMessage($"ExecuteReaction {reaction} on {part}, force:{force}");
        //PrintMessage($"{profile.name}");

        //:::::::::::::::
        //:: REACTIONS ::
        //:::::::::::::::
        switch(reaction) {

            //::::::::::::
            //:: Flinch ::
            //::::::::::::
            case ReactionType.Flinch:

                ////if(profile.reverseDirection) hitDirection = -hitDirection;
                ////ra2Bone.ForceMultiplier = 0.1f;
                //rag.User_AddBoneImpact(ra2Bone, hitDirection * 5f, 0.2f, ForceMode.Impulse, 0f, waitFixedFrames: 1);
                //rag.User_AddChainImpact(ra2Bone.ParentChain, hitDirection * 3f, .4f, ForceMode.Impulse);
                //rag.User_AddRigidbodyImpactAtPosition(dummyRb, hitDirection * 5f,hitPoint,0.25f,ForceMode.Impulse);
                //dummyRb.AddForceAtPosition(0.1f*(hitDirection), hitPoint, ForceMode.Impulse);
                //rag.User_AddRigidbodyImpactAtPosition(dummyRb,hitDirection,hitPoint,.1f,forceMode:ForceMode.Impulse, waitFixedFrames: 2);
                //rag.User_TransitionMusclesPowerMultiplier(.7f, .1f);
                //rag.User_UpdateAllBonesParametersAfterManualChanges();
                PrintMessage("Flinched");
            
            break;

            //:::::::::::
            //:: Knock ::
            //:::::::::::
            case ReactionType.Knock:
            
                //if(profile.reverseDirection) hitDirection = -hitDirection;
                //rag.User_FallImpact(hitDirection, force * .1f, 0.1f, 0.5f, dummyRb);
                //rag.User_AddBoneImpact(ra2Bone, hitDirection * 10f, 0.1f, ForceMode.Impulse, 0f, waitFixedFrames: 1);
                ////rag.User_SwitchFallState()
                PrintMessage("Knocked");

            break;

            //:::::::::::
            //:: Death ::
            //:::::::::::
            case ReactionType.Death:

                // fall to ground + go limp
                //if(profile.reverseDirection) hitDirection = -hitDirection;
                //rag.User_FallImpact(-hitDirection, 2f, 0.15f, 1f, dummyRb);//force * 10f //-hitdireciton is forward
                //rag.User_AddBoneImpact(ra2Bone, hitDirection * 8f, 0.1f, ForceMode.Impulse, 0f, waitFixedFrames: 1);//FORWARD HEAD
                //rag.User_FadeMusclesPowerMultiplicator(0.1f, 0.3f);
                PrintMessage("Died");

            break;

            //:::::::::::::
            //:: Explode ::
            //:::::::::::::
            case ReactionType.explode:

                //Rigidbody centralRb = GetCentralRigidbody();
                //if(centralRb == null) centralRb = dummyRb;
                ////ONLY the central/hip rigidbody gets explosion force.
                //rag.User_FallImpact(hitDirection, force*.5f, 0.15f, 1f, centralRb);
                //rag.User_FadeMusclesPowerMultiplicator(0.1f, 0.3f);
                PrintMessage("Exploded");

            break;

            //:::::::::::
            //:: Sever ::
            //:::::::::::
            case ReactionType.Dismember:

                //yield return new WaitForFixedUpdate();
                //yield return new WaitForFixedUpdate();

                //Transform sourceBone = DummyBoneToSourceBone(hitBone);
                //if(sourceBone == null)
                //dismember.TryDismember(sourceBone, ra2Bone);
                PrintMessage("Dismembered");

            break;


        }

        //rag.User_UpdateAllBonesParametersAfterManualChanges();

        //if(reaction == ReactionType.explode && dismember != null) {
        //    yield return new WaitForFixedUpdate();
        //    yield return new WaitForFixedUpdate();

        //    dismember.DismemberAll();
        //    yield return new WaitForFixedUpdate();//wait for RA2 to finish separating joints

        //    ApplyExplosionForce(hitPoint, force, 5f, 0.5f);
        //    yield break;
        //}


        //if(tryDismember && dismember != null) {
        //    yield return new WaitForFixedUpdate();
        //    yield return new WaitForFixedUpdate();

        //    Transform sourceBone = DummyBoneToSourceBone(hitBone);
        //    if(sourceBone.name == "Head") yield break;
        //    dismember.TryDismember(sourceBone, ra2Bone);
        //}

    }


    //private void ApplyExplosionForce( Vector3 explosionPosition, float force, float radius, float upwardsModifier = 0f) {
    //    HashSet<Rigidbody> uniqueBodies = new HashSet<Rigidbody>(DummyRbDict.Values);

    //    foreach(Rigidbody rb in uniqueBodies) {
    //        if(rb == null) continue;

    //        rb.AddExplosionForce(
    //            force,
    //            explosionPosition,
    //            radius,
    //            upwardsModifier,
    //            ForceMode.Impulse
    //        );
    //    }

    //}


    #endregion


    //========================//
    // Hit/reaction behaviour //
    //========================//
    #region

    public bool ResolveHit(Transform hitTransform, out BodyPart part, out Transform sourceBone) {
        for(Transform t = hitTransform; t != null; t = t.parent) {
            if(BodyPartDict.TryGetValue(t, out part) && part != BodyPart.Other) {
                sourceBone = t;
                return true;
            }
        }

        part = BodyPart.Other;
        sourceBone = null;
        return false;
    }


    private Rigidbody GetCentralRigidbody() {
        Transform hip = GetSourceBone("hip");

        if(hip != null &&DummyRbDict.TryGetValue(hip, out Rigidbody hipRb)) {
            return hipRb;
        }

        PrintError("Could not find central hip Rigidbody.");
        return null;
    }


    #endregion


    //==============================//
    // Bodypart conversions/lookups //
    //==============================//
    #region

    //private readonly Dictionary<string, Rigidbody> ragRbDict = new();

    private void BuildSourceBoneDict() {
        if(armatureRoot == null) return;
        if(SourceBoneDict.Count > 0) return;
        SourceBoneDict.Clear();

        foreach(Transform bone in armatureRoot.GetComponentsInChildren<Transform>(true)) {
            SourceBoneDict[bone.name] = bone;
        }

        PrintDict("Source Bones: ", SourceBoneDict);
    }

    private void BuildDerivedDicts() {
        if(SourceBoneDict.Count == 0) { PrintError("SourceBoneDict is empty"); return; }
        //if(ragHandler == null) { PrintError("no ragHandler reference found"); return; }

        BodyPartDict.Clear();
        //RA2BoneDict.Clear();
        DummyRbDict.Clear();

        foreach(Transform t in SourceBoneDict.Values) {
            if(t == null) continue;

            //1. BodyPart enum dict
            BodyPartDict[t] = ClassifyBone(t.name);//transform.name (str) ---> part (BodyPart) mapping

            ////2. ra2bone dict
            //RagdollChainBone ra2Bone = ragHandler.User_GetBoneSetupBySourceAnimatorBone(t);
            //if(ra2Bone == null) continue;
            //RA2BoneDict[t] = ra2Bone;

            ////3.DummyRbDict
            //if(ra2Bone == null) continue;
            //Rigidbody dummyRb = ra2Bone.PhysicalDummyBone.GetComponent<Rigidbody>();
            //if(dummyRb == null) continue;
            //DummyRbDict[t] = dummyRb;
        }

        //PrintMessage($"Built source-derived dicts. parts:{BodyPartDict.Count} ra2:{RA2BoneDict.Count} rbs:{DummyRbDict.Count}");
    }

    private void BuildProfileDict() {
        profileDict.Clear();
        foreach(BodyPartProfile p in profiles) {
            if(p == null) continue;
            profileDict[p.part] = p;
        }
    }

    public BodyPartProfile GetProfile(BodyPart part) {
        return profileDict.TryGetValue(part, out BodyPartProfile p) ? p : defaultProfile;
    }

    //public Transform DummyBoneToSourceBone(Transform dummyBone) {
    //    //dummy bone -> source bone, so it matches the source keyed dicts
    //    Transform sourceBone = rag.User_GetSourceBoneByPhysicalBone(dummyBone);
    //    if(sourceBone != null) dummyBone = sourceBone;
    //    return dummyBone;
    //}

    public Transform GetSourceBone(string boneName) {
        SourceBoneDict.TryGetValue(boneName, out Transform bone);
        return bone;
    }

    public BodyPart ClassifyBone(string boneName) {
        if(string.IsNullOrEmpty(boneName)) return BodyPart.Other; //part w no RA2 component

        string nameBodyPart = boneName.ToLower();

        //::TODO:: MAKE FUNCTION FOR nameBodyPart.Contains("");
        bool head = nameBodyPart.Contains("head");
        bool torso = nameBodyPart.Contains("chest") || nameBodyPart.Contains("hips");
        bool armLeft = nameBodyPart.Contains("upperarm.l") || nameBodyPart.Contains("lowerarm.l") || nameBodyPart.Contains("hand.l");
        bool armRight = nameBodyPart.Contains("upperarm.r") || nameBodyPart.Contains("lowerarm.r") || nameBodyPart.Contains("hand.r");
        bool legLeft = nameBodyPart.Contains("upperleg.l") || nameBodyPart.Contains("lowerleg.l") || nameBodyPart.Contains("foot.l");
        bool legRight = nameBodyPart.Contains("upperleg.r") || nameBodyPart.Contains("lowerleg.r") || nameBodyPart.Contains("foot.r");
        
        if (armRight) { return BodyPart.ArmRight; } 
        else if (armLeft) { return BodyPart.ArmLeft; } 
        else if (legRight) { return BodyPart.LegRight; } 
        else if (legLeft) { return BodyPart.LegLeft; }
        else if (torso) { return BodyPart.Chest; } 
        else if (head) { return BodyPart.Head; } 
        else { return BodyPart.Other; }
        /*
        //bool arms =
        //    nameBodyPart.Contains("upperarm.l") || nameBodyPart.Contains("upperarm.r") ||
        //    nameBodyPart.Contains("lowerarm.l") || nameBodyPart.Contains("lowerarm.r") ||
        //    nameBodyPart.Contains("hand.l") || nameBodyPart.Contains("hand.r");
        //bool legs = 
        //    nameBodyPart.Contains("upperleg.l") || nameBodyPart.Contains("lowerleg.l") ||
        //    nameBodyPart.Contains("upperleg.r") || nameBodyPart.Contains("lowerleg.r") ||
        //    nameBodyPart.Contains("foot.l") || nameBodyPart.Contains("foot.r");
        */
    }

    public void PrintDict<TKey, TValue>(string header, Dictionary<TKey, TValue> dict) {
        string formattedString = string.Join(
            Environment.NewLine,
            dict.Select(
                kvp => $"NAME(str): {kvp.Key}   =   {kvp.Value} ... type: {kvp.Value.GetType().Name}"
                )
            );

        PrintMessage($"{header}:\n{formattedString}");
        //AddInfoValue($"{header}:\n",formattedString);
    }

    /*
       Head,
       Chest,
       Hip,
       Arm,
       Hand,
       Leg,
       Knee,
       Other

       hip,
           chest,
               head,
               upperarm.l
                   lowerarm.l
                       hand.l
               upperarm.r
                   lowerarm.r
                       hand.r
               upperleg.l
                   lowerleg.l
                       foot.l
               upperleg.r
                   lowerleg.r
                       foot.r
       */


    #endregion


    //=============================//
    // SPECIFIC BODYPART BEHAVIOUR //
    //=============================//
    #region


    public bool IsHandHit(Transform hitBone) {
        if(hitBone == null) return false;

        Transform sourceBone = hitBone;
        if(sourceBone == null) return false;

        string boneName = sourceBone.name.ToLower();

        return boneName.Contains("hand.l");
    }


    #endregion


}

