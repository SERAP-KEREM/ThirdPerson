using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private Button _serverButton = null;
    [SerializeField] private Button _clientButton = null;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _serverButton.onClick.AddListener(StartServer);
        _clientButton.onClick.AddListener(StartClient);
    }

    private void StartServer()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _serverButton.gameObject.SetActive(false);
        _clientButton.gameObject.SetActive(false);
        SessionManager.singleton.StartServer();
    }

    private void StartClient()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _serverButton.gameObject.SetActive(false);
        _clientButton.gameObject.SetActive(false);
        SessionManager.singleton.StartClient();
    }
    void Update()
    {
        // Eğer ESC tuşuna basılırsa, mouse tekrar görünür hale gelir.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


}
