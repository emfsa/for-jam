using System.Collections;
using TMPro;
using UnityEngine;

public class CampFire : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _fireTimeText;
    private float _fireTime = 100f; // Бревно + 20 секунд, враг -10 секунд,
    private bool _isBurning = true;

    private void Awake()
    {
        if(_fireTimeText == null)
        {
            _fireTimeText = GameObject.Find("FireTimeText").GetComponent<TextMeshProUGUI>();
            if(_fireTimeText == null)
            {
                Debug.LogWarning("FireTimeTextNone");
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

    //--------работа с костром
    public void AddFireTime()
    {
        _fireTime += 20f;
        UpdateFireTimeText();
    }
    public void RemoveTime(float _enemy)
    {
        _fireTime -= _enemy;
        UpdateFireTimeText();
    }
    //--------

    public void UpdateFireTimeText()
    {
        _fireTimeText.text = _fireTime.ToString("F0");
    }
    //--------логика таймера костра
    private void StartFireTime()
    {
        if (!_isBurning)
        { 
            return;
        }
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
            loose();
        }
    }
    //--------

    //--------проигрыш итд
    private void loose()
    {
        Debug.Log("You lost!");
        //TODO: добавить проигрыш (мб спавн большого количества врагов итд)
    }


   /* private void OnCollisionEnter(Collision collision)
    {
        if (_isBurning)
        {
            switch(collision.gameObject.tag)
            {
                case "Enemy":
                    RemoveTime(10f);
                    Destroy(collision.gameObject);
                    break;
            }
        }
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerStats player))
        {
            player.SetCurrentCampFire(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerStats player))
        {
            player.SetCurrentCampFire(null);
        }
    }
}
