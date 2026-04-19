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

    void Awake()
    {
        players = PlayerManager.Instance.players;
    }

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        listView = root.Q<ListView>();
        roomId = root.Q<Label>("RoomId");
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
    }

    void OnDisable()
    {
        RoomManager.OnRoomIdChanged -= updateUI;
        PlayerManager.OnPLayerChange -= OnReceivePlayerList;
    }

    void updateUI(string id)
    {
        roomId.text = id;
    }

    void OnReceivePlayerList(List<PlayerData> newList)
    {
        players = newList;

        listView.itemsSource = players;
        listView.Rebuild();
    }
}
