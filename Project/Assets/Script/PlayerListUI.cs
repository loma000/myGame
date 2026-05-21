using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerListUI : MonoBehaviour
{
    public VisualTreeAsset itemTemplate;

    private ListView listView;
    private Label roomId;
    private List<PlayerData> players;
    private StompClient stompClient;
    private Button StartButton;

    void Awake()
    {
        stompClient = StompClient.Instance;
        players = PlayerManager.Instance.players;

        var root = GetComponent<UIDocument>().rootVisualElement;

        listView = root.Q<ListView>();
        roomId = root.Q<Label>("RoomId");
        StartButton = root.Q<Button>("Start");
    }

    void Start()
    {
        listView.itemsSource = players;

        listView.makeItem = () =>
        {
            return itemTemplate.CloneTree();
        };

        listView.bindItem = (element, index) =>
        {
            var p = players[index];

            element.Q<Label>("name").text = p.name;
            element.Q<Label>("id").text = p.id;
        };

        listView.fixedItemHeight = 50;
    }

    void OnEnable()
    {
        RoomManager.OnRoomIdChanged += updateUI;
        PlayerManager.OnPLayerChange += OnReceivePlayerList;
        StartButton.text = PlayerManager.Instance.player.isHost ? "Start" : "wating for host...";
        StartButton.SetEnabled(
            players.Count >= RoomManager.Instance.maxPlayers && PlayerManager.Instance.player.isHost
        );
        StartButton.clicked += OnSendStartGame;
    }

    void OnDisable()
    {
        RoomManager.OnRoomIdChanged -= updateUI;
        PlayerManager.OnPLayerChange -= OnReceivePlayerList;
        StartButton.clicked -= OnSendStartGame;
    }

    void updateUI(string id)
    {
        Debug.Log(id);
        roomId.text = id;
    }

    void OnReceivePlayerList(List<PlayerData> newList)
    {
        players = newList;

        listView.itemsSource = players;
        listView.Rebuild();
        Debug.Log("isHost: " + PlayerManager.Instance.player.isHost);
        StartButton.text = PlayerManager.Instance.player.isHost ? "Start" : "wating for host...";
        StartButton.SetEnabled(
            players.Count >= RoomManager.Instance.maxPlayers && PlayerManager.Instance.player.isHost
        );
    }

    void OnSendStartGame()
    {
        stompClient.Send("/app/game/startspawn/" + RoomManager.Instance.roomId, "");
    }
}
