using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : SneedSpace
{
    [Space(20)]
    [Header("Wave Settings")]
    [SerializeField] private float spacing = 1.0f;   //distance between slots
    [SerializeField] private bool randomYaw = false; //optional
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject spawnLocation;
    [SerializeField] GameObject spawnLocationStatic;
    [SerializeField] int numEnemies;
    [SerializeField] int numEnemiesStatic;

    private GameObject[] enemies;
    private GameObject[] enemiesStatic;
    private bool canSpawn;
    private int enemyCounter = 0;

    void Start() {

        //allocate fixed slots
        enemies = new GameObject[numEnemies];
        enemiesStatic = new GameObject[numEnemiesStatic];

        //fill on start
        RefillMissingSlots(enemyPrefab, enemies, spawnLocation, numEnemies, true, true);
        RefillMissingSlots(enemyPrefab, enemiesStatic, spawnLocationStatic, numEnemiesStatic, false, false);
    }

    void Update() {

        //number of alive enemies = numEnemies. when a slot is null (enemy destroyed), respawn just that one
        RefillMissingSlots(enemyPrefab, enemies, spawnLocation, numEnemies, true, true);
        RefillMissingSlots(enemyPrefab, enemiesStatic, spawnLocationStatic, numEnemiesStatic, false, false);
    }


    //manual refill trigger (call from UI button)
    public void ForceRefill() { RefillMissingSlots(enemyPrefab, enemies, spawnLocation, numEnemies, false, true); }


    private void RefillMissingSlots(GameObject enemyPrefab, GameObject[] enemies, GameObject location, int numEnemies, bool randRot, bool canMove) {
        if(enemyPrefab == null || location == null) return;

        //if numEnemies was changed in inspector at runtime, resize
        if(enemies == null || enemies.Length != numEnemies) {
            enemies = new GameObject[numEnemies];
        }

        for(int i = 0; i < enemies.Length; i++) {
            if(enemies[i] != null) continue;
            //if(!canMove) disableMovement(enemies[i]);

            enemies[i] = SpawnOne(location, i, enemyPrefab, randRot, canMove);
        }
    }


    private GameObject SpawnOne(GameObject location, int slotIndex, GameObject enemyPrefab, bool randRot, bool canMove) {

        //slot based offset (line formation), uses spawnLocation.right so it respects rotation
        Vector3 basePos = location.transform.position;
        Vector3 offset  = location.transform.right * (slotIndex * spacing);
        Vector3 pos     = basePos + offset;
        Quaternion rot  = location.transform.rotation;
        if(!randRot) {
            if(randomYaw) { rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f); }
        }

        GameObject entity = Instantiate(enemyPrefab, pos, rot);
        if(!canMove) { disableMovement(entity); }

        enemyCounter++;
        return entity;
    }


    private void disableMovement(GameObject enemy) {
        if(enemy == null) return;
        
        NPCMovement wandering = enemy.GetComponentInChildren<NPCMovement>();
        Animator animator = enemy.GetComponentInChildren<Animator>();
        if(wandering != null) 
            wandering.enabled = false;
        if(animator != null) {
            //animator.enabled = false;
            //animator.def
            //animator.Play("Idle", -1, 0f);
            //animator.Play("Cultist_idle", -1, 0f);
        }
        //PrintMessage("No wandering behaviour found");
    }


}
