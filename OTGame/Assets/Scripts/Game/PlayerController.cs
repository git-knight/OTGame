using System;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class MapCursorSpritePack
{
    public Sprite green;
    public Sprite red;
    public Sprite fight;
}

public class PlayerController : MonoBehaviour
{
    public MapCursorSpritePack cursorSprites;

    public Unit Player { get; set; }
    private Map Map { get { return Player.transform.parent.parent.GetComponent<Map>(); } }
    private MapModel MapModel { get { return Player.transform.parent.GetComponent<MapModel>(); } }

    PointHex? movementTarget;

    void Update()
    {
        if (Player == null)
            return;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit) && hit.transform.GetComponent<Map>() != null && !EventSystem.current.IsPointerOverGameObject())
        {
            var hitCoord = PointHex.FromScreenCoordCentered(hit.point);
            Map.UpdateMapCursor(Map.TryGetMonster(hitCoord)!=null ? cursorSprites.fight : (Map.IsBlocked(hitCoord) ? cursorSprites.red : cursorSprites.green), hitCoord);
            if (Input.GetMouseButtonDown(0))
                movementTarget = hitCoord;
        }

        var cameraMoveTo = Player.transform.position - Camera.main.transform.forward * 430;
        Camera.main.transform.position = Camera.main.transform.position * 0.9f + cameraMoveTo * 0.1f;

        if (!Player.IsMoving && movementTarget.HasValue && movementTarget != Player.CoordHex)
        {
            int nextDirection = findPath(movementTarget.Value);
            if (nextDirection != -1)
            {
                GameHub.Invoke("DoPlayerMove", nextDirection);
                Player.CoordHex += directions[nextDirection];
                if (Map.IsUnit(Player.CoordHex))
                {
                    Player.CoordHex -= directions[nextDirection];
                    movementTarget = null;
                }
            }
        }
    }


    static PointHex[] directions = {
        new PointHex(-1, 1, 0), new PointHex(-1, 0, 1), new PointHex(0, -1, 1),
        new PointHex(1, -1, 0), new PointHex(1, 0, -1), new PointHex(0, 1, -1)
    };
    int findPath(PointHex target)
    {
        var visited = new int[64 * 64];
        for (int i = 0; i < 64 * 64; i++)
            visited[i] = 10000;

        var q = new PointHex[64 * 64];
        q[0] = target;
        visited[q[0].ToIndex(MapModel.Width)] = 0;

        int q0 = 0, q1 = 1;
        while (q0 != q1)
        {
            PointHex pt = q[q0++];

            for (int i = 0; i < 6; i++)
            {
                PointHex nextPt = pt + directions[i];

                if (nextPt == Player.CoordHex)
                {
                    int closestCellIdx = -1;
                    float dist = 10000000;
                    Vector3 targetLoc = target.ToScreenPoint();

                    for (int j = 0; j < 6; j++)
                    {
                        if (visited[(nextPt + directions[j]).ToIndex(MapModel.Width)] >= visited[nextPt.ToIndex(MapModel.Width)])
                            continue;

                        if (Map.IsBlocked(nextPt + directions[j], false))
                            continue;

                        Vector3 nextLoc = (nextPt + directions[j]).ToScreenPoint();
                        float newDist = (targetLoc - nextLoc).sqrMagnitude;
                        if (newDist < dist)
                        {
                            dist = newDist;
                            closestCellIdx = j;
                        }
                    }

                    return closestCellIdx;
                }

                if (!Map.IsBlocked(nextPt, true) && visited[nextPt.ToIndex(MapModel.Width)] == 10000)
                {
                    q[q1++] = nextPt;
                    visited[nextPt.ToIndex(MapModel.Width)] = visited[pt.ToIndex(MapModel.Width)] + 1;
                }
            }
        }

        return -1;
    }

}
