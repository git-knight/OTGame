using System;
using UnityEngine;
using UnityEngine.UI;

class ValueAnimator : MonoBehaviour
{
    Motion currAnim;

    bool isInit;
    float targetValue;

    public float ChangeTime = 0.5f;

    public int TargetValue { set { SetValue(value); } }
    public float TargetValueF { set { SetValue(value); } }

    public float CurrentValue { get; set; }

    void Start()
    {
    }

    void Update()
    {
        if (currAnim != null)
        {
            var tStamp = currAnim.Update();
            CurrentValue = tStamp["val"];

            if (currAnim.IsFinished)
                currAnim = null;
        }
    }

    public void SetValue(float val)
    {
        if (!isInit)
        {
            isInit = true;
            targetValue = val;
            CurrentValue = val;
            //text.text = TextTemplate.Replace("{val}", targetValue + "");
        }

        /*if (val == targetValue)
        {
            currAnim = null;
            return;
        }*/

        if (val != targetValue)
        {
            currAnim = new Motion(this).AddTimeStamp(
                valuesStart: new { val = targetValue },
                valuesEnd: new { val },
                totalTime: ChangeTime);
            targetValue = val;
        }
    }
}