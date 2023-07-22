using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyMonoSingleon<T> : MonoBehaviour
{
    public static T Instance
    {
        get
        {
            return instance;
        }
    }

    private static T instance;

    private void Awake()
    {
        if (instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            instance = GetComponent<T>();
            OnAwake();
            DontDestroyOnLoad(gameObject);
        }
    }

    protected virtual void OnAwake()
    {

    }
}
