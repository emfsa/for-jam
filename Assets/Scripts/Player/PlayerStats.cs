using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int _logCounts = 0;
    private TextMeshProUGUI _logCountText;

    [Header("camera")]
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
                Debug.LogWarning("logCountTextNone");
            }
        }
        ResetProgressBar();
    }

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        HandleHoldInteraction();
    }

    public void addLogs(int amount)
    {
        _logCounts += amount;
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
        if(!val.isPressed)
        {
            return;
        }
        if(_camScript == null) return;
        Debug.Log("attack");
        Ray ray = _camScript.GetCenterRay();

        if(Physics.Raycast(ray, out RaycastHit hit, _attackDistance, _interactionLayerMask))
        {
            Debug.Log("attack2");

            if (hit.collider.TryGetComponent(out Enemy enemy))
            {
                Debug.Log("attack3");

                enemy.TakeDamage(_attackDamage);
            }
        }
    }
    public void OnTakeItem(InputValue val)
    {
        if (!val.isPressed)
        {
            return;
        }

      

        Ray ray = _camScript.GetCenterRay();

        if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _interactionLayerMask))
        {
            if (hit.collider.TryGetComponent(out Log log))
            {
                log.pickUP(this);
                return;
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
            

            if (hit.collider.TryGetComponent(out CampFire campFire) && _logCounts > 0)
            {
                _targetCampFire = campFire;
                _isHolding = true;
                _currentHoldTime = 0f;

                if (_progressBar != null)
                    _progressBar.gameObject.SetActive(true);
            }
        }
    }    
    private void UpdateUI()
    {
        if (_logCountText != null)
        {
            _logCountText.text = _logCounts.ToString();
        }
    }

    private void HandleHoldInteraction()
    {
        if (!_isHolding || _targetCampFire == null) return;

        _currentHoldTime += Time.deltaTime;

        if (_progressBar != null)
        {
            _progressBar.fillAmount = _currentHoldTime / _holdDuration;
        }

        if (_currentHoldTime >= _holdDuration)
        {
            if (TryUseLog())
            {
                _targetCampFire.AddFireTime();
            }
            CancelHold();
        }
    }

    private void CancelHold()
    {
        _isHolding = false;
        _currentHoldTime = 0f;
        _targetCampFire = null;
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
}