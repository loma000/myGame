using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;
    public string roomId;
    public static Action<string> OnRoomIdChanged;
    public int maxPlayers = 2;

    void Awake()
    {
        Instance = this;
    }

    public void createdRoom()
    {
        StartCoroutine(CreateRoomRequest());
    }

    public void joinRoom(string roomId)
    {
        StartCoroutine(JoinRoomRequest(roomId));
    }

    IEnumerator CreateRoomRequest()
    {
        UnityWebRequest req = UnityWebRequest.Get(
            "http://localhost:8080/createRoom/"
                + PlayerManager.Instance.player.id
                + "/"
                + maxPlayers
        );
        yield return req.SendWebRequest();
        var data = JsonUtility.FromJson<RoomData>(req.downloadHandler.text);

        Connect(data);
    }

    IEnumerator JoinRoomRequest(string roomId)
    {
        UnityWebRequest req = UnityWebRequest.Get(
            "http://localhost:8080/joinRoom/" + roomId + "/" + PlayerManager.Instance.player.id
        );
        yield return req.SendWebRequest();
        var data = JsonUtility.FromJson<RoomData>(req.downloadHandler.text);
        if (data == null)
        {
            Debug.Log("room not found");
        }
        else
            Connect(data);
    }

    void Connect(RoomData res)
    {
        roomId = res.Id;
        Debug.Log(roomId);
        PlayerManager.Instance.players = res.players;
        OnRoomIdChanged?.Invoke(roomId);
    }
}

[System.Serializable]
public class RoomData
{
    public string Id;
    public List<PlayerData> players;

    public int maxPlayers;
}

[System.Serializable]
public class PlayerData
{
    public string name;
    public string id;
}
