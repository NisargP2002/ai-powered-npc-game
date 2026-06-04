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

    [Header("AI Settings")]
    public string apiKey = "";

    private List<Message> conversationHistory = new List<Message>();

    private const string SYSTEM_PROMPT =
        "You are Eolindra, an ancient elven warden guarding the Ashwood forest. " +
        "You are suspicious of strangers but honorable. You speak formally. " +
        "Keep responses to 2-3 sentences maximum.";

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

    public void OpenDialogue()
    {
        if (dialogueUI == null) return;
        conversationHistory.Clear();
        conversationHistory.Add(new Message {
            role = "system",
            content = SYSTEM_PROMPT
        });
        dialogueUI.Show();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        string greeting = "Halt. You tread on sealed ground. State your purpose, wanderer.";
        dialogueUI.DisplayNPCText(greeting);
        conversationHistory.Add(new Message {
            role = "assistant",
            content = greeting
        });
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
        conversationHistory.Add(new Message {
            role = "user",
            content = playerMessage
        });
        dialogueUI.ShowThinking(true);
        StartCoroutine(SendToGroq());
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

        yield return request.SendWebRequest();

        dialogueUI.ShowThinking(false);

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<ResponseBody>(
                request.downloadHandler.text);
            string reply = response.choices[0].message.content;
            dialogueUI.DisplayNPCText(reply);
            conversationHistory.Add(new Message {
                role = "assistant",
                content = reply
            });
        }
        else
        {
            dialogueUI.DisplayNPCText("The forest spirits are silent... (API error)");
            Debug.LogError("Groq Error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }
}