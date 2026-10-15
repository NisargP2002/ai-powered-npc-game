using UnityEngine;
using UnityEngine.UI;

public class FloatingEmotionIcon : MonoBehaviour
{
    [Header("References")]
    public Transform eolindraTransform;
    public NPCEmotionSystem emotionSystem;
    public RectTransform rectTransform;
    public Image iconImage;

    [Header("Sprites")]
    public Sprite spriteThinking;
    public Sprite spriteNeutral;
    public Sprite spriteWarm;
    public Sprite spriteSad;
    public Sprite spriteAngry;
    public Sprite spriteHostile;
    public Sprite spriteSuspicious;
    public Sprite spriteCurious;
    public Sprite spriteGrateful;

    [Header("Position above her head (world units)")]
    public float worldYOffset = 3.2f;

    [Header("Bob Animation")]
    public float bobSpeed = 1.5f;
    public float bobAmount = 8f;

    private Camera mainCamera;
    private bool isVisible = false;
    private EmotionState lastEmotion = EmotionState.Neutral;

    void Start()
    {
        mainCamera = Camera.main;
        if (iconImage != null)
            iconImage.enabled = false;
    }

    void Update()
    {
        if (!isVisible || eolindraTransform == null || mainCamera == null) return;
        if (rectTransform == null || iconImage == null) return;

        FollowEolindra();
        Bob();
        CheckEmotionChanged();
    }

    void FollowEolindra()
    {
        Vector3 worldPos = eolindraTransform.position + Vector3.up * worldYOffset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        if (screenPos.z > 0)
        {
            rectTransform.position = screenPos;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }

    void Bob()
    {
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        rectTransform.anchoredPosition += new Vector2(0, bob * Time.deltaTime);
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
        Sprite newSprite = spriteNeutral;

        switch (emotion)
        {
            case EmotionState.Hostile:    newSprite = spriteHostile; break;
            case EmotionState.Angry:      newSprite = spriteAngry; break;
            case EmotionState.Suspicious: newSprite = spriteSuspicious; break;
            case EmotionState.Sad:        newSprite = spriteSad; break;
            case EmotionState.Neutral:    newSprite = spriteNeutral; break;
            case EmotionState.Curious:    newSprite = spriteCurious; break;
            case EmotionState.Warm:       newSprite = spriteWarm; break;
            case EmotionState.Grateful:   newSprite = spriteGrateful; break;
        }

        if (newSprite != null && iconImage != null)
            iconImage.sprite = newSprite;

        Debug.Log("[EmotionIcon] " + emotion);
    }

    public void ShowIcon()
    {
        isVisible = true;
        if (iconImage != null) iconImage.enabled = true;
    }

    public void HideIcon()
    {
        isVisible = false;
        if (iconImage != null) iconImage.enabled = false;
    }

    public void SetThinking(bool thinking)
    {
        if (thinking && spriteThinking != null && iconImage != null)
            iconImage.sprite = spriteThinking;
        else if (!thinking)
            ApplyEmotion(lastEmotion);
    }
}
