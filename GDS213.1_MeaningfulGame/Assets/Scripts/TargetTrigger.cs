using UnityEngine;
using UnityEngine.Events;

public class TargetTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent events;
    [SerializeField] private GameObject target;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == target)
        {
            events.Invoke();
            gameObject.SetActive(false);
        }
    }
}
