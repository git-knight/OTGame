using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestObject : MonoBehaviour
{
    Vector2 coord;

    public int Id { get; set; }

    public string Nickname
    {
        get
        {
            return transform.GetChild(0).GetChild(0).gameObject.GetComponent<Text>().text;
        }
        set
        {
            transform.GetChild(0).GetChild(0).gameObject.GetComponent<Text>().text = value;
        }
    }

    public Vector2 Location { get { return coord; } set { coord = value; transform.position = coord; } }

    public string QuestStatus { get; set; }
}
