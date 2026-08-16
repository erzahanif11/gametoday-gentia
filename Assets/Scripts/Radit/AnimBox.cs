using UnityEngine;
using DG.Tweening;

public class AnimBox : MonoBehaviour
{

    [SerializeField] private float dropDistance = 0.3f;
    [SerializeField] private float duration = 0.6f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    [ContextMenu("AnimTest")]
    public void AnimTest()
    {
        Vector3 finalPos = transform.localPosition;
        Vector3 startPos = finalPos + Vector3.up * dropDistance;

        // set the starting state before animating
        transform.localPosition = startPos;
        Color c = spriteRenderer.color;
        spriteRenderer.color = new Color(c.r, c.g, c.b, 0f);

        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOLocalMove(finalPos, duration).SetEase(Ease.OutCubic));
        seq.Join(spriteRenderer.DOFade(1f, duration).SetEase(Ease.OutCubic));
    }
}
