using System;

public static class ActionUtils
{
    public static void Execute(this Action action)
    {
        action?.Invoke();
    }

    public static void Execute<T>(this Action<T> action, T data)
    {
        action?.Invoke(data);
    }

    public static void ExecuteOnce(ref Action action)
    {
        var temp = action;
        action = null;
        temp.Execute();
    }

    public static void ExecuteOnce<T>(ref Action<T> action, T data)
    {
        var temp = action;
        action = null;
        temp.Execute(data);
    }
}
