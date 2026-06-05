using UnityEngine;

// The three emotional states Eolindra can be in.
public enum EmotionState { Neutral, Warm, Sad }

// Attached to: Eolindra
// Tracks trust score and emotion. Read by DialogueManager and the UI.
public class NPCEmotionSystem : MonoBehaviour
{
    [Header("Trust Score (0 = distrusts player, 100 = fully trusts)")]
    [Range(0, 100)]
    public float trustScore = 0f;

    [Header("Current emotion -- updates automatically from trust")]
    public EmotionState currentEmotion = EmotionState.Neutral;

    // Called by DialogueManager after every exchange
    public void ModifyTrust(float amount)
    {
        trustScore = Mathf.Clamp(trustScore + amount, 0f, 100f);
        UpdateEmotionFromTrust();
        Debug.Log("[Trust] Score: " + trustScore + " | Emotion: " + currentEmotion);
    }

    void UpdateEmotionFromTrust()
    {
        if (trustScore >= 70f) currentEmotion = EmotionState.Warm;
        else currentEmotion = EmotionState.Neutral;
    }

    public void SetEmotion(EmotionState e) { currentEmotion = e; }

    // Returns a text description used in the AI system prompt
    public string GetEmotionDescription()
    {
        switch (currentEmotion)
        {
            case EmotionState.Warm:
                return "warm and open, beginning to trust this person, " +
                       "more willing to share memories of the forest";
            case EmotionState.Sad:
                return "melancholic and reflective, speaking softly, " +
                       "remembering things that were lost";
            default:
                return "wary and formal, guarded but not hostile, " +
                       "watching the stranger carefully";
        }
    }
}