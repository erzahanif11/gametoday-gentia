using UnityEngine;

public class LoadLevel : MonoBehaviour
{
    public int levelIndex;
    public LevelManager levelManager;
    public GameObject entryDoor;
    public GameObject exitDoor;

    void OnEnable(){
        levelManager = FindAnyObjectByType<LevelManager>();

        if(levelManager != null){
            levelManager.OnLevelCompleted += OpenExitDoor;
        }
    }

    void OnDisable(){
        if(levelManager != null){
            levelManager.OnLevelCompleted -= OpenExitDoor;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player")){
            if(levelManager.LoadLevel(levelIndex)){
                entryDoor.SetActive(true);
                exitDoor.SetActive(true);
            }
            
        }
    }

    void OpenExitDoor(){
        exitDoor.SetActive(false);
    }
}
