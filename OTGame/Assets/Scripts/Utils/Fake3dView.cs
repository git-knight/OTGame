using Spriter2UnityDX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fake3dView : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Camera.main == null)
            return;

        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

        if (TryGetComponent<SpriteRenderer>(out var sprite))
            sprite.sortingOrder = (int)(10000 - Vector3.Distance(Camera.main.transform.position, transform.position) * 10);

        if (TryGetComponent<EntityRenderer>(out var entity))
            entity.SortingOrder = (int)(10000 - Vector3.Distance(Camera.main.transform.position, transform.position) * 10);
    }
}
