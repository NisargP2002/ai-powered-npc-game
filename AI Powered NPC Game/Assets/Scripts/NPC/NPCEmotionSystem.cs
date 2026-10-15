using UnityEngine;
using System.Collections.Generic;

public enum EmotionState
{
    Hostile,
    Angry,
    Suspicious,
    Sad,
    Neutral,
    Curious,
    Warm,
    Grateful
}

public class NPCEmotionSystem : MonoBehaviour
{
    [Header("Trust Score (0 = distrusts, 100 = fully trusts)")]
    [Range(0, 100)]
    public float trustScore = 0f;

    [Header("Current emotion -- fully AI driven")]
    public EmotionState currentEmotion = EmotionState.Suspicious;

    [Header("Emotion History")]
    public List<string> emotionHistory = new List<string>();

    [Header("Trust Momentum (-1 to 1, tracks trend)")]
    [Range(-1f, 1f)]
    public float trustMomentum = 0f;

    [Header("Consecutive negative count")]
    public int consecutiveNegative = 0;

    [Header("Attack threshold -- Game Over if reached")]
    public int attackThreshold = 4;

    // Event for other systems to react to emotion changes
    public System.Action<EmotionState, EmotionState> OnEmotionChanged;
    public System.Action OnAttackTriggered;

    private EmotionState previousEmotion;

    void Start()
    {
        previousEmotion = currentEmotion;
    }

    public void ModifyTrust(float amount)
    {
        float oldScore = trustScore;
        trustScore = Mathf.Clamp(trustScore + amount, 0f, 100f);

        // Update momentum based on direction
        if (amount > 0)
        {
            trustMomentum = Mathf.Lerp(trustMomentum, 1f, 0.3f);
            consecutiveNegative = 0;
        }
        else if (amount < 0)
        {
            trustMomentum = Mathf.Lerp(trustMomentum, -1f, 0.3f);
            consecutiveNegative++;
        }

        Debug.Log("[Trust] Score: " + trustScore +
                  " | Change: " + amount +
                  " | Momentum: " + trustMomentum.ToString("F2") +
                  " | Emotion: " + currentEmotion +
                  " | Consecutive Negative: " + consecutiveNegative);

        // Check for attack trigger
        if (consecutiveNegative >= attackThreshold && trustScore < 15f)
        {
            Debug.Log("[TRUST] ATTACK TRIGGERED! Too many insults!");
            if (OnAttackTriggered != null)
                OnAttackTriggered.Invoke();
        }
    }

    public void SetEmotion(EmotionState emotion)
    {
        previousEmotion = currentEmotion;
        currentEmotion = emotion;
        emotionHistory.Add(currentEmotion.ToString());
        if (emotionHistory.Count > 20) emotionHistory.RemoveAt(0);

        if (previousEmotion != currentEmotion && OnEmotionChanged != null)
            OnEmotionChanged.Invoke(previousEmotion, currentEmotion);
    }

    public void SetEmotionFromString(string emotionString)
    {
        if (string.IsNullOrEmpty(emotionString))
        {
            currentEmotion = EmotionState.Neutral;
            return;
        }

        previousEmotion = currentEmotion;

        switch (emotionString.ToLower().Trim())
        {
            case "hostile":    currentEmotion = EmotionState.Hostile; break;
            case "angry":      currentEmotion = EmotionState.Angry; break;
            case "suspicious": currentEmotion = EmotionState.Suspicious; break;
            case "sad":        currentEmotion = EmotionState.Sad; break;
            case "neutral":    currentEmotion = EmotionState.Neutral; break;
            case "curious":    currentEmotion = EmotionState.Curious; break;
            case "warm":       currentEmotion = EmotionState.Warm; break;
            case "grateful":   currentEmotion = EmotionState.Grateful; break;
            default:           currentEmotion = EmotionState.Neutral; break;
        }

        // Track emotion history
        emotionHistory.Add(currentEmotion.ToString());
        if (emotionHistory.Count > 20) emotionHistory.RemoveAt(0);

        // Fire event if emotion actually changed
        if (previousEmotion != currentEmotion && OnEmotionChanged != null)
            OnEmotionChanged.Invoke(previousEmotion, currentEmotion);

        Debug.Log("[Emotion] " + previousEmotion + " -> " + currentEmotion +
                  " (from LLM: '" + emotionString + "')");
    }

    public string GetEmotionDescription()
    {
        switch (currentEmotion)
        {
            case EmotionState.Hostile:
                return "furious and threatened. She is on the edge of attacking. " +
                       "Her voice is ice-cold, her hands glow with ancient power. " +
                       "She speaks through clenched teeth. Every word is a final warning. " +
                       "One more insult and she WILL strike.";

            case EmotionState.Angry:
                return "stern and deeply offended. Her patience is wearing thin. " +
                       "She gives a clear warning — respect the forest or face consequences. " +
                       "Her tone is sharp and commanding.";

            case EmotionState.Suspicious:
                return "deeply suspicious and cold. She watches every movement, " +
                       "gives nothing away, speaks in clipped guarded sentences. " +
                       "She is ready to end this encounter at any moment.";

            case EmotionState.Sad:
                return "melancholic and reflective, speaking softly. " +
                       "She remembers things that were lost. She wonders if the forest " +
                       "will ever know peace. The stranger's words have hurt her.";

            case EmotionState.Neutral:
                return "wary and formal, guarded but not hostile. " +
                       "She watches the stranger carefully, measuring each word, " +
                       "neither welcoming nor rejecting.";

            case EmotionState.Curious:
                return "cautiously curious, beginning to lower her guard. " +
                       "She asks more questions, intrigued by this stranger. " +
                       "She wonders if they might be different from the others who came before.";

            case EmotionState.Warm:
                return "warm and open, beginning to truly trust this person. " +
                       "She shares memories of the forest willingly. " +
                       "Her voice carries a gentle fondness not heard in centuries.";

            case EmotionState.Grateful:
                return "deeply grateful and moved. Her centuries of loneliness are finally lifting. " +
                       "She speaks with quiet emotion and sincerity, " +
                       "as if rediscovering what it means to have a companion. " +
                       "She is close to tears of joy.";

            default:
                return "wary and formal, guarded but not hostile, " +
                       "watching the stranger carefully.";
        }
    }

    // Returns full context string for the AI prompt
    public string GetFullEmotionContext()
    {
        string history = "";
        if (emotionHistory.Count > 0)
        {
            history = "Eolindra's recent emotional journey: ";
            int start = Mathf.Max(0, emotionHistory.Count - 5);
            for (int i = start; i < emotionHistory.Count; i++)
            {
                history += emotionHistory[i];
                if (i < emotionHistory.Count - 1) history += " -> ";
            }
            history += ". ";
        }

        string momentum = "";
        if (trustMomentum > 0.5f)
            momentum = "The conversation trend is very positive. Eolindra is warming up quickly. ";
        else if (trustMomentum > 0.1f)
            momentum = "The conversation trend is slightly positive. ";
        else if (trustMomentum < -0.5f)
            momentum = "The conversation trend is very negative. Eolindra is losing patience rapidly. ";
        else if (trustMomentum < -0.1f)
            momentum = "The conversation trend is slightly negative. ";

        string danger = "";
        if (consecutiveNegative >= 3)
            danger = "WARNING: The player has been hostile " + consecutiveNegative +
                     " times in a row. Eolindra is close to attacking. ";
        else if (consecutiveNegative >= 2)
            danger = "The player has been negative twice in a row. Eolindra is getting impatient. ";

        return history + momentum + danger;
    }
}
