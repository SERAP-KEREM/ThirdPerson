using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.VisualScripting;

public class CameraManager : MonoBehaviour
{


    [SerializeField][Range(-5f, 5f)] private float _defaultSensitivity = 1.5f; public static float defaultSensitivity { get { return singleton._defaultSensitivity; } }
    [SerializeField][Range(-5f, 5f)] private float _aimingSensitivity = 0.5f; public static float aimingSensitivity { get { return singleton._aimingSensitivity; } }

    [SerializeField] private Camera _camera = null; public static Camera mainCamera { get { return singleton._camera; } }
    [SerializeField] private CinemachineVirtualCamera _playerCamera = null; public static CinemachineVirtualCamera playerCamera { get { return singleton._playerCamera; } }
    [SerializeField] private CinemachineVirtualCamera _aimingCamera = null; public static CinemachineVirtualCamera aimingCamera { get { return singleton._aimingCamera; } }
    [SerializeField] private CinemachineBrain _cameraBrain = null;
    [SerializeField] private LayerMask _aimLayer;

    private static CameraManager _singleton = null;

    public static CameraManager singleton
    {
        get
        {

            if (_singleton == null)
            {
                _singleton = FindFirstObjectByType<CameraManager>();
            }
            return _singleton;
        }
    }

    private bool _aiming = false; public bool aiming { get { return _aiming; } set { _aiming = value; } }

    private Vector3 _aimTargetPoint = Vector3.zero; public Vector3 aimTargetPoint { get { return _aimTargetPoint; } }

    private void Awake()
    {
        _cameraBrain.m_DefaultBlend.m_Time = 0.1f;
    }
     private void Update()
      {
      _aimingCamera.gameObject.SetActive(_aiming);
    }
  
private void SetAimTarget()
    {
        Ray ray = _camera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _aimLayer))
        {
            _aimTargetPoint = hit.point;
        }
        else
        {
            _aimTargetPoint = ray.GetPoint(1000);
        }
    }

    #if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_aimTargetPoint, 0.1f);
    }
    #endif











}

