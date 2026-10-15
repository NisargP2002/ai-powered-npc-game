using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

// Attached to: Eolindra
// Sends messages to Groq API and returns a structured AIResponse.
public class AIRequestHandler : MonoBehaviour
{
    [Header("Drag the APIConfig asset here")]
    public APIConfig config;

    [Header("Drag Canvas here")]
    public DialogueUIController dialogueUI;

    [Header("Drag FloatingHeart here")]
    public FloatingEmotionIcon emotionIcon;

    public void SendRequest(string systemPrompt, string userMessage,
                            System.Action<AIResponse> onResponse)
    {
        StartCoroutine(CallGroqAPI(systemPrompt, userMessage, onResponse));
    }

    IEnumerator CallGroqAPI(string systemPrompt, string userMessage,
                            System.Action<AIResponse> onResponse)
    {
        // Show thinking state in UI and emotion icon
        if (dialogueUI != null) dialogueUI.ShowThinking(true);
        if (emotionIcon != null) emotionIcon.SetThinking(true);

        if (config == null || string.IsNullOrEmpty(config.groqApiKey))
        {
            Debug.LogWarning("[Groq] APIConfig missing or key empty.");
            if (dialogueUI != null) dialogueUI.ShowThinking(false);
            if (emotionIcon != null) emotionIcon.SetThinking(false);
            onResponse?.Invoke(AIResponse.Fallback());
            yield break;
        }

        string q = "\"";

        string sysEscaped = EscapeJson(systemPrompt);
        string userEscaped = EscapeJson(userMessage);

        string jsonBody =
            "{" +
                q + "model" + q + ": " + q + config.groqModel + q + ", " +
                q + "messages" + q + ": [" +
                    "{" +
                        q + "role" + q + ": " + q + "system" + q + ", " +
                        q + "content" + q + ": " + q + sysEscaped + q +
                    "}, " +
                    "{" +
                        q + "role" + q + ": " + q + "user" + q + ", " +
                        q + "content" + q + ": " + q + userEscaped + q +
                    "}" +
                "], " +
                q + "response_format" + q + ": {" +
                    q + "type" + q + ": " + q + "json_object" + q +
                "}, " +
                q + "max_tokens" + q + ": " + config.maxTokens + ", " +
                q + "temperature" + q + ": 0.8" +
            "}";

        using (UnityWebRequest request = new UnityWebRequest(config.groqEndpoint, "POST"))
        {
            byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + config.groqApiKey);
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 20;

            yield return request.SendWebRequest();

            // Hide thinking state in UI and emotion icon
            if (dialogueUI != null) dialogueUI.ShowThinking(false);
            if (emotionIcon != null) emotionIcon.SetThinking(false);

            if (request.result == UnityWebRequest.Result.Success)
            {
                AIResponse result = ParseGroqResponse(request.downloadHandler.text);
                Debug.Log("[Groq] OK | Trust: " + result.trustChange +
                          " | Emotion: " + result.emotion +
                          " | Reason: " + result.reasoning);
                onResponse?.Invoke(result);
            }
            else
            {
                Debug.LogWarning("[Groq] Error: " + request.error);
                Debug.LogWarning("[Groq] Body: " + request.downloadHandler.text);
                onResponse?.Invoke(AIResponse.Fallback());
            }
        }
    }

    AIResponse ParseGroqResponse(string json)
    {
        try
        {
            GroqApiResponse outer = JsonUtility.FromJson<GroqApiResponse>(json);

            if (outer == null || outer.choices == null || outer.choices.Length == 0)
            {
                Debug.LogWarning("[Groq] No choices in response.");
                return AIResponse.Fallback();
            }

            string content = outer.choices[0].message.content;

            if (string.IsNullOrEmpty(content))
            {
                Debug.LogWarning("[Groq] Content is empty.");
                return AIResponse.Fallback();
            }

            // Strip markdown fences if model added them anyway
            content = content.Trim();
            if (content.StartsWith("```"))
            {
                int firstNewline = content.IndexOf('\n');
                int lastFence = content.LastIndexOf("```");
                if (firstNewline > 0 && lastFence > firstNewline)
                    content = content.Substring(
                        firstNewline + 1,
                        lastFence - firstNewline - 1).Trim();
            }

            LLMEvaluation eval = JsonUtility.FromJson<LLMEvaluation>(content);

            if (eval == null || string.IsNullOrEmpty(eval.response))
            {
                Debug.LogWarning("[Groq] Could not parse LLM JSON. Raw: " + content);
                return AIResponse.Fallback();
            }

            AIResponse result = new AIResponse();
            result.dialogueText = eval.response;
            result.trustChange = Mathf.Clamp(eval.trust_change, -20f, 20f);
            result.emotion = string.IsNullOrEmpty(eval.emotion) ? "neutral" : eval.emotion;
            result.reasoning = eval.reasoning;
            return result;
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Groq] Parse exception: " + e.Message);
            return AIResponse.Fallback();
        }
    }

    string EscapeJson(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        string result = text;
        result = result.Replace("\\", "\\\\");
        result = result.Replace("\"", "\\\"");
        result = result.Replace("\n", "\\n");
        result = result.Replace("\r", "");
        result = result.Replace("\t", "\\t");
        return result;
    }
}