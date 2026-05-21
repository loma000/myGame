using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Character : MonoBehaviour
{
    public string Name;
    public string Id;
    public string OwnerId;
    public int row;

    public int col;

    public float Hp;
    public float maxHp;
    public float Atk;

    [SerializeField]
    private HealthBar healthBar;
    public Vector3 position;
    private Vector3 lastPos;
    string moveId;
    public Camera cam;
    public bool isLocal;
    float interval = 0;
    StompClient stompClient;

    public CharacterVisual _characterVisual;

    public void InstanceCharacter(
        bool isLocal,
        string name,
        string id,
        string OwnerId,
        int row,
        int col,
        float Atk,
        float maxHp,
        float Hp
    )
    {
        this.isLocal = isLocal;
        this.Name = name;
        this.Id = id;
        this.OwnerId = OwnerId;
        this.row = row;
        this.col = col;
        this.Atk = Atk;
        this.maxHp = maxHp;
        this.Hp = Hp;
        healthBar.UpdateHealthBar(maxHp, Hp);
    }

    void OnEnable()
    {
        stompClient.OnConnected += OnConnect;
        stompClient.OnDisconnected += OnDisconnect;
        if (stompClient.IsConnected)
            OnConnect();

        _characterVisual = GetComponent<CharacterVisual>();
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
        var move = JsonConvert.DeserializeObject<MovementData>(body);
        if (move.type.Equals("move") && move.Id.Equals(Id))
        {
            col = move.endCol;
            row = move.endRow;
            if (isLocal)
            {
                GameManager.OnMoving?.Invoke();
            }

            StartCoroutine(MoveAlongPath(move.path, row, col));
        }
    }

    IEnumerator MoveAlongPath(List<GridData> path, int endRow, int endCol, float speed = 2f)
    {
        _characterVisual.SetWalking(true);
        foreach (var step in path)
        {
            Vector3 target = GridTool.HexToWorld(step.col, step.row);
            transform.LookAt(new Vector3(target.x, transform.position.y, target.z));
            while (Vector3.Distance(transform.position, target) >= 0.1f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    speed * Time.deltaTime
                );
                yield return null;
            }
            transform.position = target;
        }
        transform.position = GridTool.HexToWorld(endCol, endRow);
        _characterVisual.SetWalking(false);
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
        position = GridTool.HexToWorld(col, row);
        transform.position = position;
    }

    public void TakeDamage(float newHp)
    {
        Hp = newHp;
        healthBar.UpdateHealthBar(maxHp, Hp);
        deadCheck();
    }

    void deadCheck()
    {
        if (Hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}

[System.Serializable]
public class MovementDto
{
    public string type;
    public string Id;
    public int startRow;
    public int startCol;
    public int endRow;
    public int endCol;
}

[System.Serializable]
public class MovementData
{
    public string type;
    public string Id;

    public int endRow;
    public int endCol;
    public List<GridData> path;
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

    public int attackRadius;
    public float Hp;
    public float maxHp;
    public float Atk;
}
