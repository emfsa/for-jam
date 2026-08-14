using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private CinemachineCamera _cam;

    [SerializeField] private float _currentSpeed = 5f;
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _runSpeed = 8f;
    [SerializeField] private float _gravity = -9.81f;
    
    private Vector3 _velocity;
    private Vector2 _move;
    private void Awake()
    {
        if( _characterController == null )
            _characterController = GetComponent<CharacterController>();
        if(_cam == null)
            _cam = GetComponentInChildren<CinemachineCamera>();
    }
    public void OnMove(InputValue val)
    {
       _move = val.Get<Vector2>();
    }
    public void OnSprint(InputValue val)
    {
        _currentSpeed = val.isPressed ? _runSpeed : _walkSpeed;
        Debug.Log($"Current Speed: {_currentSpeed}");
    }
    private void Update()
    {
        HandleGravity();
        HandleMovement();
    }

    private void HandleGravity()
    {
        if(_characterController.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        _velocity.y += _gravity * Time.deltaTime;

    }
    private void HandleMovement()
    {
        Vector3 moveDirection = ((GetForward() * _move.y + GetRight() * _move.x) * _currentSpeed);

        Vector3 finalMove = moveDirection + _velocity;

        _characterController.Move(finalMove * Time.deltaTime);
    }
    private Vector3 GetForward()
    {
        Vector3 forward = _cam.transform.forward;
        forward.y = 0;
        forward = forward.normalized;
        return forward;
    }
    
    private Vector3 GetRight()
    {
        Vector3 right = _cam.transform.right;
        right.y = 0;
        right = right.normalized;
        return right;
    }
}
