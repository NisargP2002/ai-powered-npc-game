using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactionRange = 10f;

    [Header("References")]
    public GameObject interactPromptUI;
    public GameObject npcObject;

    private NPCPatrol npcPatrol;
    private NPCStop npcStop;
    private DialogueManager dialogueManager;
    private bool isNearNPC = false;
    private bool isTalking = false;

    private FirstPersonController fpsController;

    void Start()
    {
        fpsController = GetComponent<FirstPersonController>();

        if (npcObject != null)
        {
            npcPatrol = npcObject.GetComponent<NPCPatrol>();
            npcStop = npcObject.GetComponent<NPCStop>();
            dialogueManager = npcObject.GetComponent<DialogueManager>();
            Debug.Log("DialogueManager found: " + dialogueManager);
        }
    }

    void Update()
    {
        if (npcObject == null) return;
        if (isTalking) return;

        float distance = Vector3.Distance(
            transform.position, npcObject.transform.position);

        if (distance <= interactionRange)
        {
            isNearNPC = true;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(true);
            if (Keyboard.current.eKey.wasPressedThisFrame)
                StartInteraction();
        }
        else
        {
            if (isNearNPC)
            {
                isNearNPC = false;
                if (interactPromptUI != null)
                    interactPromptUI.SetActive(false);
            }
        }
    }

    void StartInteraction()
    {
        Debug.Log("StartInteraction called. DialogueManager: " + dialogueManager);
        isTalking = true;

        // Disable FPS controller so it stops locking cursor
        if (fpsController != null) fpsController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);
        if (npcPatrol != null) npcPatrol.enabled = false;
        if (npcStop != null) npcStop.FacePlayer(transform);
        if (dialogueManager != null) dialogueManager.OpenDialogue();
        Debug.Log("Interaction started with Eolindra.");
    }

    public void EndInteraction()
    {
        isTalking = false;

        // Re-enable FPS controller
        if (fpsController != null) fpsController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (npcPatrol != null) npcPatrol.enabled = true;
        Debug.Log("Interaction ended.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}