using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class LoadLevel : MonoBehaviour
{
    public int levelIndex;
    public LevelManager levelManager;
    public GameObject entryDoor;
    public GameObject exitDoor;
    public CinemachineCamera gateCamera;
    public CinemachineCamera spiritAreaCamera;

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

    public void InteractLoadLevel()
    {
        if(levelManager.LoadLevel(levelIndex)){
            entryDoor.SetActive(true);
            exitDoor.SetActive(true);
            StartCoroutine(CameraTransition(spiritAreaCamera));
        }
    }

    void OpenExitDoor(int completedLevelIndex){
        if (completedLevelIndex != levelIndex){
            return;
        }

        exitDoor.SetActive(false);
        StartCoroutine(CameraTransition(gateCamera));
    }

    IEnumerator CameraTransition(CinemachineCamera cam)
    {
        cam.Priority = 2;

        yield return new WaitForSeconds(3f);

        cam.Priority = 0;
    }
}
