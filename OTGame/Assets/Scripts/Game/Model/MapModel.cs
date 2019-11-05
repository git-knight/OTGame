using System;
using UnityEngine;

public class MapModel : MonoBehaviour
{
    public int Width;
    public int Height;

    public byte[] Passability;

    internal void Init(MapModel mapModel)
    {
        Width = mapModel.Width;
        Height = mapModel.Height;
        Passability = (byte[])mapModel.Passability.Clone();
    }
}