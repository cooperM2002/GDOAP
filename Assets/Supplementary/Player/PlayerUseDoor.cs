using UnityEngine;

public class PlayerUseDoor : SneedSpace
{

    public float radius = 1.5f;

    //start
    void Start()
    {
        
    }

    //update
    void Update() {
        if (Input.GetKeyDown(KeyCode.E)) {
        
            Collider[] HitColliders = Physics.OverlapSphere(transform.position, radius);
            foreach(Collider hitCollider in HitColliders) {

                Vector3 hitPosition = hitCollider.transform.position;
                hitPosition.y=transform.position.y;

                Vector3 direction = hitPosition - transform.position;
                if (Vector3.Dot(transform.forward, direction.normalized) > 0.5f) {
                    hitCollider.SendMessage("Operate", SendMessageOptions.DontRequireReceiver);
                }

            }
        }

    }


}
