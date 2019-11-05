using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ValueBinding : MonoBehaviour
{
    public Binding[] Bindings;

    Action<object>[] setters;

    // Start is called before the first frame update
    void Start()
    {
        setters = Bindings.Select(x => Magic.FollowPath(this, x.TargetPath, true) as Action<object>).ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var obj in Bindings.Zip(setters, (b, s) => new { b, s }))
            obj.s(Magic.FollowPath(this, obj.b.SourcePath, false));
    }
}
