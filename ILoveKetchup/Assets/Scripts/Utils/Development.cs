using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Development
{
    public static bool IsHaveLog()
    {
#if HAVE_LOG || UNITY_EDITOR
        return true;
#endif
        return false;
    }
    public static void Log(object message)
    {
        if (IsHaveLog())
        {
            Debug.Log(message);
        }
    }
    
    public static void LogFormat(string format, params object[] args)
    {
        if (IsHaveLog())
        {
            Debug.unityLogger.LogFormat(LogType.Log, format, args);
        }
    }
    
    public static void LogError(object message)
    {
        // if (IsHaveLog())
        {
            Debug.LogError(message);
        }
    }
}
