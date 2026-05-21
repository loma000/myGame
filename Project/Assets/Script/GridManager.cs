using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public GameObject GridPrefab;
    int ColSize = 8;
    int RowSize = 8;
    public List<GridObj> grids;
    public List<GridData> MoveableGrid;
    public List<GridData> AttackableGrid;
    private StompClient stompClient;
    public bool isShowing = false;
    private GameManager gameManager;

    private string getCharGridId;

    void Awake()
    {
        Instance = this;
        stompClient = StompClient.Instance;
        grids = new List<GridObj>();
        gameManager = GameManager.Instance;
        GameManager.OnGameStart += GridGenerator;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.OnGameStart += OnConnect;
        GameManager.OnMoving += Resetting;
        GameManager.OnAttacking += Resetting;
    }

    void OnConnect()
    {
        if (getCharGridId != null)
        {
            stompClient.Unsubscribe(getCharGridId);
            getCharGridId = null;
        }

        getCharGridId = stompClient.Subscribe(
            "/topic/getCharacter/Grid/"
                + RoomManager.Instance.roomId
                + "/"
                + PlayerManager.Instance.player.id,
            getCharacterGrid
        );
    }

    void getCharacterGrid(string body)
    {
        var data = JsonConvert.DeserializeObject<CharacterGridData>(body);
        MoveableGrid = data.moveAbleGrid;
        AttackableGrid = data.attackAbleGrid;
        foreach (var g in MoveableGrid)
            Debug.Log($"col: {g.col}, row: {g.row}");
    }

    // Update is called once per frame
    void Update()
    {
        if (
            (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S))
            && CharacterSelector.Instance.selectCharacter != null
        )
        {
            if (!isShowing)
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    gameManager.actionMode = ActionMode.Move;
                    ShowMoveAbleGrid();
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    gameManager.actionMode = ActionMode.Attack;
                    ShowAttackAbleGrid();
                }
            }
            else
            {
                ResetGrid();
                gameManager.actionMode = ActionMode.Normal;
            }
            isShowing = !isShowing;
        }
    }

    void Resetting()
    {
        isShowing = false;
        gameManager.actionMode = ActionMode.Normal;
        ResetGrid();
    }

    void ShowAttackAbleGrid()
    {
        ResetGrid();

        foreach (GridData data in AttackableGrid)
        {
            GridObj match = grids.Find(g => g.col == data.col && g.row == data.row);

            if (match != null)
                match.SetGridTex("ShowAttack");
        }
    }

    void ShowMoveAbleGrid()
    {
        ResetGrid();

        foreach (GridData data in MoveableGrid)
        {
            GridObj match = grids.Find(g => g.col == data.col && g.row == data.row);

            if (match != null)
                match.SetGridTex("ShowMove");
        }
    }

    void ResetGrid()
    {
        foreach (GridObj grid in grids)
        {
            grid.SetGridTex("Normal");
        }
    }

    void GridGenerator()
    {
        for (int i = 1; i <= ColSize; i++)
        {
            for (int j = 1; j <= RowSize; j++)
            {
                Vector3 pos = GridTool.HexToWorld(i, j);
                var grid = Instantiate(GridPrefab, pos, Quaternion.identity, transform);
                GridObj g = grid.GetComponent<GridObj>();
                g.col = i;
                g.row = j;
                grids.Add(g);
            }
        }
    }

    public static Grid getGrid(Camera cam)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Debug.Log("a");
            if (hit.collider.CompareTag("Tile"))
            {
                var tile = hit.collider.gameObject.GetComponent<Grid>();
                Debug.Log(tile.row + "," + tile.col);
                return tile;
            }
        }
        return null;
    }
}

[System.Serializable]
public class CharacterGridData
{
    public List<GridData> moveAbleGrid;
    public List<GridData> attackAbleGrid;
}

public enum ActionMode
{
    Normal,
    Attack,
    Move,
}
