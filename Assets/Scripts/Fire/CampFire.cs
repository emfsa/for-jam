using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class CampFire : MonoBehaviour
{
    public static event Action OnTimeEnd;
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _fireTimeText;
    [SerializeField] private GameObject _looseUI;

    [Header("Settings")]
    [SerializeField] private float _fireTime = 100f;
    [SerializeField] private float _logTime = 20f; // Базовое время от одного бревна
   
    [Header("lightning")]
    [SerializeField] private Light _light;
    [SerializeField] private VisualEffect _effects;
    [SerializeField] private float _powerMultiplier = 1.2f; // Множитель: сколько power дает 1 секунда
    [SerializeField] private float _lightMultiplier = 0.5f;

    private string _sceneName;
    private bool _isBurning = true;
    public bool _isDay => _sceneName == "Day";
    private bool _isCompleted = false;
    private void Awake()
    {
        if (_fireTimeText == null)
        {
            GameObject textObj = GameObject.Find("FireTimeText");
            if (textObj != null)
            {
                _fireTimeText = textObj.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Debug.LogWarning("CampFire: FireTimeText не найден!");
            }
        }
        if (_effects == null)
        {
            _effects = GetComponentInChildren<VisualEffect>();
            if (_effects == null)
            {
                Debug.Log("VisualEffectsNULL {CampFire}");
            }
        }
        if (_light == null)
        {
            _light = GetComponentInChildren<Light>();
            if (_light == null)
            {
                Debug.Log("LightNull {CampFire}");
            }
        }
        if (_sceneName == null)
        {
            Scene currentScene = SceneManager.GetActiveScene();
            _sceneName = currentScene.name;
        }
        if(_looseUI != null)
        {
            _looseUI.SetActive(false);
        }
    }
    private void OnEnable()
    {
        EnemySpawner.OnWaveCompleted += StopFireTimer;
    }

    private void OnDisable()
    {
        EnemySpawner.OnWaveCompleted -= StopFireTimer;
    }


    private void Start()
    {
        ApplySavedProgress();
        HideFireTime();
        UpdateFireTimeText();
        UpdateFirePower();
        if (!TryStartLight())
        {
            FinishLight();
        }
    }

    private void Update()
    {
        StartFireTime();
    }

    private void ApplySavedProgress()
    {
        if (GameData.Instance == null) return;

        // Рассчитываем время горения от бревна: Базовое (20) + (Уровень * Бонус (2))
        _logTime = 20f + (GameData.Instance.fireLevel * 2f);
    }

    public void AddFireTime()
    {
        if (_isDay) return;
        _fireTime += _logTime;
        UpdateFireTimeText();
        UpdateFirePower();
    }

    public void RemoveTime(float enemyAmount)
    {
        _fireTime -= enemyAmount;

        if (_fireTime < 0)
        {
            _fireTime = 0;
        }

        UpdateFireTimeText();
        UpdateFirePower();
    }

    public void UpdateFireTimeText()
    {
        if (_fireTimeText != null)
        {
            _fireTimeText.text = Mathf.CeilToInt(_fireTime).ToString();
        }
    }

    private void StartFireTime()
    {
        if (!_isBurning || _isDay || _isCompleted) return;


        if (_fireTime > 0)
        {
            _fireTime -= Time.deltaTime;
            UpdateFirePower();
            UpdateFireTimeText();
        }
        else
        {
            _fireTime = 0;
            UpdateFirePower();
            UpdateFireTimeText();
            _isBurning = false;
            Lose();
        }
    }

    private void UpdateFirePower()
    {
        if (_effects != null)
        {
            int currentPower = Mathf.Max(0, Mathf.RoundToInt(_fireTime * _powerMultiplier));
            _effects.SetInt("power", currentPower);
        }

        if (_light != null)
        {
            float currentLightIntensity = _fireTime * _lightMultiplier;
            _light.intensity = Mathf.Max(0, currentLightIntensity);
        }
    }

    private void StopFireTimer()
    {
        _isCompleted = true;
        HideFireTime();
    }
    public void HideFireTime()
    {
        if (_isDay || _isCompleted)
        {
            _isBurning = false;
            if (_fireTimeText != null)
            {
                _fireTimeText.gameObject.SetActive(false);
            }
        }
    }

    private bool TryStartLight()
    {
        if (_isDay) return false;
        _effects.gameObject.SetActive(true);
        _light.gameObject.SetActive(true);
        return true;
    }
    private void FinishLight()
    {
        if (_isDay || _fireTime == 0)
        {
            _light.gameObject.SetActive(false);
            _effects.gameObject.SetActive(false);
        }
    }
    private void Lose()
    {
        OnTimeEnd?.Invoke();
        _looseUI.SetActive(true);
        CamScript cam = new CamScript();
        cam.LockCursor(false);
    }

    public void UpgradeBaseFireTime()
    {
        _logTime += 2f;
    }
}