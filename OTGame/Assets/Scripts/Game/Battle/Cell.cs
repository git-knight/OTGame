using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Point = UnityEngine.Vector2Int;

public class Cell
{
    public Point coord;
    public int Value;
    public Image Image;

    public bool IsNone { get { return Value == -1; } }

    public Cell(int x, int y, int value)
    {
        coord = new Point(x, y);
        Value = value;
    }

    internal void Move(TimeStamp spr)
    {
        var values = spr.currentValues;

        if (values.ContainsKey("x"))
            (Image.transform as RectTransform).localPosition = new Vector3(values["x"], values["y"], 0);


    }

    public float X
    {
        get { return (Image.transform as RectTransform).localPosition.x; }
        set { (Image.transform as RectTransform).localPosition = new Vector3(value, (Image.transform as RectTransform).localPosition.y, 0); }
    }

    public float Y
    {
        get { return (Image.transform as RectTransform).localPosition.y; }
        set { (Image.transform as RectTransform).localPosition = new Vector3((Image.transform as RectTransform).localPosition.x, value, 0); }
    }

    //public float CellX { get { return coord.x; } set { coord.x = (int)value; } }
}
