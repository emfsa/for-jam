using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CampFire : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _fireTimeText;

    [Header("Settings")]
    [SerializeField] private float _fireTime = 100f;
    [SerializeField] private float _logTime = 20f; // Базовое время от одного бревна

    private string _sceneName;
    private bool _isBurning = true;
    public bool _isDay  => _sceneName == "Day";
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

        if(_sceneName == null)
        {
            Scene currentScene = SceneManager.GetActiveScene();
            _sceneName = currentScene.name;
        }
    }

    private void Start()
    {
        ApplySavedProgress();
        HideFireTime();
        UpdateFireTimeText();
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
    }

    public void RemoveTime(float enemyAmount)
    {
        _fireTime -= enemyAmount;

        if (_fireTime < 0)
        {
            _fireTime = 0;
        }

        UpdateFireTimeText();
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
        if (!_isBurning || _isDay) return;
      

        if (_fireTime > 0)
        {
            _fireTime -= Time.deltaTime;
            UpdateFireTimeText();
        }
        else
        {
            _fireTime = 0;
            UpdateFireTimeText();
            _isBurning = false;
            Lose();
        }
    }
  
    public void HideFireTime()
    {
        if (_isDay)
        {
            _isBurning = false;
            if (_fireTimeText != null)
            {
                _fireTimeText.gameObject.SetActive(false);
            }
        }
    }


    private void Lose()
    {
        Debug.Log("You lost!");
    }

    public void UpgradeBaseFireTime()
    {
        _logTime += 2f;
    }
}