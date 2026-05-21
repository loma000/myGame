using UnityEngine;

public class GameUiManager : MonoBehaviour
{
    [SerializeField]
    private GameObject GameUi;

    [SerializeField]
    private GameObject EndGameUi;

    void Awake()
    {
        GameManager.OnGameStart += StartGame;
        GameManager.OnEndGame += EndGame;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update() { }

    void StartGame()
    {
        GameUi.SetActive(true);
        EndGameUi.SetActive(false);
    }

    void EndGame()
    {
        GameUi.SetActive(false);
        EndGameUi.SetActive(true);
    }
}
