using UnityEngine;

public class TestButtons : MonoBehaviour
{
    [SerializeField] private Canvas _UI;
    [SerializeField] private GameObject _shopUI;
    [SerializeField] private CamScript _camScript;
  
    private void Start()
    {
        if(_UI == null)
        {
            GameObject ui = GameObject.Find("UI");
            if(ui != null )
            {
                _UI = ui.GetComponent<Canvas>();
            }
        }
        if(_camScript == null)
        {
            _camScript = GetComponentInChildren<CamScript>();
        }
        if(_shopUI == null)
        {
            if(_UI != null)
            {
                Transform shop = _UI.transform.Find("Shop");
                if(shop != null )
                {
                    _shopUI = shop.gameObject;
                }
            }
            
        }
        
    }
    public void OnShop()
    {
        Debug.Log("H");
        if(_shopUI != null)
        {
            if (_camScript != null )
            {
                _camScript.LockCursor(_shopUI.activeSelf);
            }

            _shopUI.SetActive(!_shopUI.activeSelf);
        }
    }
}
