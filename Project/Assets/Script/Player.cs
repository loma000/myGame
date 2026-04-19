using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string Id;
    public Vector3 position;
    private Vector3 lastPos;
    string moveId;
    public Camera cam;
    public bool isLocal;
    float interval = 0;
    StompClient stompClient;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        stompClient = StompClient.Instance;
    }
}
