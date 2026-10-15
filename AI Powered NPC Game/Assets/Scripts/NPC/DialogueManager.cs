using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("References -- assign all in Inspector")]
    public DialogueUIController dialogueUI;
    public AIRequestHandler aiHandler;
    public NPCPatrol npcPatrol;
    public NPCStop npcStop;
    public PlayerInteraction playerInteraction;
    public NPCEmotionSystem emotionSystem;
    public FloatingEmotionIcon emotionIcon;

    [Header("Game Over UI")]
    public GameObject gameOverCanvas;

    private List<string> memoryLines = new List<string>();
    private const int MAX_MEMORY = 8;
    private bool endingStarted = false;
    private bool gameOverTriggered = false;

    void Start()
    {
        if (emotionSystem != null)
            emotionSystem.OnAttackTriggered += TriggerGameOver;

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(false);

        // Hide icon at start
        if (emotionIcon != null) emotionIcon.HideIcon();
    }

    void OnDestroy()
    {
        if (emotionSystem != null)
            emotionSystem.OnAttackTriggered -= TriggerGameOver;
    }

    public void OpenDialogue()
    {
        if (dialogueUI == null) return;

        dialogueUI.Show();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Show the emotion icon above Eolindra's head
        if (emotionIcon != null) emotionIcon.ShowIcon();

        string greeting = GetGreeting();
        dialogueUI.DisplayNPCText(greeting);
        AddMemory("EOLINDRA: " + greeting);
    }

    public void CloseDialogue()
    {
        if (dialogueUI != null) dialogueUI.Hide();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Hide the emotion icon when conversation ends
        if (emotionIcon != null) emotionIcon.HideIcon();

        if (npcPatrol != null) npcPatrol.enabled = true;
        if (npcStop != null) npcStop.StopFacingPlayer();
        if (playerInteraction != null) playerInteraction.EndInteraction();
    }

    public void OnPlayerSubmitMessage(string playerMessage)
    {
        if (gameOverTriggered || endingStarted) return;
        playerMessage = playerMessage.Trim();
        if (string.IsNullOrEmpty(playerMessage)) return;

        AddMemory("PLAYER: " + playerMessage);

        if (aiHandler != null)
        {
            aiHandler.SendRequest(
                BuildSystemPrompt(),
                BuildUserMessage(playerMessage),
                OnAIResponse);
        }
        else
        {
            Debug.LogWarning("[DialogueManager] AIRequestHandler not assigned!");
            OnAIResponse(AIResponse.Fallback());
        }
    }

    void OnAIResponse(AIResponse aiResult)
    {
        if (dialogueUI != null)
            dialogueUI.DisplayNPCText(aiResult.dialogueText);

        AddMemory("EOLINDRA: " + aiResult.dialogueText);

        if (emotionSystem != null)
        {
            emotionSystem.ModifyTrust(aiResult.trustChange);
            emotionSystem.SetEmotionFromString(aiResult.emotion);
        }

        CheckForEnding();
    }

    void CheckForEnding()
    {
        if (endingStarted) return;
        if (emotionSystem == null) return;
        if (emotionSystem.trustScore < 80f) return;
        if (WaystoneManager.Instance == null) return;
        if (!WaystoneManager.Instance.AllFound()) return;

        endingStarted = true;
        StartCoroutine(TriggerEndingSequence());
    }

    IEnumerator TriggerEndingSequence()
    {
        if (dialogueUI != null) dialogueUI.LockInput(true);

        if (emotionIcon != null) emotionIcon.HideIcon();

        yield return new WaitForSeconds(1.5f);

        string finalSpeech =
            "I did not think anyone would come. Not after so long. " +
            "You have given me something I had forgotten existed. " +
            "Thank you, wanderer. Whatever lies beyond this forest... " +
            "I think I am ready to find out.";

        if (dialogueUI != null) dialogueUI.DisplayNPCText(finalSpeech);

        yield return new WaitForSeconds(4f);

        if (EndingManager.Instance != null)
            EndingManager.Instance.TriggerEnding();
    }

    void TriggerGameOver()
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;
        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {
        if (dialogueUI != null)
        {
            dialogueUI.LockInput(true);
            dialogueUI.DisplayNPCText(
                "ENOUGH! You dare defile this sacred ground with your venom? " +
                "The Ashwood does not forgive. And neither do I.");
        }

        if (emotionIcon != null) emotionIcon.HideIcon();

        yield return new WaitForSeconds(2f);

        if (npcStop != null)
            npcStop.PlayAttack();

        yield return new WaitForSeconds(1.5f);

        if (dialogueUI != null) dialogueUI.Hide();

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    string BuildSystemPrompt()
    {
        string mood = (emotionSystem != null)
            ? emotionSystem.GetEmotionDescription()
            : "wary and formal, guarded but not hostile";

        string waystones = (WaystoneManager.Instance != null)
            ? WaystoneManager.Instance.GetWaystoneContext()
            : "";

        string emotionContext = (emotionSystem != null)
            ? emotionSystem.GetFullEmotionContext()
            : "";

        string storyContext = GetStoryContext();

        string q = "\"";

        return
            "You are Eolindra, the Warden of the Ashwood. " +
            "You are an ancient forest guardian spirit, over 200 years old. " +
            "You are bound by a druid oath to protect this sacred forest. " +
            "You have been alone for two centuries. " +
            "You speak in a formal, slightly old-fashioned way. " +
            "Your current emotional state: " + mood + ". " +
            emotionContext +
            storyContext + " " +
            waystones +
            "\n\nYou MUST respond in valid JSON format only. No other text. No markdown. " +
            "Use exactly this structure:\n" +
            "{" +
                q + "response" + q + ": " + q + "Your in-character reply. 2 to 3 sentences." + q + ", " +
                q + "trust_change" + q + ": 10, " +
                q + "emotion" + q + ": " + q + "neutral" + q + ", " +
                q + "reasoning" + q + ": " + q + "One sentence reason." + q +
            "}" +
            "\n\nRules for trust_change (integer -20 to +20):\n" +
            "Strongly positive (+15 to +20): genuine empathy, wanting to help, asking about forest history.\n" +
            "Mildly positive (+5 to +14): polite, curious, respectful.\n" +
            "Neutral (0 to +4): small talk, simple questions.\n" +
            "Negative (-1 to -20): rude, dismissive, threatening, or mocking.\n" +
            "\nRules for emotion -- choose EXACTLY one based on what the player just said:\n" +
            "hostile:    player made death threats, wants to destroy the forest, extreme aggression.\n" +
            "angry:      player was rude, dismissive, impatient, insulting, or threatening.\n" +
            "suspicious: player seems untrustworthy, evasive, or has mixed intentions.\n" +
            "sad:        player mentioned loneliness, grief, loss, or things that were destroyed.\n" +
            "neutral:    player asked a plain question with no emotional weight.\n" +
            "curious:    player is asking thoughtful questions, showing genuine interest.\n" +
            "warm:       player said something kind, empathetic, caring, or helpful.\n" +
            "grateful:   player showed deep understanding of Eolindra's loneliness or duty.\n" +
            "IMPORTANT: do not default to neutral out of caution. " +
            "If the message has any warmth or kindness, return warm. " +
            "If deep empathy or understanding, return grateful. " +
            "If hostile or threatening, return hostile or angry. " +
            "Emotions must shift GRADUALLY. No jumping from hostile to warm.\n" +
            "Return raw JSON only. No markdown code blocks. No extra text.";
    }

    string GetStoryContext()
    {
        float trust = (emotionSystem != null) ? emotionSystem.trustScore : 0f;

        int stonesFound = (WaystoneManager.Instance != null)
            ? WaystoneManager.Instance.activatedCount : 0;

        bool allStonesFound = (WaystoneManager.Instance != null)
            && WaystoneManager.Instance.AllFound();

        if (trust < 25f)
        {
            return
                "You do not yet trust this stranger. " +
                "You answer questions guardedly and keep your answers short. " +
                "You do not reveal anything about the Accord of Roots " +
                "or the Waystones yet. You watch them carefully.";
        }

        if (trust < 50f)
        {
            return
                "You are beginning to cautiously trust this wanderer. " +
                "You may mention that the Ashwood is trapped by an unfinished " +
                "pact called the Accord of Roots. " +
                "Long ago, druids placed three ancient Waystones in this forest " +
                "to seal the Accord. When the last druid died, " +
                "the ritual was never completed. " +
                "You hint that perhaps a living human could finish " +
                "what the druids started. Do not give explicit instructions yet.";
        }

        if (trust < 80f && !allStonesFound)
        {
            if (stonesFound == 0)
            {
                return
                    "You trust this wanderer enough to ask for their help directly. " +
                    "Tell them clearly: three ancient Waystones are hidden in this forest. " +
                    "They were placed by the druids to seal the Accord of Roots. " +
                    "If a living human awakens each Waystone, " +
                    "the Accord can finally be completed and you will be free. " +
                    "You cannot do this yourself -- only a living soul can activate them. " +
                    "Ask the player to find and activate all three Waystones. " +
                    "Speak with urgency and hope -- " +
                    "this is the first chance you have had in 200 years.";
            }

            return
                "The player is helping you -- they have already found " +
                stonesFound + " of the 3 Waystones. " +
                "Express genuine gratitude and encourage them to find the rest. " +
                "You can feel the forest stirring as each stone awakens. " +
                "Describe what it feels like as the Accord slowly comes back to life. " +
                "You are emotional -- this has not happened in two centuries.";
        }

        if (allStonesFound && trust < 80f)
        {
            return
                "The player has found all three Waystones. " +
                "The Accord of Roots is nearly complete. " +
                "You are overwhelmed with emotion -- relief, joy, and disbelief. " +
                "Tell the player the seal is breaking. " +
                "Thank them deeply and personally. " +
                "The forest can feel it too -- things are changing. " +
                "You sense your long vigil is almost over.";
        }

        return
            "The player has earned your complete trust. " +
            "You are at peace for the first time in two centuries. " +
            "Speak warmly and openly. " +
            "If the player says anything kind, respond with deep personal gratitude. " +
            "Your answers can be slightly longer and more reflective at this stage.";
    }

    string BuildUserMessage(string playerMessage)
    {
        string memory = "";

        if (memoryLines.Count > 0)
        {
            memory = "[CONVERSATION HISTORY]\n";
            foreach (string line in memoryLines)
                memory += line + "\n";
            memory += "[END HISTORY]\n\n";
        }

        return memory + "The player says: " + playerMessage;
    }

    void AddMemory(string line)
    {
        memoryLines.Add(line);
        if (memoryLines.Count > MAX_MEMORY)
            memoryLines.RemoveAt(0);
    }

    string GetGreeting()
    {
        float trust = (emotionSystem != null) ? emotionSystem.trustScore : 0f;

        if (trust >= 70f)
            return "Ah, you return. The trees have kept your footsteps in memory.";
        if (trust >= 30f)
            return "You walk these paths again. What is it you seek?";

        return "Halt. You tread on sealed ground. State your purpose, wanderer.";
    }
}
