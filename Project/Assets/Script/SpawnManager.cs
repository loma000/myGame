using System.Collections.Generic;
using System.Data.Common;
using Newtonsoft.Json;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
    public GameObject playerObj;
    private List<Character> spawnedCharacters = new List<Character>();

    [SerializeField]
    private List<CharacterData> characters = new List<CharacterData>();
    private StompClient stompClient;

    private string fetchCharacter;

    void Awake()
    {
        stompClient = StompClient.Instance;
        Instance = this;
        GameManager.OnGameStart += OnConnect;
    }

    void OnConnect()
    {
        if (fetchCharacter != null)
        {
            stompClient.Unsubscribe(fetchCharacter);
            fetchCharacter = null;
        }
        stompClient.Subscribe(
            "/topic/fetchCharacter/" + RoomManager.Instance.roomId,
            onFetchMinion
        );
        stompClient.Send("/app/game/getCharacter/fetch/" + RoomManager.Instance.roomId, "");
    }

    void onFetchMinion(string body)
    {
        Debug.Log(body);
        var data = JsonConvert.DeserializeObject<List<CharacterData>>(body);
        characters = data;
        SpawnGlobalPlayer();
    }

    public void SpawnGlobalPlayer()
    {
        foreach (var c in spawnedCharacters)
        {
            if (c != null)
                Destroy(c.gameObject);
        }
        spawnedCharacters.Clear();

        foreach (var c in characters)
        {
            Character character = Instantiate(playerObj).GetComponent<Character>();
            character.isLocal = c.OwnerId.Equals(PlayerManager.Instance.player.id);
            character.Data = c;

            spawnedCharacters.Add(character);
        }
    }
}
