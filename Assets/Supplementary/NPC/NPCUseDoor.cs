using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCUseDoor : SneedSpace {

    [SerializeField] private float doorOpenWaitTime = 0.5f;

    private NavMeshAgent agent;
    private bool handlingLink;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();

        //pause at link
        agent.autoTraverseOffMeshLink = false;
    }

    private void Update() {

        if(handlingLink)
            return;

        if(agent.isOnOffMeshLink) {
            StartCoroutine(HandleCurrentLink());

        }
    }

    private IEnumerator HandleCurrentLink() {

        handlingLink = true;

        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        DoorNavLink doorLink = GetDoorNavLink(linkData);

        if(doorLink != null) {
        
            agent.isStopped = true;
            doorLink.AgentOpenDoor();

            //wait for door anim finish
            yield return new WaitForSeconds(doorOpenWaitTime);            
            agent.isStopped = false;

        }

        yield return TraverseLink(linkData);
        agent.CompleteOffMeshLink();
        handlingLink = false;

    }

    private IEnumerator TraverseLink(OffMeshLinkData linkData) {
        Vector3 start = transform.position;
        Vector3 end = linkData.endPos + Vector3.up * agent.baseOffset;

        float d = Vector3.Distance(start, end);
        float duration = d / Mathf.Max(agent.speed, 0.01f);

        float elapsed = 0f;
        while(elapsed < duration) {

            float t = elapsed / duration;
            transform.position = Vector3.Lerp(start, end, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
    }

    private DoorNavLink GetDoorNavLink(OffMeshLinkData linkData)
{
        Object owner = linkData.owner;

        if (owner == null)
            return null;

        if (owner is GameObject ownerGameObject)
            return ownerGameObject.GetComponent<DoorNavLink>();

        if (owner is Component ownerComponent)
            return ownerComponent.GetComponent<DoorNavLink>();

        return null;
    }


}
