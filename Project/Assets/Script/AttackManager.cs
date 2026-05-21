using System;
using Newtonsoft.Json;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public static AttackManager Instance;
    string AttackId = null;
    private StompClient stompClient;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        stompClient = StompClient.Instance;
        GameManager.OnGameStart += OnAttackSubscribe;
    }

    // Update is called once per frame
    void OnAttackSubscribe()
    {
        if (AttackId != null)
        {
            stompClient.Unsubscribe(AttackId);
            AttackId = null;
        }
        AttackId = stompClient.Subscribe(
            "/topic/game/attack/" + RoomManager.Instance.roomId,
            OnAttack
        );
    }

    void OnAttack(string body)
    {
        var data = JsonConvert.DeserializeObject<AttackData>(body);
        Character attacker = SpawnManager.Instance.GetCharacter(data.attacker);
        Character target = SpawnManager.Instance.GetCharacter(data.target);
        Debug.Log(target.Name + "takes " + data.damage + " damage");
        StartCoroutine(attacker._characterVisual.PlayAttackAndWait(target, data.targetHp));
    }

    void Update()
    {
        Attacking();
    }

    void Attacking()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (GameManager.Instance.actionMode == ActionMode.Attack)
            {
                Character target = CharacterSelector.Instance.selector();

                if (target == null)
                {
                    Debug.Log("no target");
                    return;
                }
                Debug.Log("Found target");
                var data = new AttackDto
                {
                    type = "Attack",
                    attacker = CharacterSelector.Instance.selectCharacter.Id,
                    target = target.Id,
                };
                stompClient.Send(
                    "/app/game/attack/"
                        + RoomManager.Instance.roomId
                        + "/"
                        + PlayerManager.Instance.player.id,
                    JsonUtility.ToJson(data)
                );
            }
        }
    }
}

[System.Serializable]
public class AttackData
{
    public string type;
    public string attacker;
    public string target;
    public float damage;
    public float targetHp;
}

[System.Serializable]
public class AttackDto
{
    public string type;
    public string attacker;
    public string target;
}
