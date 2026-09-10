using UnityEngine;
using DG.Tweening;

public class PlayerEntrance : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;

    [Tooltip("Titik target berhentinya player di dalam scene.")]
    public Transform targetPosition;

    [Header("Entrance Settings")]
    public float walkDuration = 3f;
    public Ease walkEase = Ease.Linear; // Linear agar gerakan kaki sinkron dengan perpindahan

    [Header("Animator Parameters")]
    [Tooltip("Nilai parameter HorizontalInput saat berjalan ke kiri. Biasanya -1.")]
    public float walkLeftValue = -1f;


    [Tooltip("References")]
    [SerializeField] private GameObject _boundary;
    private void Start()
    {
        // Beri sedikit jeda agar Animator dan script lain selesai inisialisasi
        Invoke("StartEntrance", 0.1f);
    }

    public void StartEntrance()
    {
        // 1. Matikan kontrol player agar tidak bentrok dengan input pemain
        GetComponent<MovePlayer>().enabled = false;

        // 2. Manipulasi parameter Animator untuk memicu animasi jalan ke kiri
        // Ini akan otomatis mengikuti transisi/blend tree yang sudah ada
        animator.SetFloat("HorizontalInput", walkLeftValue);
        animator.SetFloat("VerticalInput", 0f); // Pastikan tidak ada input vertikal

        // 3. Gerakkan posisi player menggunakan DOTween
        transform.DOMove(targetPosition.position, walkDuration)
            .SetEase(walkEase)
            .OnComplete(OnEntranceComplete);
    }

    private void OnEntranceComplete()
    {
        // 4. Kembalikan parameter ke 0 agar transisinya otomatis memicu animasi Idle
        animator.SetFloat("HorizontalInput", 0f);

        // 5. Nyalakan kembali script pergerakan agar player bisa dimainkan
        GetComponent<MovePlayer>().enabled = true;

        _boundary.SetActive(true);

        Debug.Log("Player sudah di posisi dan siap dimainkan!");
    }

    void OnDestroy()
    {
        // Hapus tween jika object hancur sebelum selesai berjalan
        transform.DOKill();
    }
}