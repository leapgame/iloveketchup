using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Math = UnityEngine.Mathf;

public class Runner : MonoBehaviour
{
    private static Runner instance;
    private static List<RunInfo> runInfos = new List<RunInfo>();

    private static Runner GetInstance()
    {
        if (instance == null)
        {
            GameObject obj = new GameObject();
            obj.name = "Runner";
            DontDestroyOnLoad(obj);
            instance = obj.AddComponent<Runner>();
        }

        return instance;
    }

    public static void CreateInstanceIfNot()
    {
        GetInstance();
    }

    public static RunInfo Delay(float seconds, Action callback, bool skipAddToList = false)
    {
        RunInfo runInfo = new RunInfo(GetInstance())
            .SetType(RunInfoType.Delay)
            .SetDelay(seconds)
            .SetCallback(callback);

        if (skipAddToList)
        {
            runInfo.SkipAdd();
        }

        runInfo.Start();
        return runInfo;
    }

    public static RunInfo DelayOneFrame(Action callback)
    {
        RunInfo runInfo = new RunInfo(GetInstance())
            .SetType(RunInfoType.DelayOneFrame)
            .SetCallback(callback);
        runInfo.Start();
        return runInfo;
    }

    public static RunInfo WaitCondition(Func<bool> condition, Action callback)
    {
        RunInfo runInfo = new RunInfo(GetInstance())
            .SetType(RunInfoType.WaitCondition)
            .SetCondition(condition)
            .SetCallback(callback);
        runInfo.Start();
        return runInfo;
    }

    public static RunInfo Repeat(float interval, Action callback)
    {
        RunInfo runInfo = new RunInfo(GetInstance())
            .SetType(RunInfoType.Repeat)
            .SetInterval(interval)
            .SetCallback(callback);
        runInfo.Start();
        return runInfo;
    }

    public static RunInfo Update(Action callback)
    {
        RunInfo runInfo = new RunInfo(GetInstance())
            .SetType(RunInfoType.Update)
            .SetCallback(callback);
        runInfo.Start();
        return runInfo;
    }

    public static RunInfo FixedUpdate(Action callback)
    {
        RunInfo runInfo = new RunInfo(GetInstance())
            .SetType(RunInfoType.FixedUpdate)
            .SetCallback(callback);
        runInfo.Start();
        return runInfo;
    }

    public static RunInfo Lerp(float time, Easings.Functions easing, Action<float, bool> lerpCallback)
    {
        RunInfo runInfo = new RunInfo(GetInstance())
            .SetType(RunInfoType.Lerp)
            .SetLerpTime(time)
            .SetEasingFunction(easing)
            .SetLerpCallback(lerpCallback);
        runInfo.Start();
        return runInfo;
    }

    public static RunInfo Lerp(float time, Action<float, bool> lerpCallback)
    {
        return Lerp(time, Easings.Functions.Linear, lerpCallback);
    }

    public static void AddRunInfo(RunInfo runInfo)
    {
        if (!runInfo.IsSkipAdd())
        {
            runInfos.Add(runInfo);
        }
    }

    public static void StopAll()
    {
        foreach (RunInfo runInfo in runInfos)
        {
            runInfo.Stop();
        }

        runInfos.Clear();
    }
}

public enum RunInfoType
{
    Delay,
    DelayOneFrame,
    WaitCondition,
    Repeat,
    Update,
    Lerp,
    FixedUpdate
}

public class RunInfo
{
    private MonoBehaviour monoBehaviour;
    private RunInfoType type;
    private float delay;
    private float interval;
    private Action callback;
    private Coroutine coroutine;
    private Func<bool> condition;
    private Action<float, bool> lerpCallback;
    private float lerpTime;
    private Easings.Functions easingFunction;
    private bool skipAdd;

    public RunInfo(MonoBehaviour monoBehaviour)
    {
        this.monoBehaviour = monoBehaviour;
    }

    public RunInfo SetType(RunInfoType type)
    {
        this.type = type;
        return this;
    }

    public RunInfo SetDelay(float delay)
    {
        this.delay = delay;
        return this;
    }

    public RunInfo SetInterval(float interval)
    {
        this.interval = interval;
        return this;
    }

    public RunInfo SetCallback(Action callback)
    {
        this.callback = callback;
        return this;
    }

    public RunInfo SetCondition(Func<bool> condition)
    {
        this.condition = condition;
        return this;
    }

    public RunInfo SetLerpCallback(Action<float, bool> lerpCallback)
    {
        this.lerpCallback = lerpCallback;
        return this;
    }

    public RunInfo SetLerpTime(float time)
    {
        this.lerpTime = time;
        return this;
    }

    public RunInfo SetEasingFunction(Easings.Functions easingFunction)
    {
        this.easingFunction = easingFunction;
        return this;
    }

    public RunInfo SkipAdd()
    {
        skipAdd = true;
        return this;
    }

    public bool IsSkipAdd()
    {
        return skipAdd;
    }

    public void Start()
    {
        switch (type)
        {
            case RunInfoType.Delay:
                coroutine = monoBehaviour.StartCoroutine(DelayCo(delay, callback));
                break;
            case RunInfoType.DelayOneFrame:
                coroutine = monoBehaviour.StartCoroutine(DelayOneFrameCo(callback));
                break;
            case RunInfoType.WaitCondition:
                coroutine = monoBehaviour.StartCoroutine(WaitConditionCo(condition, callback));
                break;
            case RunInfoType.Repeat:
                coroutine = monoBehaviour.StartCoroutine(RepeatCo(interval, callback));
                break;
            case RunInfoType.Update:
                coroutine = monoBehaviour.StartCoroutine(UpdateCo(callback));
                break;
            case RunInfoType.FixedUpdate:
                coroutine = monoBehaviour.StartCoroutine(FixedUpdateCo(callback));
                break;
            case RunInfoType.Lerp:
                coroutine = monoBehaviour.StartCoroutine(LerpCo(lerpTime, easingFunction, lerpCallback));
                break;
        }

        Runner.AddRunInfo(this);
    }

    public void Stop()
    {
        if (monoBehaviour != null && coroutine != null)
        {
            monoBehaviour.StopCoroutine(coroutine);
            callback = null;
        }
    }

    private static IEnumerator DelayCo(float seconds, Action callback)
    {
        yield return new WaitForSeconds(seconds);
        if (callback != null) callback();
    }

    private static IEnumerator DelayOneFrameCo(Action callback)
    {
        yield return null;
        if (callback != null) callback();
    }

    private static IEnumerator WaitConditionCo(Func<bool> condition, Action callback)
    {
        while (!condition())
        {
            yield return null;
        }

        if (callback != null) callback();
    }

    private static IEnumerator RepeatCo(float interval, Action callback)
    {
        while (true)
        {
            if (callback != null) callback();
            yield return new WaitForSeconds(interval);
        }
    }

    private static IEnumerator UpdateCo(Action callback)
    {
        while (true)
        {
            if (callback != null) callback();
            yield return null;
        }
    }

    private static IEnumerator FixedUpdateCo(Action callback)
    {
        while (true)
        {
            if (callback != null) callback();
            yield return new WaitForFixedUpdate();
        }
    }

    private static IEnumerator LerpCo(float time, Easings.Functions easingFunction, Action<float, bool> lerpCallback)
    {
        var value = 0f;
        var speed = 1f / time;

        while (true)
        {
            if (value >= 1)
            {
                lerpCallback(Easings.Interpolate(1, easingFunction), true);
                break;
            }
            else
            {
                lerpCallback(Easings.Interpolate(value, easingFunction), false);
            }

            value += speed * Time.deltaTime;
            yield return null;
        }
    }
}

// https://github.com/acron0/Easings
static public class Easings
{
    /// <summary>
    /// Constant Pi.
    /// </summary>
    private const float PI = Math.PI;

    /// <summary>
    /// Constant Pi / 2.
    /// </summary>
    private const float HALFPI = Math.PI / 2.0f;

    /// <summary>
    /// Easing Functions enumeration
    /// </summary>
    public enum Functions
    {
        Linear,
        QuadIn,
        QuadOut,
        QuadInOut,
        CubicIn,
        CubicOut,
        CubicInOut,
        QuartIn,
        QuartOut,
        QuartInOut,
        QuintIn,
        QuintOut,
        QuintInOut,
        SineIn,
        SineOut,
        SineInOut,
        CircIn,
        CircOut,
        CircInOut,
        ExpoIn,
        ExpoOut,
        ExpoInOut,
        ElasticIn,
        ElasticOut,
        ElasticInOut,
        BackIn,
        BackOut,
        BackInOut,
        BounceIn,
        BounceOut,
        BounceInOut
    }

    /// <summary>
    /// Interpolate using the specified function.
    /// </summary>
    static public float Interpolate(float p, Functions function)
    {
        switch (function)
        {
            default:
            case Functions.Linear: return Linear(p);
            case Functions.QuadOut: return QuadraticEaseOut(p);
            case Functions.QuadIn: return QuadraticEaseIn(p);
            case Functions.QuadInOut: return QuadraticEaseInOut(p);
            case Functions.CubicIn: return CubicEaseIn(p);
            case Functions.CubicOut: return CubicEaseOut(p);
            case Functions.CubicInOut: return CubicEaseInOut(p);
            case Functions.QuartIn: return QuarticEaseIn(p);
            case Functions.QuartOut: return QuarticEaseOut(p);
            case Functions.QuartInOut: return QuarticEaseInOut(p);
            case Functions.QuintIn: return QuinticEaseIn(p);
            case Functions.QuintOut: return QuinticEaseOut(p);
            case Functions.QuintInOut: return QuinticEaseInOut(p);
            case Functions.SineIn: return SineEaseIn(p);
            case Functions.SineOut: return SineEaseOut(p);
            case Functions.SineInOut: return SineEaseInOut(p);
            case Functions.CircIn: return CircularEaseIn(p);
            case Functions.CircOut: return CircularEaseOut(p);
            case Functions.CircInOut: return CircularEaseInOut(p);
            case Functions.ExpoIn: return ExponentialEaseIn(p);
            case Functions.ExpoOut: return ExponentialEaseOut(p);
            case Functions.ExpoInOut: return ExponentialEaseInOut(p);
            case Functions.ElasticIn: return ElasticEaseIn(p);
            case Functions.ElasticOut: return ElasticEaseOut(p);
            case Functions.ElasticInOut: return ElasticEaseInOut(p);
            case Functions.BackIn: return BackEaseIn(p);
            case Functions.BackOut: return BackEaseOut(p);
            case Functions.BackInOut: return BackEaseInOut(p);
            case Functions.BounceIn: return BounceEaseIn(p);
            case Functions.BounceOut: return BounceEaseOut(p);
            case Functions.BounceInOut: return BounceEaseInOut(p);
        }
    }

    /// <summary>
    /// Modeled after the line y = x
    /// </summary>
    static public float Linear(float p)
    {
        return p;
    }

    /// <summary>
    /// Modeled after the parabola y = x^2
    /// </summary>
    static public float QuadraticEaseIn(float p)
    {
        return p * p;
    }

    /// <summary>
    /// Modeled after the parabola y = -x^2 + 2x
    /// </summary>
    static public float QuadraticEaseOut(float p)
    {
        return -(p * (p - 2));
    }

    /// <summary>
    /// Modeled after the piecewise quadratic
    /// y = (1/2)((2x)^2)             ; [0, 0.5)
    /// y = -(1/2)((2x-1)*(2x-3) - 1) ; [0.5, 1]
    /// </summary>
    static public float QuadraticEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 2 * p * p;
        }
        else
        {
            return (-2 * p * p) + (4 * p) - 1;
        }
    }

    /// <summary>
    /// Modeled after the cubic y = x^3
    /// </summary>
    static public float CubicEaseIn(float p)
    {
        return p * p * p;
    }

    /// <summary>
    /// Modeled after the cubic y = (x - 1)^3 + 1
    /// </summary>
    static public float CubicEaseOut(float p)
    {
        float f = (p - 1);
        return f * f * f + 1;
    }

    /// <summary>	
    /// Modeled after the piecewise cubic
    /// y = (1/2)((2x)^3)       ; [0, 0.5)
    /// y = (1/2)((2x-2)^3 + 2) ; [0.5, 1]
    /// </summary>
    static public float CubicEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 4 * p * p * p;
        }
        else
        {
            float f = ((2 * p) - 2);
            return 0.5f * f * f * f + 1;
        }
    }

    /// <summary>
    /// Modeled after the quartic x^4
    /// </summary>
    static public float QuarticEaseIn(float p)
    {
        return p * p * p * p;
    }

    /// <summary>
    /// Modeled after the quartic y = 1 - (x - 1)^4
    /// </summary>
    static public float QuarticEaseOut(float p)
    {
        float f = (p - 1);
        return f * f * f * (1 - p) + 1;
    }

    /// <summary>
    // Modeled after the piecewise quartic
    // y = (1/2)((2x)^4)        ; [0, 0.5)
    // y = -(1/2)((2x-2)^4 - 2) ; [0.5, 1]
    /// </summary>
    static public float QuarticEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 8 * p * p * p * p;
        }
        else
        {
            float f = (p - 1);
            return -8 * f * f * f * f + 1;
        }
    }

    /// <summary>
    /// Modeled after the quintic y = x^5
    /// </summary>
    static public float QuinticEaseIn(float p)
    {
        return p * p * p * p * p;
    }

    /// <summary>
    /// Modeled after the quintic y = (x - 1)^5 + 1
    /// </summary>
    static public float QuinticEaseOut(float p)
    {
        float f = (p - 1);
        return f * f * f * f * f + 1;
    }

    /// <summary>
    /// Modeled after the piecewise quintic
    /// y = (1/2)((2x)^5)       ; [0, 0.5)
    /// y = (1/2)((2x-2)^5 + 2) ; [0.5, 1]
    /// </summary>
    static public float QuinticEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 16 * p * p * p * p * p;
        }
        else
        {
            float f = ((2 * p) - 2);
            return 0.5f * f * f * f * f * f + 1;
        }
    }

    /// <summary>
    /// Modeled after quarter-cycle of sine wave
    /// </summary>
    static public float SineEaseIn(float p)
    {
        return Math.Sin((p - 1) * HALFPI) + 1;
    }

    /// <summary>
    /// Modeled after quarter-cycle of sine wave (different phase)
    /// </summary>
    static public float SineEaseOut(float p)
    {
        return Math.Sin(p * HALFPI);
    }

    /// <summary>
    /// Modeled after half sine wave
    /// </summary>
    static public float SineEaseInOut(float p)
    {
        return 0.5f * (1 - Math.Cos(p * PI));
    }

    /// <summary>
    /// Modeled after shifted quadrant IV of unit circle
    /// </summary>
    static public float CircularEaseIn(float p)
    {
        return 1 - Math.Sqrt(1 - (p * p));
    }

    /// <summary>
    /// Modeled after shifted quadrant II of unit circle
    /// </summary>
    static public float CircularEaseOut(float p)
    {
        return Math.Sqrt((2 - p) * p);
    }

    /// <summary>	
    /// Modeled after the piecewise circular function
    /// y = (1/2)(1 - Math.Sqrt(1 - 4x^2))           ; [0, 0.5)
    /// y = (1/2)(Math.Sqrt(-(2x - 3)*(2x - 1)) + 1) ; [0.5, 1]
    /// </summary>
    static public float CircularEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 0.5f * (1 - Math.Sqrt(1 - 4 * (p * p)));
        }
        else
        {
            return 0.5f * (Math.Sqrt(-((2 * p) - 3) * ((2 * p) - 1)) + 1);
        }
    }

    /// <summary>
    /// Modeled after the exponential function y = 2^(10(x - 1))
    /// </summary>
    static public float ExponentialEaseIn(float p)
    {
        return (p == 0.0f) ? p : Math.Pow(2, 10 * (p - 1));
    }

    /// <summary>
    /// Modeled after the exponential function y = -2^(-10x) + 1
    /// </summary>
    static public float ExponentialEaseOut(float p)
    {
        return (p == 1.0f) ? p : 1 - Math.Pow(2, -10 * p);
    }

    /// <summary>
    /// Modeled after the piecewise exponential
    /// y = (1/2)2^(10(2x - 1))         ; [0,0.5)
    /// y = -(1/2)*2^(-10(2x - 1))) + 1 ; [0.5,1]
    /// </summary>
    static public float ExponentialEaseInOut(float p)
    {
        if (p == 0.0 || p == 1.0) return p;

        if (p < 0.5f)
        {
            return 0.5f * Math.Pow(2, (20 * p) - 10);
        }
        else
        {
            return -0.5f * Math.Pow(2, (-20 * p) + 10) + 1;
        }
    }

    /// <summary>
    /// Modeled after the damped sine wave y = sin(13pi/2*x)*Math.Pow(2, 10 * (x - 1))
    /// </summary>
    static public float ElasticEaseIn(float p)
    {
        return Math.Sin(13 * HALFPI * p) * Math.Pow(2, 10 * (p - 1));
    }

    /// <summary>
    /// Modeled after the damped sine wave y = sin(-13pi/2*(x + 1))*Math.Pow(2, -10x) + 1
    /// </summary>
    static public float ElasticEaseOut(float p)
    {
        return Math.Sin(-13 * HALFPI * (p + 1)) * Math.Pow(2, -10 * p) + 1;
    }

    /// <summary>
    /// Modeled after the piecewise exponentially-damped sine wave:
    /// y = (1/2)*sin(13pi/2*(2*x))*Math.Pow(2, 10 * ((2*x) - 1))      ; [0,0.5)
    /// y = (1/2)*(sin(-13pi/2*((2x-1)+1))*Math.Pow(2,-10(2*x-1)) + 2) ; [0.5, 1]
    /// </summary>
    static public float ElasticEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 0.5f * Math.Sin(13 * HALFPI * (2 * p)) * Math.Pow(2, 10 * ((2 * p) - 1));
        }
        else
        {
            return 0.5f * (Math.Sin(-13 * HALFPI * ((2 * p - 1) + 1)) * Math.Pow(2, -10 * (2 * p - 1)) + 2);
        }
    }

    /// <summary>
    /// Modeled after the overshooting cubic y = x^3-x*sin(x*pi)
    /// </summary>
    static public float BackEaseIn(float p)
    {
        return p * p * p - p * Math.Sin(p * PI);
    }

    /// <summary>
    /// Modeled after overshooting cubic y = 1-((1-x)^3-(1-x)*sin((1-x)*pi))
    /// </summary>	
    static public float BackEaseOut(float p)
    {
        float f = (1 - p);
        return 1 - (f * f * f - f * Math.Sin(f * PI));
    }

    /// <summary>
    /// Modeled after the piecewise overshooting cubic function:
    /// y = (1/2)*((2x)^3-(2x)*sin(2*x*pi))           ; [0, 0.5)
    /// y = (1/2)*(1-((1-x)^3-(1-x)*sin((1-x)*pi))+1) ; [0.5, 1]
    /// </summary>
    static public float BackEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            float f = 2 * p;
            return 0.5f * (f * f * f - f * Math.Sin(f * PI));
        }
        else
        {
            float f = (1 - (2 * p - 1));
            return 0.5f * (1 - (f * f * f - f * Math.Sin(f * PI))) + 0.5f;
        }
    }

    /// <summary>
    /// </summary>
    static public float BounceEaseIn(float p)
    {
        return 1 - BounceEaseOut(1 - p);
    }

    /// <summary>
    /// </summary>
    static public float BounceEaseOut(float p)
    {
        if (p < 4 / 11.0f)
        {
            return (121 * p * p) / 16.0f;
        }
        else if (p < 8 / 11.0f)
        {
            return (363 / 40.0f * p * p) - (99 / 10.0f * p) + 17 / 5.0f;
        }
        else if (p < 9 / 10.0f)
        {
            return (4356 / 361.0f * p * p) - (35442 / 1805.0f * p) + 16061 / 1805.0f;
        }
        else
        {
            return (54 / 5.0f * p * p) - (513 / 25.0f * p) + 268 / 25.0f;
        }
    }

    /// <summary>
    /// </summary>
    static public float BounceEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 0.5f * BounceEaseIn(p * 2);
        }
        else
        {
            return 0.5f * BounceEaseOut(p * 2 - 1) + 0.5f;
        }
    }
}