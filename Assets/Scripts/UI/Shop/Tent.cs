using UnityEngine;
using UnityEngine.SceneManagement;

public class Tent : MonoBehaviour
{
    /*[SerializeField] private CampFire _camp;
    private string _sceneName;*/
    /*private void Start()
    {
        if(_camp == null)
        {
            _camp = FindAnyObjectByType<CampFire>();
            if(_camp == null)
            {
                Debug.Log("CampNULL[Tent]");
            }
        }
        *//*if (_sceneName == null)
        {
            Scene scene = SceneManager.GetActiveScene();
            _sceneName = scene.name;
        }*//*
    }*/
    public void OpenShop(GameObject _shopUI,CamScript cam)
    {
        if (_shopUI == null /*|| (_sceneName == "Night" && _camp.GetFireTime() != 0f)*/) return;
        cam.LockCursor(false);

        _shopUI.SetActive(true);
    }
    public void CloseOpenShop(GameObject _shopUI, CamScript cam)
    {
        if (_shopUI != null )
        {
            if (cam != null)
            {
                cam.LockCursor(_shopUI.activeSelf);
            }

            _shopUI.SetActive(!_shopUI.activeSelf);
        }
    }
}
