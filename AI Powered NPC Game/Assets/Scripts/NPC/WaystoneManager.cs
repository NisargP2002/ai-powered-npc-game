using UnityEngine;

// Attached to: an empty GameObject called _WaystoneManager
// Tracks how many of the 3 Waystones the player has found.
public class WaystoneManager : MonoBehaviour
{
    public static WaystoneManager Instance;

    [Header("How many Waystones have been activated so far")]
    public int activatedCount = 0;
    public int totalWaystones = 3;

    void Awake()
    {
        Instance = this;
    }

    public void WaystoneFound()
    {
        activatedCount++;
        Debug.Log("[Waystones] " + activatedCount + " / " + totalWaystones + " found");
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