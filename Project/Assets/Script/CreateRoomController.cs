using UnityEngine;
using UnityEngine.UIElements;

public class CreateRoomController : MonoBehaviour
{
    private VisualElement ui;
    private Label roomId;

    [SerializeField]
    private StompClient stompClient;

    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        roomId = ui.Q<Label>("RoomId");
    }

    void OnEnable()
    {
        RoomManager.OnRoomIdChanged += UpdateUI;
    }

    void OnDisable()
    {
        RoomManager.OnRoomIdChanged -= UpdateUI;
    }

    void UpdateUI(string id)
    {
        roomId.text = id;
    }
}
