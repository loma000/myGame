using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpawnSelector : MonoBehaviour
{
    public string SpawnName = "";

    Camera cam;

    [SerializeField]
    private Button FinishButton;

    [SerializeField]
    private Text finishText;

    [SerializeField]
    private CharacterCardList _characterCardList;
    Dictionary<string, Spawn> Spawns = new Dictionary<string, Spawn>();

    public static Action<string> SpawnCharacter;
    public List<string> CharacterName = new List<string> { "Loma", "Kota" };
    public static SpawnSelector Instance;
    private StompClient stompClient;

    private bool isFinish = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Instance = this;
        SpawnCharacter += setSpawnName;
        stompClient = StompClient.Instance;
    }

    void Start()
    {
        cam = Camera.main;
        FinishButton.onClick.AddListener(finishSetupSpawn);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!despawn())
                SpawnVisual();

            FinishButton.interactable = Spawns.Count >= 1;
        }
    }

    void setSpawnName(string name)
    {
        if (!Spawns.ContainsKey(name))
            SpawnName = name;
    }

    void SpawnVisual()
    {
        if (isFinish)
            return;
        var grid = GridManager.getGrid(cam);
        if (grid == null || SpawnName.Equals(""))
            return;

        GameObject model = Resources.Load<GameObject>($"Characters/{SpawnName}");
        if (model != null)
        {
            var obj = Instantiate(model, grid.transform);
            obj.name = SpawnName;
            Spawns.Add(
                SpawnName,
                new Spawn
                {
                    name = SpawnName,
                    row = grid.row,
                    col = grid.col,
                }
            );

            SpawnName = "";
        }
        else
            Debug.LogWarning($"Model not found: {SpawnName}");
    }

    bool despawn()
    {
        if (isFinish)
            return false;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        // int layerMask = LayerMask.GetMask("CharacterSelect");
        if (
            Physics.Raycast(ray, out hit, Mathf.Infinity)
            && hit.collider.CompareTag("CharacterModel")
        )
        {
            GameObject selectModel = hit.collider.gameObject;
            Spawns.Remove(selectModel.name);
            Destroy(selectModel);
            return true;
        }
        return false;
    }

    void finishSetupSpawn()
    {
        SpawnSetupData data;
        if (!isFinish)
        {
            if (Spawns.Values.ToList().Count == 0)
                return;
            data = new SpawnSetupData
            {
                playerId = PlayerManager.Instance.player.id,
                setUpSpawns = Spawns.Values.ToList(),
            };
            finishText.text = "ready";
        }
        else
        {
            data = new SpawnSetupData
            {
                playerId = PlayerManager.Instance.player.id,
                setUpSpawns = new List<Spawn> { },
            };
            finishText.text = "Finish";
        }
        Debug.Log(data.setUpSpawns.Count);
        stompClient.Send(
            "/app/game/FinishSpawn/" + RoomManager.Instance.roomId,
            JsonUtility.ToJson(data)
        );
        isFinish = !isFinish;
    }
}

[System.Serializable]
class Spawn
{
    public string name;
    public int row;
    public int col;
}

[System.Serializable]
class SpawnSetupData
{
    public string playerId;
    public List<Spawn> setUpSpawns;
}
