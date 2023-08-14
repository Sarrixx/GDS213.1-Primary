using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GalahAgent : MonoBehaviour
{
    [SerializeField] private float idleTimeMin;
    [SerializeField] private float idleTimeMax;
    [SerializeField] private float flightSpeedMultiplier;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private GalahFlock flockManager;

    private float walkSpeed = 0;
    private float time = 0;
    private float timer = -1;
    private bool flying = false;
    private bool landing = false;
    private bool takingOff = false;
    private Vector3 landingPoint = Vector3.zero;
    private NavMeshAgent agent;
    private Animator anim;
    private AudioSource aSrc;
    private Coroutine transitionRoutine;
    private Transform flightTarget;

    private Transform CurrentFlightTarget
    {
        get
        {
            if (flightTarget == null)
            {
                if (flockManager != null && flockManager.FlightTarget != null)
                {
                    return flockManager.FlightTarget;
                }
                return null;
            }
            else
            {
                return flightTarget;
            }
        }
    }

    private void Awake()
    {
        TryGetComponent(out aSrc);
        TryGetComponent(out agent);
        anim = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (flockManager != null)
        {
            flockManager.TakeoffEvent += DelayedTakeoff;
        }
    }

    private void OnDestroy()
    {
        if (flockManager != null)
        {
            flockManager.TakeoffEvent -= DelayedTakeoff;
        }
    }

    private void OnDisable()
    {
        if (flockManager != null)
        {
            flockManager.TakeoffEvent -= DelayedTakeoff;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        walkSpeed = agent.speed;
        if (agent.isOnNavMesh == true)
        {
            if (idleTimeMin < 0)
            {
                idleTimeMin = 0;
            }
            if (idleTimeMax < idleTimeMin)
            {
                idleTimeMax = idleTimeMin;
            }
            //SetAnimator("idle");
            WalkToRandomPosition();
        }
        else
        {
            TakeOff();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(timer >= 0)
        {
            timer += Time.deltaTime;
            if(timer > time)
            {
                if(flying == false && takingOff == false && landing == false && flockManager != null && agent != null)
                {
                    if (Random.Range(0, 100) >= 65)
                    {
                        SetAnimator("eating");
                        time = Random.Range(idleTimeMin, idleTimeMax);
                        timer = 0;
                    }
                    else
                    {
                        WalkToRandomPosition();
                        timer = -1;
                        time = 0;
                    }
                }
            }
        }
        else
        {
            if(flying == false && agent.enabled == true && agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.isStopped = true;
                timer = 0;
                time = Random.Range(idleTimeMin, idleTimeMax);
                SetAnimator("idle");
            }
        }

        if (agent.enabled == false)
        {
            if (flockManager != null)
            {
                if (landing == false)
                {
                    if (CurrentFlightTarget != null)
                    {
                        Vector3 targetDirection = CurrentFlightTarget.position - transform.position;

                        if (targetDirection != Vector3.zero)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                            transform.position += transform.forward * walkSpeed * flightSpeedMultiplier * Time.deltaTime;
                        }
                    }
                }
                else
                {
                    if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.9f) == true && hit.transform != transform)
                    {
                        if (transitionRoutine == null)
                        {
                            //Debug.Log("Landing");
                            SetAnimator("landing");
                            TransitionAnimation("idle", 0.2f, flying == true);
                        }
                    }
                    else
                    {
                        Vector3 targetDirection = landingPoint - transform.position;
                        if (targetDirection != Vector3.zero)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                            transform.position += transform.forward * walkSpeed * flightSpeedMultiplier * Time.deltaTime;
                        }
                    }
                }
                    Debug.DrawRay(transform.position, Vector3.down * 0.9f);
            }
        }

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    bool success = flying == false ? TakeOff() : Land();
        //}
    }

    private void WalkToRandomPosition()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(flockManager.GetRandomPositionInBounds());
            SetAnimator("walking");
        }
    }

    private bool SetAnimator(string parameterName)
    {
        switch(parameterName)
        {
            default:
                anim.SetBool("idle", true);
                anim.SetBool("walking", false);
                anim.SetBool("eating", false);
                anim.SetBool("landing", false);
                anim.SetBool("jumping", false);
                anim.SetBool("takeoff", false);
                return false;
            case "idle":
                anim.SetBool("idle", true);
                anim.SetBool("walking", false);
                anim.SetBool("eating", false);
                anim.SetBool("landing", false);
                anim.SetBool("jumping", false);
                anim.SetBool("takeoff", false);
                return true;
            case "walking":
                anim.SetBool("idle", false);
                anim.SetBool("walking", true);
                anim.SetBool("eating", false);
                anim.SetBool("landing", false);
                anim.SetBool("jumping", false);
                anim.SetBool("takeoff", false);
                return true;
            case "eating":
                anim.SetBool("idle", false);
                anim.SetBool("walking", false);
                anim.SetBool("eating", true);
                anim.SetBool("landing", false);
                anim.SetBool("jumping", false);
                anim.SetBool("takeoff", false);
                TransitionAnimation("idle", 0.2f);
                return true;
            case "landing":
                anim.SetBool("idle", false);
                anim.SetBool("walking", false);
                anim.SetBool("eating", false);
                anim.SetBool("landing", true);
                anim.SetBool("takeoff", false);
                anim.SetBool("jumping", false);
                anim.SetBool("flying", false);
                anim.SetBool("glide", false);
                anim.SetBool("flyright", false);
                anim.SetBool("flyleft", false);
                return true;
            case "takeoff":
                anim.SetBool("idle", false);
                anim.SetBool("walking", false);
                anim.SetBool("eating", false);
                anim.SetBool("landing", false);
                anim.SetBool("jumping", false);
                anim.SetBool("takeoff", true);
                return true;
            case "flying":
                anim.SetBool("idle", false);
                anim.SetBool("walking", false);
                anim.SetBool("eating", false);
                anim.SetBool("landing", false);
                anim.SetBool("jumping", false);
                anim.SetBool("takeoff", false);
                anim.SetBool("flying", true);
                anim.SetBool("glide", false);
                anim.SetBool("flyright", false);
                anim.SetBool("flyleft", false);
                return true;
            case "glide":
                anim.SetBool("idle", false);
                anim.SetBool("walking", false);
                anim.SetBool("eating", false);
                anim.SetBool("landing", false);
                anim.SetBool("jumping", false);
                anim.SetBool("takeoff", false);
                anim.SetBool("flying", false);
                anim.SetBool("glide", true);
                anim.SetBool("flyright", false);
                anim.SetBool("flyleft", false);
                return true;
            case "flyleft":
                anim.SetBool("idle", false);
                anim.SetBool("walking", false);
                anim.SetBool("eating", false);
                anim.SetBool("landing", false);
                anim.SetBool("jumping", false);
                anim.SetBool("takeoff", false);
                anim.SetBool("flying", false);
                anim.SetBool("glide", false);
                anim.SetBool("flyright", false);
                anim.SetBool("flyleft", true);
                return true;
            case "flyright":
                anim.SetBool("idle", false);
                anim.SetBool("walking", false);
                anim.SetBool("eating", false);
                anim.SetBool("landing", false);
                anim.SetBool("jumping", false);
                anim.SetBool("takeoff", false);
                anim.SetBool("flying", false);
                anim.SetBool("glide", false);
                anim.SetBool("flyright", true);
                anim.SetBool("flyleft", false);
                return true;
        }
    }

    private void TransitionAnimation(string parameterName, float time, bool toggleFlying = false)
    {
        if(transitionRoutine != null)
        {
            //StopCoroutine(transitionRoutine);
        }
        transitionRoutine = StartCoroutine(TransitionAfterTime(parameterName, time, toggleFlying));
    }

    private IEnumerator TransitionAfterTime(string parameterName, float time, bool toggleFlying = false)
    {
        yield return new WaitForSeconds(time);
        SetAnimator(parameterName);
        transitionRoutine = null;
        if(toggleFlying == true)
        {
            if(flying == true)
            {
                flying = false;
                landing = false;
                agent.enabled = true;
            }
            else
            {
                flying = true;
                takingOff = false;
            }
        }
    }

    private IEnumerator TakeoffAfterDelay()
    {
        if (flying == false)
        {
            yield return new WaitForSeconds(Random.Range(0, 0.35f));
            TakeOff();
        } 
    }

    private IEnumerator LandAfterDelay()
    {
        if (flying == true)
        {
            yield return new WaitForSeconds(Random.Range(0, 0.35f));
            Land();
        } 
    }

    private void DelayedTakeoff()
    {
        if (flying == false)
        {
            StartCoroutine(TakeoffAfterDelay());
        }
        else
        {
            StartCoroutine(LandAfterDelay());
        }
    }

    private bool TakeOff()
    {
        if (takingOff == false && flying == false)
        {
            //anim.applyRootMotion = true;
            agent.enabled = false;
            //flying = true;
            takingOff = true;
            SetAnimator("takeoff");
            TransitionAnimation("flying", 0.8f, flying == false);
            return true;
        }
        return false;
    }

    private bool Land()
    {
        if (flying == true && landing == false)
        {
            //anim.applyRootMotion = false;
            //flying = false;
            landingPoint = flockManager.GetRandomPositionInBounds();
            landing = true;
            return true;
        }
        return false;
    }
}
