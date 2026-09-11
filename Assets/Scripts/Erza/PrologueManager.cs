using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class MonologueLine
{
    [TextArea(2, 5)]
    public string line;
    public Sprite background;
    public bool hasSpirit;
}

public class PrologueManager : MonoBehaviour
{
    public InputActionReference enterAction;
    public TextMeshProUGUI monologueText;
    public GameObject monologueBox;
    public Image backgroundImage;

    public GameObject spirit;

    public MonologueLine[] monologueLines;

    public float typingSpeed = 0.05f;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    public bool isEpilogue = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (enterAction != null)
        {
            enterAction.action.Enable();
        }
        ShowLine();
    }

    // Update is called once per frame
    void Update()
    {
        if (enterAction.action.triggered)
        {
            if (isTyping)
            {
                StopAllCoroutines();
                backgroundImage.color = Color.white;
                monologueText.text = monologueLines[currentLineIndex].line;
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    void ShowLine()
    {
        MonologueLine line = monologueLines[currentLineIndex];
        if (line.background != null && backgroundImage.sprite != line.background)
        {
            StartCoroutine(fadeBackground(line.background, 0.5f));
        }
        StartCoroutine(TypeLine(line.line));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        monologueText.text = "";
        foreach (char letter in line.ToCharArray())
        {
            monologueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void NextLine()
    {
        currentLineIndex++;
        if (currentLineIndex < monologueLines.Length)
        {
            ShowLine();

        }
        else
        {
            EndMonologue();
        }
    }

    void EndMonologue()
    {
        monologueBox.SetActive(false);
        if (isEpilogue)
        {
            FadeTransition.Instance.TransitionToScene("MainMenu");
        }
        else
        {
            FadeTransition.Instance.TransitionToScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    IEnumerator fadeBackground(Sprite newSprite, float duration)
    {
        float elapsedTime = 0f;
        Sprite originalSprite = backgroundImage.sprite;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            backgroundImage.sprite = newSprite;
            if (spirit != null) spirit.SetActive(monologueLines[currentLineIndex].hasSpirit);
            backgroundImage.color = new Color(1f, 1f, 1f, t);
            yield return null;
        }
        backgroundImage.sprite = newSprite;
        backgroundImage.color = Color.white;
        backgroundImage.color = new Color(1f, 1f, 1f);
    }
}
