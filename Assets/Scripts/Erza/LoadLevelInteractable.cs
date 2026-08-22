using UnityEngine;

public class LoadLevelInteractable : MonoBehaviour, IInteractable
{
    public LoadLevel loadLevel;

    void OnEnable(){
        loadLevel = GetComponent<LoadLevel>();
    }

    public void Interact(){
        loadLevel.InteractLoadLevel();
    }

    public string GetInteractText(){
        return "(F) Load/Reset Level";
    } 
}
