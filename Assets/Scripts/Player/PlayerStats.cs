using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Logs")]
    [SerializeField] private int _logCounts = 0;
    [SerializeField] private int _maxLogCounts = 5;
    [SerializeField] private TextMeshProUGUI _logCountText;

    public bool IsMaxLogs => _logCounts >= _maxLogCounts;

    [Header("Camera")]
    [SerializeField] private CamScript _camScript;
    [SerializeField] private float _interactionDistance = 3f;
    [SerializeField] private LayerMask _interactionLayerMask;

    [Header("Hold")]
    [SerializeField] private Image _progressBar;
    [SerializeField] private float _holdDuration = 2f;

    [Header("Attack")]
    [SerializeField] private float _attackDamage = 10f;
    [SerializeField] private float _attackDistance = 3f;

    [Header("Money")]
    [SerializeField] private int _money = 0;
    [SerializeField] private TextMeshProUGUI _moneyCountText;

    private Canvas _UI;
    private GameObject _shopUI;

    private bool _isHolding = false;
    private float _currentHoldTime = 0f;
    private CampFire _targetCampFire;
    private LogStorage _targetLogStorage;
    private Tent _targetTent;

    private void Awake()
    {
        if (_camScript == null)
        {
            _camScript = GetComponentInChildren<CamScript>();
        }

        if (_moneyCountText == null)
        {
            GameObject moneyObj = GameObject.Find("MoneyCountText");
            if (moneyObj != null) _moneyCountText = moneyObj.GetComponent<TextMeshProUGUI>();
            else Debug.LogWarning("PlayerStats: MoneyCountText не найден!");
        }

        if (_logCountText == null)
        {
            GameObject logObj = GameObject.Find("LogCountText");
            if (logObj != null) _logCountText = logObj.GetComponent<TextMeshProUGUI>();
            else Debug.LogWarning("PlayerStats: LogCountText не найден!");
        }

        if (_UI == null)
        {
            GameObject ui = GameObject.Find("UI");
            if (ui != null)
            {
                _UI = ui.GetComponent<Canvas>();
            }
        }

        if (_shopUI == null && _UI != null)
        {
            Transform shop = _UI.transform.Find("Shop");
            if (shop != null)
            {
                _shopUI = shop.gameObject;
                if (_shopUI.activeSelf)
                {
                    _shopUI.SetActive(false);
                }
            }
        }

        ApplySavedProgress();
        ResetProgressBar();
    }

    private void Start() => UpdateUI();

    private void Update() => HandleHoldInteraction();

    private void ApplySavedProgress()
    {
        if (GameData.Instance == null) return;

        // 1. Восстанавливаем ресурсы
        _money = GameData.Instance.money;
        _logCounts = GameData.Instance.logInventory;

        // 2. Рассчитываем урон: Базовый (10) + (Уровень * Бонус за уровень (5))
        _attackDamage = 10f + (GameData.Instance.damageLevel * 5f);

        // 3. Рассчитываем вместимость: Базовая (5) + (Уровень * Бонус за уровень (1))
        _maxLogCounts = 5 + (GameData.Instance.logLevel * 1);
    }

    public void AddLogs(int amount)
    {
        _logCounts += amount;

        if (IsMaxLogs)
        {
            _logCounts = _maxLogCounts;
        }

        UpdateUI();
    }

    public bool TryUseLog()
    {
        if (_logCounts > 0 && !_targetCampFire._isDay)
        {
            _logCounts--;
            UpdateUI();
            return true;
        }
        return false;
    }

    public void OnAttack(InputValue val)
    {
        if (!val.isPressed || _camScript == null) return;

        Ray ray = _camScript.GetCenterRay();

        if (Physics.Raycast(ray, out RaycastHit hit, _attackDistance, _interactionLayerMask))
        {
            switch (hit.collider)
            {
                case var c when c.TryGetComponent(out Enemy enemy):
                    enemy.TakeDamage(_attackDamage, this);
                    break;

                case var c when c.TryGetComponent(out TreeLogic tree):
                    tree.TakeDamage(_attackDamage);
                    break;
            }
        }
    }

    public void OnTakeItem(InputValue val)
    {
        if (!val.isPressed || _camScript == null) return;

        Ray ray = _camScript.GetCenterRay();

        if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _interactionLayerMask))
        {
            switch (hit.collider)
            {
                case var c when c.TryGetComponent(out Log log) && !IsMaxLogs:
                    log.pickUP(this);
                    break;

                case var c when c.TryGetComponent(out LogStorage logStorage) && !IsMaxLogs && !_isHolding:
                    if (logStorage.TryRemoveLog())
                    {
                        AddLogs(1);
                    }
                    break;
            }
        }
    }

    public void OnHold(InputValue val)
    {
        if (!val.isPressed)
        {
            CancelHold();
            return;
        }

        if (_camScript == null) return;

        Ray ray = _camScript.GetCenterRay();

        if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _interactionLayerMask))
        {
            switch (hit.collider)
            {
                case var c when c.TryGetComponent(out CampFire campFire) && _logCounts > 0:
                    _targetCampFire = campFire;
                    StartHolding();
                    break;

                case var c when c.TryGetComponent(out LogStorage logStorage) && _logCounts > 0 && !logStorage.IsFull:
                    _targetLogStorage = logStorage;
                    StartHolding();
                    break;
                case var c when c.TryGetComponent(out Tent tent): 
                    _targetTent = tent;
                    StartHolding();
                    break;
                default:
                    CancelHold();
                    break;
            }
        }
    }

    private void HandleHoldInteraction()
    {
        if (!_isHolding) return;

        Ray ray = _camScript.GetCenterRay();
        bool isLookingAtTarget = Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _interactionLayerMask) &&
            ((_targetCampFire != null && hit.collider.GetComponent<CampFire>() == _targetCampFire) ||
             (_targetLogStorage != null && hit.collider.GetComponent<LogStorage>() == _targetLogStorage)||
             (_targetTent != null && hit.collider.GetComponent<Tent>() == _targetTent));

        if (!isLookingAtTarget)
        {
            CancelHold();
            return;
        }

        _currentHoldTime += Time.deltaTime;

        if (_progressBar != null)
            _progressBar.fillAmount = _currentHoldTime / _holdDuration;

        if (_currentHoldTime >= _holdDuration)
        {
            if (_targetCampFire != null && TryUseLog())
            {
                _targetCampFire.AddFireTime();
            }
            else if (_targetLogStorage != null)
            {
                while (_logCounts > 0 && !_targetLogStorage.IsFull)
                {
                    if (_targetLogStorage.TryAddLog())
                    {
                        _logCounts--;
                    }
                    else
                    {
                        break;
                    }
                }
                UpdateUI();
            }
            else if (_targetTent != null)
            {
                _targetTent.OpenShop(_shopUI,_camScript);
            }

                CancelHold();
        }
    }

    private void StartHolding()
    {
        _isHolding = true;
        _currentHoldTime = 0f;

        if (_progressBar != null)
            _progressBar.gameObject.SetActive(true);
    }

    private void CancelHold()
    {
        _isHolding = false;
        _currentHoldTime = 0f;
        _targetCampFire = null;
        _targetLogStorage = null;
        ResetProgressBar();
    }

    private void ResetProgressBar()
    {
        if (_progressBar != null)
        {
            _progressBar.fillAmount = 0f;
            _progressBar.gameObject.SetActive(false);
        }
    }

    private void UpdateUI()
    {
        if (_logCountText != null)
        {
            _logCountText.text = $"{_logCounts} / {_maxLogCounts}";
        }
        if (_moneyCountText != null)
        {
            _moneyCountText.text = _money.ToString();
        }
    }

    public void UpgradeDamage(float damage)
    {
        _attackDamage += damage;
    }

    public void UpgradeMaxLogCounts(int addedMaxLogCount)
    {
        _maxLogCounts += addedMaxLogCount;
        UpdateUI();
    }

    public float GetAttackDamage() => _attackDamage;
    public int GetMaxLogCount() => _maxLogCounts;

    public int GetMoney() => _money;

    public void AddMoney(int amount)
    {
        _money += amount;
        UpdateUI();
    }

    public bool TrySpendMoney(int amount)
    {
        if (_money < amount || amount <= 0) return false;

        _money -= amount;
        UpdateUI();
        return true;
    }

    public int GetLogCount() => _logCounts;

    public CamScript getCam()
    {
        return _camScript;
    }
}