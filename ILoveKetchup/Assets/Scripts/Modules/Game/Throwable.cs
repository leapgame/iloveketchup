using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Throwable : MonoBehaviour
{
    public static Action<Throwable> OnThrowableHitTarget = delegate { };
    bool m_IsThrown = false;
    Transform m_Transform;

    // Start is called before the first frame update
    void Start()
    {
        m_Transform = transform;
    }

    public void Throw(Vector3 destination, float power)
    {
        if (m_IsThrown) return;

        m_IsThrown = true;

        DOTween.Kill(this);
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(2f);
        seq.Append(m_Transform.DORotate(new Vector3(90f, 0, 0), 0.5f, RotateMode.WorldAxisAdd).SetEase(Ease.OutBack));
        seq.Join(m_Transform.DOLocalMove(new Vector3(2f, -3f, -2f), 0.5f));
        seq.Append(m_Transform.DOShakePosition(0.1f, 0.25f).SetLoops(10));
        seq.Append(m_Transform.DOJump(destination - new Vector3(0, -3f, 1f), 15f / power, 1, 5f / power)).SetEase(Ease.OutBack);
        seq.Join(m_Transform.DORotate(new Vector3(360f, 0, 0), 5f / power, RotateMode.WorldAxisAdd).SetEase(Ease.OutBack));
        // seq.AppendInterval(0.75f);
        // seq.Append(m_Transform.DOMove(new Vector3(destination.x, destination.y, destination.z - UnityEngine.Random.Range(1f, 1.5f)), 1f));
        // seq.Join(m_Transform.DORotate(new Vector3(-120f, 0, 0), 1f, RotateMode.WorldAxisAdd).SetEase(Ease.OutBack));
        seq.AppendCallback(() =>
        {
            OnThrowableHitTarget(this);
        });
        seq.SetTarget(this);
    }
}
