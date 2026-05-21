using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    private StompClient stompClient;

    [SerializeField]
    private Button endTurnButton;

    [SerializeField]
    private Text AP;
    private string updateAPId;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        stompClient = StompClient.Instance;
        GameManager.OnNextTurn += OnNextTurn;
        GameManager.OnSetFinishButton += setFinishInteractable;
    }

    void Start()
    {
        if (updateAPId != null)
        {
            stompClient.Unsubscribe(updateAPId);
            updateAPId = null;
        }
        updateAPId = stompClient.Subscribe(
            "/topic/game/updateAP/" + PlayerManager.Instance.player.id,
            setAP
        );

        endTurnButton.onClick.AddListener(() =>
        {
            stompClient.Send("/app/game/endturn/" + RoomManager.Instance.roomId, "");
        });

        OnNextTurn();
    }

    void OnNextTurn()
    {
        stompClient.Send("/app/game/player/getcurrent/" + RoomManager.Instance.roomId, "");

        Debug.Log(PlayerManager.Instance.player.id);
        Debug.Log(GameManager.Instance.currentTurnPlayer);
        stompClient.Send(
            "/app/game/update/actionpoint/"
                + RoomManager.Instance.roomId
                + "/"
                + PlayerManager.Instance.player.id,
            ""
        );
    }

    void setFinishInteractable(string currentPlayerId)
    {
        if (PlayerManager.Instance.player.id.Equals(currentPlayerId))
        {
            endTurnButton.interactable = true;
        }
        else
        {
            endTurnButton.interactable = false;
        }
    }

    void setAP(string body)
    {
        Debug.Log(body);
        var data = JsonUtility.FromJson<APdata>(body);
        AP.text = "AP : " + data.AP.ToString();
    }

    // Update is called once per frame
    void Update() { }
}

[System.Serializable]
class APdata
{
    public int AP;
}
