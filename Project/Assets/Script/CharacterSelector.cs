using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    public static CharacterSelector Instance;
    public Character selectCharacter = null;
    public Camera cam;
    public StompClient stompClient;

    void Awake()
    {
        Instance = this;
        stompClient = StompClient.Instance;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.OnMoving += (() => selectCharacter = null);
        GameManager.OnAttacking += (() => selectCharacter = null);
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        setGridShowingData();
        if (Input.GetKeyDown(KeyCode.A) && selectCharacter != null) { }
    }

    public Character selector()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("CharacterSelect");
        if (
            Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)
            && hit.collider.CompareTag("Character")
        )
        {
            return hit.collider.GetComponent<Character>();
        }
        else
            return null;
    }

    void setGridShowingData()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!GridManager.Instance.isShowing)
            {
                var currentSelect = selector();
                if (currentSelect == null)
                    return;
                if (selectCharacter == currentSelect)
                    return;
                selectCharacter = currentSelect;

                if (selectCharacter.isLocal)
                {
                    Debug.Log(selectCharacter.Name);
                    var data = new GetCharacterGridDto { characterId = selectCharacter.Id };

                    stompClient.Send(
                        "/app/game/getCharacter/Grid/"
                            + RoomManager.Instance.roomId
                            + "/"
                            + PlayerManager.Instance.player.id,
                        JsonUtility.ToJson(data)
                    );
                }
            }
        }
    }
}

[System.Serializable]
class GetCharacterGridDto
{
    public string characterId;
}
