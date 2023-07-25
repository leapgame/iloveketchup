using System;
using System.Collections;
using DG.Tweening;
using Doozy.Runtime.Common;
using UnityEngine;

public partial class GameController : MonoBehaviour
{
    public enum State
    {
        INIT = 0, 
        CREATING_OBJECT,
        KETCHUP,
        POWER,
        THROWING,
        HITTING,
        GAMEOVER,
    }

    public enum EndingType
    {
        LOSE = 0, 
        WIN,
        CRITICAL_WIN,
    }

    #region const anim state name
    private const string ANIM_CAM_STATE_KEPCHUP = "ketchup";
    private const string ANIM_CAM_STATE_POWER = "power";
    private const string ANIM_CAM_STATE_HIT = "hit";
    private const string ANIM_CAM_STATE_FAILED = "failed";
    #endregion
    
    [SerializeField] private Animator animCam;
    [SerializeField] private KetchupHandler ketchup;
    [SerializeField] private PowerHandler power;
    [SerializeField] private TargetHandler target;
    [SerializeField] private FoodHandler food;
    [SerializeField] private GameObject foodContainer;

    //TODO: test, temporary assign
    [SerializeField] private Throwable m_Throwable;

    public State state { get; private set; } = State.INIT;
    private int requireScore = 100;
    private int currentScore = 0;
    private EndingType endingType = EndingType.LOSE;
    
    #region singleton

    public static GameController instance;
    private void Awake()
    {
        instance = this;
    }

    #endregion
    
    #region Init games
    
    void Start()
    {
#if !UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif
        Init();
    }

    private void OnEnable()
    {
        Throwable.OnThrowableHitTarget += OnThrowDone;
    }

    private void OnDisable()
    {
        Throwable.OnThrowableHitTarget -= OnThrowDone;
    }

    private void Init()
    {
        StartCoroutine(C_ProcessLogic());
    }

    private IEnumerator C_ProcessLogic()
    {
        yield return C_InitData();
        yield return C_SpawnObjects();
        this.state = State.KETCHUP;
        this.animCam.SetTrigger(ANIM_CAM_STATE_KEPCHUP);
    }
    
    #region child steps

    private IEnumerator C_InitData()
    {
        yield break;
    }
    
    private IEnumerator C_SpawnObjects()
    {
        this.ketchup.SetTargetKetchup(this.foodContainer.transform);
        yield break;
    }
    
    #endregion

    #endregion

    #region logics
    
    private void OnKetchupReady()
    {
        Development.Log("OnKetchupReady");
        //hide current UI node
        ketchup.SetBottleVisible(false);
        DoPower();
    }

    private void DoPower()
    {
        Development.Log("DoPower");
        UISetPowerState();
        this.animCam.SetTrigger(ANIM_CAM_STATE_POWER);
        this.ketchup.SetTargetThrowing(target.GetAttachableKetchup().transform);
        this.target.Idle();

        //goes to next step
        power.InitPower();
        
        this.state = State.POWER;
        power.StartPowerWheel();
        power.OnPowerDone = score =>
        {
            //check score here 
            this.currentScore = score;
            //the goes to next step 
            OnPowerReady();
        };
    }
    private void OnPowerReady()
    {
        Development.Log("OnPowerReady");
        UISetHittingState();
        //goes to next step
        this.state = State.HITTING;

        bool isWinning = this.currentScore >= this.requireScore;
        float ratio = (float)this.currentScore / (float)this.requireScore;

        Vector3 targetPosition = TargetHandler.Instance.TargetPosition();
        float power = 5.0f;
        if (isWinning)
        {
            this.endingType = EndingType.WIN;
            if (ratio >= 1.5f)
            {
                power = 15.0f;
                this.endingType = EndingType.CRITICAL_WIN;
            }
        }
        else
        {
            this.endingType = EndingType.LOSE;
            targetPosition += Vector3.down * (1.0f - ratio) * 20.0f;
            targetPosition.x = -10.0f; // out of camera
        }

        //power should be in range (5, 15) to forms nice curves
        m_Throwable.Throw(targetPosition, power);
        
    }

    private void OnThrowDone(Throwable justDoneObj)
    {
        UISetGameoverState();
        if (endingType == EndingType.LOSE)
        {
            this.animCam.SetTrigger(ANIM_CAM_STATE_FAILED);
            this.target.Happy();
            return;
        }
        
        //if win
        Transform attachBone = target.GetAttachableKetchup().transform;
        this.ketchup.SetTargetParticleVisible(true);
        this.ketchup.ClearCakeFluid();
        this.food.DoEffectExplode(attachBone);

        if (endingType == EndingType.CRITICAL_WIN)
        {
            this.target.ClearImpact();
        }
        else
        {
            this.target.Impact();
        }

        DOVirtual.DelayedCall(1.0f, () =>
        {
            this.animCam.SetTrigger(ANIM_CAM_STATE_HIT);
        });
    }
    
    #region power 
    
    #endregion
    
    #endregion
}