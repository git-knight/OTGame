using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    float current;
    float max;

    public RawImage Bar { get; set; }
    public float MaxWidth { get; set; }

    public float Current
    {
        set
        {
            current = Mathf.Min(Max, value);
            Bar.uvRect = new Rect(0, 0, value / Max, 1);
            var rect = (Bar.gameObject.transform as RectTransform);
            rect.sizeDelta = new Vector2(MaxWidth * (value / Max), rect.sizeDelta.y);
        }
    }
    public float Max { get { return max; } set { max = value; Current = current; } }

    public void Initialize(int curr, int max)
    {
        Bar = gameObject.GetComponent<RawImage>();
        MaxWidth = (Bar.gameObject.transform as RectTransform).sizeDelta.x;

        current = curr;
        Max = max;
    }
}
