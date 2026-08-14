using UnityEngine;

public class Tent : MonoBehaviour
{

    public void OpenShop(GameObject _shopUI,CamScript cam)
    {
        if (_shopUI == null) return;
        cam.LockCursor(false);

        _shopUI.SetActive(true);
    }
    public void CloseOpenShop(GameObject _shopUI, CamScript cam)
    {
        if (_shopUI != null)
        {
            if (cam != null)
            {
                cam.LockCursor(_shopUI.activeSelf);
            }

            _shopUI.SetActive(!_shopUI.activeSelf);
        }
    }
}
