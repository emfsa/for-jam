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
    private TextMeshProUGUI _logCountText;

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

    private bool _isHolding = false;
    private float _currentHoldTime = 0f;
    private CampFire _targetCampFire;
    private LogStorage _targetLogStorage;

    private void Awake()
    {
        if (_camScript == null)
        {
            _camScript = GetComponentInChildren<CamScript>();
        }

        if (_logCountText == null)
        {
            GameObject textObj = GameObject.Find("LogCountText");
            if (textObj != null)
            {
                _logCountText = textObj.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Debug.LogWarning("PlayerStats: LogCountText не найден!");
            }
        }

        ResetProgressBar();
    }

    private void Start() => UpdateUI();

    private void Update() => HandleHoldInteraction();

    public void addLogs(int amount)
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
        if (_logCounts > 0)
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
                    enemy.TakeDamage(_attackDamage);
                    break;

                case var c when c.TryGetComponent(out TreeLogic tree):
                    tree.TakeDamage(_attackDamage);
                    break;
            }
        }
    }

    // Клик E: Только забор бревен (с земли или со склада)
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

                // Забираем бревно со склада, только если не зажата выгрузка
                case var c when c.TryGetComponent(out LogStorage logStorage) && !IsMaxLogs && !_isHolding:
                    if (logStorage.TryRemoveLog())
                    {
                        addLogs(1);
                    }
                    break;
            }
        }
    }

    // Зажатие E: Пополнение костра или полная выгрузка бревен на склад
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
             (_targetLogStorage != null && hit.collider.GetComponent<LogStorage>() == _targetLogStorage));

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
            // На костер отдаем 1 бревно за одно удержание
            if (_targetCampFire != null && TryUseLog())
            {
                _targetCampFire.AddFireTime();
            }
            // На склад отдаем ВСЕ бревна сразу (сколько влезет)
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
    }
}