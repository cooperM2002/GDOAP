using UnityEngine;


public class OperableSwitch : SneedSpace {

    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private float facingThreshold = 0.5f;
    [SerializeField] KeyCode interactKey = KeyCode.E;
    public OperableObject[] operables;
    private Collider rootCollider;


    void Update() {
        if (!Input.GetKeyDown(interactKey)) return;
        if (!PlayerInRange()) return;

        foreach(OperableObject i in operables) 
            if (i!=null) i.Operate();
    }


    private bool PlayerInRange() {
        rootCollider = GetComponent<Collider>();
        Vector3 center = rootCollider.bounds.center;
        Collider[] hits = Physics.OverlapSphere(center, interactionRadius);

        foreach(Collider i in hits) {
            Vector3 hitPos = i.transform.position;
            hitPos.y = transform.position.y;
            Vector3 dir = (hitPos - transform.position).normalized; // dir = target-origin, 0<=dir<=1

            if (Vector3.Dot(transform.forward, dir) > facingThreshold) 
                return true;
        }

        return false;
    }


    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * interactionRadius);
    }


}
