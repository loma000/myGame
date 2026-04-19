using UnityEngine;

public class Character : MonoBehaviour
{
    public CharacterData Data;

    public Vector3 position;
    private Vector3 lastPos;
    string moveId;
    public Camera cam;
    public bool isLocal;
    float interval = 0;
    StompClient stompClient;

    void OnEnable()
    {
        stompClient.OnConnected += OnConnect;
        stompClient.OnDisconnected += OnDisconnect;
        if (stompClient.IsConnected)
            OnConnect();
    }

    void OnDestroy()
    {
        stompClient.OnConnected -= OnConnect;
        stompClient.OnDisconnected -= OnDisconnect;
        OnDisconnect();
    }

    void OnConnect()
    {
        if (moveId != null)
            stompClient.Unsubscribe(moveId);
        moveId = stompClient.Subscribe("/topic/movement/" + RoomManager.Instance.roomId, OnSetPos);
    }

    void OnSetPos(string body)
    {
        var move = JsonUtility.FromJson<MovementData>(body);
        if (move.type.Equals("move") && move.Id.Equals(Data.Id))
        {
            Data.col = move.endCol;
            Data.row = move.endRow;

            position = GridTool.HexToWorld(move.endCol, move.endRow);
        }
    }

    void OnDisconnect()
    {
        stompClient.Unsubscribe(moveId);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        stompClient = StompClient.Instance;
        position = transform.position;
        lastPos = position;
        cam = Camera.main;
    }

    void Start()
    {
        position = GridTool.HexToWorld(Data.col, Data.row);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && isLocal && Time.time - interval >= 0.1f)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Tile"))
                {
                    var tile = hit.collider.gameObject.GetComponent<GridObj>();
                    Debug.Log(tile.row + "," + tile.col);

                    var newPosition = GridTool.HexToWorld(tile.col, tile.row);

                    //  Debug.Log("position: " + position);

                    if (!newPosition.Equals(lastPos))
                    {
                        GameManager.OnMoving?.Invoke();
                        Debug.Log(Vector3.Distance(position, lastPos));
                        lastPos = newPosition;
                        var movement = new MovementData
                        {
                            type = "move",
                            Id = Data.Id,
                            startRow = Data.row,
                            startCol = Data.col,
                            endRow = tile.row,
                            endCol = tile.col,
                        };
                        stompClient.Send(
                            "/app/update/movement/" + RoomManager.Instance.roomId,
                            JsonUtility.ToJson(movement)
                        );
                    }
                }
                interval = Time.time;
            }
        }

        transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 10f);
    }
}

[System.Serializable]
public class MovementData
{
    public string type;
    public string Id;
    public int startRow;
    public int startCol;
    public int endRow;
    public int endCol;
}

[System.Serializable]
public class GridData
{
    public int row;
    public int col;

    public bool empty;
}

[System.Serializable]
public class CharacterData
{
    public string name;
    public string Id;
    public string OwnerId;
    public int row;

    public int col;
    public int moveRadius;
}
