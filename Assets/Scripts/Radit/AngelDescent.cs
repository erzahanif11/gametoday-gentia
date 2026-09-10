using UnityEngine;
using DG.Tweening;

public class AngelDescent : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("The Animator attached to your player prefab.")]
    public Animator animator;

    [Tooltip("An empty GameObject placed where you want the angel to land.")]
    public Transform landingSpot;

    [Header("Descent Settings")]
    public float descendDuration = 4f;
    public Ease descendEase = Ease.InOutQuad; // InOutQuad gives a smooth start and soft landing

    [Header("Animator State Names")]
    [Tooltip("The exact name of the Animator state containing your descending/holy circle clip.")]
    public string descendAnimStateName = "TurunAngel";

    [Tooltip("The exact name of your normal Idle/Movement blend tree or state.")]
    public string normalAnimStateName = "IdleAngel";

    [Tooltip("References")]
    [SerializeField] private GameObject _boundary;

    private void Start()
    {
        Invoke("StartDescent", 0.1f);
    }

    // You can call this method from a timeline, a trigger collider, or another script
    public void StartDescent()
    {
        // 1. Disable player movement/input here if you have a controller script
        // GetComponent<PlayerController>().enabled = false;

        // 2. Tell the Animator to play the descending clip (Clip 1)
        // CrossFade blends smoothly into the animation over 0.1 seconds
        GetComponent<MovePlayer>().enabled = false;
        animator.CrossFade(descendAnimStateName, 0.1f);

        // 3. Move the player to the landing spot using DOTween
        transform.DOMove(landingSpot.position, descendDuration)
            .SetEase(descendEase)
            .OnComplete(OnLanded); // When the tween finishes, call the OnLanded method
    }

    private void OnLanded()
    {
        // 4. Transition smoothly back to normal walking/idle animation
        animator.CrossFade(normalAnimStateName, 0.25f);

        // 5. Re-enable player movement/input here
        // GetComponent<PlayerController>().enabled = true;

        // Optional: Play a landing particle effect or sound here!
        Debug.Log("Angel has landed!");
        GetComponent<MovePlayer>().enabled = true;
        _boundary.SetActive(true);
    }

    void OnDestroy()
    {
        // Always good practice to kill tweens if the object is destroyed mid-flight
        transform.DOKill();
    }
}