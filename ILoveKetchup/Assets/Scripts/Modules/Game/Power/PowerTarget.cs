using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PowerTarget : MonoBehaviour
{
    [SerializeField] private Transform transTargetAnchor;
    [SerializeField] private Image sprtRangeScore;

    public void SetData(float rotation, float range, Color col)
    {
        this.transTargetAnchor.eulerAngles = new Vector3(0.0f, 0.0f, rotation);
        this.sprtRangeScore.fillAmount = (range * 2.0f ) / 360.0f;
        this.sprtRangeScore.transform.localEulerAngles = new Vector3(0.0f, 0.0f, range);
        this.sprtRangeScore.color = col; 
    }

    public void DoEffectScored()
    {
        this.gameObject.SetActive(true);
        float animTime = 0.5f;
        
        transform.DOKill();
        transform.localScale = Vector3.one;
        
        transform
            .DOScale(Vector3.one * 1.1f, animTime)
            .SetEase(Ease.OutBack);

        sprtRangeScore.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), animTime).SetEase(Ease.OutQuart);
    }
}
