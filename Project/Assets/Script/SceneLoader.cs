using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    void Awake()
    {
        Instance = this;
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update() { }

   public void LoadGame()
    {
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync("GameScene");

        while (!op.isDone)
        {
            Debug.Log(op.progress);
            yield return null;
        }
        GameManager.OnGameStart?.Invoke();
         StompClient.Instance.PrintRegistered();
    }
}
