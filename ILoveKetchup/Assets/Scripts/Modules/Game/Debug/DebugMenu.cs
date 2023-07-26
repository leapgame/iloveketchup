using System.Collections.Generic;
using Obi;
using UnityEngine;

public class DebugMenu : MonoBehaviour
{
    [SerializeField] private KetchupHandler ketchup;
    [SerializeField] private List<ObiSolver> solvers;
    [SerializeField] private GameObject objPostprocessing;
    public void OnColorChanged(Color col)
    {
        ketchup.SetKetchupColor(col);
    }

    public void OnSizeChanged(float size)
    {
        ketchup.ParticleSize = size;
    }
    
    #region callbacks

    public void OnSlider_Size(float ratio)
    {
        float finalSize = 1.0f + ratio * 2.0f;
        OnSizeChanged(finalSize);
    }

    public void OnToggle_Fluid(bool isOn)
    {
        solvers.ForEach(sol => sol.gameObject.SetActive(isOn));
    }
    
    public void OnToggle_Brust(bool isOn)
    {
        solvers.ForEach(sol => sol.backendType = isOn ? ObiSolver.BackendType.Burst : ObiSolver.BackendType.Oni);
    }

    
    public void OnToggle_PP(bool isOn)
    {
        objPostprocessing.SetActive(isOn);
    }

    public void OnTouch_Low()
    {
        QualitySettings.SetQualityLevel(0);   
    }
    
    
    public void OnTouch_Normal()
    {
        QualitySettings.SetQualityLevel(1);   
    }
    
    #endregion
}
