using UnityEngine;

// Holds everything the Groq LLM returns in one call.
[System.Serializable]
public class AIResponse
{
    public string dialogueText = "";
    public float trustChange = 0f;
    public string emotion = "neutral";
    public string reasoning = "";

    public static AIResponse Fallback()
    {
        AIResponse r = new AIResponse();
        r.dialogueText = "The roots speak when the sky falls silent. Ask me again.";
        r.trustChange = 0f;
        r.emotion = "neutral";
        r.reasoning = "fallback";
        return r;
    }
}

// These three classes parse the outer Groq API response envelope
[System.Serializable]
public class GroqApiResponse
{
    public GroqChoice[] choices;
}

[System.Serializable]
public class GroqChoice
{
    public GroqMessage message;
}

[System.Serializable]
public class GroqMessage
{
    public string content;
}

// This class parses the inner JSON that the LLM writes
[System.Serializable]
public class LLMEvaluation
{
    public string response = "";
    public float trust_change = 0f;
    public string emotion = "neutral";
    public string reasoning = "";
}