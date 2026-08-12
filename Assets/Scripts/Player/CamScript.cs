using Unity.Cinemachine;
using UnityEngine;

public class CamScript : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _cam;

    void Awake()
    {
        if(_cam == null) { _cam = GetComponent<CinemachineCamera>(); }
        
        LockCursor(true);
    }

    /*public void LockCursor()
    {
       
    }

    public void UnlockCursor() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }*/

    public void LockCursor(bool locked)
    {
        if(locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    public Ray GetCenterRay()
    {
        return Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
    }
}
