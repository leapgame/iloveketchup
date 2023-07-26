using UnityEngine;

public class DebugMenu : MonoBehaviour
{
    [SerializeField] private KetchupHandler ketchup;
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
    
    #endregion
}
