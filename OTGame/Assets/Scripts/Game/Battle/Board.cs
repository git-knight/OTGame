using NotSoSimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Point = UnityEngine.Vector2Int;

public class BattlerInfo
{
    struct BattlerView
    {
        public GameObject hpline;
        public Text hpText;
        public Text name;
        public Text level;
        public Text[] stones;
    }

    BattlerView view;

    public string Nickname { get; set; }
    float hp, hpMax;
    int level;

    public int[] Stones { get; set; } = new int[3];

    public float Health
    {
        get
        {
            return hp;
        }
        set
        {
            hp = Mathf.Max(0, value);

            //(view.hpline.transform as RectTransform).sizeDelta = new Vector2(13, 250 * );
            //view.hpText.text = Mathf.Floor(hp) + "/" + hpMax;
        }
    }

    public float HealthMax { get { return hpMax; } }
    public float HealthPercent
    {
        get { return (hp / hpMax); }
        set
        {
            (view.hpline.transform as RectTransform).sizeDelta = new Vector2(13, 250 * value);
        }
    }

    public BattlerInfo(JSONNode data, GameObject battleBG)
    {
        Nickname = data["name"].AsString;

        int pIndex = Nickname == GameHub.DisplayName ? 0 : 1;

        view.hpline = battleBG.transform.Find("hpline" + pIndex).gameObject;
        view.hpText = battleBG.transform.Find("hp" + pIndex).GetComponent<Text>();
        view.name = battleBG.transform.Find("nick" + pIndex).GetComponent<Text>();
        view.stones = 3.Times().Select(x => battleBG.transform.Find("colorvalue" + pIndex + '_' + x).GetComponent<Text>()).ToArray();

        level = data["level"].AsInt.Value;
        hpMax = data["hpMax"].AsInt.Value;
        Health = (float)data["hp"].AsDouble.Value;
        Stones = data["stones"].AsArray.Select(x => x.AsInt.Value).ToArray();
    }

}

public class Board : MonoBehaviour
{
    public Image CellPrefab, BlockPrefab;
    public Sprite[] blockSprites;

    public BattlerInfo[] players { get; private set; }
    int myId;

    BattleTimer battleTimer;

    public Image SelectionSprite;
    const int boardWidth = 5;
    const int boardHeight = 6;
    Vector2 cellSize;

    Point[] boardCoords;

    public List<Cell> cells = new List<Cell>();
    public Cell this[Point pt] { get { return cells.FirstOrDefault(c => c.coord == pt); } }

    public int turn = 0;

    public bool isMyTurn { get { return turn % 2 == myId; } }
    public bool IsAnimating { get; set; } = false;


    internal void Initialize(JSONNode board)
    {
        var boardSize = (this.transform as RectTransform).rect;
        cellSize = new Vector2(boardSize.width / boardWidth, boardSize.height / boardHeight);

        (SelectionSprite.transform as RectTransform).sizeDelta = cellSize;

        boardCoords = new Point[boardHeight * boardWidth];

        for (var j = 0; j < boardHeight; j++)
            for (var i = 0; i < boardWidth; i++)
            {
                boardCoords[j * boardWidth + i] = new Point(i, j);
                var cell = Instantiate(CellPrefab);
                cell.transform.SetParent(this.transform);

                var cellCoord = cell.transform as RectTransform;

                cellCoord.localPosition = GetCellCoord(i, j);
                cellCoord.sizeDelta = cellSize;
            }

        players = new BattlerInfo[]
        {
            new BattlerInfo(board["left"], transform.parent.gameObject),
            new BattlerInfo(board["right"], transform.parent.gameObject)
        };

        if (players[1].Nickname == GameHub.DisplayName)
        {
            myId = 1;
            (players[1], players[0]) = (players[0], players[1]);
        }

        battleTimer = transform.parent.Find("timer").gameObject.GetComponent<BattleTimer>();
        battleTimer.IsRunning = true;

        turn = board["turn"].AsInt.Value;
        if (!isMyTurn)
            UICommon.FadeIn(transform.parent.Find("boardShade").gameObject);

        UICommon.FadeIn(transform.parent.parent.gameObject, () => OnRegenerated(board));
    }


    public void OnRegenerated(JSONNode boardData)
    {
        battleTimer.Timer = (float)boardData["turnEndsIn"].AsDouble.Value;

        StartCoroutine(Regenerate(boardData));
    }

    public IEnumerator Regenerate(JSONNode boardData) {
        //if (boardData == null)
        //    RandomizeBoard();

        while (IsAnimating || turn != boardData["turn"].AsInt)
            yield return null;

        IsAnimating = true;

        if (cells.Any())
        {
            var animations = new List<Motion>();
            var f = -0.1f;
            var lastX = -1;
            foreach (var cell in cells.OrderBy(c => c.coord.x).ThenByDescending(c => c.coord.y)) 
            {
                if (cell.coord.x != lastX)
                {
                    lastX = cell.coord.x;
                    f = -0.1f;
                }
                animations.Add(CreateFalldownAnimation(cell, true, boardHeight + 1, (f += 0.04f) + cell.coord.x * 0.003f));
            }

            foreach (var el in new MotionPack(animations.ToArray()).YieldUpdate())
                yield return el;
        }

        foreach (var cell in cells)
            Destroy(cell.Image);

        cells.Clear();

        for (var j = 0; j < boardHeight; j++)
            for (var i = 0; i < boardWidth; i++)
            {
                var cell = new Cell(i, j, boardData["cells"][i][j].AsInt.Value);
                if (cell.IsNone)
                    continue;

                cells.Add(cell);

                var cellImg = Instantiate(BlockPrefab);
                cellImg.GetComponent<BlockMouseHandler>().parent = cell;
                cellImg.transform.SetParent(this.transform);
                cellImg.sprite = blockSprites[cell.Value];

                cell.Image = cellImg;

                var cellCoord = cellImg.transform as RectTransform;

                cellCoord.localPosition = GetCellCoord(i, j);
                cellCoord.sizeDelta = cellSize;
            }

        {
            var animations = new List<Motion>();
            var f = -0.1f;
            var lastX = -1;
            foreach (var cell in cells.OrderBy(c => c.coord.x).ThenByDescending(c => c.coord.y))
            {
                if (cell.coord.x != lastX)
                {
                    lastX = cell.coord.x;
                    f = -0.1f;
                }
                cell.Y = 300 + (boardHeight - 1 - cell.coord.y) * 48;
                animations.Add(CreateFalldownAnimation(cell, true, cell.coord.y, (f += 0.065f) + cell.coord.x * 0.007f));
            }

            foreach (var el in new MotionPack(animations.ToArray()).YieldUpdate())
                yield return el;
        }

        IsAnimating = false;
        turn = boardData["turn"].AsInt.Value;
    }

    void DestroyBlock(Cell cell)
    {
        Destroy(cell.Image);
        cells.Remove(cell);
    }

    void DestroyBlock(Point cell)
    {
        DestroyBlock(this[cell]);
    }
    /*
    void RandomizeBoard()
    {
        for (var j = 0; j < boardHeight; j++)
            for (var i = 0; i < boardWidth; i++)
                cells[i, j] = new Cell(i, j, Random.Range(0, 4));
    }
    */

    Vector3 GetCellCoord(int i, int j)
    {
        return new Vector3(cellSize.x * i, cellSize.y * (boardHeight - 1 - j), 0);
    }

    Cell selectedCell = null;
    internal void SelectBlock(Cell cell)
    {
        if (IsAnimating || !isMyTurn || isWaitingSwapResponse)
            return;


        if (selectedCell == cell || selectedCell != null && TrySwap(selectedCell.coord, cell.coord))
        {
            selectedCell = null;
            SelectionSprite.GetComponent<Image>().enabled = false;
        }
        else
        {
            selectedCell = cell;
            SelectionSprite.GetComponent<Image>().enabled = true;
            (SelectionSprite.transform as RectTransform).localPosition = GetCellCoord(cell.coord.x, cell.coord.y) + new Vector3(cellSize.x / 2, cellSize.y / 2, 0);
        }
    }

    Cell draggedCell;
    Point draggedCoord;
    internal void StartDragging(Cell cell)
    {
        if (IsAnimating)
            return;

        draggedCell = cell;
        draggedCoord = cell.coord;
    }

    internal void StopDragging(Cell cell)
    {
    }

    void Update()
    {
        // movement = (2 + -1.0886621 * t)*t*t
        // movement = sin(-Math.PI/2 + t*Math.PI)*0.5 + 0.5

        if (draggedCell != null && isMyTurn && !IsAnimating && !isWaitingSwapResponse)
        {
            if (!Input.GetMouseButton(0))
            {
                if (isMyTurn && draggedCell.coord != draggedCoord)
                    TrySwap(draggedCell.coord, draggedCoord);
                else draggedCell = null;
                return;
            }

            var boardRect = (transform as RectTransform);
            var mouseCursor = Input.mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(boardRect, mouseCursor, null, out Vector2 mouseLoc);
            mouseLoc.y = boardRect.rect.height - mouseLoc.y;
            Point mouseCoord = (mouseLoc / cellSize).VFloor();
            var coordDraggedTo = boardCoords.Where(pt => Vector2Int.Distance(pt, draggedCoord) <= 1).OrderBy(pt => (new Vector2(pt.x + 0.5f, pt.y + 0.5f) * cellSize - mouseLoc).magnitude).FirstOrDefault();
            if (draggedCell.coord != coordDraggedTo)
            {
                if (Vector2Int.Distance(coordDraggedTo, draggedCell.coord) > 1)
                    StartCoroutine(SwapSprites(draggedCell.coord, draggedCoord).OnFinished(() => IsAnimating = false).Play());
                else StartCoroutine(SwapSprites(draggedCell.coord, coordDraggedTo).OnFinished(() => IsAnimating = false).Play());
                IsAnimating = true;
            }
        }

        if (false && !isMyTurn && !IsAnimating && draggedCell != null && draggedCell.coord != draggedCoord)
        {
            StartCoroutine(SwapSprites(draggedCell.coord, draggedCoord).OnFinished(() => IsAnimating = false).Play());
            IsAnimating = true;
            draggedCell = null;
        }
    }

    MotionPack SwapSprites(params Point[] points)
    {
        var cell0 = this[points[0]];
        var cell1 = this[points[1]];

        if (cell0 != null) cell0.coord = points[1];
        if (cell1 != null) cell1.coord = points[0];

        var swappy = new MotionPack(new Cell[] { cell0, cell1 }
            .Where(c => c != null)
            .Select(c => new Motion(c).AddTimeStamp(
                totalTime: 0.3f,
                valuesEnd: new
                {
                    X = c.coord.x * cellSize.x,
                    Y = (boardHeight - 1 - c.coord.y) * cellSize.y
                }
              )).ToArray());

        return swappy;
    }

    IEnumerator DoMove(int serverTurn, params Point[] points)
    {
        //while (IsAnimating)
        //    yield return null;

        //IsAnimating = true;

        if (isWaitingSwapResponse && draggedCell != null && draggedCell.coord != draggedCoord)
        {
            draggedCell = null;
        }
        else
        {
            foreach (var tick in SwapSprites(points).YieldUpdate())
                yield return tick;
        }

        foreach (var el in DoCombinations())
            yield return el;

        FinishTurn();
    }

    bool isWaitingSwapResponse = false;
    bool TrySwap(Point pt0, Point pt1)
    {
        if (IsAnimating || !isMyTurn || isWaitingSwapResponse)
            return false;

        if (pt0.x > pt1.x || pt0.y > pt1.y)
            (pt0, pt1) = (pt1, pt0);

        if (pt1.x - pt0.x + pt1.y - pt0.y != 1)
            return false;

        Debug.Log("DoBoardMove called");
        GameHub.Invoke("DoBoardMove", pt0, pt0.x == pt1.x);
        isWaitingSwapResponse = true;
        selectedCell = null;
        SelectionSprite.GetComponent<Image>().enabled = false;
        return true;
    }

    public void UseSkill(int id)
    {
        if (!isMyTurn || isWaitingSwapResponse)
            return;

        GameHub.Invoke("UseSkill", id);
        isWaitingSwapResponse = true;
    }

    JSONNode _actionResult;

    public bool OnSkillUsed(int id, JSONNode actionResult)
    {
        if (IsAnimating)
            return false;

        _actionResult = actionResult;
        FinishTurn();
        return true;
    }

    public bool OnActionPerformed(JSONNode pt, bool isVertical, JSONNode actionResult)
    {
        if (IsAnimating)
            return false;

        _actionResult = actionResult;

        var pt0 = pt.ToVecI();
        if (pt0.x == -1)
        {
            if (draggedCell != null && isWaitingSwapResponse)
            {
                StartCoroutine(SwapSprites(draggedCell.coord, draggedCoord).OnFinished(() => IsAnimating = false).Play());
                IsAnimating = true;
                draggedCell = null;
            }
            FinishTurn();
            return true;
        }

        IsAnimating = true;

        var pt1 = new Point(pt0.x + (isVertical ? 0 : 1), pt0.y + (isVertical ? 1 : 0));
        StartCoroutine(DoMove(actionResult["turn"].AsInt.Value, pt0, pt1));

        return true;
    }

    void SpawnMessage(string templateName, float pause, int? value = null)
    {
        var message = Instantiate(transform.parent.Find("templates/" + templateName), transform.parent);

        if (value.HasValue)
            message.GetComponent<Text>().text = message.GetComponent<Text>().text.Replace("10", "" + value);

        var animator = message.GetComponent<BattleMessage>();
        animator.Play(pause);
    }

    void FinishTurn()
    {
        IsAnimating = false;
        isWaitingSwapResponse = false;
        draggedCell = null;

        //if (this.spritesSwapping.length != 0)
        //    this.swapSprites(this.spritesSwapping, () => this.isAnimating = false);

        //turn++;

        const float deltaPause = 0.24f;
        float pause = -deltaPause;

        int pid0 = (turn % 2) ^ myId;
        int pid1 = pid0 ^ 1;

        if (_actionResult["dmgSelf"].AsInt < 0)
            SpawnMessage("healed", pause += deltaPause, -1 * _actionResult["dmgSelf"].AsInt);
        else if (_actionResult["dmgSelf"].AsInt > 0)
            SpawnMessage(pid0 == 0 ? "damagetaken" : "damagedealt", pause += deltaPause, _actionResult["dmgSelf"].AsInt);

        if (_actionResult["dmgEnemy"].AsInt > 0)
            SpawnMessage(pid0 == 1 ? "damagetaken" : "damagedealt", pause += deltaPause, _actionResult["dmgEnemy"].AsInt);

        players[pid0].Health -= _actionResult["dmgSelf"].AsInt.Value;
        players[pid1].Health -= _actionResult["dmgEnemy"].AsInt.Value;

        players[pid0].Stones = players[pid0].Stones.Zip(_actionResult["colors"].AsArray, (a, b) => a + b.AsInt.Value).ToArray();

        var wasMyTurn = isMyTurn;
        turn = _actionResult["turn"].AsInt.Value;
        if (isMyTurn != wasMyTurn)
        {
            if (isMyTurn)
                UICommon.FadeOut(transform.parent.Find("boardShade").gameObject, false);
            else UICommon.FadeIn(transform.parent.Find("boardShade").gameObject);
        }

        if (isMyTurn)
            SpawnMessage("yourturn", pause += deltaPause);
        else SpawnMessage("enemyturn", pause += deltaPause);

        battleTimer.Timer = 25;
    }

    IEnumerable DoCombinations()
    {
        Point right = new Point(1, 0);
        Point bottom = new Point(0, 1);
        int W = boardWidth;
        int H = boardHeight;

        var toRemove = new List<Point>(64);

        for (int dir = 0; dir < 2; dir++)
        {
            for (int i = 0; i < W; i++)
            {
                var repeats = new List<Point>(7);
                int repColor = -1;

                for (int j = 0; j < H; j++)
                {
                    Point coord = right * i + bottom * j;
                    int color = this[coord]?.Value ?? -1;
                    if (color != repColor)
                    {
                        if (repColor != -1 && repeats.Count >= 3)
                            toRemove.AddRange(repeats);

                        repeats.Clear();
                        repColor = color;
                    }

                    repeats.Add(coord);
                }

                if (repColor != -1 && repeats.Count >= 3)
                    toRemove.AddRange(repeats);
            }

            W = boardHeight;
            H = boardWidth;
            right = bottom;
            bottom = new Point(bottom.y, bottom.x);
        }

        if (toRemove.Count == 0)
            IsAnimating = false;

        else
        {
            foreach (var coord in toRemove.Distinct())
                DestroyBlock(coord);

            foreach (var el in PlayGravity())
                yield return el;
        }
    }

    IEnumerable PlayGravity()
    {

        Point right = new Point(1, 0);
        Point bottom = new Point(0, 1);
        int W = boardWidth;
        int H = boardHeight;

        for (int dir = 0; dir < 2; dir++)
        {
            var animations = new List<Motion>();
            var fx = 0f;
            for (int i = 0; i < W; i++)
            {
                var y = H - 1;
                var f = -0.04f;
                for (int j = H - 1; j >= 0; j--)
                {
                    var cell = this[right * i + bottom * j];
                    if (cell != null && y != j)
                    {
                        animations.Add(CreateFalldownAnimation(cell, dir == 0, y, (f += 0.04f) + fx));
                        //var t = this[coord].Value;
                        //this[coord].Value = -1;
                        //this[right * i + bottom * (y--)].Value = t;
                    }
                    if (cell != null)
                        y--;
                }
                if (f != -0.04f)
                    fx += 0.023f;
            }

            foreach (var el in new MotionPack(animations.ToArray()).YieldUpdate())
                yield return el;

            W = boardHeight;
            H = boardWidth;
            right = bottom;
            bottom = new Point(bottom.y, bottom.x);

        }

        foreach (var el in DoCombinations())
            yield return el;
    }

    Motion CreateFalldownAnimation(Cell cell, bool isY, int newCoord, float pauseTime)
    {
        if (!isY)
            cell.coord.x = newCoord;
        else cell.coord.y = newCoord;

        return new Motion(cell)
          .AddTimeStamp(totalTime: pauseTime)
          .AddTimeStamp(
            changeAcceleration: (!isY) ? (object)new { X = 1000.0f } : new { Y = -1000.0f },
            valuesEnd: (!isY) ? (object)new { X = cellSize.x * newCoord - 0.01f } : new { Y = cellSize.y * (boardHeight - 1 - newCoord) + 0.01f }
            )
          .AddTimeStamp(
            changeAcceleration: (!isY) ? (object)new { X = 1000.0f } : new { Y = -1000.0f },
            changeSpeed: (!isY) ? (object)new { X = -100.0f } : new { Y = 100.0f },
            valuesEnd: (!isY) ? (object)new { X = cellSize.x * newCoord } : new { Y = cellSize.y * (boardHeight - 1 - newCoord) }
          );
    }
}