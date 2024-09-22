using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class MultiplayerAgent : NetworkBehaviour
{
    [SerializeField] private List<Transform> positions = new List<Transform>();
    private NavMeshAgent _agent;
    private int positionIndex = 0;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Sunucu kontrolü ekleyin, sadece sunucu bu fonksiyonu ça??rabilir
        if (IsServer && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("1");
            NextPositionServerRpc(); // Fonksiyonu burada ça??r?n
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void NextPositionServerRpc() // 'ServerRpc' ile bitmesi gerekiyor
    {
        positionIndex++;
        if (positionIndex >= positions.Count)
            positionIndex = 0;
        _agent.SetDestination(positions[positionIndex].position);
    }

    private void UpdateAnimation()
    {
        // Animasyon güncellemesi buraya eklenebilir
        // Örne?in: animator.SetBool("IsMoving", true);
    }
}
