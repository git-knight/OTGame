using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Reference
{
    public string Name;
    public string Path;

}
public class TextBinding : MonoBehaviour
{
    Text text;
    public Reference[] values;

    public string TextTemplate = "{val}";
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
    }

    string Stringify(object val)
    {
        if (val is string)
            return val as string;

        return ""+(int)Math.Round(Convert.ToSingle(val));
    }

    // Update is called once per frame
    void Update()
    {
        text.text = values.Aggregate(TextTemplate, (t, v) => t.Replace('{' + v.Name + '}', Stringify(Magic.FollowPath(this, v.Path, false)) + ""));
    }
}
