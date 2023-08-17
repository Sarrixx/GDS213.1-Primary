using UnityEngine;
using UnityEngine.AI;

public class FoxAgent : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [Header("Navigation")]
    [SerializeField] private PatrolPattern pattern;
    [SerializeField] private Transform[] waypoints;
    [Header("Idle Behaviour")]
    [SerializeField] private float idleTimeMin;
    [SerializeField] private float idleTimeMax;
    [SerializeField] [Range(1, 10)] private float sprintMultiplier = 1;

    private float idleTime = -1f;
    private float idleTimer = -1f;
    private float defaultSpeed = 0;
    private int targetIndex = 0;
    private bool pathInverted = false;
    private NavMeshAgent agent;
    private Transform currentTarget;

    private void Awake()
    {
        TryGetComponent(out agent);
        defaultSpeed = agent.speed;
    }

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
        //GoToNextWaypoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTarget != null)
        {
            agent.SetDestination(currentTarget.position);
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (animator != null)
                {
                    animator.SetBool("moving", false);
                }
                agent.isStopped = true;
            }
            else if (agent.isStopped == true)
            {
                if (agent.remainingDistance > agent.stoppingDistance * 1.5)
                {
                    if (animator != null)
                    {
                        animator.SetBool("moving", true);
                    }
                    agent.isStopped = false;
                }
            }
        }
        else
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (idleTimer < 0)
                {
                    if (animator != null)
                    {
                        animator.SetBool("moving", false);
                    }
                    agent.isStopped = true;
                    idleTime = Random.Range(idleTimeMin, idleTimeMax);
                    idleTimer = 0;
                }
            }
        }

        if (idleTimer >= 0)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > idleTime)
            {
                idleTimer = -1;
                if(currentTarget == null)
                {
                    GoToNextWaypoint();
                }
            }
        }
    }

    private int GetNextTargetIndex()
    {
        if (waypoints.Length == 0) { return -1; }
        switch (pattern)
        {
            default:
            case PatrolPattern.roundRobin:
                if (targetIndex + 1 >= waypoints.Length)
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

    private void GoToNextWaypoint()
    {
        targetIndex = GetNextTargetIndex();
        if (targetIndex >= 0)
        {
            agent.isStopped = false;
            agent.SetDestination(waypoints[targetIndex].position);
            if (animator != null)
            {
                animator.SetBool("moving", true);
            }
        }
    }

    public void GoToTarget(Transform target)
    {
        currentTarget = target;
        if (currentTarget != null)
        {
            agent.isStopped = false;
            if (animator != null)
            {
                animator.SetBool("moving", true);
            }
        }
    }

    public void ToggleSprint(bool toggle)
    {
        if (toggle == true)
        {
            if (animator != null)
            {
                animator.SetBool("running", true);
            }
            agent.speed = defaultSpeed * sprintMultiplier;
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool("running", false);
            }
            agent.speed = defaultSpeed;
        }
    }

    public void ClearTarget(bool resumePatrol)
    {
        currentTarget = null;
        if (animator != null)
        {
            animator.SetBool("moving", false);
        }
        agent.isStopped = true;
        if (resumePatrol == true)
        {
            GoToNextWaypoint();
        }
    }
}
