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
    private Vector2 _move;
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _cam = GetComponentInChildren<CinemachineCamera>();
    }
    public void OnMove(InputValue val)
    {
       _move = val.Get<Vector2>();
    }

    private void Update()
    {
        _characterController.Move((GetForward() * _move.y + GetRight() * _move.x) * Time.deltaTime * _currentSpeed);
    }
    private Vector3 GetForward()
    {
        Vector3 forward = _cam.transform.forward;
        forward.y = 0;
        forward = forward.normalized;
        return forward;
    }
    public void OnSprint(InputValue val)
    {
        _currentSpeed = val.isPressed ? _runSpeed : _walkSpeed;
        Debug.Log($"Current Speed: {_currentSpeed}");
    }
    private Vector3 GetRight()
    {
        Vector3 right = _cam.transform.right;
        right.y = 0;
        right = right.normalized;
        return right;
    }
}
