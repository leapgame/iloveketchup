using UnityEngine;
using Suriyun;

public class TargetHandler : AbstractSingleton<TargetHandler>
{
    [SerializeField] AnimatorController m_AnimatorController;

    bool m_IsFree = true;
    float m_TimeCount = 0f;
    float m_RandomAnimationTime = 0f;
    private readonly string s_Animation = "animation";
    private readonly string s_Impact = "7";
    private readonly string s_ClearImpact = "8";
    private readonly string s_Happy = "2";

    // Start is called before the first frame update
    void Start()
    {
        PlayRandomAnimation();
    }

    void PlayRandomAnimation()
    {
        int[] availableIdleAnimationIndex = { 1, 3, 4, 5, 6, 10, 11, 12, 13, 14, 15, 16, 17, 18 };
        int randomIdx = availableIdleAnimationIndex[Random.Range(0, availableIdleAnimationIndex.Length)];
        m_AnimatorController.SetInt(s_Animation + "," + randomIdx.ToString());
    }

    public void Impact ()
    {
        m_IsFree = false;
        m_AnimatorController.SetInt(s_Animation + "," + s_Impact);
    }

    public void ClearImpact ()
    {
        m_IsFree = false;
        m_AnimatorController.SetInt(s_Animation + "," + s_ClearImpact);
    }

    public void Happy ()
    {
        m_IsFree = false;
        m_AnimatorController.SetInt(s_Animation + "," + s_Happy);
    }


    private void Update()
    {
        if (!m_IsFree) return;

        m_TimeCount += Time.deltaTime;

        if (m_TimeCount > m_RandomAnimationTime)
        {
            PlayRandomAnimation();

            m_TimeCount -= m_RandomAnimationTime;
            m_RandomAnimationTime = Random.Range(1f, 5f);
        }
    }
}
