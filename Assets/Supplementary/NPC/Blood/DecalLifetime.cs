using UnityEngine;

public class DecalLifetime : MonoBehaviour
{
    [SerializeField] private float decalLifetime = 60f;
    void Start(){
        Destroy(gameObject, decalLifetime);
    }

}
