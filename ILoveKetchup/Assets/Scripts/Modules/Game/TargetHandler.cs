using UnityEngine;
using Suriyun;
using UnityEngine.Serialization;

public class TargetHandler : AbstractSingleton<TargetHandler>
{
    [SerializeField] AnimatorController m_AnimatorController;
    [SerializeField] Swapper m_Swapper;
    public Transform transKetchupRaycastAnchor;

    bool m_IsFree = true;
    float m_TimeCount = 0f;
    float m_RandomAnimationTime = 0f;
    int m_NumberOfChanges = 0;
    private readonly string s_Animation = "animation";
    private readonly string s_Impact = "Impact";
    private readonly string s_ClearImpact = "ImpactHigh";
    private readonly string s_Happy = "Happy";
    private readonly string s_Idle = "Idle";

    // Start is called before the first frame update
    void Start()
    {
        PlayRandomAnimation();

        Throwable.OnThrowableHitTarget += OnThrowableHitTargetListener;
    }

    private void OnDestroy()
    {
        Throwable.OnThrowableHitTarget -= OnThrowableHitTargetListener;
    }

    void PlayRandomAnimation()
    {
        m_NumberOfChanges++;

        if (m_NumberOfChanges % 2 == 0)
        {
            m_AnimatorController.SetInt(s_Animation + "," + s_Idle);
        } else
        {
            int[] availableIdleAnimationIndex = { 1, 3, 4, 5, 6, 10, 11, 12, 13, 14, 15, 16, 17, 18 };
            int randomIdx = availableIdleAnimationIndex[Random.Range(0, availableIdleAnimationIndex.Length)];
            m_AnimatorController.SetInt(s_Animation + "," + randomIdx.ToString());
        }
    }

    public GameObject GetCurrentActiveTarget()
    {
        try
        {
            return this.m_Swapper.ActiveCharacter;
        }
        catch
        {
            return null;
        }
    }

    public void Idle ()
    {
        m_AnimatorController.SetTriggerSingleParam(s_Idle);
    }

    public void Impact ()
    {
        m_IsFree = false;
        m_AnimatorController.SetTriggerSingleParam(s_Impact);
    }

    public void ClearImpact ()
    {
        m_IsFree = false;
        m_AnimatorController.SetTriggerSingleParam(s_ClearImpact);
    }

    public void Happy ()
    {
        m_IsFree = false;
        m_AnimatorController.SetTriggerSingleParam(s_Happy);
    }

    public Vector3 TargetPosition()
    {
        return m_Swapper.CharacterPosition();
    }

    private void Update()
    {
        if (!m_IsFree) return;

        m_TimeCount += Time.deltaTime;

        if (m_TimeCount > m_RandomAnimationTime)
        {
            PlayRandomAnimation();

            m_TimeCount -= m_RandomAnimationTime;
            m_RandomAnimationTime = Random.Range(2f, 5f);
        }
    }

    void OnThrowableHitTargetListener(Throwable throwable)
    {
        Impact();
    }
}
