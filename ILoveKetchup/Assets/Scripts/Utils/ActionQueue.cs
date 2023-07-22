using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionQueue
{
    private Queue<Action<Action>> actionQueue;

    public ActionQueue()
    {
        actionQueue = new Queue<Action<Action>>();
    }

    public void AddAction(Action<Action> action)
    {
        actionQueue.Enqueue(action);
        if (actionQueue.Count == 1)//run first action
        {
            actionQueue.Peek()(() => RunNext());
        }
    }

    void RunNext()
    {
        if (actionQueue.Count <= 0) return;
        actionQueue.Dequeue(); //dequeue last one
        if (actionQueue.Count <= 0) return;
        actionQueue.Peek()(() => RunNext());
    }

    public void Reset()
    {
        actionQueue.Clear();
    }

}
