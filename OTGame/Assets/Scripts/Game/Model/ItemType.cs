using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemType : MonoBehaviour
{
    public int Id { get; set; }

    public int Price;

    public virtual bool IsStackable => true;
    public abstract object ToModel();
}
