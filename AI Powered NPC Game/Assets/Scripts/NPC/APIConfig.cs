using UnityEngine;

[CreateAssetMenu(fileName = "APIConfig",
                 menuName = "NPC/API Config")]
public class APIConfig : ScriptableObject
{
    [Header("Groq Settings")]
    public string apiKey = "";

    public string model = "llama-3.3-70b-versatile";

    public int maxTokens = 100;
}