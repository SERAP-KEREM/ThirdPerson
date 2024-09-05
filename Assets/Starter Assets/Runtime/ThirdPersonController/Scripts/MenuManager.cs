using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DevelopersHub.RealtimeNetworking.Client;

public class MenuManager : MonoBehaviour
{

    [SerializeField] private float _reconnectPeriod = 3f;
    [SerializeField] public TextMeshProUGUI _connectionStatus = null;
    [SerializeField] public TextMeshProUGUI _username = null;
    [SerializeField] public TextMeshProUGUI _matchmakingText = null;
    [SerializeField] public Button _matchmakingStart = null;
    [SerializeField] public Button _matchmakingStop = null;

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
    private void ClientAwake()
    {
        _matchmakingText.text = "";
        _username.text = "";
        _matchmakingStart.onClick.AddListener(StartMatchmaking);
        _matchmakingStop.onClick.AddListener(StopMatchmaking);
        RealtimeNetworking.OnConnectingToServerResult += OnConnectingToServerResult;
        RealtimeNetworking.OnDisconnectedFromServer += OnDisconnected;
        RealtimeNetworking.OnAuthentication += OnAuthentication;
        Connect();
    }

    private void OnAuthentication(RealtimeNetworking.AuthenticationResponse response, Data.PlayerProfile accountData = null)
    {
        if (response == RealtimeNetworking.AuthenticationResponse.SUCCESSFULL)
        {
            _matchmakingStart.gameObject.SetActive(true);
            _username.text = accountData.username;
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
        StartCoroutine(Reconnect());
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
            StartCoroutine(Reconnect());
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

    }

    private void StopMatchmaking()
    {

    }

    private void OnDestroy()
    {
        if (SessionManager.role == SessionManager.Role.Client)
        {
            RealtimeNetworking.OnConnectingToServerResult -= OnConnectingToServerResult;
            RealtimeNetworking.OnDisconnectedFromServer -= OnDisconnected;
            RealtimeNetworking.OnAuthentication -= OnAuthentication;
        }
    }
    #endregion

    #region Server
    private void ServerAwake()
    {

    }
    #endregion

}