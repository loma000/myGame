using UnityEngine;

public class MovementManager : MonoBehaviour
{
    private StompClient stompClient;
    float interval = 0f;
    Camera cam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        moveToGrid();
    }

    private void Awake()
    {
        stompClient = StompClient.Instance;
        cam = Camera.main;
    }

    void moveToGrid()
    {
        Character MoveCharacter = CharacterSelector.Instance.selectCharacter;
        if (GameManager.Instance.actionMode != ActionMode.Move)
            return;

        if (Input.GetMouseButton(0) && MoveCharacter.isLocal && Time.time - interval >= 0.1f)
        {
            var tile = GridManager.getGrid(cam);
            if (tile == null)
                return;

            //  Debug.Log("position: " + position);

            var movement = new MovementDto
            {
                type = "Move",
                Id = MoveCharacter.Id,
                startRow = MoveCharacter.row,
                startCol = MoveCharacter.col,
                endRow = tile.row,
                endCol = tile.col,
            };
            stompClient.Send(
                "/app/update/movement/"
                    + RoomManager.Instance.roomId
                    + "/"
                    + PlayerManager.Instance.player.id,
                JsonUtility.ToJson(movement)
            );

            interval = Time.time;
        }
    }
}
