using Doozy.Runtime.Common;
using Doozy.Runtime.Nody;
using UnityEngine;

public partial class GameController : MonoBehaviour
{
    private const string NODE_READY = "Ready";
    private const string NODE_POWER = "Power";
    private const string NODE_HITTING = "Hitting";
    private const string NODE_GAMEOVER = "Gameover";
    
    [Header("UI")] 
    [SerializeField] private FlowController flowUI;

    private void UISetPowerState()
    {
        flowUI.SetActiveNodeByName("Power");
    }
    
    #region callbacks

    public void OnTouch_ReadyConfirm()
    {
        OnKetchupReady();
    }

    #endregion
}
