using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmotionBubbleController : MonoBehaviour
{
    [Header("Drag Eolindra here")]
    public NPCEmotionSystem emotionSystem;

    [Header("UI References")]
    public Image bubblePanel;
    public Image bubbleIconImage;
    public TMP_Text labelText;

    [Header("Emotion Sprites -- drag one image per emotion")]
    public Sprite hostileSprite;
    public Sprite angrySprite;
    public Sprite suspiciousSprite;
    public Sprite sadSprite;
    public Sprite neutralSprite;
    public Sprite curiousSprite;
    public Sprite warmSprite;
    public Sprite gratefulSprite;

    [Header("Bob Animation")]
    public float bobSpeed = 1.4f;
    public float bobHeight = 0.06f;

    private Camera mainCamera;
    private Vector3 startLocalPos;
    private EmotionState lastEmotion = EmotionState.Neutral;

    // Panel background colours per emotion
    private static readonly Color PanelHostile    = new Color(0.50f, 0.00f, 0.00f, 0.90f);
    private static readonly Color PanelAngry      = new Color(0.74f, 0.13f, 0.13f, 0.90f);
    private static readonly Color PanelSuspicious = new Color(0.45f, 0.35f, 0.20f, 0.90f);
    private static readonly Color PanelSad        = new Color(0.16f, 0.30f, 0.68f, 0.90f);
    private static readonly Color PanelNeutral    = new Color(0.27f, 0.27f, 0.43f, 0.90f);
    private static readonly Color PanelCurious    = new Color(0.20f, 0.50f, 0.55f, 0.90f);
    private static readonly Color PanelWarm       = new Color(0.13f, 0.55f, 0.29f, 0.90f);
    private static readonly Color PanelGrateful   = new Color(0.60f, 0.45f, 0.10f, 0.90f);

    void Start()
    {
        mainCamera = Camera.main;
        startLocalPos = transform.localPosition;
        ApplyEmotion(EmotionState.Neutral);
    }

    void Update()
    {
        FaceCamera();
        Bob();
        CheckEmotionChanged();
    }

    void FaceCamera()
    {
        if (mainCamera == null) return;
        transform.LookAt(
            transform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up);
    }

    void Bob()
    {
        float newY = startLocalPos.y
                   + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = new Vector3(
            startLocalPos.x, newY, startLocalPos.z);
    }

    void CheckEmotionChanged()
    {
        if (emotionSystem == null) return;
        if (emotionSystem.currentEmotion == lastEmotion) return;
        lastEmotion = emotionSystem.currentEmotion;
        ApplyEmotion(lastEmotion);
    }

    void ApplyEmotion(EmotionState emotion)
    {
        Color panelCol;
        Sprite iconSprite;
        string label;

        switch (emotion)
        {
            case EmotionState.Hostile:
                panelCol = PanelHostile;
                iconSprite = hostileSprite;
                label = "Hostile";
                break;
            case EmotionState.Angry:
                panelCol = PanelAngry;
                iconSprite = angrySprite;
                label = "Angry";
                break;
            case EmotionState.Suspicious:
                panelCol = PanelSuspicious;
                iconSprite = suspiciousSprite;
                label = "Suspicious";
                break;
            case EmotionState.Sad:
                panelCol = PanelSad;
                iconSprite = sadSprite;
                label = "Sad";
                break;
            case EmotionState.Curious:
                panelCol = PanelCurious;
                iconSprite = curiousSprite;
                label = "Curious";
                break;
            case EmotionState.Warm:
                panelCol = PanelWarm;
                iconSprite = warmSprite;
                label = "Warm";
                break;
            case EmotionState.Grateful:
                panelCol = PanelGrateful;
                iconSprite = gratefulSprite;
                label = "Grateful";
                break;
            default:
                panelCol = PanelNeutral;
                iconSprite = neutralSprite;
                label = "Neutral";
                break;
        }

        if (bubblePanel != null)
            bubblePanel.color = panelCol;

        if (bubbleIconImage != null)
        {
            if (iconSprite != null)
                bubbleIconImage.sprite = iconSprite;
            bubbleIconImage.color = Color.white;
            bubbleIconImage.preserveAspect = true;
        }

        if (labelText != null)
            labelText.text = label;

        Debug.Log("[EmotionBubble] " + label);
    }
}