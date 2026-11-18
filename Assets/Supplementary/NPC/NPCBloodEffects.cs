using UnityEngine;

public class BloodEffects : SneedSpace {

    [Space(20)]

    [SerializeField] private GameObject bloodSpray;
    [SerializeField] private BloodDecalSpawner bloodDecalSpawner;
    [SerializeField] private GameObject oneShotBloodPrefab;


    private void Awake() {
        bloodDecalSpawner = bloodSpray.GetComponentInChildren<BloodDecalSpawner>();
        if(bloodDecalSpawner == null) {
            PrintError("No Decal Spawner found.");
        }
    }


    public void SpawnBloodSpray(RaycastHit hit, NPCRagdollHandler ragdoll) {

        if(bloodSpray == null) { 
            PrintMessage("blood spray is NULLLLLLLL"); 
            return; 
        }
        if(hit.transform == null) { 
            PrintMessage("hit.transform is NULLLLLLLLLLLLLL"); 
            return; 
        }

        Transform bone = ragdoll.GetSourceBone(hit.transform.name);
        Transform parent = bone != null ? bone.parent : hit.transform;

        Quaternion rot = Quaternion.LookRotation(hit.normal);
        GameObject blood = Instantiate(bloodSpray, hit.point, rot);
        blood.transform.SetParent(bone, worldPositionStays: true);

        //account for scale, ie scale of parent armature is 0.1 -> to to 1
        Vector3 parentScale = parent.lossyScale;
        //blood.transform.localScale = new Vector3(1f /parentScale.x, 1f/parentScale.y,1f/parentScale.z);
        blood.transform.localScale = parentScale / 100f;


        blood.GetComponent<ParticleSystem>().Play();
        Destroy(blood, 10f);
    }


    public void SpawnBloodOneShot(RaycastHit hit, Vector3 direction) {

        if(bloodSpray == null)    { PrintMessage("blood spray is NULLLLLLLL"); return; }
        
        if(!TryGetExitPoint(hit, direction, out Vector3 exitPoint)) {
            return;
        }
        
        if(hit.transform == null) { PrintMessage("hit.transform is NULLLLLL"); return; }

        Vector3 dir = direction.normalized;
        Vector3 spawnPoint = exitPoint + direction * 0.02f;


        //Transform bone = ragdoll.GetSourceBone(hit.transform.name);
        Quaternion rot = Quaternion.LookRotation(dir);
        //Transform parent = 
        //    bone != null 
        //        ? bone.parent 
        //        : hit.transform;

        GameObject blood = Instantiate(
            oneShotBloodPrefab,
            spawnPoint, 
            rot
        );
        //blood.transform.SetParent(bone, worldPositionStays: true);

        //account for scale, ie scale of parent armature is 0.1 -> to to 1
        //Vector3 parentScale = parent.lossyScale;
        //blood.transform.localScale = new Vector3(1f /parentScale.x, 1f/parentScale.y,1f/parentScale.z);
        //blood.transform.localScale = parentScale / 100f;


        blood.GetComponent<ParticleSystem>().Play();
        Destroy(blood, 100f);
        PrintMessage("END of bloodimpact");
    }

    private bool TryGetExitPoint(RaycastHit hit, Vector3 direction, out Vector3 exitPoint) {
        exitPoint = hit.point;
        Vector3 dir = direction.normalized;
        float dist = 2f;
        Vector3 reverseStart = hit.point + dir * dist;

        Ray reverseRay = new Ray(reverseStart, -dir);

        if(hit.collider.Raycast(reverseRay, out RaycastHit exitHit, dist + 0.1f)) {
            exitPoint = exitHit.point;
            return true;
        }


        return false;
    }


    public void SpawnBloodImpact(RaycastHit hit, NPCRagdollHandler ragdoll, GameObject bloodPrefab) {

        if(bloodSpray == null) { PrintMessage("blood spray is NULLLLLLLL"); return; }
        if(hit.transform == null) { PrintMessage("hit.transform is NULLLLLLLLLLLLLL"); return; }

        //Transform sourceBone = ragdoll.DummyBoneToSourceBone(hit.transform);
        Transform bone = ragdoll.GetSourceBone(hit.transform.name);
        //Transform bone = hit.transform;

        Transform parent = bone != null ? bone.parent : hit.transform;
        Quaternion rot = Quaternion.LookRotation(hit.normal);


        //GameObject blood = Instantiate(bloodSpray, hit.point, rot);
        GameObject blood = Instantiate(bloodPrefab, hit.point, rot, bone);
        blood.transform.SetParent(bone, worldPositionStays: true);
        //blood.transform.position = parent.position;

        //account for scale, ie scale of parent armature is 0.1 -> to to 1
        Vector3 parentScale = parent.lossyScale;
        //blood.transform.localScale = new Vector3(1f /parentScale.x, 1f/parentScale.y,1f/parentScale.z);
        blood.transform.localScale = parentScale / 100f;


        blood.GetComponent<ParticleSystem>().Play();
        Destroy(blood, 10f);
        PrintMessage("END of bloodimpact");
    }

    public void SpawnRatBloodImpact(RaycastHit hit, GameObject bloodPrefab) {

        if(bloodSpray == null) { PrintMessage("blood spray is NULLLLLLLL"); return; }
        if(hit.transform == null) { PrintMessage("hit.transform is NULLLLLLLLLLLLLL"); return; }

        //Transform sourceBone = ragdoll.DummyBoneToSourceBone(hit.transform);
        Transform bone = hit.transform;
        Transform parent = bone != null ? bone.parent : hit.transform;
        Quaternion rot = Quaternion.LookRotation(hit.normal);


        //GameObject blood = Instantiate(bloodSpray, hit.point, rot);
        GameObject blood = Instantiate(bloodPrefab, hit.point, rot);
        //blood.transform.SetParent(bone, worldPositionStays: true);

        //account for scale, ie scale of parent armature is 0.1 -> to to 1
        Vector3 parentScale = parent.lossyScale;
        //blood.transform.localScale = new Vector3(1f /parentScale.x, 1f/parentScale.y,1f/parentScale.z);
        blood.transform.localScale = parentScale / 100f;


        blood.GetComponent<ParticleSystem>().Play();
        Destroy(blood, 10f);
        PrintMessage("END of bloodimpact");
    }
}
