using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.AspNet.SignalR.Client;
using UnityEngine.Networking;
using NotSoSimpleJSON;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Reflection;
using System;
 
public class GameHub : MonoBehaviour
{
    public static GameHub Instance { get; private set; }

    public GameObject GameWorldPrefab;

    private GameWorld gameWorld;

    void Awake()
    {
        if (Instance != null)
            Destroy(Instance);

        Instance = this;
    }

    struct AwaitingEvent
    {
        public object target;
        public MethodInfo method;
        public object[] args;

        public bool TryInvoke()
        {
            var res = (bool)method.Invoke(target, args);
            return res;
        }
    }

    List<AwaitingEvent> eventQueue = new List<AwaitingEvent>();

    void Update()
    {
        eventQueue.RemoveAll(e => e.TryInvoke());
    }

    void OnDestroy()
    {
        if (Connection != null)
            Connection.Stop();
    }

    public void OnConnectedRaw(string playerData)
    {
        OnConnected(JSON.Parse(playerData));
    }

    public void OnConnected(JSONNode playerData)
    {
        Connecting.transform.parent.parent.gameObject.SetActive(false);

        gameWorld = Instantiate(GameWorldPrefab).GetComponent<GameWorld>();
        gameWorld.OnConnected(playerData.AsObject);
    }












    public void InvokeMethod(string jsonData)
    {
        Debug.Log("InvokeMethod: " + jsonData);
        var args = JSON.Parse(jsonData);
        MethodInvoked(args[0].AsString, args.AsArray.Skip(1).ToArray());
    }

    public void MethodInvoked(string target, JSONNode[] args)
    {
        object origin = gameWorld; 

        var path = target.Split('.');
        for (int i = 0; i < path.Length - 1; i++)
        {
            var index = path[i].Split('#');
            var prop = origin.GetType().GetProperty(index[0]);

            if (index.Length == 1)
                origin = prop.GetValue(origin);
            else
            {
                var dict = prop.GetValue(origin) as IDictionary;
                origin = dict[index[1]];
            }
        }

        var func = origin.GetType().GetMethod(path[path.Length - 1]);
        var argsTyped = args.Zip(func.GetParameters(), (a, b) => b.ParameterType == typeof(string) ? a.AsString : b.ParameterType == typeof(bool) ? a.AsBool : b.ParameterType == typeof(int) ? a.AsInt : b.ParameterType == typeof(JSONArray) ? a.AsArray : b.ParameterType == typeof(Vector2Int) ? (object)a.ToVecI() : a.AsObject).ToArray();

        if (func.ReturnType == typeof(bool))
        {
            eventQueue.Add(new AwaitingEvent
            {
                target = origin,
                method = func,
                args = argsTyped
            });
        }
        else if (func.ReturnType == typeof(IEnumerator))
            StartCoroutine(func.Invoke(origin, argsTyped) as IEnumerator);
        else
            func.Invoke(origin, argsTyped);

    }


#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern void WebGL_Invoke(string target, string values);
#endif

    static public void Invoke(string target, params object[] values)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("serialized invoke = " + JSON.FromData(values).Serialize());
        WebGL_Invoke(target, JSON.FromData(values).Serialize());
#else
        Connection.Invoke(target, values);
#endif
    }










    const string BaseUrl = "://localhost:33322/";
    const string GameServerUrl = BaseUrl + "gameserver";
    const string ApiBaseUrl = "http" + BaseUrl + "api/";

    static public string DisplayName { get; private set; } = "";
    static string authToken;

    static HubConnection<GameHub> Connection;
    static IHubProxy HubProxy;

    public async Task TryLogin(string email, string password)
    {
        Debug.Log("Logging in as " + email);
        Console.WriteLine("Console.WriteLine()...");

        var authBody = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\"}";
        var req = UnityWebRequest.Post(ApiBaseUrl + "user/auth", authBody);
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(authBody));
        req.SetRequestHeader("user-agent", "X-Unity-Agent");
        req.SetRequestHeader("Content-Type", "application/json");

        await req.SendAsync(CancellationToken.None);

        if (req.isNetworkError || req.isHttpError)
        {
            Debug.Log(req.error);
            return;
        }

        var data = JSON.Parse(req.downloadHandler.text).AsObject;

        DisplayName = data["displayName"].AsString;
        authToken = data["auth_token"].AsString;

        Debug.Log("received data: " + req.downloadHandler.text);
        Debug.Log("auth_token=" + authToken);

        await ConnectToHub();

    }

    public async Task ConnectToHub()
    {
        Connection = new HubConnection<GameHub>(Instance, "http" + GameServerUrl, false);
        Connection.TraceLevel = TraceLevels.All;
        Connection.TraceWriter = new UnityConsoleTextWriter();

        Connection.Headers.Add("user-agent", "X-Unity-Agent");
        Connection.Headers.Add("Content-Type", "text/plain;charset=UTF-8");
        Connection.Headers.Add("Authorization", "Bearer " + authToken);

        HubProxy = Connection.CreateHubProxy("gameserver");

        Connection.Error += Connection_Error;

        await Connection.Start();
        
        //Debug.Log("Connected");
    }

    private void Connection_Error(Exception obj)
    {
        MainThreadDispatcher.Instance.Enqueue(() => {
            ConnFailed.SetActive(true);
            Reconnect.SetActive(true);
            Connecting.SetActive(false);
        });
    }

    public GameObject Connecting;
    public GameObject ConnFailed;
    public GameObject Reconnect;


#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern void WebGL_Start();

    [DllImport("__Internal")]
    public static extern string WebGL_GetAuthToken();
    
    void Start()
    {
        Connecting.transform.parent.parent.Find("User1").gameObject.SetActive(false);
        Connecting.transform.parent.parent.Find("User2").gameObject.SetActive(false);
        ConnFailed.SetActive(false);
        Reconnect.SetActive(false);
    
        WebGL_Start();
        
    }
#else
    void Start()
    {
        ConnFailed.SetActive(false);
        Reconnect.SetActive(false);
        Connecting.SetActive(false);


    }
#endif

    bool isConnecting = false;
    public async void Connect1()
    {
        if (isConnecting)
            return;
        isConnecting = true;

        Connecting.SetActive(true);
        await TryLogin("e@e.ru", "qwe");
    }

    public async void Connect2()
    {
        if (isConnecting)
            return;
        isConnecting = true;

        Connecting.SetActive(true);
        await TryLogin("x@x.ru", "qwe");
    }
}