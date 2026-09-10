using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class AngelAscentTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("The name of the scene to load. Leave empty to use build index instead.")]
    [SerializeField] private string _nextSceneName;
    [SerializeField] private bool _useBuildIndex = true; // Default loads next scene in build settings
    
    [Header("Ascension Settings")]
    [Tooltip("Reference to the player/angel GameObject to animate. If left empty, will use the Player that enters the trigger.")]
    [SerializeField] private GameObject _angelReference;
    
    [SerializeField] private float _ascendHeight = 10f;
    [SerializeField] private float _ascendDuration = 3f;
    [SerializeField] private string _ascendAnimStateName = "TurunAngel";
    
    private bool _hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_hasTriggered) return;

        // Check if the object entering the trigger is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            _hasTriggered = true;
            
            // If _angelReference wasn't set in the inspector, use the object that collided
            GameObject targetAngel = _angelReference != null ? _angelReference : collision.gameObject;
            
            StartAscension(targetAngel);
        }
    }

    private void StartAscension(GameObject targetAngel)
    {
        // Disable MovePlayer so they can't move during the transition
        MovePlayer movePlayer = targetAngel.GetComponent<MovePlayer>();
        if (movePlayer != null)
        {
            movePlayer.enabled = false;
        }

        // Play the animation
        Animator anim = targetAngel.GetComponent<Animator>();
        if (anim != null)
        {
            anim.CrossFade(_ascendAnimStateName, 0.1f);
        }

        // Animate upwards using DOTween
        targetAngel.transform.DOMoveY(targetAngel.transform.position.y + _ascendHeight, _ascendDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                TransitionToNextScene();
            });
    }

    private void TransitionToNextScene()
    {
        if (FadeTransition.Instance != null)
        {
            if (_useBuildIndex)
            {
                FadeTransition.Instance.TransitionToScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                FadeTransition.Instance.TransitionToScene(_nextSceneName);
            }
        }
        else
        {
            Debug.LogWarning("FadeTransition instance not found! Trying to load scene directly.");
            if (_useBuildIndex)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            else
                SceneManager.LoadScene(_nextSceneName);
        }
    }
}
