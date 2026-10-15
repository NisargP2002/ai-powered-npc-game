using UnityEngine;
// ScriptableObject -- stores API credentials.
// Create via: right-click in Project -> NPC -> API Config
// Add Assets/Settings/APIConfig.asset to .gitignore -- never commit keys.
[CreateAssetMenu(fileName = "APIConfig", menuName = "NPC/API Config")]
public class APIConfig : ScriptableObject
{
    [Header("Groq API Settings")]
    [TextArea]
    public string groqApiKey = ""; // paste your key here in the Inspector
    public string groqModel = "llama-3.3-70b-versatile";
    public string groqEndpoint = "https://api.groq.com/openai/v1/chat/completions";
    [Header("Response Settings")]
    // Keep this at 400-500. JSON responses need more tokens than plain text.
    public int maxTokens = 450;
}