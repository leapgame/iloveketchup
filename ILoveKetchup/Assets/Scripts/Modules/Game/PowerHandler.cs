using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Doozy.Runtime.UIManager.Containers;
using Game.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class PowerHandler : MonoBehaviour
{
    [SerializeField] private InputActions input;
    [SerializeField] private List<Image> stepSprite;
    [SerializeField] private TMP_Text lbScore;
    
    [SerializeField] private PowerTarget target;
    [SerializeField] private PowerTarget targetEffect;
    [SerializeField] private PowerMoving movingMain;
    [SerializeField] private PowerMoving movingEffect;

    [SerializeField] private AnimationCurve curveSpeedDifficulty;
    [SerializeField] private List<Color> comboColors;
    [SerializeField] private List<Sprite> comboSprites;

    [Header("Counting")] 
    [SerializeField] private UIContainer containerCountingPanel;
    [SerializeField] private TMP_Text lbCounting;
    
    public Action<int> OnPowerDone;
    
    private const float ADD_MAX_SPEED = 180.0f;
    private const float MIN_SPEED = 180.0f;
    private const float MAX_ADD_RANGE_SCORE = 20.0f;
    private const float MIN_RANGE_SCORE = 5.0f;
    private const int MAX_COMBO = 5;

    private int score;
    private float speed = 10.0f;
    private int power;
    
    private int maxStep = 5;
    private int step = 0;
    private int combo = 0;

    private float rangeScoreOffset = 20.0f; 
    private float currentRotation = 0.0f;
    private float scoreRotation = 0.0f;

    private int[] stepData;
    private bool isTouchable = false;
    
    #region logics
    private void OnEnable()
    {
        input.OnSelectDown += OnTouch;
    }

    private void OnDisable()
    {
        input.OnSelectDown -= OnTouch;
    }

    public void InitPower()
    {
        this.step = 0;
        this.currentRotation = 0.0f;
        this.combo = 0;
        this.isTouchable = false;
        this.score = 0;
        
        this.stepData = new int[maxStep];
        for (int i = 0; i < this.stepData.Length; i++)
        {
            this.stepData[i] = -1; //set no step yet
        }
        GenerateNewStep();
        UIUpdateCurrentRotation();
        UIUpdateCurrentSteps();
        UIUpdateNewTargetScore();
    }

    public void StartPowerWheel()
    {
        this.isTouchable = false;
        StartCoroutine(C_StartCountingDown(3, () => isTouchable = true));
    }

    private WaitForSeconds routineOneSec = new WaitForSeconds(1.0f);
    private IEnumerator C_StartCountingDown(int secs, Action onDone = null)
    {
        Transform transAnim = this.lbCounting.transform;
        void DoAnim()
        {
            transAnim.localEulerAngles = new Vector3(0.0f, 0.0f, Random.Range(-55.0f, 55.0f));
            transAnim.localScale = Vector3.one * 2.0f;
            
            transAnim.DOKill();
            transAnim.DOScale(Vector3.one, 0.25f);
            transAnim.DOLocalRotate(Vector3.zero, 0.25f);
        }
        int curSec = secs;
        this.lbCounting.text = curSec + "";
        DoAnim();
        while (--curSec > 0)
        {
            yield return routineOneSec;
            this.lbCounting.text = curSec + "";
            DoAnim();
        }
        
        yield return routineOneSec;
        this.containerCountingPanel.Hide();
        onDone?.Invoke();
    }

    private void FixedUpdate()
    {
        if (GameController.instance.state != GameController.State.POWER) return;
        if (isTouchable == false) return;
        UpdatePowerRotation();
    }

    private void UpdatePowerRotation()
    {
        this.currentRotation += speed * Time.fixedDeltaTime;
        UIUpdateCurrentRotation();
    }

    private void OnTouch(Vector2 touchPos)
    {
        if (GameController.instance.state != GameController.State.POWER) return;
        if (isTouchable == false) return;
        //check if current rotation is in range 
        DOEffect_MovingBar(this.currentRotation);
        bool isScored = IsInScoreRange(this.currentRotation);
        Development.Log("OnTouch: isScored " + isScored + " current rotation " + this.currentRotation);
        try
        {
            combo = Math.Clamp(isScored ? combo + 1 : 0, 0, MAX_COMBO);
            this.stepData[this.step] = combo;
        }
        catch (Exception e)
        {
            Development.LogError("Power touch error " + e);
        }
        
        if (isScored)
        {
            //add score
            this.score += this.GetScoreOnCurrentState();
            
            DoEffect_ScoreBar();
            UIUpdateScore();
        }

        GenerateNewStep();
        UIUpdateNewTargetScore();
        UIUpdateCurrentSteps();
        
        if (++this.step >= this.maxStep)
        {
            isTouchable = false;
            DOVirtual.DelayedCall(2.0f, () =>
            {
                OnPowerDone?.Invoke(this.score);
            });
        }
    }

    private void DOEffect_MovingBar(float rotation)
    {
        this.movingEffect.gameObject.SetActive(true);
        this.movingEffect.SetData(rotation);
        this.movingEffect.DoEffectLastTouch();
    }

    private void DoEffect_ScoreBar()
    {
        targetEffect.SetData(this.scoreRotation, this.rangeScoreOffset, GetComboColor(this.combo + 1)); //+1 for next combo
        targetEffect.DoEffectScored();
    }

    private int GetScoreOnCurrentState()
    {
        if (combo <= 0) return 0;
        return (int)(GameConstant.BASE_POWER_SCORE * combo);
    }

    private void GenerateNewStep()
    {
        this.scoreRotation += Random.Range(180.0f + this.rangeScoreOffset * 2.0f, 360.0f - this.rangeScoreOffset * 2.0f);
        float evaluateValue = (float)this.combo / (float)MAX_COMBO;
        float curveValue = this.curveSpeedDifficulty.Evaluate(evaluateValue);
        this.rangeScoreOffset = MIN_RANGE_SCORE + MAX_ADD_RANGE_SCORE * ( 1.0f - curveValue);
        this.speed = MIN_SPEED + ADD_MAX_SPEED * curveValue;
        
        Development.Log("GenerateNewStep level value: " + curveValue + " range score: " + rangeScoreOffset + " speed: " + this.speed + " combo: " + this.combo);
    }

    private bool IsInScoreRange(float checkRotation)
    {
        float finalCheck = checkRotation % 360.0f;
        float finalCheckScore = scoreRotation % 360.0f;
        return finalCheck >= finalCheckScore - this.rangeScoreOffset &&
               finalCheck <= this.rangeScoreOffset + finalCheckScore;
    }

    private Color GetComboColor(int combo)
    {
        try
        {
            return this.comboColors[combo];
        }
        catch
        {
            return Color.white;
        }
    }
    
    private Sprite GetComboSprite(int combo)
    {
        try
        {
            return this.comboSprites[combo];
        }
        catch
        {
            return null;
        }
    }
    #endregion
    
    #region visualize

    private void UIUpdateScore()
    {
        this.lbScore.text = score + "";
        //anim
        this.lbScore.color = this.GetComboColor(this.combo);
        this.lbScore.transform.localScale = Vector3.one * 1.5f;

        float animTime = 0.35f;
        this.lbScore.transform.DOKill();
        this.lbScore.transform.DOScale(Vector3.one, animTime).SetEase(Ease.InBack);

        this.lbScore.DOColor(Color.white, animTime);
    }

    private void UIUpdateCurrentRotation()
    {
        this.movingMain.SetData(this.currentRotation);
    }

    private void UIUpdateNewTargetScore()
    {
        target.SetData(this.scoreRotation, this.rangeScoreOffset, GetComboColor(this.combo + 1)); //+1 for next combo
    }

    private void UIUpdateCurrentSteps()
    {
        for (int i = 0; i < this.stepData.Length; i++)
        {
            try
            {
                bool isActive = this.stepData[i] >= 0;
                this.stepSprite[i].gameObject.SetActive(isActive);
                if (isActive)
                {
                    this.stepSprite[i].color = this.stepData[i] == 0 ? new Color(0.0f, 0.0f, 0.0f, 0.5f) : Color.white;
                    this.stepSprite[i].sprite = GetComboSprite(this.stepData[i]);
                }
            }
            catch
            {
                Development.LogError("UIUpdateCurrentSteps WRONG " + i);
            }
        }
    }
    
    #endregion
}
