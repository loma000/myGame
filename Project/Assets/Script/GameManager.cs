using System;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static Action OnGameStart;
    public static Action OnWaitingPlayer;
    public static Action OnMoving;
    public static Action OnAttacking;
    public static Action FinishAttacking;
    public static Action OnNextTurn;
    public static Action OnEndGame;
    public ActionMode actionMode;
    private StompClient stompClient;

    public static Action<string> OnSetFinishButton;
    public string currentTurnPlayer = "";
    public string startGameId;
    private string getCurrentPlayerId;
    private string getWinnerId;

    private string winner = "";

    void Awake()
    {
        Instance = this;
        stompClient = StompClient.Instance;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        OnWaitingPlayer += WaitingPLayer;
        FinishAttacking += GetWinner;
    }

    public void WaitingPLayer()
    {
        if (startGameId != null)
        {
            stompClient.Unsubscribe(startGameId);
            startGameId = null;
        }

        startGameId = stompClient.Subscribe(
            "/topic/gameState/" + RoomManager.Instance.roomId,
            OnGameState
        );
        if (getCurrentPlayerId != null)
        {
            stompClient.Unsubscribe(getCurrentPlayerId);
            getCurrentPlayerId = null;
        }
        getCurrentPlayerId = stompClient.Subscribe(
            "/topic/getcurrent/player/" + RoomManager.Instance.roomId,
            setCurrentPlayer
        );

        if (getWinnerId != null)
        {
            stompClient.Unsubscribe(getWinnerId);
            getWinnerId = null;
        }
        getWinnerId = stompClient.Subscribe(
            "/topic/winner/" + RoomManager.Instance.roomId,
            (string body) =>
            {
                winner = body;
            }
        );
    }

    public void GetWinner()
    {Debug.Log("get winner");
        stompClient.Send("/app/game/checkendgame/" + RoomManager.Instance.roomId, "");
    }

    public void OnGameState(string body)
    {
        Debug.Log("OnGameState: " + body);
        Debug.Log("OnNextTurn subscribers: " + OnNextTurn?.GetInvocationList().Length);
        switch (body)
        {
            case "Spawn":
                LobbyUIManager.Instance.SpawnPhase();
                break;
            case "Start":
                SceneLoader.Instance.LoadGame();

                break;
            case "NextTurn":
                Debug.Log("aa");
                OnNextTurn?.Invoke();
                break;
            case "EndGame":
                OnEndGame?.Invoke();
                break;
            default:
                break;
        }
    }

    public void setCurrentPlayer(string body)
    {
        Debug.Log(body);
        currentTurnPlayer = body;
        OnSetFinishButton?.Invoke(body);
    }
}
