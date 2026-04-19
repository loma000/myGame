using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public PlayerData player;
    public List<PlayerData> players;
    public static Action<List<PlayerData>> OnPLayerChange;
    private StompClient stompClient;
    private string fetchPlayerId;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        Instance = this;
        stompClient = StompClient.Instance;
    }

    void Start()
    {
        CreatePlayer();
    }

    public void OnFetchPlayerConnect(string roomId)
    {
        Debug.Log("[DEBUG] OnFetchPlayerConnect called"); // ขึ้นกี่ครั้ง?

        if (fetchPlayerId != null)
            stompClient.Unsubscribe(fetchPlayerId);
        fetchPlayerId = stompClient.Subscribe("/topic/fetchPlayer/" + roomId, OnFetchPlayer);
    }

    void OnFetchPlayer(string body)
    {
        Debug.Log(body);
        players = JsonConvert.DeserializeObject<List<PlayerData>>(body);
        OnPLayerChange?.Invoke(players);
        UIManager.Instance.CloseUIandOpenLobby();
        Debug.Log(players.Count);
        if (players.Count >= RoomManager.Instance.maxPlayers)
        {
            GameManager.OnGameStart?.Invoke();
            UIManager.Instance.CloseLobby();
           
        }
    }

    public void CreatePlayer()
    {
        StartCoroutine(CreatePlayerRequest());
    }

    IEnumerator CreatePlayerRequest()
    {
        UnityWebRequest req = UnityWebRequest.Get("http://localhost:8080/create/player/" + "gay");
        yield return req.SendWebRequest();
        var data = JsonUtility.FromJson<PlayerData>(req.downloadHandler.text);
        player = data;
    }

    // Update is called once per frame
    void Update() { }
}
