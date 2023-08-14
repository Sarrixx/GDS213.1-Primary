using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_HUD : MonoBehaviour
{
    [SerializeField] private Text interactionText;

    private void Awake()
    {
        Interaction.OnInteractionDetected += SetInteractionText;
    }

    private void OnDestroy()
    {
        Interaction.OnInteractionDetected -= SetInteractionText;
    }

    // Start is called before the first frame update
    void Start()
    {
        interactionText.text = string.Empty;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetInteractionText(GameObject obj)
    {
        interactionText.text = obj != null ? $"{obj.name}" : string.Empty;
    }
}
