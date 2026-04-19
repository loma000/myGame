using Unity.Multiplayer.Center.Common;
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
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selector();
        }

        if (Input.GetKeyDown(KeyCode.A) && selectCharacter != null) { }
    }

    void selector()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Character"))
            {
                selectCharacter = hit.collider.GetComponent<Character>();

                if (selectCharacter.isLocal)
                {
                    Debug.Log(selectCharacter.Data.name);
                    var data = new GetCharacterGridDto { character = selectCharacter.Data };

                    stompClient.Send(
                        "/app/game/getCharacter/Grid/"
                            + RoomManager.Instance.roomId
                            + "/"
                            + PlayerManager.Instance.player.id,
                        JsonUtility.ToJson(data)
                    );
                }
            }
            else
            {
                selectCharacter = null;
            }
        }
    }
}

[System.Serializable]
class GetCharacterGridDto
{
    public CharacterData character;
}
