//using FIMSpace.FProceduralAnimation;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCBodyPartHandler : SneedSpace
{
    //=======//
    // Setup //
    //=======//
    #region setup

    //::::::::::::::::::::
    //:: cut definition ::
    //::::::::::::::::::::

    [Serializable]
    private sealed class CutDefinition
    {
        public string cutName;
        [Header("Cut Bone")]             public Transform detachedRootBone;//Source armature bone used to identify this cut.
        [Tooltip("Cut Material")]        public Material dismemberedCutMaterial;
        [Header("Cut Material Index")]   public int bodyMaterialIndex;//Material slot on the living body's SkinnedMeshRenderer.
        [Header("Detached Chain Blood")] public Texture2D detachedChainMask;
        // UV1 mask for the entire detached chain
        // White = receives detached-limb blood
        // Example: LowerLegL mask contains LowerLegL + FootL

        public NPCLimbHealth limbType;
        [NonSerialized] public bool detached;
        [NonSerialized] public int currentHealth;


    }

    //:::::::::::::::::::::
    //:: main references ::
    //:::::::::::::::::::::

    [Header("RA2")]
    //[SerializeField] private RagdollAnimator2 ragdollAnimator;
    [SerializeField] private string featureName = "Dismemberment";

    [Header("Body Mesh")]
    [SerializeField] private SkinnedMeshRenderer bodyRenderer;

    [Tooltip("Default body material used by every body material slot before dismemberment")]
    [SerializeField] private Material defaultBodyMaterial;

    [Header("Cuts")]
    [SerializeField] private CutDefinition[] cuts;


    //::::::::::::::::::::::::::
    //:: Individual Limb data ::
    //::::::::::::::::::::::::::
    #region limbs
    [SerializeField] private Transform Head;
    [SerializeField] private Transform leftLegUpper;
    [SerializeField] private Transform leftLegLower;
    [SerializeField] private Transform leftFoot;
    [SerializeField] private Transform leftArmUpper;
    [SerializeField] private Transform leftArmLower;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightLegUpper;
    [SerializeField] private Transform rightLegLower;
    [SerializeField] private Transform rightFoot;
    [SerializeField] private Transform rightArmUpper;
    [SerializeField] private Transform rightArmLower;
    [SerializeField] private Transform rightHand;

    public bool LeftLegUpper => IsAttachedToBody(leftLegUpper);
    public bool LeftLegLower => IsAttachedToBody(leftLegLower);
    public bool LeftFoot => IsAttachedToBody(leftFoot);
    public bool LeftArmUpper => IsAttachedToBody(leftArmUpper);
    public bool LeftArmLower => IsAttachedToBody(leftArmLower);
    public bool LeftHand => IsAttachedToBody(leftHand);
    public bool RightLegUpper => IsAttachedToBody(rightLegUpper);
    public bool RightLegLower => IsAttachedToBody(rightLegLower);
    public bool RightFoot => IsAttachedToBody(rightFoot);
    public bool RightArmUpper => IsAttachedToBody(rightArmUpper);
    public bool RightArmLower => IsAttachedToBody(rightArmLower);
    public bool RightHand => IsAttachedToBody(rightHand);
    #endregion

    //:::::::::::::::::::::::
    //:: shader properties ::
    //:::::::::::::::::::::::

    private static readonly int BloodMaskID = Shader.PropertyToID("_BloodMask");
    private readonly List<Material> runtimeCutMaterials = new();// Runtime wound materials created when cuts happen.
    private Material runtimeDefaultMaterial;
    private Texture2D accumulatedBloodMask;
    private Color32[] accumulatedMaskPixels;// CPU copy of accumulated mask pixels.

    //:::::::::::::
    //:: Default ::
    //:::::::::::::

    private void Awake() {

        //if(ragdollAnimator == null) ragdollAnimator = GetComponent<RagdollAnimator2>();
        if(bodyRenderer != null) bodyRenderer.updateWhenOffscreen = true;

        InitializeBodyMaterials();//fill all 14 slots with default
        InitializeAccumulatedMask();

        foreach(CutDefinition cut in cuts) {
            cut.currentHealth = (int)cut.limbType;
            //PrintMessage($"LIMBTYPE:{cut.limbType}, HEALTH: {cut.currentHealth}");
        }
    }

    private void OnDestroy() {

        if(runtimeDefaultMaterial != null) {
            Destroy(runtimeDefaultMaterial);
        }

        foreach(Material material in runtimeCutMaterials) {
            if(material != null) Destroy(material);
        }

        runtimeCutMaterials.Clear();

        if(accumulatedBloodMask != null) {
            Destroy(accumulatedBloodMask);
        }

    }

    private void Update() {

        //if(!LeftLegUpper) PrintMessage("LeftLegUpper DISMEMBERED");
        //if(!LeftLegLower ) PrintMessage("LeftLegLower DISMEMBERED");
        //if(!LeftFoot ) PrintMessage("LeftFoot DISMEMBERED");
        //if(!LeftArmUpper ) PrintMessage("LeftArmUpper DISMEMBERED");
        //if(!LeftArmLower ) PrintMessage("LeftArmLower DISMEMBERED");
        //if(!LeftHand ) PrintMessage("LeftHand DISMEMBERED");
        //if(!RightLegUpper ) PrintMessage("RightLegUpper DISMEMBERED");
        //if(!RightLegLower ) PrintMessage("RightLegLower DISMEMBERED");
        //if(!RightFoot ) PrintMessage("RightFoot DISMEMBERED");
        //if(!RightArmUpper ) PrintMessage("RightArmUpper DISMEMBERED");
        //if(!RightArmLower ) PrintMessage("RightArmLower DISMEMBERED");
        //if(!RightHand) PrintMessage("RightHand DISMEMBERED");
            

        
}

    #endregion setup


    //====================//
    // Runtime Body Setup //
    //====================//
    #region runtime mask->material 

    private void InitializeBodyMaterials() {

        if(bodyRenderer == null) {
            PrintError("Body Renderer is missing.");
            return;
        }

        if(defaultBodyMaterial == null) {
            PrintError("Default Body Material is missing.");
            return;
        }

        //THIS NPCs single material instance,Root NPC SKMs number of material slots, fill defaults
        runtimeDefaultMaterial = new Material(defaultBodyMaterial);
        Material[] materials = bodyRenderer.sharedMaterials;

        for(int i = 0; i < materials.Length; i++) {
            materials[i] = runtimeDefaultMaterial;
        }

        bodyRenderer.sharedMaterials = materials;
    }


    private void InitializeAccumulatedMask() {

        //:::::: Reference Mask :::::::

        Texture2D referenceMask = FindFirstConfiguredMask();
        if(referenceMask == null) {
            PrintError("No detached mask found, cant create accumulated blood mask");
            return;
        }

        int width = referenceMask.width;
        int height = referenceMask.height;

        //:::::: Accumulated Mask :::::::::

        accumulatedBloodMask =              new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false, linear: true);
        accumulatedBloodMask.name =         $"{gameObject.name}_AccumulatedBloodMask";
        accumulatedBloodMask.wrapMode =     TextureWrapMode.Clamp;
        accumulatedBloodMask.filterMode =   referenceMask.filterMode;

        accumulatedMaskPixels = new Color32[width * height]; //Start entirely black.
        Color32 black = new Color32(0, 0, 0, 255);
        for(int i = 0; i < accumulatedMaskPixels.Length; i++) {
            accumulatedMaskPixels[i] = black;
        }

        //::::: Apply Pixels to Mask, Mask to Material ::::::::

        accumulatedBloodMask.SetPixels32(accumulatedMaskPixels);
        accumulatedBloodMask.Apply(false, false);

        // shared default materials' base texture is now the mask, applies to entire character
        if(runtimeDefaultMaterial != null) {
            runtimeDefaultMaterial.SetTexture(BloodMaskID,accumulatedBloodMask);
        }
    }


    private Texture2D FindFirstConfiguredMask() {
        if(cuts == null) return null;

        foreach(CutDefinition cut in cuts) {
            if(cut == null) continue;
            if(cut.detachedChainMask != null) {
                return cut.detachedChainMask;
            }
        }

        return null;
    }


    #endregion


    //===================//
    // Dismember Request //
    //===================//
    #region Dismember


    public bool TryDismember(Transform sourceBone) {
        if(sourceBone == null) { PrintError("Source bone is null."); return false; }
        //if(ra2Bone == null)    { PrintError("RA2 bone is null.");    return false; }

        CutDefinition cut = FindCut(sourceBone);

        if(cut == null)  { /*PrintError($"No cut config for: {sourceBone.name}");*/   return false; }
        if(cut.detached) { PrintError($"Cut: {cut.cutName} is already detached"); return false; }


        //Detach from root armature, still childed
        //RAF_DismembermentManager manager = GetDismembermentManager();
        //if(manager == null) 
        //    return false;

        //manager.DismemberBone(ra2Bone,EDismemberType.Disconnect);
        cut.detached = true;


        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //:: Add detached chain to NPCs accumulated blood mask ::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::
        AddDetachedChainToBloodMask(cut);

        //:::::::::::::::::::::::::::::::::::::::::::::::::::
        //:: Replace part material with wound/cut material ::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::
        ApplyCutMaterial(cut);

        //PrintMessage($"Dismembered ({cut.cutName}) from bone ({sourceBone.name})");
        return true;
    }


    public void DismemberAll() {
        if(cuts == null || cuts.Length == 0) return;
        
        //RAF_DismembermentManager manager = GetDismembermentManager();
        //if(manager == null)
        //    return;

        //distal bones first, Hand -> LowerArm -> UpperArm
        List<CutDefinition> orderedCuts = new List<CutDefinition>(cuts);
        orderedCuts.Sort(
            (a, b) =>
                GetBoneDepth(b?.detachedRootBone).CompareTo(GetBoneDepth(a?.detachedRootBone))
        );

        foreach(CutDefinition cut in orderedCuts) {
            if(cut == null) continue;
            if(cut.detached) continue;
            if(cut.detachedRootBone == null) continue;

            //RagdollChainBone ra2Bone = ragdollAnimator.GetRagdollHandler
            //    .User_GetBoneSetupBySourceAnimatorBone(cut.detachedRootBone);

            //if(ra2Bone == null) {
            //    PrintError($"Could not resolve RA2 bone for {cut.cutName}");
            //    continue;
            //}

            ////RA2 disconnect
            //manager.DismemberBone(ra2Bone, EDismemberType.Disconnect);
            cut.detached = true;

            AddDetachedChainToBloodMask(cut); //add limb chain to accumulated blood mask
            ApplyCutMaterial(cut); //apply cut local wound material
        }

        //PrintMessage("Dismembered all configured cuts.");
    }


    private bool IsAttachedToBody(Transform limbBone) {
        if(limbBone == null) return false;

        Transform current = limbBone;
        while(current != null) {
            foreach(CutDefinition cut in cuts) {
                if(cut == null) continue;
                if(cut.detachedRootBone == current && cut.detached) {
                    return false;
                }
            }
            current = current.parent;
        }

        return true;
    }


    private CutDefinition FindCut(Transform sourceBone) {
        if(cuts == null) return null;

        Transform current = sourceBone;

        while(current != null) {
            foreach(CutDefinition cut in cuts) {
                if(cut == null)continue;
                if(cut.detachedRootBone == current) return cut;
            }
            current = current.parent;
        }

        return null;
    }


    public bool ApplyLimbDamage(Transform sourceBone, int damage) {
        CutDefinition cut = FindCut(sourceBone);

        if(cut == null || cut.detached)
            return false;

        cut.currentHealth -= damage;

        return cut.currentHealth <= 0;
    }


    private int GetBoneDepth(Transform bone) {
        if(bone == null) return -1;

        int depth = 0;
        Transform current = bone;

        while(current.parent != null) {
            depth++;
            current = current.parent;
        }

        return depth;
    }


    //private RAF_DismembermentManager GetDismembermentManager() {
    //    if(ragdollAnimator == null) { PrintError("Missing RagdollAnimator2."); return null; }

    //    RagdollHandler handler = ragdollAnimator.GetRagdollHandler;

    //    if(handler == null) {
    //        PrintError("RA2 handler is null.");
    //        return null;
    //    }

    //    RagdollAnimatorFeatureHelper helper = handler.GetExtraFeatureHelper(featureName);

    //    if(helper == null) {
    //        PrintError($"No RA2 Extra feature named '{featureName}'.");
    //        return null;
    //    }

    //    RAF_DismembermentManager manager = helper.ActiveFeature as RAF_DismembermentManager;

    //    if(manager == null) {
    //        PrintError($"Feature '{featureName}' is not an active " + $"{nameof(RAF_DismembermentManager)}.");
    //        return null;
    //    }

    //    return manager;
    //}


    #endregion


    //============//
    // Blood Mask //
    //============//
    #region bloodmask


    private bool AddDetachedChainToBloodMask(CutDefinition cut) {
        if(cut.detachedChainMask == null) {
            PrintError($"Cut '{cut.cutName}' has no detached chain mask.");
            return false;
        }

        if(accumulatedBloodMask == null ||accumulatedMaskPixels == null) {
            PrintError("Accumulated blood mask was not initialized.");
            return false;
        }

        Texture2D incomingMask = cut.detachedChainMask;
        if(incomingMask.width != accumulatedBloodMask.width || incomingMask.height != accumulatedBloodMask.height) {
            PrintError(
                $"Mask '{incomingMask.name}' for cut " +
                $"'{cut.cutName}' has size " +
                $"{incomingMask.width}x{incomingMask.height}, " +
                $"but accumulated mask is " +
                $"{accumulatedBloodMask.width}x" +
                $"{accumulatedBloodMask.height}."
            );
            return false;
        }

        if(!incomingMask.isReadable) {
            PrintError($"Mask '{incomingMask.name}' is not readable, Enable Read/Write in its Unity import settings.");
            return false;
        }

        Color32[] incomingPixels = incomingMask.GetPixels32();

        // (MAX) or (OR) the new mask into accumulated mask
        // existing white stays, new white  is added
        // Grey values are preserved for anti-aliased edges.
        for(int i = 0; i < accumulatedMaskPixels.Length; i++) {

            byte value = Math.Max(
                accumulatedMaskPixels[i].r,
                incomingPixels[i].r);

            accumulatedMaskPixels[i] = new Color32(value, value, value, 255);
        }

        accumulatedBloodMask.SetPixels32(accumulatedMaskPixels);
        accumulatedBloodMask.Apply(false, false);
        return true;
    }


    private bool ApplyCutMaterial(CutDefinition cut) {
        if(bodyRenderer == null) {
            PrintError("Body Renderer is missing.");
            return false;
        }

        if(cut.dismemberedCutMaterial == null) {
            PrintError($"Cut {cut.cutName} has no dismembered material");
            return false;
        }

        Material[] materials = bodyRenderer.sharedMaterials;
        if(cut.bodyMaterialIndex < 0 || cut.bodyMaterialIndex >= materials.Length) {
            PrintError($"Cut {cut.cutName} has invalid material index {cut.bodyMaterialIndex}");
            return false;
        }

        materials[cut.bodyMaterialIndex] = cut.dismemberedCutMaterial;
        bodyRenderer.sharedMaterials = materials;

        //::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //:: Create copy of project material, DO NOT OVERRIDE ::
        //::::::::::::::::::::::::::::::::::::::::::::::::::::::
        Material runtimeCutMaterial = new Material(cut.dismemberedCutMaterial);
        runtimeCutMaterial.name = $"{cut.dismemberedCutMaterial.name}_Runtime";

        //Wound material ALSO uses this NPCs accumulated detached limb blood mask
        if(accumulatedBloodMask != null) {
            runtimeCutMaterial.SetTexture(BloodMaskID,accumulatedBloodMask);
        }

        runtimeCutMaterials.Add(runtimeCutMaterial);

        //replace only this cuts material slot
        materials[cut.bodyMaterialIndex] =runtimeCutMaterial;
        bodyRenderer.sharedMaterials = materials;

        return true;
    }


    #endregion


}
