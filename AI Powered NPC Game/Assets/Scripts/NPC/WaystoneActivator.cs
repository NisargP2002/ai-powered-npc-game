using UnityEngine;
using UnityEngine.InputSystem;

// Attached to: each Waystone GameObject (3 total)
public class WaystoneActivator : MonoBehaviour
{
    [Header("Name shown in debug log")]
    public string waystoneName = "Waystone";

    [Header("Drag a Point Light here -- it brightens when activated")]
    public Light stoneLight;

    [Header("Optional particles -- play on activation")]
    public ParticleSystem activationParticles;

    [Header("Drag Eolindra here to give her trust on activation")]
    public NPCEmotionSystem eolindraEmotion;

    [Header("Trust added to Eolindra when this stone is found")]
    public float trustBonus = 15f;

    [Header("Prompt shown when player is near")]
    public GameObject activatePromptUI;

    private bool activated = false;
    private bool playerNear = false;

    void Start()
    {
        if (stoneLight != null)
        {
            stoneLight.intensity = 0.4f;
            stoneLight.color = new Color(0.6f, 0.9f, 0.7f);
        }
        if (activatePromptUI != null) activatePromptUI.SetActive(false);
    }

    void Update()
    {
        if (!activated && playerNear && Keyboard.current.eKey.wasPressedThisFrame)
            Activate();
    }

    void OnTriggerEnter(Collider other)
    {
        if (activated) return;
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            if (activatePromptUI != null) activatePromptUI.SetActive(true);
            Debug.Log("[Waystone] Player entered trigger for " + waystoneName);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            if (activatePromptUI != null) activatePromptUI.SetActive(false);
            Debug.Log("[Waystone] Player left trigger for " + waystoneName);
        }
    }

    void Activate()
    {
        activated = true;

        if (stoneLight != null)
        {
            stoneLight.intensity = 3f;
            stoneLight.color = new Color(0.4f, 1f, 0.6f);
        }

        if (activationParticles != null) activationParticles.Play();
        if (activatePromptUI != null) activatePromptUI.SetActive(false);

        if (WaystoneManager.Instance != null)
            WaystoneManager.Instance.WaystoneFound();

        if (eolindraEmotion != null)
            eolindraEmotion.ModifyTrust(trustBonus);

        Debug.Log("[Waystone] " + waystoneName + " activated!");
    }
}