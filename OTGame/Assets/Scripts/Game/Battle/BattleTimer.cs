using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleTimer : MonoBehaviour
{
    private Text timerText;

    public float Timer { get; set; }
    public bool IsRunning { get; set; }

    void Start()
    {
        timerText = GetComponent<Text>();
    }
    
    void Update()
    {
        if (IsRunning)
        {
            Timer -= Time.deltaTime;

            timerText.text = "" + Mathf.CeilToInt(Timer);
            if (Timer < 5)
                timerText.color = Color.red;
            else timerText.color = new Color(0, 0.8584906f, 0.1511735f);
        }
    }
}
