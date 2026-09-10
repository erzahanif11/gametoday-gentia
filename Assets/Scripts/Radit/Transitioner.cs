using UnityEngine;
using DG.Tweening;

// 1. Tambahkan IInteractable di sebelah MonoBehaviour
public class Transitioner : MonoBehaviour, IInteractable
{
    [SerializeField] private string _sceneName;
    [SerializeField] private bool _isInteractable;

    [Header("Optional Popup Animation")]
    [SerializeField] private bool _usePopup;
    [SerializeField] private GameObject _popupObject;
    [SerializeField] private float _popupMoveY = 0.5f;
    [SerializeField] private float _popupDuration = 0.3f;

    private Vector3 _originalPopupPos;

    private void Start()
    {
        if (_usePopup && _popupObject != null)
        {
            _originalPopupPos = _popupObject.transform.localPosition;
            _popupObject.SetActive(false);
        }
    }

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
        if (collision.gameObject.CompareTag("Player"))
        {
            // 3. Untuk pintu otomatis (tidak butuh ditekan)
            if (!_isInteractable)
            {
                FadeTransition.Instance.TransitionToScene(_sceneName);
            }
            else if (_usePopup && _popupObject != null)
            {
                ShowPopup();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && _isInteractable && _usePopup && _popupObject != null)
        {
            HidePopup();
        }
    }

    private void ShowPopup()
    {
        _popupObject.SetActive(true);

        _popupObject.transform.DOKill();
        _popupObject.transform.localPosition = _originalPopupPos - new Vector3(0, _popupMoveY, 0);
        _popupObject.transform.DOLocalMoveY(_originalPopupPos.y, _popupDuration).SetEase(Ease.OutBack);

        if (_popupObject.TryGetComponent<SpriteRenderer>(out var sr))
        {
            sr.DOKill();
            Color c = sr.color;
            c.a = 0;
            sr.color = c;
            sr.DOFade(1f, _popupDuration);
        }
        else if (_popupObject.TryGetComponent<CanvasGroup>(out var cg))
        {
            cg.DOKill();
            cg.alpha = 0;
            cg.DOFade(1f, _popupDuration);
        }
    }

    private void HidePopup()
    {
        _popupObject.transform.DOKill();
        _popupObject.transform.DOLocalMoveY(_originalPopupPos.y - _popupMoveY, _popupDuration).SetEase(Ease.InBack);

        if (_popupObject.TryGetComponent<SpriteRenderer>(out var sr))
        {
            sr.DOKill();
            sr.DOFade(0f, _popupDuration).OnComplete(() => _popupObject.SetActive(false));
        }
        else if (_popupObject.TryGetComponent<CanvasGroup>(out var cg))
        {
            cg.DOKill();
            cg.DOFade(0f, _popupDuration).OnComplete(() => _popupObject.SetActive(false));
        }
        else
        {
            // Jika tidak ada komponen transparansi, tunggu sebentar lalu nonaktifkan
            DOVirtual.DelayedCall(_popupDuration, () =>
            {
                if (_popupObject != null) _popupObject.SetActive(false);
            });
        }
    }
}