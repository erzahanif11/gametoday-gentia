using UnityEngine;

public class Transitioner : MonoBehaviour
{
    [SerializeField] private string _sceneName;
    private Collider2D collider2D;

    private void start()
    {
        collider2D = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            FadeTransition.Instance.TransitionToScene(_sceneName);
        }
    }
}
