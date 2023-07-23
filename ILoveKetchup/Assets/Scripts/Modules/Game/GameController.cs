using UnityEngine;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if !UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif
    }
}