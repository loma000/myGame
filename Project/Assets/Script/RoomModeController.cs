using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomModeController : MonoBehaviour
{
    public VisualElement ui;
    public GameObject watingLobby;
    public GameObject joinRoomUi;
    public Button createButton;
    public Button joinButton;

    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        createButton = ui.Q<Button>("CreateButton");
        createButton.clicked += createRoom;
        joinButton = ui.Q<Button>("JoinButton");
        joinButton.clicked += joinRoom;
    }

    private void createRoom()
    {
        Debug.Log("Create room");
        RoomManager.Instance.createdRoom();
        watingLobby.SetActive(true);
        gameObject.SetActive(false);
    }

    private void joinRoom()
    {
        Debug.Log("Join room");
        joinRoomUi.SetActive(true);
        gameObject.SetActive(false);
    }
}
