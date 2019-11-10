using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleMessage : MonoBehaviour
{
    Motion motion;

    public float Y { get => transform.position.y; set { (transform as RectTransform).localPosition = new Vector3(0, value, 0); } }
    public float Alpha { get => GetComponent<CanvasGroup>().alpha; set { GetComponent<CanvasGroup>().alpha = value; } }

    public void Play(float pause)
    {
        motion = new Motion(this)
            .AddTimeStamp(totalTime: pause)
            .AddTimeStamp(
                valuesStart: new { Y = -15f - pause * 10, Alpha = 1.5f },
                valuesEnd: new { Alpha = 0f },
                changeSpeed: new { Y = 150f, },
                changeAcceleration: new { Y = -57f, },
                totalTime: 1.2f
            );

        Alpha = 0;
        Y = -15f;
    }

    void Update()
    {
        if (motion != null)
        {
            motion.Update();
            if (motion.IsFinished)
                Destroy(gameObject);
        }
    }
}
