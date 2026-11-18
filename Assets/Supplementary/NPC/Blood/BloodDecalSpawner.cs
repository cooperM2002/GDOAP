using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BloodDecalSpawner : SneedSpace {

    [SerializeField] private GameObject bloodDecal;
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private int maxDecals = 350;
    [SerializeField] private float offset = 0.1f;//avoid z-fighting

    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();//individual particle collisions
    private static Queue<GameObject> activeDecals = new Queue<GameObject>();//spawned decals lineup


    private void Awake() {
        if(ps == null) ps = GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject other) {
        //PrintMessage("Entered OnParticleCollision");
        int count = ps.GetCollisionEvents(other, collisionEvents);

        Vector3 p = collisionEvents[0].intersection;
        Vector3 n = collisionEvents[0].normal;
        Quaternion rot = Quaternion.LookRotation(-n); // -Z = forward = -normal
        Vector3 spawnPosition = p+n*offset; // z-fighting

        // point projector INTO the surface 
        float roll = Random.Range(0f, 360f); // random roll for orientation variation
        rot *= Quaternion.Euler(0f, 0f, roll); // only Z
        GameObject decal = Instantiate(bloodDecal, spawnPosition, rot);


        DecalProjector pj = decal.GetComponent<DecalProjector>();
        if(pj == null) 
            return;

        //size based on modulo
        float size = Random.Range(.05f, 3.5f);
        float opacity = Random.Range(.05f, 1f);

        pj.size = new Vector3(size, size, pj.size.z);
        pj.fadeFactor = opacity;
            
        //:::::::::::::::::::::
        //:: pool management ::
        //:::::::::::::::::::::

        activeDecals.Enqueue(decal);
        if(activeDecals.Count > maxDecals) {

            GameObject oldest = activeDecals.Dequeue();
            if(oldest != null) {
                Destroy(oldest);
            }

        }
        
    }


}
