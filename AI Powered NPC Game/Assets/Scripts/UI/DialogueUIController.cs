using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DialogueUIController : MonoBehaviour
{
    [Header("Panel")]
    public GameObject dialoguePanel;

    [Header("Text Elements")]
    public TMP_Text npcNameText;
    public TMP_Text npcDialogueText;
    public GameObject thinkingIndicator;

    [Header("Input")]
    public TMP_InputField playerInputField;
    public Button sendButton;
    public Button closeButton;

    [Header("Trust Bar")]
    public Slider trustBar;
    public Slider trustBarOverlay;

    [Header("References")]
    public DialogueManager dialogueManager;
    public NPCEmotionSystem emotionSystem;

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (thinkingIndicator != null) thinkingIndicator.SetActive(false);
        if (sendButton != null) sendButton.onClick.AddListener(OnSendClicked);
        if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
        if (npcNameText != null) npcNameText.text = "Eolindra";
    }

    void Update()
    {
        if (dialoguePanel != null && dialoguePanel.activeSelf)
            if (Keyboard.current.enterKey.wasPressedThisFrame) OnSendClicked();

        if (trustBarOverlay != null && emotionSystem != null)
            trustBarOverlay.value = emotionSystem.trustScore / 100f;

        if (trustBar != null && emotionSystem != null)
            trustBar.value = emotionSystem.trustScore / 100f;
    }

    public void Show()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (playerInputField != null)
        {
            playerInputField.text = "";
            playerInputField.ActivateInputField();
        }
    }

    public void Hide()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public void DisplayNPCText(string text)
    {
        if (npcDialogueText != null) npcDialogueText.text = text;
    }

    public void ShowThinking(bool show)
    {
        if (thinkingIndicator != null) thinkingIndicator.SetActive(show);
        if (sendButton != null) sendButton.interactable = !show;
    }

    // Called by DialogueManager when the ending sequence starts (Phase 8)
    public void LockInput(bool locked)
    {
        if (playerInputField != null) playerInputField.interactable = !locked;
        if (sendButton != null) sendButton.interactable = !locked;
    }

    void OnSendClicked()
    {
        if (playerInputField == null) return;
        string msg = playerInputField.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;
        playerInputField.text = "";
        playerInputField.ActivateInputField();
        if (dialogueManager != null) dialogueManager.OnPlayerSubmitMessage(msg);
    }

    void OnCloseClicked()
    {
        if (dialogueManager != null) dialogueManager.CloseDialogue();
    }
}