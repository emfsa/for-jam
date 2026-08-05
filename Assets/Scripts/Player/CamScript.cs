using Unity.Cinemachine;
using UnityEngine;

public class CamScript : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _cam;

    void Awake()
    {
        if(_cam == null) { _cam = GetComponent<CinemachineCamera>(); }
        
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
   
    public Ray GetCenterRay()
    {
        return new Ray(transform.position, transform.forward);
    }
}
