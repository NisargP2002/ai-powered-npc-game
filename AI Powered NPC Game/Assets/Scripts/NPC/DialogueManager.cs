using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    public DialogueUIController dialogueUI;
    public NPCPatrol npcPatrol;
    public NPCStop npcStop;
    public PlayerInteraction playerInteraction;
    public NPCEmotionSystem emotionSystem;

    [Header("AI Settings")]
    public string apiKey = "";

    [System.Serializable]
    class Message { public string role; public string content; }

    [System.Serializable]
    class RequestBody
    {
        public string model = "llama-3.3-70b-versatile";
        public int max_tokens = 100;
        public List<Message> messages;
    }

    [System.Serializable]
    class Choice { public Message message; }

    [System.Serializable]
    class ResponseBody { public Choice[] choices; }

    private List<Message> conversationHistory = new List<Message>();

    // Memory log -- used by keyword detection and (later) ending logic
    private List<string> memoryLines = new List<string>();

    // Phase 8 flag -- prevents the ending from triggering twice
    private bool endingStarted = false;

    public void OpenDialogue()
    {
        if (dialogueUI == null) return;

        conversationHistory.Clear();
        conversationHistory.Add(new Message
        {
            role = "system",
            content = BuildSystemPrompt()
        });

        dialogueUI.Show();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        string greeting = "Halt. You tread on sealed ground. State your purpose, wanderer.";
        dialogueUI.DisplayNPCText(greeting);
        conversationHistory.Add(new Message
        {
            role = "assistant",
            content = greeting
        });
        AddMemory("Eolindra said: " + greeting);
    }

    public void CloseDialogue()
    {
        if (dialogueUI != null) dialogueUI.Hide();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (npcPatrol != null) npcPatrol.enabled = true;
        if (npcStop != null) npcStop.StopFacingPlayer();
        if (playerInteraction != null) playerInteraction.EndInteraction();
    }

    public void OnPlayerSubmitMessage(string playerMessage)
    {
        playerMessage = playerMessage.Trim();
        if (string.IsNullOrEmpty(playerMessage)) return;

        conversationHistory.Add(new Message
        {
            role = "user",
            content = playerMessage
        });
        AddMemory("Player said: " + playerMessage);

        // Refresh system prompt before each call so mood is current
        conversationHistory[0].content = BuildSystemPrompt();

        dialogueUI.ShowThinking(true);
        StartCoroutine(SendToGroq());
    }

    string BuildSystemPrompt()
    {
        string mood = (emotionSystem != null)
            ? emotionSystem.GetEmotionDescription()
            : "wary and formal, guarded but not hostile";

        string waystoneContext = (WaystoneManager.Instance != null)
    ? WaystoneManager.Instance.GetWaystoneContext()
    : "";

        return
            "You are Eolindra, the Warden of the Ashwood. You are an ancient forest " +
            "guardian spirit, over 200 years old. You are bound by a druid oath to " +
            "protect this sacred forest. You have been alone for two centuries. " +
            "You speak in a formal, slightly old-fashioned way. " +
            "You are currently feeling: " + mood + ". " +
            "Reply in 2 to 3 sentences. Stay in character. Never say you are an AI.";
    }

    void AddMemory(string line)
    {
        memoryLines.Add(line);
        if (memoryLines.Count > 50) memoryLines.RemoveAt(0);
    }

    float GetTrustChange()
    {
        string lastLine = "";
        for (int i = memoryLines.Count - 1; i >= 0; i--)
        {
            if (memoryLines[i].StartsWith("Player said:"))
            {
                lastLine = memoryLines[i].ToLower();
                break;
            }
        }

        string[] goodWords = { "forest", "protect", "help", "please",
                               "grateful", "ancient", "accord", "spirit",
                               "beautiful", "sorry", "understand", "trust" };

        string[] badWords = { "stupid", "boring", "leave me",
                              "shut up", "useless", "destroy" };

        foreach (string w in goodWords)
            if (lastLine.Contains(w)) return 18f;

        foreach (string w in badWords)
            if (lastLine.Contains(w)) return -15f;

        return 10f;
    }

    void OnAIResponse(string response)
    {
        if (dialogueUI != null) dialogueUI.DisplayNPCText(response);
        AddMemory("Eolindra said: " + response);
        conversationHistory.Add(new Message
        {
            role = "assistant",
            content = response
        });

        if (emotionSystem != null)
            emotionSystem.ModifyTrust(GetTrustChange());

        CheckForEnding();
    }

    // Phase 8 placeholder -- will trigger ending when trust hits 80
    void CheckForEnding()
    {
        // Filled in during Phase 8
    }

    IEnumerator SendToGroq()
    {
        var body = new RequestBody { messages = conversationHistory };
        string json = JsonUtility.ToJson(body);

        var request = new UnityWebRequest(
            "https://api.groq.com/openai/v1/chat/completions", "POST");
        request.uploadHandler = new UploadHandlerRaw(
            System.Text.Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.timeout = 15;

        yield return request.SendWebRequest();

        dialogueUI.ShowThinking(false);

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<ResponseBody>(
                request.downloadHandler.text);

            if (response != null && response.choices != null && response.choices.Length > 0)
            {
                OnAIResponse(response.choices[0].message.content);
            }
            else
            {
                OnAIResponse("The roots whisper, but I cannot hear them clearly.");
            }
        }
        else
        {
            dialogueUI.DisplayNPCText("The forest spirits are silent... (API error)");
            Debug.LogError("Groq Error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }
}