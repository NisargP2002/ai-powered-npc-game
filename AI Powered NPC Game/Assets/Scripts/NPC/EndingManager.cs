using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance;

    [Header("Ending UI")]
    public GameObject endingCanvas;
    public Image fadeImage;
    public TextMeshProUGUI endingText;

    private bool endingStarted = false;

    void Awake()
    {
        Instance = this;
    }

    public void TriggerEnding()
    {
        if (endingStarted)
            return;

        endingStarted = true;
        StartCoroutine(EndingSequence());
    }

    [ContextMenu("Test Ending")]
    public void TestEnding()
    {
        TriggerEnding();
    }

    IEnumerator EndingSequence()
    {
        // Show ending canvas
        endingCanvas.SetActive(true);

        // Fade to black
        Color c = fadeImage.color;

        float timer = 0f;

        while (timer < 3f)
        {
            timer += Time.deltaTime;

            c.a = Mathf.Lerp(0f, 1f, timer / 3f);

            fadeImage.color = c;

            yield return null;
        }

        // Show ending text
        endingText.gameObject.SetActive(true);

        endingText.text =
            "The Accord is complete.\n\n" +
            "The Ashwood remembers.\n\n" +
            "Thank you for playing.";

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}