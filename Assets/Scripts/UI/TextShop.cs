using TMPro;
using UnityEngine;

public class TextShop : MonoBehaviour
{
    public enum UpgradeType { Damage, MaxLogs, Storage, Fire }
    public enum DisplayType { Level, Price, Money }

    [SerializeField] private UpgradeType _upgradeType;
    [SerializeField] private DisplayType _displayType;
    [SerializeField] private Shop _shop;

    private TextMeshProUGUI _text;

    private void Awake()
    {
        if (_text == null)
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        if (_shop == null)
        {
            _shop = GetComponentInParent<Shop>();
            if (_shop == null)
            {
                _shop = FindAnyObjectByType<Shop>();
            }
        }
    }

    private void Start()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        if (_text == null || _shop == null) return;

        int value = 0;

        switch (_displayType)
        {
            case DisplayType.Level:
                value = GetLevelValue();
                break;

            case DisplayType.Price:
                value = GetPriceValue();
                break;

            case DisplayType.Money:
                value = _shop.GetMoney();
                break;
        }

        _text.text = value.ToString();
    }

    private int GetPriceValue()
    {
        return _upgradeType switch
        {
            UpgradeType.Damage => _shop.GetDamageUpgradeCost(),
            UpgradeType.MaxLogs => _shop.GetLogUpgradeCost(),
            UpgradeType.Storage => _shop.GetStorageUpgradeCost(),
            UpgradeType.Fire => _shop.GetFireUpgradeCost(),
            _ => 0
        };
    }

    private int GetLevelValue()
    {
        return _upgradeType switch
        {
            UpgradeType.Damage => _shop.GetDamageUpgradeLevel(),
            UpgradeType.MaxLogs => _shop.GetLogUpgradeLevel(),
            UpgradeType.Storage => _shop.GetStorageUpgradeLevel(),
            UpgradeType.Fire => _shop.GetFireUpgradeLevel(),
            _ => 0
        };
    }
}