using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] private PlayerStats _player;
    [SerializeField] private LogStorage _logStorage;
    [SerializeField] private CampFire _campFire;

    [Header("1. Апгрейд Урона")]
    [SerializeField] private float _damageBonus = 5f;
    [SerializeField] private int _baseDamageCost = 50;
    [SerializeField] private float _damageCostMultiplier = 1.2f;

    [Header("2. Апгрейд Карманов (Бревна игрока)")]
    [SerializeField] private int _logBonus = 1;
    [SerializeField] private int _baseLogCost = 30;
    [SerializeField] private float _logCostMultiplier = 1.25f;

    [Header("3. Апгрейд Склада")]
    [SerializeField] private int _storageCapacityBonus = 5;
    [SerializeField] private int _baseStorageCost = 100;
    [SerializeField] private float _storageCostMultiplier = 1.3f;

    [Header("4. Апгрейд Костра")]
    [SerializeField] private int _baseFireCost = 40;
    [SerializeField] private float _fireCostMultiplier = 1.15f;

    private void Start()
    {
        if (_player == null) _player = FindAnyObjectByType<PlayerStats>();
        if (_campFire == null) _campFire = FindAnyObjectByType<CampFire>();
        if (_logStorage == null) _logStorage = FindAnyObjectByType<LogStorage>();

        UpdateAllShopTexts();
    }

    private void ApplySavedUpgrades()
    {
        if (GameData.Instance == null) return;

        for (int i = 0; i < GameData.Instance.damageLevel; i++)
        {
            _player.UpgradeDamage(_damageBonus);
        }
        for (int i = 0; i < GameData.Instance.logLevel; i++)
        {
            _player.UpgradeMaxLogCounts(_logBonus);
        }
        for (int i = 0; i < GameData.Instance.logStorageLevel; i++)
        {
            _logStorage.UpgradeStorage(_storageCapacityBonus);
        }
        for (int i = 0; i < GameData.Instance.fireLevel; i++)
        {
            _campFire.UpgradeBaseFireTime();
        }
    }

    private int GetCurrentCost(int baseCost, float multiplier, int level)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(multiplier, level));
    }

    // --- Методы покупок ---

    public void BuyDamageUpgrade()
    {
        int currentCost = GetDamageUpgradeCost();

        if (_player != null && _player.TrySpendMoney(currentCost))
        {
            _player.UpgradeDamage(_damageBonus);
            GameData.Instance.damageLevel++;
            UpdateAllShopTexts();
        }
    }

    public void BuyMaxLogUpgrade()
    {
        int currentCost = GetLogUpgradeCost();

        if (_player != null && _player.TrySpendMoney(currentCost))
        {
            _player.UpgradeMaxLogCounts(_logBonus);
            GameData.Instance.logLevel++;
            UpdateAllShopTexts();
        }
    }

    public void BuyStorageUpgrade()
    {
        int currentCost = GetStorageUpgradeCost();

        if (_logStorage != null && _player != null && _player.TrySpendMoney(currentCost))
        {
            _logStorage.UpgradeStorage(_storageCapacityBonus);
            GameData.Instance.logStorageLevel++;
            UpdateAllShopTexts();
        }
    }

    public void BuyFireUpgrade()
    {
        int currentCost = GetFireUpgradeCost();

        if (_campFire != null && _player != null && _player.TrySpendMoney(currentCost))
        {
            _campFire.UpgradeBaseFireTime();
            GameData.Instance.fireLevel++;
            UpdateAllShopTexts();
        }
    }

    public void UpdateAllShopTexts()
    {
        TextShop[] texts = FindObjectsByType<TextShop>();
        foreach (var textScript in texts)
        {
            textScript.UpdateText();
        }
    }

    public int GetDamageUpgradeCost() => GetCurrentCost(_baseDamageCost, _damageCostMultiplier, GetDamageUpgradeLevel());
    public int GetLogUpgradeCost() => GetCurrentCost(_baseLogCost, _logCostMultiplier, GetLogUpgradeLevel());
    public int GetStorageUpgradeCost() => GetCurrentCost(_baseStorageCost, _storageCostMultiplier, GetStorageUpgradeLevel());
    public int GetFireUpgradeCost() => GetCurrentCost(_baseFireCost, _fireCostMultiplier, GetFireUpgradeLevel());

    public int GetDamageUpgradeLevel() => GameData.Instance != null ? GameData.Instance.damageLevel : 0;
    public int GetLogUpgradeLevel() => GameData.Instance != null ? GameData.Instance.logLevel : 0;
    public int GetStorageUpgradeLevel() => GameData.Instance != null ? GameData.Instance.logStorageLevel : 0;
    public int GetFireUpgradeLevel() => GameData.Instance != null ? GameData.Instance.fireLevel : 0;

    public int GetMoney() => _player != null ? _player.GetMoney() : 0;
}