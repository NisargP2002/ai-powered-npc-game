using UnityEngine;
using TMPro;

// Attached to: an empty GameObject called _WaystoneManager
// Tracks how many of the 3 Waystones the player has found.
public class WaystoneManager : MonoBehaviour
{
    public static WaystoneManager Instance;

    [Header("How many Waystones have been activated so far")]
    public int activatedCount = 0;
    public int totalWaystones = 3;

    [Header("Drag the WaystoneCounter text object here")]
    public TMP_Text counterText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Set the counter to show 0 at the start of the game
        if (counterText != null)
            counterText.text = "Waystones: 0 / " + totalWaystones;
    }

    public void WaystoneFound()
    {
        activatedCount = Mathf.Min(activatedCount + 1, totalWaystones);
        Debug.Log("[Waystones] " + activatedCount + " / " + totalWaystones + " found");

        // Update the on-screen counter
        if (counterText != null)
            counterText.text = "Waystones: " + activatedCount + " / " + totalWaystones;
    }

    public bool AllFound()
    {
        return activatedCount >= totalWaystones;
    }

    public string GetWaystoneContext()
    {
        if (activatedCount == 0)
            return "The player has not yet found any of the ancient Waystones.";

        if (AllFound())
            return "The player has found all three Waystones. " +
                   "The forest path is clear. The Accord can be completed.";

        return "The player has found " + activatedCount +
               " out of 3 Waystones so far.";
    }
}
