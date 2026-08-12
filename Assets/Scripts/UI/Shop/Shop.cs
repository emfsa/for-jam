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
    private int _damageLevel = 0;

    [Header("2. Апгрейд Карманов (Бревна игрока)")]
    [SerializeField] private int _logBonus = 1;
    [SerializeField] private int _baseLogCost = 30;
    [SerializeField] private float _logCostMultiplier = 1.25f;
    private int _logLevel = 0;

    [Header("3. Апгрейд Склада")]
    [SerializeField] private int _storageCapacityBonus = 5;
    [SerializeField] private int _baseStorageCost = 100;
    [SerializeField] private float _storageCostMultiplier = 1.3f;
    private int _storageLevel = 0;

    [Header("4. Апгрейд Костра")]
    [SerializeField] private int _baseFireCost = 40;
    [SerializeField] private float _fireCostMultiplier = 1.15f;
    private int _fireLevel = 0;

    private void Start()
    {
        if (_player == null) _player = FindAnyObjectByType<PlayerStats>();
        if (_campFire == null) _campFire = FindAnyObjectByType<CampFire>();
        if (_logStorage == null) _logStorage = FindAnyObjectByType<LogStorage>();

        UpdateAllShopTexts();
    }

    // Расчет стоимости по формуле: База * (Множитель ^ Уровень)
    private int GetCurrentCost(int baseCost, float multiplier, int level)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(multiplier, level));
    }

    // --- Методы покупок для OnClick() кнопок ---

    public void BuyDamageUpgrade()
    {
        int currentCost = GetCurrentCost(_baseDamageCost, _damageCostMultiplier, _damageLevel);

        if (_player != null && _player.TrySpendMoney(currentCost))
        {
            _player.UpgradeDamage(_damageBonus);
            _damageLevel++;
            UpdateAllShopTexts();
        }
    }

    public void BuyMaxLogUpgrade()
    {
        int currentCost = GetCurrentCost(_baseLogCost, _logCostMultiplier, _logLevel);

        if (_player != null && _player.TrySpendMoney(currentCost))
        {
            _player.UpgradeMaxLogCounts(_logBonus);
            _logLevel++;
            UpdateAllShopTexts();
        }
    }

    public void BuyStorageUpgrade()
    {
        int currentCost = GetCurrentCost(_baseStorageCost, _storageCostMultiplier, _storageLevel);

        if (_logStorage != null && _player != null && _player.TrySpendMoney(currentCost))
        {
            _logStorage.UpgradeStorage(_storageCapacityBonus);
            _storageLevel++;
            UpdateAllShopTexts();
        }
    }

    public void BuyFireUpgrade()
    {
        int currentCost = GetCurrentCost(_baseFireCost, _fireCostMultiplier, _fireLevel);

        if (_campFire != null && _player != null && _player.TrySpendMoney(currentCost))
        {
            _campFire.UpgradeBaseFireTime();
            _fireLevel++;
            UpdateAllShopTexts();
        }
    }

    public void UpdateAllShopTexts()
    {
        TextShop[] texts = FindObjectsByType<TextShop>(FindObjectsSortMode.None);
        foreach (var textScript in texts)
        {
            textScript.UpdateText();
        }
    }


    public int GetDamageUpgradeCost() => GetCurrentCost(_baseDamageCost, _damageCostMultiplier, _damageLevel);
    public int GetLogUpgradeCost() => GetCurrentCost(_baseLogCost, _logCostMultiplier, _logLevel);
    public int GetStorageUpgradeCost() => GetCurrentCost(_baseStorageCost, _storageCostMultiplier, _storageLevel);
    public int GetFireUpgradeCost() => GetCurrentCost(_baseFireCost, _fireCostMultiplier, _fireLevel);

    public int GetDamageUpgradeLevel() => _damageLevel;
    public int GetLogUpgradeLevel() => _logLevel;
    public int GetStorageUpgradeLevel() => _storageLevel;
    public int GetFireUpgradeLevel() => _fireLevel;

    public int GetMoney() => _player != null ? _player.GetMoney() : 0;
}