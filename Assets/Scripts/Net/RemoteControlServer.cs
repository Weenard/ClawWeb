using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.SceneManagement;
using WebSocketSharp;
using WebSocketSharp.Server;

public class RemoteControlServer : MonoBehaviour
{
    [Header("Network")]
    public int Port = 8080;

    [Header("Scene Names")]
    public string CrowdSceneName = "CrowdScene";
    public string ObstaclesSceneName = "ObstaclesScene";

    [Header("Scenario references")]
    public CrowdScenarioController Crowd;
    public ObstaclesScenarioController Obstacles;

    private WebSocketServer _wss;
    private string _activeScenario = "crowd";

    private void Awake()
    {
        MainThreadDispatcher.EnsureExists();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartServer();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[WS] Scene loaded: " + scene.name);
        ResolveSceneReferences();
    }

    private void OnDestroy()
    {
        StopServer();
    }

    // =========================
    // SCENE MANAGEMENT
    // =========================

    public void SetActiveScenario(string id)
    {
        if (id != "crowd" && id != "obstacles") return;

        _activeScenario = id;

        string sceneName = id == "crowd" ? CrowdSceneName : ObstaclesSceneName;

        Debug.Log($"[WS] Loading scene: {sceneName}");

        SceneManager.LoadScene(sceneName);

        BroadcastState();
    }

    private void ResolveSceneReferences()
    {
        Crowd = FindObjectOfType<CrowdScenarioController>();
        Obstacles = FindObjectOfType<ObstaclesScenarioController>();

        Debug.Log("[WS] Scene refs updated: " +
            $"Crowd={(Crowd != null)}, Obstacles={(Obstacles != null)}");

        BroadcastState();
    }

    // =========================
    // SERVER
    // =========================

    private void StartServer()
    {
        try
        {
            _wss = new WebSocketServer(System.Net.IPAddress.Any, Port);
            _wss.AddWebSocketService<TabletWsBehavior>("/", b => b.Init(this));
            _wss.Start();

            Debug.Log($"[WS] Server started ws://0.0.0.0:{Port}/");
        }
        catch (Exception ex)
        {
            Debug.LogError("[WS] Failed to start: " + ex);
        }
    }

    private void StopServer()
    {
        try
        {
            _wss?.Stop();
            _wss = null;
        }
        catch { }
    }

    // =========================
    // STATE
    // =========================

    private void BroadcastState()
    {
        if (_wss == null) return;

        var st = new JObject
        {
            ["type"] = "state",
            ["payload"] = new JObject
            {
                ["scenario"] = _activeScenario,
                ["crowdDensity"] = Crowd ? Crowd.CrowdDensity01 : 0,
                ["obstacleCount"] = Obstacles ? Obstacles.ObstacleCount : 0,
                ["sceneLoaded"] = Crowd != null || Obstacles != null
            }
        };

        foreach (var path in _wss.WebSocketServices.Paths)
        {
            var host = _wss.WebSocketServices[path];
            host.Sessions.Broadcast(st.ToString());
        }
    }

    private void SendRanges(WebSocketSessionManager sessions)
    {
        var msg = new JObject
        {
            ["type"] = "ranges",
            ["payload"] = new JObject
            {
                ["maxObstacles"] = Obstacles ? Obstacles.MaxObstacles : 5,
                ["obstacles"] = new JObject
                {
                    ["min"] = Obstacles ? new JObject { ["x"] = Obstacles.Min.x, ["y"] = Obstacles.Min.y, ["z"] = Obstacles.Min.z } : new JObject(),
                    ["max"] = Obstacles ? new JObject { ["x"] = Obstacles.Max.x, ["y"] = Obstacles.Max.y, ["z"] = Obstacles.Max.z } : new JObject(),
                    ["step"] = Obstacles ? Obstacles.Step : 0.1f
                }
            }
        };

        sessions.Broadcast(msg.ToString());
    }

    public void OnClientOpened(WebSocketSessionManager sessions)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            sessions.Broadcast(new JObject
            {
                ["type"] = "hello",
                ["payload"] = new JObject { ["server"] = "unity", ["version"] = "1.0" }
            }.ToString());

            SendRanges(sessions);
            BroadcastState();
        });
    }

    // =========================
    // MESSAGE HANDLING
    // =========================

    private void HandleMessage(WebSocketSessionManager sessions, string raw)
    {
        JObject obj;

        try { obj = JObject.Parse(raw); }
        catch
        {
            sessions.Broadcast(new JObject
            {
                ["type"] = "error",
                ["payload"] = new JObject { ["message"] = "Bad JSON" }
            }.ToString());
            return;
        }

        var type = (string)obj["type"];
        var payload = obj["payload"] as JObject ?? new JObject();

        MainThreadDispatcher.Enqueue(() =>
        {
            switch (type)
            {
                case "hello":
                    {
                        sessions.Broadcast(new JObject
                        {
                            ["type"] = "hello",
                            ["payload"] = new JObject { ["server"] = "unity", ["version"] = "1.0" }
                        }.ToString());

                        SendRanges(sessions);
                        BroadcastState();
                        break;
                    }

                case "setScenario":
                    {
                        Debug.Log("SET SCENARIO CALLED");

                        var id = (string)payload["id"];
                        Debug.Log("ID = " + id);

                        SetActiveScenario(id);
                        break;
                    }

                case "setCrowdDensity":
                    {
                        if (_activeScenario != "crowd" || Crowd == null) break;

                        float d = payload["density"] != null ? (float)payload["density"] : 0f;
                        Crowd.SetCrowdDensity(d);

                        BroadcastState();
                        break;
                    }

                case "addObstacle":
                    {
                        if (_activeScenario != "obstacles" || Obstacles == null) break;

                        float x = payload["x"] != null ? (float)payload["x"] : 0;
                        float y = payload["y"] != null ? (float)payload["y"] : 0;
                        float z = payload["z"] != null ? (float)payload["z"] : 0;

                        if (!Obstacles.TryAddObstacle(new Vector3(x, y, z), out var err))
                        {
                            sessions.Broadcast(new JObject
                            {
                                ["type"] = "error",
                                ["payload"] = new JObject { ["message"] = err }
                            }.ToString());
                        }

                        BroadcastState();
                        break;
                    }

                case "clearObstacles":
                    {
                        if (_activeScenario != "obstacles") break;

                        Obstacles?.ClearObstacles();
                        BroadcastState();
                        break;
                    }

                case "resetScenario":
                    {
                        if (_activeScenario == "crowd") Crowd?.ResetScenario();
                        else if (_activeScenario == "obstacles") Obstacles?.ResetScenario();

                        BroadcastState();
                        break;
                    }

                case "resetAll":
                    {
                        Crowd?.ResetScenario();
                        Obstacles?.ResetScenario();

                        BroadcastState();
                        break;
                    }

                case "ping":
                    {
                        sessions.Broadcast(new JObject
                        {
                            ["type"] = "pong",
                            ["payload"] = new JObject { ["t"] = payload["t"] ?? 0 }
                        }.ToString());
                        break;
                    }
            }
        });
    }

    // =========================
    // WS BEHAVIOR
    // =========================

    private class TabletWsBehavior : WebSocketBehavior
    {
        private RemoteControlServer _server;

        public TabletWsBehavior() { }

        public void Init(RemoteControlServer server) => _server = server;

        protected override void OnOpen()
        {
            _server?.OnClientOpened(Sessions);
        }

        protected override void OnMessage(WebSocketSharp.MessageEventArgs e)
        {
            _server?.HandleMessage(Sessions, e.Data);
        }
    }
}