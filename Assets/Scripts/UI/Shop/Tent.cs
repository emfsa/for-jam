using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tent : MonoBehaviour
{
    private bool _isNight => SceneManager.GetActiveScene().name == "Night";
    [SerializeField] private bool _isEnd = false;
    private SceneSwitcher _sceneSwitcher;

    private void Start()
    {
        if (_sceneSwitcher == null)
        {
            _sceneSwitcher = GetComponentInChildren<SceneSwitcher>();
        }
    }
    private void OnEnable()
    {
        EnemySpawner.OnWaveCompleted += HandleWaveCompleted;
    }

    
    private void OnDisable()
    {
        EnemySpawner.OnWaveCompleted -= HandleWaveCompleted;
    }
    public void OpenShop(GameObject _shopUI,CamScript cam)
    {
        if (_shopUI == null || (_isNight && !_isEnd)) return;
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


    private void HandleWaveCompleted()
    {
        _isEnd = true;
    }
    public void Switch(string sceneName)
    {
        if (_sceneSwitcher == null|| (_isNight && !_isEnd)) return;
        _sceneSwitcher.SwitchScene(sceneName);
    }
}
