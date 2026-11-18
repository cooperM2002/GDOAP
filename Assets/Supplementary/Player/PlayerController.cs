using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : SneedSpace
{
    //========//
    // Fields //
    //========//
    #region FIELDS


    [Space(20)]
    [Header("Stats")]
    //[SerializeField] private LifeState state = LifeState.Alive;
    [SerializeField] private float startHealth;
    //[SerializeField] private float walkSpeed;//maybe change speed based on health
    //[SerializeField] private float runSpeed;
   
    private bool isAlive = true;
    private bool canDie;
    public float damageCount;
    private Coroutine reactRountine;
    //private int ragdolledHash;

    public bool IsAlive => isAlive;
    public float Health => startHealth;
    public bool CanDie => canDie;
   

    #endregion


    //=================//
    // DEFAULT METHODS //
    //=================//
    #region DEFAULT METHODS


    private void Awake() {

    }


    void Start() {
        isAlive = true;
        //startHealth = 100;

        StartInfo();
    }


    private void StartInfo() {
        ClearPrintInfo();

        //AddInfoHeader("References");
        //AddInfoRefCheck("Wandering AI", movement);

        //AddInfoHeader("Attributes");
        //AddInfoValue("Health", startHealth);

        //AddInfoHeader("Conditions");
        //AddInfoCheck("Health above zero", startHealth > 0);

        PrintStartInfo();
    }


    private void Update() {
        canDie = (startHealth <= damageCount);
    }


    #endregion


    //================//
    // alive/dead/etc //
    //================//
    #region LIFECYCLE METHODS


    public void SetAlive(bool alive) {
        isAlive = alive;
        //state = LifeState.Alive;
    }


    public void ApplyDeath(Transform hitBone, Vector3 hitPoint, Vector3 hitNormal) {
        if(isAlive==false) return;
        //itemHolder?.DropHeldItem();//player drop item

        SetAlive(false); //set dead state


        //SetAnims(false);


        //StartCoroutine(DestroyAfterDelay(5f)); //get rid of instance
    }


    private IEnumerator DestroyAfterDelay(float seconds) {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }


    #endregion


    //===============//
    // knocked state //
    //===============//
    #region knocked


    public void ApplyReact(Transform hitBone, Vector3 hitPoint, Vector3 hitNormal) {

        //if(state == LifeState.Knocked) return;//block reknocking

    }




    public void OnGetUpAnimComplete() {
        PrintMessage("animation done");

        

        //if(movement != null) movement.enabled = true;
       
        //NPCHitManager hitManager = GetComponent<NPCHitManager>();
        //if(hitManager != null) hitManager.ResetReactionGate();

        StartDazedWalk(5f);
    }


    public void StartDazedWalk(float duration) {
        if(reactRountine != null) {
            StopCoroutine(reactRountine);
        }

        reactRountine = StartCoroutine(ReactRoutine(duration));
    }


    private IEnumerator ReactRoutine(float duration) {

        //animatorController?.SetDazed(true);
        yield return new WaitForSeconds(duration);
        //animatorController?.SetDazed(false);

        reactRountine = null;
    }


    #endregion


    //=================//
    // Ragdoll helpers //
    //=================//
    #region RAGDOLL METHODS


    //public void SetAnims(bool enabled) {

    //    //animatorController?.SetAnimatorsEnabled(enabled);

    //}


    #endregion
}
