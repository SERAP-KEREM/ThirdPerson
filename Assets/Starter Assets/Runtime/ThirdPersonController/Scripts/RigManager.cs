using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.TextCore.Text;

public class RigManager : MonoBehaviour
{
    [SerializeField] private MultiAimConstraint _rightHand = null;
    [SerializeField] private TwoBoneIKConstraint _leftHand = null;
    [SerializeField] private MultiAimConstraint _body = null;
    [SerializeField] private Transform _aimTarget = null;
    [SerializeField] private  Vector3 _weaponHandKickDirection=new Vector3(0,0,0);
    [SerializeField] private  Vector3 _weaponBodyKickDirection=new Vector3(-1,0,0);

    public Vector3 aimTarget { set { _aimTarget.position = value; } }
    public float leftHandWeight { set { _leftHand.weight = value; } }
    public float aimWeight { set { _rightHand.weight = value;
            _body.weight = value; } }

    private Vector3 _originalRightHandOffset = Vector3.zero;
    private Vector3 _originalBodyOffset= Vector3.zero;




    private void Awake()
    {
        _originalRightHandOffset = _rightHand.data.offset;
        _originalBodyOffset= _body.data.offset;
    }

    public void SetLeftHandGripData(Vector3 position, Vector3 rotation)
    {
        if (_leftHand.data.target != null)
        {
            _leftHand.data.target.localPosition = position;
            _leftHand.data.target.localEulerAngles = rotation;
        }

    }

    public void ApplyWeaponKick(float hand, float body)
    {
        _rightHand.data.offset = _originalRightHandOffset + _weaponHandKickDirection* hand;
        _body.data.offset = _originalBodyOffset + _weaponBodyKickDirection* body;
    }
    private void Update()
    {
        
        if (_rightHand.data.offset != _originalRightHandOffset)
        {
            _rightHand.data.offset = Vector3.Lerp(_rightHand.data.offset, _originalRightHandOffset, 10f* Time.deltaTime);
        }

        if (_body.data.offset != _originalBodyOffset)
        {
            _body.data.offset = Vector3.Lerp(_body.data.offset, _originalBodyOffset, 10f * Time.deltaTime);

        } 
    }
}
