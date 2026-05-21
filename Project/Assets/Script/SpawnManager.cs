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
            "/topic/fetchCharacter/"
                + RoomManager.Instance.roomId
                + "/"
                + PlayerManager.Instance.player.id,
            onFetchMinion
        );
        stompClient.Send(
            "/app/game/getCharacter/fetch/"
                + RoomManager.Instance.roomId
                + "/"
                + PlayerManager.Instance.player.id,
            ""
        );
    }

    public Character GetCharacter(string Id)
    {
        return spawnedCharacters.Find((c) => c.Id.Equals(Id));
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
            GameObject character = Instantiate(playerObj);

            var characterVisual = character.GetComponent<CharacterVisual>();
            characterVisual.SetModel(c.name);
            Character characterData = character.GetComponent<Character>();
            characterData.InstanceCharacter(
                c.OwnerId.Equals(PlayerManager.Instance.player.id),
                c.name,
                c.Id,
                c.OwnerId,
                c.row,
                c.col,
                c.Atk,
                c.maxHp,
                c.Hp
            );

            spawnedCharacters.Add(characterData);
        }
    }
}
