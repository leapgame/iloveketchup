using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PowerMoving : MonoBehaviour
{
    [SerializeField] private Transform transMovingAnchor;
    [SerializeField] private Image sprtBar;
    public void SetData(float rotation)
    {
        this.transMovingAnchor.eulerAngles = new Vector3(0.0f, 0.0f, rotation);
    }

    public void DoEffectLastTouch()
    {
        this.gameObject.SetActive(true);
        float animTime = 0.35f;
        transform.DOKill();
        
        transform.localScale = Vector3.one;
        sprtBar.CrossFadeAlpha(1.0f, 0.0f, true);
        
        transform
            .DOScale(Vector3.one * 1.1f, animTime)
            .SetEase(Ease.OutBack);

        sprtBar.CrossFadeAlpha(0.0f, animTime, true);
    }
}
