using Unity.Cinemachine;
using UnityEngine;

public class CamScript : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _cam;

    void Start()
    {
        _cam = GetComponent<CinemachineCamera>();
        LockCursor();
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
   
}
