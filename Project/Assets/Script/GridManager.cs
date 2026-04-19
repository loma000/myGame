using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject GridPrefab;
    int ColSize = 8;
    int RowSize = 8;
    public List<GridObj> grids;
    public List<GridData> moveableGrid;
    public List<GridData> AttackableGrid;
    private StompClient stompClient;
    private bool isShowing = false;
    private string getCharGridId;

    void Awake()
    {
        stompClient = StompClient.Instance;
        grids = new List<GridObj>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GridGenerator();
        GameManager.OnGameStart += OnConnect;
        GameManager.OnMoving += moving;
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
        moveableGrid = data.moveAbleGrid;
        foreach (var g in moveableGrid)
            Debug.Log($"col: {g.col}, row: {g.row}");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && CharacterSelector.Instance.selectCharacter != null)
        {
            if (!isShowing)
            {
                ShowGrid();
            }
            else
            {
                ResetGrid();
            }
            isShowing = !isShowing;
        }
    }

    void moving()
    {
        isShowing = false;
        ResetGrid();
    }

    void ShowGrid()
    {
        ResetGrid();

        foreach (GridData data in moveableGrid)
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
}

[System.Serializable]
public class CharacterGridData
{
    public List<GridData> moveAbleGrid;
}
