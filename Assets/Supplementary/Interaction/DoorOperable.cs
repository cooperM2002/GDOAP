using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using static UnityEngine.GraphicsBuffer;

public class DoorOperable : SneedSpace {
    public Transform Hinge;

    public float openAngle = 80f;
    public float openSpeed = 80f;
    public bool hingeOnRight = true;
    public bool hingeOnTop = false;
    public bool hingeIsHorizontal = false;
    public bool slideUp = false;

    [SerializeField] private AudioSource soundSource;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;

    [SerializeField] private NavMeshObstacle obstacle;
    [SerializeField] private NavMeshLink link;


    private bool open;
    private float currentAngle;
    private float targetAngle;
    private Vector3 closedPos;
    private Vector3 openPos;
    BoxCollider doorCollider;

    public bool isOpen => open;

    void Awake() {
        doorCollider = GetComponent<BoxCollider>();
        if(doorCollider==null) { 
            PrintMessage($"Door needs a BoxCollider {this}"); 
            enabled = false; 
            return; 
        }

        SetDoor();

    }

    private void SetDoor() {
        Vector3 size = doorCollider.size * 0.5f;
        Vector3 center = doorCollider.center;

        Vector3 hingePos = transform.TransformPoint(
            hingeOnRight 
                ? center.x+size.x 
                : center.x-size.x,
            hingeOnTop 
                ? center.y+size.y 
                : center.y-size.y,
            center.z
            );

        Hinge.position = hingePos;
        Hinge.rotation = transform.rotation;

        closedPos = transform.position;
        // offset UP in world space by the door's local height; tweak multiplier to taste
        openPos = closedPos + transform.up * (doorCollider.size.y * 2);
    }

    public void Operate() {

        open = !open;
        targetAngle = open ? openAngle : 0f;   // still useful for the rotating case
        if(open) {
            PlayDoorSound(soundSource, openClip);
            obstacle.enabled = false;
            link.enabled = false;

        }
        if(!open) {
            PlayDoorSound(soundSource, closeClip);
            obstacle.enabled = true;
            link.enabled = true;

        }

        //if(!open) PlayDoorSoundReverse(soundSource, openClip);

        PrintMessage($"is open? {open}");
    }

    void Update() {
        //vent
        if(slideUp) {
            
            Vector3 target = open ? openPos : closedPos;
            transform.position = Vector3.MoveTowards(transform.position, target, openSpeed * Time.deltaTime);
        
        //door
        } else {
            
            float angle = Mathf.MoveTowardsAngle(
                currentAngle, 
                targetAngle, 
                ((openSpeed) * Time.deltaTime)/8
                );

            float angleDiff = Mathf.DeltaAngle(currentAngle, angle);
            
            if(Mathf.Abs(angleDiff) > 0.0001f) {
                Vector3 axis = hingeIsHorizontal 
                    ? Hinge.right 
                    : Hinge.up;
                
                transform.RotateAround(Hinge.position, axis, angleDiff);
            }

            currentAngle = angle;
        }


    }

    private void PlayDoorSound(AudioSource source, AudioClip clip) {
        source.Stop();
        source.clip = null;
        source.clip = clip;
        source.Play();
    }

    private void PlayDoorSoundReverse(AudioSource source, AudioClip clip) {
        source.Stop();
        source.clip = null;
        source.clip = clip;
        source.pitch = -1.0f; //pitch to negative to reverse direction
        source.timeSamples = source.clip.samples - 1; //point head to the very last sample of the clip

        source.Play();
    }
}
