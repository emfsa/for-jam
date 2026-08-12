using TMPro;
using UnityEngine;

public class CampFire : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _fireTimeText;
    private float _fireTime = 100f;
    private bool _isBurning = true;
    private float _logTime = 20f;

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
    }

    private void Start()
    {
        UpdateFireTimeText();
    }

    private void Update()
    {
        StartFireTime();
    }

    public void AddFireTime()
    {
        _fireTime += _logTime;
        UpdateFireTimeText();
    }

    public void RemoveTime(float enemyAmount)
    {
        _fireTime -= enemyAmount;
        UpdateFireTimeText();
    }

    public void UpdateFireTimeText()
    {
        if (_fireTime < 0)
        {
            _fireTime = 0;
        }

        if (_fireTimeText != null)
        {
            _fireTimeText.text = _fireTime.ToString("F0");
        }
    }

    private void StartFireTime()
    {
        if (!_isBurning) return;

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

    private void Lose()
    {
        Debug.Log("You lost!");
    }

    public void UpgradeBaseFireTime()
    {
        _logTime += 2f;
    }
}