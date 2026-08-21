using UnityEngine;

public class LoadLevel : MonoBehaviour
{
    public int levelIndex;
    public LevelManager levelManager;

    void OnEnable(){
        levelManager = FindAnyObjectByType<LevelManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player")){
            levelManager.LoadLevel(levelIndex);
        }
    }
}
