using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Dialogue : MonoBehaviour
{
    [SerializeField] private GameObject responseContainer;
    [SerializeField] private GameObject subtitlePanel;

    private int selectedResponseIndex = -1;
    private int responses = 0;
    private Image[] responseImages;
    private Text[] responseTexts;
    private Text subtitleText;

    private void Awake()
    {
        if (responseContainer != null)
        {
            responseImages = responseContainer.GetComponentsInChildren<Image>();
            responseTexts = new Text[responseImages.Length];
            for (int i = 0; i < responseImages.Length; i++)
            {
                responseTexts[i] = responseImages[i].GetComponentInChildren<Text>();
            }
            DisableResponses();
        }
        if (subtitlePanel != null)
        {
            subtitleText = subtitlePanel.GetComponentInChildren<Text>();
            UpdateSubtitles(null);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DialogueManager.Instance.OnInvokeDialogueNode += UpdateSubtitles;
        DialogueManager.Instance.OnInvokeResponseNode += DisplayResponseNode;
    }

    // Update is called once per frame
    void Update()
    {
        if(responseContainer != null && responseContainer.activeSelf == true && Time.timeScale == 1)
        {
            if (Input.GetButtonDown("Toggle Dialogue Response") || Input.mouseScrollDelta.y < 0)
            {
                selectedResponseIndex++;
                if(selectedResponseIndex >= responses)
                {
                    selectedResponseIndex = 0;
                }
                SelectResponse(selectedResponseIndex);
            }
            else if (Input.mouseScrollDelta.y > 0)
            {
                selectedResponseIndex--;
                if (selectedResponseIndex < 0)
                {
                    selectedResponseIndex = responses - 1;
                }
                SelectResponse(selectedResponseIndex);
            }
            else if(Input.GetButtonDown("Use") == true)
            {
                ConfirmResponse(selectedResponseIndex);
            }
        }
    }

    private void UpdateSubtitles(DialogueNode node)
    {
        if (node != null)
        {
            subtitleText.text = $"{node.Speaker}: {node.Subtitle}";
            subtitlePanel.SetActive(true);
        }
        else
        {
            subtitlePanel.SetActive(false);
        }
    }

    private void DisplayResponseNode(ConversationResponseNode node, DecisionNode characterPrompt)
    {
        UpdateSubtitles(characterPrompt.Dialogue);
        UpdateResponses(node);
    }

    private void UpdateResponses(ConversationResponseNode node)
    {
        responses = 0;
        responseContainer.SetActive(true);
        for (int i = 0; i < responseTexts.Length; i++)
        {
            if (i < node.PlayerResponses.Length)
            {
                responses++;
                responseImages[i].gameObject.SetActive(true);
                responseTexts[i].text = node.PlayerResponses[i].ResponseCaption;
            }
            else
            {
                responseImages[i].gameObject.SetActive(false);
            }
        }
        SelectResponse(-1);
    }

    private void SelectResponse(int index)
    {
        for (int i = 0; i < responseImages.Length; i++)
        {
            if (i == index)
            {
                responseImages[i].color = new Color(0, 0, 0, 0.8f);
                responseTexts[i].color = Color.yellow;
            }
            else
            {
                responseImages[i].color = new Color(0, 0, 0, 0);
                responseTexts[i].color = Color.white;
            }
        }
    }

    public void ConfirmResponse(int index)
    {
        if (index >= 0)
        {
            DialogueManager.Instance.SelectedDialogueIndex = index;
            DisableResponses();
            selectedResponseIndex = -1;
        }
    }

    public void DisableResponses()
    {
        responseContainer.SetActive(false);
    }
}
