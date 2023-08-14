using UnityEngine;

public class GalahFlock : MonoBehaviour
{
    public delegate void GalahFlockEvent();

    [SerializeField] private Bounds groundBounds;
    [SerializeField] private Transform flightTarget;
    [SerializeField] private Vector3 offsetMax;
    [SerializeField] private float changePositionTimeMin = 3, changePositionTimeMax = 5;

    private Vector3 defaultPos;
    private float timer = -1;
    private float time = 0;

    public Transform FlightTarget { get { return flightTarget; } }

    public event GalahFlockEvent TakeoffEvent;

    private void Awake()
    {
        defaultPos = flightTarget.position;
    }

    private void Start()
    {
        time = 0;
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    TakeoffEvent.Invoke();
        //}

        if (timer >= -1)
        {
            timer += Time.deltaTime;
            if(timer > time)
            {
                timer = 0;
                time = Random.Range(changePositionTimeMin, changePositionTimeMax);
                flightTarget.position = defaultPos + new Vector3(Random.Range(-offsetMax.x, offsetMax.x), Random.Range(-offsetMax.y, offsetMax.y), Random.Range(-offsetMax.z, offsetMax.z));
            }
        }
    }

    public Vector3 GetRandomPositionInBounds()
    {
        return transform.position + groundBounds.center + new Vector3(Random.Range(-groundBounds.extents.x, groundBounds.extents.x),
            Random.Range(-groundBounds.extents.y, groundBounds.extents.y),
            Random.Range(-groundBounds.extents.z, groundBounds.extents.z));
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + groundBounds.center, groundBounds.size);
        if(flightTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(flightTarget.position, 0.5f);
            Gizmos.DrawWireCube(defaultPos != Vector3.zero ? defaultPos : flightTarget.position, offsetMax * 2);
        }
    }

    public void InvokeTakeoff()
    {
        TakeoffEvent.Invoke();
    }
}
