using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TortoiseAgent : MonoBehaviour
{
    public enum PatrolPattern { roundRobin, random, pingPong }

    [SerializeField] private Animator animator;
    [Header("Navigation")]
    [SerializeField] private PatrolPattern pattern;
    [SerializeField] private Transform[] waypoints;
    [Header("Idle Behaviour")]
    [SerializeField] private float idleTimeMin;
    [SerializeField] private float idleTimeMax;
    [Header("Eating Behaviour")]
    [SerializeField] private bool canEat;
    [SerializeField] private float eatTimeMin;
    [SerializeField] private float eatTimeMax;
    [Header("Swimming")]
    [SerializeField] private LayerMask waterMask;
    [SerializeField] private float waterRayVerticalOffset = 0.05f;
    [SerializeField] private float waterRayHorizontalOffset = 0.125f;
    [SerializeField] private float waterRayDistance = 0.05f;
    [Header("Base Offsets")]
    [SerializeField] private float landAgentOffset = 0.05f;
    [SerializeField] private float waterAgentOffset = -0.03f;
    [SerializeField] private float agentOffsetTransitionTime = 0.05f;

    private float idleTime = -1f;
    private float idleTimer = -1f;
    private float eatTime = -1f;
    private float eatTimer = -1f;
    private float agentOffsetTransitionTimer = -1f;
    private float targetAgentOffset = 0;
    private float initialAgentOffset = 0;
    private float defaultSpeed = 0;
    private int targetIndex = 0;
    private bool pathInverted = false;
    private bool swimming = false;
    private NavMeshAgent agent;

    private void Awake()
    {
        TryGetComponent(out agent);
        defaultSpeed = agent.speed;
        agent.baseOffset = landAgentOffset;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (idleTimeMin < 0)
        {
            idleTimeMin = 0;
        }
        if (idleTimeMax < idleTimeMin)
        {
            if (idleTimeMin > 0)
            {
                idleTimeMax = idleTimeMin;
            }
        }
        GoToNextWaypoint();
    }

    private void FixedUpdate()
    {
        if (swimming == false)
        {
            //front ray checking for any collisions with layers other than water mask
            if (Physics.Raycast(transform.position + new Vector3(0, waterRayVerticalOffset, 0) + transform.forward * waterRayHorizontalOffset, -transform.up, waterRayDistance, ~waterMask) == false)
            {
                //front ray checking for any collisions with layers in the water mask
                if (Physics.Raycast(transform.position + new Vector3(0, waterRayVerticalOffset, 0) + transform.forward * waterRayHorizontalOffset, -transform.up, waterRayDistance, waterMask) == true)
                {
                    //center ray checking for any collisions with layers in the water mask
                    if (Physics.Raycast(transform.position + new Vector3(0, waterRayVerticalOffset, 0), -transform.up, waterRayDistance, waterMask) == true)
                    {
                        OnWaterEnter();
                    }
                }
            }
        }
        else
        {
            //front ray checking for any collisions with layers other than water mask
            if (Physics.Raycast(transform.position + new Vector3(0, waterRayVerticalOffset, 0) + transform.forward * waterRayHorizontalOffset, -transform.up, waterRayDistance, ~waterMask) == true)
            {
                OnWaterExit();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position + new Vector3(0, waterRayVerticalOffset, 0) + transform.forward * waterRayHorizontalOffset, -transform.up * waterRayDistance);
        Gizmos.DrawRay(transform.position + new Vector3(0, waterRayVerticalOffset, 0), -transform.up * waterRayDistance);
        Gizmos.DrawRay(transform.position + new Vector3(0, waterRayVerticalOffset, 0) - transform.forward * waterRayHorizontalOffset, -transform.up * waterRayDistance);
    }

    // Update is called once per frame
    void Update()
    {
        if (agent.remainingDistance <= agent.stoppingDistance && idleTimer < 0)
        {
            if (animator != null)
            {
                animator.SetBool("moving", false);
            }
            agent.isStopped = true;
            idleTime = Random.Range(idleTimeMin, idleTimeMax);
            idleTimer = 0;
            if(canEat == true)
            {
                eatTime = Random.Range(eatTimeMin, eatTimeMax);
                eatTimer = 0;
            }
        }

        if (idleTimer >= 0)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > idleTime)
            {
                idleTimer = -1;
                if (canEat == true)
                {
                    eatTimer = -1;
                }
                GoToNextWaypoint();
            }
            if (canEat == true && eatTimer >= 0 && swimming == false)
            {
                eatTimer += Time.deltaTime;
                if (eatTimer > eatTime)
                {
                    eatTimer = 0;
                    animator.SetTrigger("eat");
                }
            }
        }

        if(agentOffsetTransitionTimer >= 0)
        {
            agentOffsetTransitionTimer += Time.deltaTime;
            agent.baseOffset = Mathf.Lerp(initialAgentOffset, targetAgentOffset, agentOffsetTransitionTimer / agentOffsetTransitionTime);
            if(agentOffsetTransitionTimer > agentOffsetTransitionTime)
            {
                agentOffsetTransitionTimer = -1;
            }
        }
    }

    private void GoToNextWaypoint()
    {
        targetIndex = GetNextTargetIndex();
        if(targetIndex >= 0 )
        {
            agent.isStopped = false;
            agent.SetDestination(waypoints[targetIndex].position);
            if (animator != null)
            {
                animator.SetBool("moving", true);
            }
        }
    }

    private int GetNextTargetIndex()
    {
        if(waypoints.Length == 0) { return -1; }
        switch(pattern)
        {
            default:
            case PatrolPattern.roundRobin:
                if(targetIndex + 1 >= waypoints.Length)
                {
                    return 0;
                }
                return targetIndex + 1;
            case PatrolPattern.random:
                return Random.Range(0, waypoints.Length);
            case PatrolPattern.pingPong:
                if (pathInverted == false)
                {
                    if (targetIndex + 1 >= waypoints.Length)
                    {
                        pathInverted = true;
                        return targetIndex - 1;
                    }
                    return targetIndex + 1;
                }
                else
                {
                    if (targetIndex - 1 < 0)
                    {
                        pathInverted = false;
                        return targetIndex + 1;
                    }
                    return targetIndex - 1;
                }
        }
    }

    private void OnWaterEnter()
    {
        if (swimming == false)
        {
            if (animator != null)
            {
                animator.SetBool("swimming", true);
            }
            swimming = true;
            agent.speed = defaultSpeed * 2.5f;
            targetAgentOffset = waterAgentOffset;
            initialAgentOffset = agent.baseOffset;
            agentOffsetTransitionTimer = 0;
        }
    }

    private void OnWaterExit()
    {
        if(swimming == true)
        {
            if (animator != null)
            {
                animator.SetBool("swimming", false);
            }
            swimming = false;
            agent.speed = defaultSpeed;
            targetAgentOffset = landAgentOffset;
            initialAgentOffset = agent.baseOffset;
            agentOffsetTransitionTimer = 0;
        }
    }
}