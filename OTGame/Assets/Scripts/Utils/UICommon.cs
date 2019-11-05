using System;
using UnityEngine;
using UnityEngine.UI;

class UICommon : MonoBehaviour
{
    static UICommon instance;

    void Start()
    {
        instance = this;
    }

    static public void FadeIn(GameObject gameObject, Action then = null)
    {
        if (gameObject.GetComponent<CanvasGroup>() == null)
            gameObject.AddComponent<CanvasGroup>();

        var fadein = new Motion(gameObject.GetComponent<CanvasGroup>()).AddTimeStamp(
            valuesStart: new { alpha = 0 },
            valuesEnd: new { alpha = 1 },
            totalTime: 0.26f
        );

        instance.StartCoroutine(fadein.Play(then));
    }

    static public void FadeOut(GameObject gameObject, bool autoClose = true)
    {
        var fadeout = new Motion(gameObject.GetComponent<CanvasGroup>()).AddTimeStamp(
            valuesStart: new { alpha = 1 },
            valuesEnd: new { alpha = 0 },
            totalTime: 0.26f
        );

        instance.StartCoroutine(fadeout.Play(() =>
        {
            if (autoClose)
                Destroy(gameObject);
        }));

        
    }
}
