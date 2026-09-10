using UnityEngine;

// 1. Tambahkan IInteractable di sebelah MonoBehaviour
public class Transitioner : MonoBehaviour, IInteractable
{
    [SerializeField] private string _sceneName;
    [SerializeField] private bool _isInteractable;

    // Tidak perlu lagi InputActionReference di sini!
    // PlayerInteract yang akan mengurus tombolnya.

    // 2. Fungsi ini wajib ada karena kita memakai IInteractable
    public void Interact()
    {
        if (_isInteractable)
        {
            Debug.Log("Pintu diinteraksi! Pindah scene...");
            FadeTransition.Instance.TransitionToScene(_sceneName);
        }
    }

    public string GetInteractText()
    {
        if (_isInteractable) return "Tekan E untuk naik elevator";
        return "";
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 3. Untuk pintu otomatis (tidak butuh ditekan)
        if (collision.gameObject.CompareTag("Player") && !_isInteractable)
        {
            FadeTransition.Instance.TransitionToScene(_sceneName);
        }
    }
}