using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class AIRequestHandler : MonoBehaviour
{
    [Header("Config")]
    public APIConfig config;

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    class RequestBody
    {
        public string model;
        public int max_tokens;
        public List<Message> messages;
    }

    [System.Serializable]
    class Choice
    {
        public Message message;
    }

    [System.Serializable]
    class ResponseBody
    {
        public Choice[] choices;
    }

    public void SendRequest(
        List<Message> messages,
        System.Action<string> onResponse)
    {
        StartCoroutine(
            SendToGroq(messages, onResponse));
    }

    IEnumerator SendToGroq(
        List<Message> messages,
        System.Action<string> onResponse)
    {
        if (config == null)
        {
            onResponse?.Invoke(
                "The forest spirits are silent.");
            yield break;
        }

        var body = new RequestBody
        {
            model = config.model,
            max_tokens = config.maxTokens,
            messages = messages
        };

        string json = JsonUtility.ToJson(body);

        UnityWebRequest request =
            new UnityWebRequest(
                "https://api.groq.com/openai/v1/chat/completions",
                "POST");

        request.uploadHandler =
            new UploadHandlerRaw(
                Encoding.UTF8.GetBytes(json));

        request.downloadHandler =
            new DownloadHandlerBuffer();

        request.SetRequestHeader(
            "Content-Type",
            "application/json");

        request.SetRequestHeader(
            "Authorization",
            "Bearer " + config.apiKey);

        request.timeout = 15;

        yield return request.SendWebRequest();

        if (request.result ==
            UnityWebRequest.Result.Success)
        {
            ResponseBody response =
                JsonUtility.FromJson<ResponseBody>(
                    request.downloadHandler.text);

            if (response != null &&
                response.choices != null &&
                response.choices.Length > 0)
            {
                onResponse?.Invoke(
                    response.choices[0].message.content);
            }
            else
            {
                onResponse?.Invoke(
                    "The roots whisper, but I cannot hear them clearly.");
            }
        }
        else
        {
            Debug.LogError(request.downloadHandler.text);

            onResponse?.Invoke(
                "The forest spirits are silent...");
        }
    }
}