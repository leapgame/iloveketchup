using System;
using System.Collections;
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
    
    [SerializeField] private KetchupHandler ketchup;
    [SerializeField] private PowerHandler power;
    [SerializeField] private GameObject foodContainer;

    //TODO: test, temporary assign
    [SerializeField] private Throwable m_Throwable;

    public State state { get; private set; } = State.INIT;
    
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

    private void Init()
    {
        StartCoroutine(C_ProcessLogic());
    }

    private IEnumerator C_ProcessLogic()
    {
        yield return C_InitData();
        yield return C_SpawnObjects();
        this.state = State.KETCHUP;

        //power should be in range (5, 15) to forms nice curves
        // m_Throwable.Throw(TargetHandler.Instance.TargetPosition(), 5f);
    }
    
    #region child steps

    private IEnumerator C_InitData()
    {
        yield break;
    }
    
    private IEnumerator C_SpawnObjects()
    {
        yield break;
    }
    
    #endregion

    #endregion

    #region logics
    
    private void OnKetchupReady()
    {
        //hide current UI node
        ketchup.SetBottleVisible(false);
        DoPower();
    }

    private void DoPower()
    {
        UISetPowerState();

        //goes to next step
        power.InitPower();
        
        this.state = State.POWER;
        power.StartPowerWheel();
        power.OnPowerDone = score =>
        {
            //check score here 

            //the goes to next step 
            OnPowerReady();
        };
    }
    private void OnPowerReady()
    {
        UISetHittingState();
        //goes to next step
        this.state = State.HITTING;

        //start anim fly to target
        //power should be in range (5, 15) to forms nice curves
        m_Throwable.Throw(TargetHandler.Instance.TargetPosition(), 5f);
    }
    
    #region power 
    
    #endregion
    
    #endregion
}