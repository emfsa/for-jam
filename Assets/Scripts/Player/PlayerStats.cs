using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public static event Action<string> OnShowInfo;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _woodHitSounds;  // Звуки удара по дереву
    [SerializeField] private AudioClip[] _enemyHitSounds; // Звуки удара по врагу
    [SerializeField] private AudioClip[] _swingSounds;    // Звук замаха/промаха по воздуху
    [SerializeField] private AudioClip[] _tentSounds;     // Звук палатки
    [SerializeField] private AudioClip[] _pickUpSounds;   // Звук подбора / работы с дровами

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
    [SerializeField] private float _attackCooldown = 0.5f;
    [SerializeField] private Animator _handAnimator;
    private string[] _animationsName = { "heroL|axeAtack", "heroL|axeChop" };

    private float _lastAttackTime;

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
    [SerializeField] private TerrainTreeSystem _treeSystem;
    private bool _isDay => SceneManager.GetActiveScene().name == "Day";

    private void Awake()
    {
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }

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

        if (_handAnimator == null)
        {
            _handAnimator = GetComponentInChildren<Animator>();
            if (_handAnimator == null)
            {
                Debug.Log("HandAnimatorNULL {PlayerStats}");
            }
        }
        if (_treeSystem == null)
        {
            _treeSystem = FindAnyObjectByType<TerrainTreeSystem>();
        }
        ApplySavedProgress();
        ResetProgressBar();
    }

    private void Start() => UpdateUI();

    private void Update() => HandleHoldInteraction();

    private void ApplySavedProgress()
    {
        if (GameData.Instance == null) return;

        _money = GameData.Instance.money;
        _logCounts = GameData.Instance.logInventory;
        _attackDamage = 10f + (GameData.Instance.damageLevel * 5f);
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
        if (_isDay) return false;

        if (_logCounts > 0)
        {
            _logCounts--;
            UpdateUI();
            return true;
        }

        OnShowInfo?.Invoke("Inventory is empty!");
        return false;
    }

    /*public void OnAttack(InputValue val)
    {
        if (!val.isPressed || _camScript == null) return;

        if (Time.time < _lastAttackTime + _attackCooldown) return;

        _lastAttackTime = Time.time;

        playAttackAnim(1);

        Ray ray = _camScript.GetCenterRay();

        if (Physics.Raycast(ray, out RaycastHit hit, _attackDistance, _interactionLayerMask))
        {
            switch (hit.collider)
            {
                case var c when c.TryGetComponent(out Enemy enemy):
                    enemy.TakeDamage(_attackDamage, this);
                    PlayRandomSound(_enemyHitSounds);
                    break;

                case var c when c.TryGetComponent(out TreeLogic tree):
                    tree.TakeDamage(_attackDamage);
                    PlayRandomSound(_woodHitSounds);
                    break;

                default:
                    PlayRandomSound(_swingSounds);
                    break;
            }
        }
        else
        {
            PlayRandomSound(_swingSounds);
        }
    }*/
    public void OnAttack(InputValue val)
    {
        if (!val.isPressed || _camScript == null) return;

        if (Time.time < _lastAttackTime + _attackCooldown) return;

        _lastAttackTime = Time.time;

        playAttackAnim(1);

        Ray ray = _camScript.GetCenterRay();

        if (Physics.Raycast(ray, out RaycastHit hit, _attackDistance, _interactionLayerMask))
        {
            // 1. Попадание по уже заспавненному интерактивному дереву
            if (hit.collider.TryGetComponent(out TreeLogic tree) || hit.collider.GetComponentInParent<TreeLogic>() != null)
            {
                var targetTree = tree != null ? tree : hit.collider.GetComponentInParent<TreeLogic>();
                targetTree.TakeDamage(_attackDamage);
                PlayRandomSound(_woodHitSounds);
            }
            // 2. Попадание по врагу
            else if (hit.collider.TryGetComponent(out Enemy enemy) || hit.collider.GetComponentInParent<Enemy>() != null)
            {
                var targetEnemy = enemy != null ? enemy : hit.collider.GetComponentInParent<Enemy>();
                targetEnemy.TakeDamage(_attackDamage, this);
                PlayRandomSound(_enemyHitSounds);
            }
            // 3. Попадание по самому террейну (первый удар по дереву из TerrainData)
            else if (_treeSystem != null && _treeSystem.TryReplaceTerrainTree(hit.point, _attackDamage))
            {
                PlayRandomSound(_woodHitSounds); // Дерево успешно нашлось и заменилось — воспроизводим звук дерева!
            }
            else
            {
                PlayRandomSound(_swingSounds);
            }
        }
        else
        {
            PlayRandomSound(_swingSounds);
        }
    }

    private void PlayRandomSound(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0 || _audioSource == null) return;

        int randomIndex = UnityEngine.Random.Range(0, clips.Length);
        if (clips[randomIndex] != null)
        {
            _audioSource.PlayOneShot(clips[randomIndex]);
        }
    }

    private void playAttackAnim(int index)
    {
        if (_handAnimator == null) return;
        _handAnimator.Play(_animationsName[index - 1], -1, 0f);
    }

    public void OnTakeItem(InputValue val)
    {
        if (!val.isPressed || _camScript == null) return;

        Ray ray = _camScript.GetCenterRay();

        if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _interactionLayerMask))
        {
            bool isLog = hit.collider.GetComponent<Log>() != null;
            bool isStorage = hit.collider.GetComponent<LogStorage>() != null;

            if ((isLog || isStorage) && IsMaxLogs)
            {
                OnShowInfo?.Invoke("Logs inventory is full!");
                return;
            }

            switch (hit.collider)
            {
                case var c when c.TryGetComponent(out Log log):
                    log.pickUP(this);
                    PlayRandomSound(_pickUpSounds);
                    break;

                case var c when c.TryGetComponent(out LogStorage logStorage) && !_isHolding:
                    if (logStorage.TryRemoveLog())
                    {
                        AddLogs(1);
                        PlayRandomSound(_pickUpSounds);
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
                case var c when c.TryGetComponent(out CampFire campFire):
                    _targetCampFire = campFire;
                    StartHolding();
                    break;

                case var c when c.TryGetComponent(out LogStorage logStorage):
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
             (_targetLogStorage != null && hit.collider.GetComponent<LogStorage>() == _targetLogStorage) ||
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
            if (_targetCampFire != null)
            {
                if (_logCounts <= 0 && !_isDay)
                {
                    OnShowInfo?.Invoke("Inventory is empty!");
                }
                else if (TryUseLog())
                {
                    _targetCampFire.AddFireTime();
                    PlayRandomSound(_pickUpSounds);
                }
            }
            else if (_targetLogStorage != null)
            {
                if (_logCounts <= 0)
                {
                    OnShowInfo?.Invoke("Inventory is empty!");
                }
                else if (_targetLogStorage.IsFull)
                {
                    OnShowInfo?.Invoke("Storage is full!");
                }
                else
                {
                    bool addedAny = false;
                    while (_logCounts > 0 && !_targetLogStorage.IsFull)
                    {
                        if (_targetLogStorage.TryAddLog())
                        {
                            _logCounts--;
                            addedAny = true;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (addedAny)
                    {
                        PlayRandomSound(_pickUpSounds);
                    }

                    if (_targetLogStorage.IsFull && _logCounts > 0)
                    {
                        OnShowInfo?.Invoke("Storage is full!");
                    }

                    UpdateUI();
                }
            }
            else if (_targetTent != null)
            {
                PlayRandomSound(_tentSounds);
                if (_isDay)
                {
                    _targetTent.OpenShop(_shopUI, _camScript);
                }
                else
                {
                    _targetTent.Switch("Day");
                }
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
        _targetTent = null;
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

    public CamScript getCam() => _camScript;
}