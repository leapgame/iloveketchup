using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimpleMonoPool <T> where T : MonoBehaviour
{
    private GameObject prefab;
    private Transform container;

    private List<T> Ts = new List<T>();
    private Queue<T> cachedTs = new Queue<T>();

    public SimpleMonoPool(GameObject prefab, Transform container)
    {
        this.prefab = prefab;
        this.container = container;
    }

    public List<T> GetObjectList()
    {
        return Ts;
    }

    public void PrepareObjects(int numberOfObjects)
    {
        Ts = new List<T>(numberOfObjects);
        cachedTs = new Queue<T>(numberOfObjects);

        for (int i = 0; i < numberOfObjects; i++)
        {
            PushToPool(CreateNew());
        }
    }

    public void RemoveAllObjects()
    {
        for (int i = Ts.Count - 1; i >= 0; i--)
        {
            PushToPool(Ts[i]);
        }
    }

    public void PushToPool(T obj)
    {
        if (Ts.Contains(obj))
        {
            Ts.Remove(obj);
        }

        obj.transform.SetParent(container);
        obj.gameObject.SetActive(false);
        cachedTs.Enqueue(obj);
    }

    public T PopFromPool()
    {
        T retVal = null;  
        if (cachedTs.Count > 0)
        {
            retVal = cachedTs.Dequeue();
        }
        else
        {
            retVal = CreateNew();
        }
        retVal.gameObject.SetActive(true);
        Ts.Add(retVal);

        return retVal;
    }

    T CreateNew()
    {
        if (prefab == null || container == null) return null;
        GameObject obj = MonoBehaviour.Instantiate(prefab, container);
        return obj.GetComponent<T>();
    }
}
