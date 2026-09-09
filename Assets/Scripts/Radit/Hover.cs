using UnityEngine;
using DG.Tweening;

public class Hover : MonoBehaviour
{
    [Header("Hover Settings")]
    [Tooltip("How far the object moves up from its starting position.")]
    public float hoverDistance = 0.5f;

    [Tooltip("How many seconds it takes to complete one up or down movement.")]
    public float duration = 2f;

    void Start()
    {
        // We use DOLocalMoveY so the effect works correctly even if the object is parented.
        transform.DOLocalMoveY(transform.localPosition.y + hoverDistance, duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    void OnDestroy()
    {
        // Best practice: kill the tween if the game object is destroyed 
        // to prevent DOTween from trying to animate a null reference.
        transform.DOKill();
    }
}