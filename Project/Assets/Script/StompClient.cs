using System;
using System.Collections.Generic;
using System.Text;
using NativeWebSocket;
using UnityEngine;

public class StompClient : MonoBehaviour
{
    public static StompClient Instance;
    private WebSocket _ws;

    // เก็บทุก subscription ไม่หายตอน disconnect
    private Dictionary<string, (string destination, Action<string> callback)> _registered = new();

    // active subscription ที่ส่ง SUBSCRIBE frame แล้ว
    private Dictionary<string, (string destination, Action<string> callback)> _subscriptions =
        new();
    private int _subscriptionCounter = 0;

    [Header("Config")]
    public string url = "ws://127.0.0.1:8080/ws?uuid=player-001";
    public string login = "";
    public string passcode = "";
    public string virtualHost = "/";

    private int _reconnectAttempts = 0;
    private const int MaxReconnectAttempts = 5;

    public bool IsConnected { get; private set; } = false;

    public event Action OnConnected;
    public event Action<string> OnError;
    public event Action OnDisconnected;

    // ─── Lifecycle ─────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        await Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        _ws?.DispatchMessageQueue();
#endif
    }

    async void OnApplicationQuit()
    {
        await Disconnect();
    }

    // ─── Connect / Disconnect ──────────────────────────────────

    public async System.Threading.Tasks.Task Connect()
    {
        _ws = new WebSocket(url);

        _ws.OnOpen += () =>
        {
            Debug.Log("[STOMP] WebSocket opened → sending CONNECT");
            SendFrame(BuildConnect());
        };

        _ws.OnMessage += (bytes) =>
        {
            string raw = Encoding.UTF8.GetString(bytes);
            HandleFrame(raw);
        };

        _ws.OnError += (e) =>
        {
            Debug.LogError("[STOMP] WS Error: " + e);
            OnError?.Invoke(e);
        };

        _ws.OnClose += async (code) =>
        {
            Debug.Log("[STOMP] WebSocket closed: " + code);
            IsConnected = false;
            OnDisconnected?.Invoke();

            // Auto reconnect
            if (_reconnectAttempts < MaxReconnectAttempts)
            {
                _reconnectAttempts++;
                int delay = (int)(Mathf.Pow(2, _reconnectAttempts) * 1000); // 2, 4, 8, 16, 32 วินาที
                Debug.Log(
                    $"[STOMP] Reconnecting in {delay / 1000}s... (attempt {_reconnectAttempts}/{MaxReconnectAttempts})"
                );
                await System.Threading.Tasks.Task.Delay(delay);
                await Connect();
            }
            else
            {
                Debug.LogError("[STOMP] Max reconnect attempts reached.");
            }
        };

        await _ws.Connect();
    }

    public async System.Threading.Tasks.Task Disconnect()
    {
        _reconnectAttempts = MaxReconnectAttempts; // ปิด auto reconnect
        IsConnected = false;
        if (_ws != null && _ws.State == WebSocketState.Open)
        {
            SendFrame("DISCONNECT\n\n\0");
            await _ws.Close();
        }
    }

    // ─── Public API ────────────────────────────────────────────

    public string Subscribe(string destination, Action<string> callback)
    {
        string id = "sub-" + _subscriptionCounter++;
        _registered[id] = (destination, callback); // เก็บไว้เสมอ

        if (IsConnected)
        {
            _subscriptions[id] = (destination, callback);
            SendFrame(BuildSubscribe(destination, id));
            Debug.Log($"[STOMP] Subscribed → {destination} ({id})");
        }
        else
        {
            Debug.Log($"[STOMP] Queued → {destination} ({id})");
        }

        return id;
    }

    public void Unsubscribe(string subscriptionId)
    {
        _registered.Remove(subscriptionId); // เอาออกจากทั้งคู่

        if (_subscriptions.ContainsKey(subscriptionId))
        {
            _subscriptions.Remove(subscriptionId);
            if (IsConnected)
                SendFrame($"UNSUBSCRIBE\nid:{subscriptionId}\n\n\0");
            Debug.Log($"[STOMP] Unsubscribed {subscriptionId}");
        }
    }

    public void Send(string destination, string body, string contentType = "application/json")
    {
        SendFrame(BuildSend(destination, body, contentType));
    }

    // ─── Frame Builders ────────────────────────────────────────

    private string BuildConnect()
    {
        var sb = new StringBuilder();
        sb.Append("CONNECT\n");
        sb.Append("accept-version:1.2,1.1,1.0\n");
        sb.Append($"host:{virtualHost}\n");
        sb.Append("heart-beat:0,0\n");
        if (!string.IsNullOrEmpty(login))
        {
            sb.Append($"login:{login}\n");
            sb.Append($"passcode:{passcode}\n");
        }
        sb.Append("\n\0");
        return sb.ToString();
    }

    private string BuildSubscribe(string destination, string id)
    {
        var sb = new StringBuilder();
        sb.Append("SUBSCRIBE\n");
        sb.Append($"id:{id}\n");
        sb.Append($"destination:{destination}\n");
        sb.Append("ack:auto\n");
        sb.Append("\n\0");
        return sb.ToString();
    }

    private string BuildSend(string destination, string body, string contentType)
    {
        int len = Encoding.UTF8.GetByteCount(body);
        var sb = new StringBuilder();
        sb.Append("SEND\n");
        sb.Append($"destination:{destination}\n");
        sb.Append($"content-type:{contentType}\n");
        sb.Append($"content-length:{len}\n");
        sb.Append($"\n{body}\0");
        return sb.ToString();
    }

    // ─── Frame Parser ──────────────────────────────────────────

    private void HandleFrame(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || raw == "\n" || raw == "\r\n")
            return;

        raw = raw.TrimEnd('\0');

        int headerEnd = raw.IndexOf("\n\n");
        if (headerEnd < 0)
            return;

        string headerPart = raw.Substring(0, headerEnd);
        string body = raw.Substring(headerEnd + 2);

        string[] lines = headerPart.Split('\n');
        string command = lines[0].Trim();

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 1; i < lines.Length; i++)
        {
            int colon = lines[i].IndexOf(':');
            if (colon > 0)
            {
                string key = lines[i].Substring(0, colon).Trim();
                string val = lines[i].Substring(colon + 1).Trim();
                headers[key] = val;
            }
        }

        Debug.Log($"[STOMP] ← {command}");

        switch (command)
        {
            case "CONNECTED":
                HandleConnected(headers);
                break;
            case "MESSAGE":
                HandleMessage(headers, body);
                break;
            case "RECEIPT":
                HandleReceipt(headers);
                break;
            case "ERROR":
                HandleError(headers, body);
                break;
            default:
                Debug.LogWarning($"[STOMP] Unknown command: {command}");
                break;
        }
    }

    // ─── Frame Handlers ────────────────────────────────────────

    private void HandleConnected(Dictionary<string, string> headers)
    {
        string version = headers.TryGetValue("version", out var v) ? v : "unknown";
        Debug.Log($"[STOMP] Connected! STOMP version: {version}");
        IsConnected = true;
        _reconnectAttempts = 0; // reset counter

        // Re-subscribe ทุกตัวจาก _registered
        _subscriptions.Clear();
        foreach (var kvp in _registered)
        {
            _subscriptions[kvp.Key] = kvp.Value;
            SendFrame(BuildSubscribe(kvp.Value.destination, kvp.Key));
            Debug.Log($"[STOMP] Re-subscribed → {kvp.Value.destination} ({kvp.Key})");
        }

        OnConnected?.Invoke();
    }

    private void HandleMessage(Dictionary<string, string> headers, string body)
    {
        if (
            headers.TryGetValue("subscription", out string subId)
            && _subscriptions.TryGetValue(subId, out var sub)
        )
        {
            Debug.Log($"[STOMP] MESSAGE → {sub.destination}");
            sub.callback?.Invoke(body);
        }
        else
        {
            // fallback by destination
            string dest = headers.TryGetValue("destination", out var d) ? d : "";
            foreach (var kvp in _subscriptions)
            {
                if (kvp.Value.destination == dest)
                {
                    kvp.Value.callback?.Invoke(body);
                    break;
                }
            }
        }
    }

    private void HandleReceipt(Dictionary<string, string> headers)
    {
        string receiptId = headers.TryGetValue("receipt-id", out var r) ? r : "";
        Debug.Log($"[STOMP] RECEIPT: {receiptId}");
    }

    private void HandleError(Dictionary<string, string> headers, string body)
    {
        string message = headers.TryGetValue("message", out var m) ? m : "Unknown error";
        Debug.LogError($"[STOMP] ERROR: {message}\n{body}");
        OnError?.Invoke($"{message}\n{body}");
    }

    // ─── Send Helper ───────────────────────────────────────────

    private async void SendFrame(string frame)
    {
        if (_ws != null && _ws.State == WebSocketState.Open)
        {
            await _ws.SendText(frame);
        }
        else
        {
            Debug.LogWarning("[STOMP] Cannot send — WebSocket not open");
        }
    }

    // ใน StompClient เพิ่ม method debug
    public void PrintRegistered()
    {
        foreach (var kvp in _registered)
            Debug.Log($"registered: {kvp.Value.destination}");
    }
}
