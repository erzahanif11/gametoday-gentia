using UnityEngine;
using System.Collections; 
using TMPro;
using UnityEngine.InputSystem;

[System.Serializable]
public class MonologueLine{
    [TextArea(2, 5)]
    public string line;
    public Sprite background;
}

public class PrologueManager : MonoBehaviour
{
    public InputActionReference enterAction;
    public TextMeshProUGUI monologueText;
    public GameObject monologueBox;
    public UnityEngine.UI.Image backgroundImage;

    public MonologueLine[] monologueLines;

    public float typingSpeed = 0.05f;

    private int currentLineIndex = 0;
    private bool isTyping = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowLine();
    }

    // Update is called once per frame
    void Update()
    {
        if (enterAction.action.triggered){
            if (isTyping){
                StopAllCoroutines();
                monologueText.text = monologueLines[currentLineIndex].line;
                isTyping = false;
            } else {
                NextLine();
            }
        }
    }

    void ShowLine(){
        MonologueLine line = monologueLines[currentLineIndex];
        if (line.background != null){
            backgroundImage.sprite = line.background;
        }
        StartCoroutine(TypeLine(line.line));
    }

    IEnumerator TypeLine(string line){
        isTyping = true;
        monologueText.text = "";
        foreach (char letter in line.ToCharArray()){
            monologueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void NextLine(){
        currentLineIndex++;
        if (currentLineIndex < monologueLines.Length){
            ShowLine();
        } else {
            EndMonologue();
        }
    }

    void EndMonologue(){
        monologueBox.SetActive(false);
    }
}
