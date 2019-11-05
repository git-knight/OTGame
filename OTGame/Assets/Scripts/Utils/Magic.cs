using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NotSoSimpleJSON;
using UnityEngine;

[Serializable]
public class Binding
{
    public string SourcePath;
    public string TargetPath;

}


class RangeIterator : IEnumerator<int>
{
    int first;
    int last;

    public int Current { get; set; }
    object IEnumerator.Current => Current;

    public RangeIterator(int first, int last)
    {
        Current = first;
        this.first = first;
        this.last = last;
    }

    public void Dispose() { }

    public bool MoveNext()
    {
        if (Current == last - 1)
            return false;

        Current++;
        return true;
    }

    public void Reset()
    {
        Current = first;
    }
}

class Range : IEnumerable<int>
{
    private int min;
    private int max;

    public Range(int num)
    {
        max = num;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<int> GetEnumerator() => new RangeIterator(min, max);
}

static class Magic
{
    static public Range Times(this int num)
        => new Range(num);

    static public Vector2Int VFloor(this Vector2 v) 
        => new Vector2Int((int)Math.Floor(v.x), (int)Math.Floor(v.y));
    static public Vector2Int VRound(this Vector2 v) 
        => new Vector2Int((int)Math.Round(v.x), (int)Math.Round(v.y));

    static public Vector2Int ToVecI(this JSONNode node) 
        => new Vector2Int(node["x"].AsInt.Value, node["y"].AsInt.Value);
    static public Vector2 ToVecF(this JSONNode node) 
        => new Vector2((float)node["x"].AsDouble.Value, (float)node["y"].AsDouble.Value);

    static public object FollowPath(object source, string path, bool returnRef)
    {
        if (!returnRef)
            path += ".";

        string value = "";
        char lastKey = path[0];
        for (int i = 1; i < path.Length; i++)
        {
            var ch = path[i];
            if (!char.IsLetterOrDigit(ch))
            {
                if (lastKey == '.')
                {
                    if (value == "parent")
                        source = (source.GetType().GetProperty("transform").GetValue(source) as Transform).parent;
                    else source = source.GetType().GetProperty(value).GetValue(source);
                }
                else if (lastKey == '#')
                    source = ((Array)source).GetValue(int.Parse(value));
                else if (lastKey == '-')
                    source = (source as Transform).Find(value);
                else if (lastKey == ':')
                    source = source.GetType().GetMethod("GetComponent", new Type[] { typeof(string) }).Invoke(source, new object[] { value });

                if (i != path.Length - 1)
                    value = "";
                lastKey = ch;
            }
            else
            {
                value += ch;
            }
        }

        if (returnRef)
        {
            if (lastKey == '.')
                return (Action<object>)(x => source.GetType().GetProperty(value).SetValue(source, x));
        }

        return source;
    }
}