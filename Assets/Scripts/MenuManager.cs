using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DevelopersHub.RealtimeNetworking.Client;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    [SerializeField] private float _reconnectPeriod = 3f;
    [SerializeField] public TextMeshProUGUI _connectionStatus = null;
    [SerializeField] public TextMeshProUGUI _username = null;
    [SerializeField] public TextMeshProUGUI _matchmakingText = null;
    [SerializeField] public Button _matchmakingStart = null;
    [SerializeField] public Button _matchmakingStop = null;
    [SerializeField] public Button _openCharacters = null;

    private static MenuManager _singleton = null;
    public static MenuManager singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = FindFirstObjectByType<MenuManager>();
            }
            return _singleton;
        }
    }

    private void Awake()
    {
        _matchmakingStart.gameObject.SetActive(false);
        _matchmakingStop.gameObject.SetActive(false);
        _openCharacters.gameObject.SetActive(false);
        if (Application.platform == RuntimePlatform.WindowsServer || Application.platform == RuntimePlatform.LinuxServer || Application.platform == RuntimePlatform.OSXServer)
        {
            SessionManager.role = SessionManager.Role.Server;
            ServerAwake();
        }
        else
        {
            SessionManager.role = SessionManager.Role.Client;
            ClientAwake();
        }
    }

    #region Client
    private bool _startingMatch = false;

    private void ClientAwake()
    {
        _matchmakingText.text = "";
        _username.text = "";
        _matchmakingStart.onClick.AddListener(StartMatchmaking);
        _matchmakingStop.onClick.AddListener(StopMatchmaking);
        _openCharacters.onClick.AddListener(OpenCharacters);
        RealtimeNetworking.OnConnectingToServerResult += OnConnectingToServerResult;
        RealtimeNetworking.OnDisconnectedFromServer += OnDisconnected;
        RealtimeNetworking.OnAuthentication += OnAuthentication;
        RealtimeNetworking.OnStartMatchmaking += OnStartMatchmaking;
        RealtimeNetworking.OnStopMatchmaking += OnStopMatchmaking;
        RealtimeNetworking.OnNetcodeServerReady += OnNetcodeServerReady;
        Connect();
    }

    private void OpenCharacters()
    {
        CharactersManager.singleton.Open();
    }

    private void OnNetcodeServerReady(int port, Data.RuntimeGame gameData)
    {
        _startingMatch = true;
        RealtimeNetworking.Disconnect();
        SessionManager.port = (ushort)port;
        if (gameData.mapID == 0)
        {
            SceneManager.LoadScene(1);
        }
    }

    private void OnStopMatchmaking(RealtimeNetworking.StopMatchmakingResponse response)
    {
        if (response == RealtimeNetworking.StopMatchmakingResponse.SUCCESSFULL)
        {
            _matchmakingText.text = "";
            _matchmakingStart.gameObject.SetActive(true);
            _matchmakingStop.gameObject.SetActive(false);
        }
        _matchmakingStop.interactable = true;
    }

    private void OnStartMatchmaking(RealtimeNetworking.StartMatchmakingResponse response)
    {
        if (response == RealtimeNetworking.StartMatchmakingResponse.SUCCESSFULL)
        {
            _matchmakingText.text = "Searching ...";
            _matchmakingStart.gameObject.SetActive(false);
            _matchmakingStop.gameObject.SetActive(true);
        }
        _matchmakingStart.interactable = true;
    }

    private void OnAuthentication(RealtimeNetworking.AuthenticationResponse response, Data.PlayerProfile accountData = null)
    {
        if (response == RealtimeNetworking.AuthenticationResponse.SUCCESSFULL)
        {
            _matchmakingStart.gameObject.SetActive(true);
            _openCharacters.gameObject.SetActive(true);
            _username.text = accountData.username;
            SessionManager.accountID = accountData.id;
        }
        else
        {
            Debug.LogError("Failed to authenticate the player. Code: " + response);
        }
    }

    private void OnDisconnected()
    {
        _matchmakingText.text = "";
        _matchmakingStart.gameObject.SetActive(false);
        _matchmakingStop.gameObject.SetActive(false);
        SetConnectionStatus("Disconnected", Color.red);
        if (_startingMatch == false)
        {
            StartCoroutine(Reconnect());
        }
    }

    private void OnConnectingToServerResult(bool successful)
    {
        if (successful)
        {
            SetConnectionStatus("Connected", Color.green);
            RealtimeNetworking.Authenticate();
        }
        else
        {
            if (_startingMatch == false)
            {
                StartCoroutine(Reconnect());
            }
        }
    }

    private void Connect()
    {
        SetConnectionStatus("Disconnected", Color.red);
        RealtimeNetworking.Connect();
    }

    private IEnumerator Reconnect()
    {
        yield return new WaitForSeconds(_reconnectPeriod);
        Connect();
    }

    private void SetConnectionStatus(string text, Color color)
    {
        _connectionStatus.text = text;
        _connectionStatus.color = color;
    }

    private void StartMatchmaking()
    {
        _matchmakingStart.interactable = false;
        RealtimeNetworking.StartMatchmaking(0, 0, Data.Extension.NETCODE_SERVER);
    }

    private void StopMatchmaking()
    {
        _matchmakingStop.interactable = false;
        RealtimeNetworking.StopMatchmaking();
    }

    private void OnDestroy()
    {
        if (SessionManager.role == SessionManager.Role.Client)
        {
            RealtimeNetworking.OnConnectingToServerResult -= OnConnectingToServerResult;
            RealtimeNetworking.OnDisconnectedFromServer -= OnDisconnected;
            RealtimeNetworking.OnAuthentication -= OnAuthentication;
            RealtimeNetworking.OnStartMatchmaking -= OnStartMatchmaking;
            RealtimeNetworking.OnStopMatchmaking -= OnStopMatchmaking;
            RealtimeNetworking.OnNetcodeServerReady -= OnNetcodeServerReady;
        }
    }
    #endregion

    #region Server
    private void ServerAwake()
    {
        Data.RuntimeGame game = RealtimeNetworking.NetcodeGetGameData();
        if (game != null)
        {
            if (game.mapID == 0)
            {
                SceneManager.LoadScene(1);
            }
        }
        else
        {
            // Problem
            Application.Quit();
        }
    }
    #endregion

}