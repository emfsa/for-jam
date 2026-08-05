using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStats : MonoBehaviour
{
    



    [SerializeField] private int _logCounts = 0;
    private TextMeshProUGUI _logCountText;

    [SerializeField] private CamScript _camScript;
    [SerializeField] private float _interactionDistance = 3f;
    [SerializeField] private LayerMask _interactionLayerMask;

    private Log _log;
    private CampFire _campFire;
    //UI


    private void Awake()
    {
        if(_camScript == null)
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
      
       
    }

    private void Start()
    {
        UpdateUI();
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

    public void OnTakeItem(InputValue val)
    {
        if (!val.isPressed) return;

        Ray ray = _camScript.GetCenterRay();

        if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _interactionLayerMask))
        {
            if (hit.collider.TryGetComponent(out Log log))
            {
                log.pickUP(this);
                return;
            }

            if (hit.collider.TryGetComponent(out CampFire campFire) && _logCounts > 0)
            {
                if (TryUseLog())
                {
                    campFire.AddFireTime();
                }
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



    public void SetCurrentLog(Log log) { _log = log; }
    public void SetCurrentCampFire(CampFire camp) { _campFire = camp; }
}
