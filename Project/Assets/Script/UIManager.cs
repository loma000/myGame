using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject RoomModeUI;
    public GameObject CreateRoomUI;
    public GameObject JoinRoomUI;
    public GameObject waitingLobby;
    private string testId;

    public StompClient stompClient = StompClient.Instance;

    void Awake()
    {
        Instance = this;
        stompClient = StompClient.Instance;
    }

    void Start()
    {
        stompClient.OnConnected += OnConnect;
        stompClient.OnDisconnected += OnDisconnect;
        RoomManager.OnRoomIdChanged += OnFetch;
        if (stompClient.IsConnected)
            OnConnect();
    }

    void OnDisable()
    {
        stompClient.OnConnected -= OnConnect;
        stompClient.OnDisconnected -= OnDisconnect;
    }

    void OnConnect() { }

    public void OnFetch(string roomId)
    {
        PlayerManager.Instance.OnFetchPlayerConnect(roomId);
        stompClient.Send("/app/update/fetch/players/" + RoomManager.Instance.roomId, "");
    }

    void OnDisconnect()
    {
        stompClient.Unsubscribe(testId);
    }

    public void CloseUIandOpenLobby()
    {
        waitingLobby.SetActive(true);
        JoinRoomUI.SetActive(false);
    }

    public void CloseLobby()
    {
        waitingLobby.SetActive(false);
    }

    // Update is called once per frame
    void Update() { }
}
