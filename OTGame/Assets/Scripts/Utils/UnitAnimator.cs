using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    Unit unit;

    void Start()
    {
        unit = GetComponent<Unit>();
    }

    // Update is called once per frame
    void Update()
    {
        if (unit.IsMoving)
            transform.localScale = new Vector3(
                ((unit.CoordHex.ToScreenPoint().x > unit.transform.position.x) ? 1 : -1) * Mathf.Abs(transform.localScale.x), 
                transform.localScale.y, 
                1
            );
        
        unit.GetComponent<Animator>()?.SetBool("IsRunning", unit.IsMoving);
    }
}
