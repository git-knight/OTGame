using NotSoSimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public const float MOVEMENT_SPEED = 80;

    public int Id;
    public string Name;
    public string Key { get; internal set; }

    public PointHex CoordHex { get; set; }
    public Vector2Int Coord { get => CoordHex.ToPoint(); set { CoordHex = value; } }
    public Vector2Int ServerCoord { get; set; }

    public bool IsMoving { get { return CoordHex.ToScreenPoint() != transform.position; } }

    void Start()
    {
        transform.position = CoordHex.ToScreenPoint();
    }

    // Update is called once per frame
    void Update()
    {
        var targetPosition = CoordHex.ToScreenPoint();
        if (targetPosition != transform.position)
            transform.position = targetPosition + (transform.position - targetPosition).normalized * Mathf.Max(0, Vector2.Distance(targetPosition, transform.position) - MOVEMENT_SPEED * Time.deltaTime);
    }

    public void Moved(Vector2Int coord)
    {
        Coord = ServerCoord = coord;
    }

    // void OnCollisionEnter2D(Collision2D collision)
    // {
    //     if (collision.gameObject.TryGetComponent<Unit>(out var unit))
    //     {
    //         Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
    //     }
    // }
}
