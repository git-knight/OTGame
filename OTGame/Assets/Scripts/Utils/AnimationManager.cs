using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IMotion
{
    bool IsFinished { get; }
    void Update(float time);
}

public class TimeStamp
{
    public object Object { get; }

    public float? totalTime;
    public float timeAlreadyPassed;

    public string[] props;
    public Dictionary<string, float> valuesStart = new Dictionary<string, float>();
    public Dictionary<string, float?> valuesEnd = new Dictionary<string, float?>();
    public Dictionary<string, float> changeSpeed = new Dictionary<string, float>();
    public Dictionary<string, float> changeAcceleration = new Dictionary<string, float>();

    public Dictionary<string, float> currentValues = new Dictionary<string, float>();

    public float this[string name] { get { return GetCurrentValue(name); } }

    public TimeStamp(object obj)
    {
        Object = obj;
    }

    public bool IsValueFinal(string name, float val)
    {
        if (totalTime != null)
            return (timeAlreadyPassed >= totalTime);

        if (valuesEnd[name] != null)
        {
            var end = valuesEnd[name].Value;
            return Mathf.Approximately(val, end) || (val > end) != (valuesStart[name] > end);
        }

        return false;
    }

    public float GetCurrentValue(string name)
    {
        var val = valuesStart[name] + (changeSpeed[name] + changeAcceleration[name] * timeAlreadyPassed) * timeAlreadyPassed;
        return IsValueFinal(name, val) ? (valuesEnd[name] ?? val) : val;
    }
}

public class Motion
{
    object obj;

    List<TimeStamp> stamps = new List<TimeStamp>();
    int currentId = 0;

    TimeStamp CurrentStamp { get { return stamps[currentId]; } }

    public bool IsFinished { get; private set; }

    public Motion(object obj)
    {
        this.obj = obj;
    }

    private float? TryGetValue(object obj, string propName)
    {
        if (obj == null)
            return null;

        var prop = obj.GetType().GetProperty(propName);
        if (prop != null)
            return Convert.ToSingle(prop.GetValue(obj));

        return null;
    }

    private void TrySetValue(string propName, float value)
    {
        var prop = obj.GetType().GetProperty(propName);
        if (prop != null)
            prop.SetValue(obj, value);
    }

    public Motion AddTimeStamp(
        float? totalTime = null,
        object valuesStart = null,
        object valuesEnd = null,
        object changeSpeed = null,
        object changeAcceleration = null)
    {
        if (totalTime.HasValue && totalTime <= 0)
            return this;

        var stamp = new TimeStamp(obj);
        var prevStamp = stamps.LastOrDefault();

        stamp.totalTime = totalTime;

        stamp.props = new object[] { valuesStart, valuesEnd, changeSpeed, changeAcceleration }
            .Where(o => o != null)
            .SelectMany(o => o.GetType().GetProperties().Select(p => p.Name))
            .Distinct()
            .ToArray();

        foreach (var prop in stamp.props)
        {
            stamp.valuesEnd.Add(prop, TryGetValue(valuesEnd, prop) ?? null);
            stamp.valuesStart.Add(prop, TryGetValue(valuesStart, prop) ?? ((prevStamp?.valuesEnd.ContainsKey(prop) ?? false) ? prevStamp?.valuesEnd[prop] : null) ?? TryGetValue(obj, prop).Value);
            stamp.changeSpeed.Add(prop, TryGetValue(changeSpeed, prop) ?? ((stamp.valuesEnd[prop] - stamp.valuesStart[prop]) / stamp.totalTime) ?? 0);
            stamp.changeAcceleration.Add(prop, TryGetValue(changeAcceleration, prop) ?? 0);

            stamp.currentValues.Add(prop, stamp.valuesStart[prop]);
        }

        stamps.Add(stamp);
        if (stamps.Count == 1 && valuesStart != null)
            foreach(var prop in valuesStart.GetType().GetProperties())
                TrySetValue(prop.Name, Convert.ToSingle(prop.GetValue(valuesStart)));

        return this;
    }

    public TimeStamp Update()
    {
        float deltaTime = Time.deltaTime;

        while (deltaTime > 0 && !IsFinished) 
        {
            CurrentStamp.timeAlreadyPassed += deltaTime;

            IsFinished = CurrentStamp.totalTime == null || CurrentStamp.timeAlreadyPassed >= CurrentStamp.totalTime;
            foreach (var prop in CurrentStamp.props)
            {
                CurrentStamp.currentValues[prop] = CurrentStamp.GetCurrentValue(prop);
                TrySetValue(prop, CurrentStamp.currentValues[prop]);
                IsFinished = IsFinished && CurrentStamp.IsValueFinal(prop, CurrentStamp.currentValues[prop]);
            }

            if (IsFinished)
            {
                if (currentId != stamps.Count - 1)
                {
                    if (CurrentStamp.totalTime.HasValue)
                        deltaTime -= (CurrentStamp.totalTime.Value - (CurrentStamp.timeAlreadyPassed - deltaTime));

                    IsFinished = false;
                    currentId++;
                }
            }
            else deltaTime = 0;
        }

        return CurrentStamp;
    }

    public IEnumerator<TimeStamp> Play(Action onFinished = null)
    {
        while (!IsFinished)
            yield return Update();

        onFinished?.Invoke();
    }
}

public class MotionPack
{
    List<Motion> motions;
    Action callback;

    public MotionPack(params Motion[] _motions)
    {
        motions = _motions.ToList();
    }

    public IEnumerable YieldUpdate()
    {
        while (motions.Count > 0)
        {
            yield return motions.Select(m => m.Update()).ToArray();
            motions.RemoveAll(m => m.IsFinished);
        }

        callback?.Invoke();
    }

    public MotionPack OnFinished(Action callback)
    {
        this.callback = callback;
        return this;
    }

    internal IEnumerator Play()
    {
        foreach(var tick in YieldUpdate())
        {
            yield return tick;
        }
    }
}

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance { get; private set; }

    public List<IMotion> Motions { get; private set; }

    void Awake()
    {
        if (Instance != null)
            Destroy(Instance);

        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
