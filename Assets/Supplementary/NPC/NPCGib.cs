using Unity.VisualScripting;
using UnityEngine;

public class NPCGib : SneedSpace {
    [SerializeField] private float explosionForce;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float upwardsModifier;
    [SerializeField] private GameObject gibBloodSpray;
    [SerializeField] private BloodDecalSpawner bloodDecalSpawner;

    private GameObject[] gibs;
    private int childCount;

    private void Awake() {
        bloodDecalSpawner = gibBloodSpray.GetComponentInChildren<BloodDecalSpawner>();
        if(bloodDecalSpawner == null) {
            PrintError("No Decal Spawner found.");
        }

        GetGibs();
        if(gibs != null || gibs.Length > 0) {
            PrintMessage($"num gibs = {gibs.Length}");
        } else {
            PrintMessage("no gibs found");
        }

    }

    private void Start() {
        SpawnGibs();
    }

    private void GetGibs() {
        childCount = transform.childCount;
        gibs = new GameObject[childCount];
        for (int i = 0; i < childCount; i++) {
            Transform childTransform = transform.GetChild(i);//child transform
            gibs[i] = childTransform.gameObject;//access GameObject
        }
    }

    void SpawnGibs() {
        Vector3 explosionPoint = transform.position;//+ Vector3.down * 0.25f

        for (int i = 0; i< childCount; i++) {
            Rigidbody rb = gibs[i].GetComponent<Rigidbody>();
            rb.AddExplosionForce(explosionForce, explosionPoint, explosionRadius, upwardsModifier, ForceMode.Impulse);

            Transform gib = gibs[i].transform;
            spawnGibSpray(gib);
        }
    }

    private void spawnGibSpray(Transform gib) {
        if (gibBloodSpray == null) {
            PrintMessage("blood spray is NULLLLLLLL");
            return;
        }

        Quaternion rot = Quaternion.LookRotation(transform.forward, transform.up);
        GameObject blood = Instantiate(gibBloodSpray, transform.position, rot);
        blood.transform.SetParent(gib, worldPositionStays: true);

        //account for scale, ie scale of parent armature is 0.1 -> to to 1
        Vector3 parentScale = gib.lossyScale;
        //blood.transform.localScale = new Vector3(1f /parentScale.x, 1f/parentScale.y,1f/parentScale.z);
        blood.transform.localScale = parentScale / 100f;


        blood.GetComponent<ParticleSystem>().Play();
        Destroy(blood, 10f);
        PrintMessage("END of bloodspray");
    }

}
