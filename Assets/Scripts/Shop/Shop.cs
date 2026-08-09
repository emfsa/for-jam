using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] private PlayerStats _player;
    [SerializeField] private LogStorage _logStorage;
    [SerializeField] private CampFire _campFire;

    private void Start()
    {
        if (_player == null)
        {
            _player = FindAnyObjectByType<PlayerStats>();
            if (_player == null) Debug.LogWarning("Shop: PlayerStats не найден!");
        }

        if (_campFire == null)
        {
            _campFire = FindAnyObjectByType<CampFire>();
            if (_campFire == null) Debug.LogWarning("Shop: CampFire не найден!");
        }

        if (_logStorage == null)
        {
            _logStorage = FindAnyObjectByType<LogStorage>();
            if (_logStorage == null) Debug.LogWarning("Shop: LogStorage не найден!");
        }
    }

    public void UpgradeAttackDamage(float damage)
    {
        if (_player != null) _player.UpgradeDamage(damage);
    }

    public void UpgradeMaxLogCount(int log)
    {
        if (_player != null) _player.UpgradeMaxLogCounts(log);
    }

    public void UpgradeMaxLogCapacity(int amount)
    {
        if (_logStorage != null) _logStorage.UpgradeStorage(amount);
    }

    public void UpgradeBaseFireTime()
    {
        if (_campFire != null) _campFire.UpgradeBaseFireTime();
    }
}