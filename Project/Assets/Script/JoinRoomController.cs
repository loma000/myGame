using UnityEngine;
using UnityEngine.UIElements;

public class JoinRoomController : MonoBehaviour
{
    public VisualElement ui;
    TextField IdField;
    Button JoinButton;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        IdField = ui.Q<TextField>("RoomId");
        JoinButton = ui.Q<Button>("Join");
    }

    void OnEnable()
    {
        JoinButton.clicked += JoinRoom;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update() { }

    void JoinRoom()
    {
        Debug.Log("JoinRoom:" + IdField.value);
        RoomManager.Instance.joinRoom(IdField.value);
    }
}
