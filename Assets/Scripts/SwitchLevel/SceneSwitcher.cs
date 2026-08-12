using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] private PlayerStats _player;
    [SerializeField] private LogStorage _logStorage;

    private void Start()
    {
        if (_player == null)
        {
            _player = FindAnyObjectByType<PlayerStats>();
            if (_player == null) Debug.LogWarning("SceneSwitcher: Player NULL");
        }

        if (_logStorage == null)
        {
            _logStorage = FindAnyObjectByType<LogStorage>();
            if (_logStorage == null) Debug.LogWarning("SceneSwitcher: Storage NULL");
        }
    }

    public void SwitchScene(string sceneName)
    {
        SaveCurrentState();
        SceneManager.LoadScene(sceneName);
    }

    private void SaveCurrentState()
    {
        if (GameData.Instance == null) return;

        if (_player != null)
        {
            GameData.Instance.money = _player.GetMoney();
            GameData.Instance.logInventory = _player.GetLogCount();
        }

        if (_logStorage != null)
        {
            GameData.Instance.logStorage = _logStorage.StoredLogsCount;
        }
    }
}