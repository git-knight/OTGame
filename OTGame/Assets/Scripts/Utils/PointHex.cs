using System;
using NotSoSimpleJSON;
using UnityEngine;
using static System.Math;

using Point = UnityEngine.Vector2Int;

public struct PointHex
{
    public int x, y, z;

    public PointHex(int X, int Y, int Z)
    {
        x = X;
        y = Y;
        z = Z;
    }

    public PointHex(JSONNode obj)
    {
        this = obj.ToVecI();
    }

    static public PointHex operator +(PointHex a, PointHex b)
    {
        return new PointHex(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    static public PointHex operator -(PointHex a, PointHex b)
    {
        return new PointHex(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static bool operator ==(PointHex a, PointHex b) { return a.x == b.x && a.y == b.y && a.z == b.z; }
    public static bool operator !=(PointHex a, PointHex b) { return a.x != b.x || a.y != b.y || a.z != b.z; }

    public Point ToPoint()
    {
        return new Point(x + (z - (z & 1)) / 2, z);
    }
    public Vector3 ToScreenPoint()
    {
        var pt = ToPoint();
        return new Vector3(
            (pt.x + (pt.y % 2) * 0.5f) * 64 + 32,
            -(pt.y * 48 + 32) - 30,
            0
        );
        // return ToPoint().ToScreenPoint();
    }

    internal Vector3 ToScreenPointCentered()
        => ToScreenPoint() + Vector3.up * 30;

    static public Vector3 ToScreenPoint(Point pt)
        => ((PointHex)pt).ToScreenPoint();

    static public Vector3 ToScreenPointCentered(Point pt)
        => ((PointHex)pt).ToScreenPointCentered();

    public int ToIndex(int width) { Point pt = ToPoint(); return pt.y * width + pt.x; }

    public bool IsUnit()
    {
        return Abs(x) <= 1 && Abs(y) <= 1 && Abs(z) <= 1;
    }

    public static implicit operator PointHex(Point pt)
    {
        PointHex hex;
        hex.x = pt.x - (pt.y - (pt.y & 1)) / 2;
        hex.y = -hex.x - pt.y;
        hex.z = pt.y;
        return hex;
    }

    public static PointHex FromScreenCoord(Vector3 coord)
    {
        var pt2 = new Point((int)(coord.x / 64) - 2, (int)(-coord.y / 48) - 2);
        var minDist = 10000000f;
        var minPt = pt2;
        for (int i = 0; i < 16; i++)
        {
            var pt = pt2 + new Point(i % 4, i / 4);
            var scrPt = ToScreenPoint(pt);
            var dist = Vector3.Distance(scrPt, coord);
            if (dist < minDist)
            {
                minDist = dist;
                minPt = pt;
            }
        }
        return minPt;
    }

    public static PointHex FromScreenCoordCentered(Vector3 coord)
    {
        var pt2 = new Point((int)(coord.x / 64) - 2, (int)(-coord.y / 48) - 2);
        var minDist = 10000000f;
        var minPt = pt2;
        for (int i = 0; i < 16; i++)
        {
            var pt = pt2 + new Point(i % 4, i / 4);
            var scrPt = ToScreenPointCentered(pt);
            var dist = Vector3.Distance(scrPt, coord);
            if (dist < minDist)
            {
                minDist = dist;
                minPt = pt;
            }
        }
        return minPt;
    }


    internal static Vector3 Round(Vector3 position) 
        => FromScreenCoord(position).ToScreenPoint();
}