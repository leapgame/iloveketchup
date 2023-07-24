using System;
using System.Collections.Generic;
using Game.Input;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PowerHandler : MonoBehaviour
{
 
    [SerializeField] private InputActions input;
    [SerializeField] private Transform transMovingAnchor;
    [SerializeField] private Image sprtRangeScore;
    [SerializeField] private List<Image> stepSprite;
    [SerializeField] private Transform transTargetAnchor;
    [SerializeField] private AnimationCurve curveSpeedDifficulty;
    [SerializeField] private List<Color> comboColors;

    public Action OnPowerDone;
    
    private const float MAX_SPEED = 180.0f;
    private const float MAX_RANGE_SCORE = 20.0f;
    private const float MIN_RANGE_SCORE = 5.0f;
    private const int MAX_COMBO = 5;
    
    private float speed = 10.0f;
    private int power;
    
    private int maxStep = 5;
    private int step = 0;
    private int combo = 0;

    private float rangeScoreOffset = 20.0f; 
    private float currentRotation = 0.0f;
    private float scoreRotation = 0.0f;

    private int[] stepData;
    
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
        this.stepData = new int[maxStep];
        
        GenerateNewStep();
        UIUpdateCurrentRotation();
        UIUpdateCurrentSteps();
        UIUpdateNewTargetScore();
    }

    public void StartPowerWheel()
    {
        
    }

    private void FixedUpdate()
    {
        if (GameController.instance.state != GameController.State.POWER) return;
        UpdatePowerRotation();
    }

    private void UpdatePowerRotation()
    {
        this.currentRotation = speed * Time.fixedDeltaTime;
    }

    private void OnTouch(Vector2 touchPos)
    {
        if (GameController.instance.state != GameController.State.POWER) return;
        //check if current rotation is in range 
        bool isScored = IsInScoreRange(this.currentRotation);
        try
        {
            this.stepData[this.step++] = isScored ? ++combo : 0;
        }
        catch (Exception e)
        {
            Development.LogError("Power touch error " + e);
        }

        if (this.step > this.maxStep)
        {
            OnPowerDone?.Invoke();
            return;
        }
        
        GenerateNewStep();
        UIUpdateNewTargetScore();
    }

    private void GenerateNewStep()
    {
        this.scoreRotation += Random.Range(180.0f + this.rangeScoreOffset * 2.0f, 360.0f - this.rangeScoreOffset * 2.0f);
        float curveValue = this.curveSpeedDifficulty.Evaluate((this.combo / MAX_COMBO));
        this.rangeScoreOffset = (MAX_RANGE_SCORE - MIN_RANGE_SCORE) * curveValue;
        this.speed = MAX_SPEED * curveValue;
    }

    private bool IsInScoreRange(float checkRotation)
    {
        return checkRotation >= scoreRotation - this.rangeScoreOffset &&
               checkRotation <= this.rangeScoreOffset + scoreRotation;
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
    #endregion
    
    #region visualize

    private void UIUpdateCurrentRotation()
    {
        this.transMovingAnchor.eulerAngles = new Vector3(0.0f, 0.0f, this.currentRotation);
    }

    private void UIUpdateNewTargetScore()
    {
        this.transTargetAnchor.eulerAngles = new Vector3(0.0f, 0.0f, this.scoreRotation);
        this.sprtRangeScore.fillAmount = this.rangeScoreOffset * 2.0f / 360.0f;
        this.sprtRangeScore.color = this.GetComboColor(this.combo);
    }

    private void UIUpdateCurrentSteps()
    {
        for (int i = 0; i < this.stepSprite.Count; i++)
        {
            try
            {
                this.stepSprite[i].color = GetComboColor(this.stepData[i]);
            }
            catch
            {
                Development.LogError("UIUpdateCurrentSteps WRONG " + i);
            }
        }
    }
    
    #endregion
}
