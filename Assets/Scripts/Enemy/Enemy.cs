using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("HealthStats")]
    [SerializeField] private float _baseHealth = 30f;
    [SerializeField] private float _healthPerLevel = 10f; // Прирост ХП за каждый уровень урона
    [SerializeField] private bool _isSuicide = true;

    [Header("AttackStats")]
    [SerializeField] private float _baseDamageToFire = 10f;
    [SerializeField] private float _damagePerLevel = 2f; // Прирост урона костру за уровень
    [SerializeField] private float _attackDistance = 2f;
    [SerializeField] private float _attackCooldown = 2f;

    [Header("Loot")]
    [SerializeField] private int _baseMoney = 20;
    [SerializeField] private int _moneyPerLevel = 5; // Доп. деньги за каждый уровень урона

    private float _currentHealth;
    private float _currentDamageToFire;
    private int _currentMoney;

    private NavMeshAgent _navMeshAgent;
    private CampFire _campFire;
    private EnemySpawner _enemySpawner;
    private float _lastAttackTime;
    private bool _isDead = false;

    private PlayerStats _playerStats;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        _campFire = FindAnyObjectByType<CampFire>();
        _enemySpawner = FindAnyObjectByType<EnemySpawner>();

        ApplyStatsScaling();

        if (_campFire != null)
        {
            _navMeshAgent.SetDestination(_campFire.transform.position);
        }
        else
        {
            Debug.LogWarning("CampFire not found in the scene.");
        }
    }

    private void ApplyStatsScaling()
    {
        int damageLevel = 0;
        if (GameData.Instance != null)
        {
            damageLevel = GameData.Instance.damageLevel;
        }

        // Масштабируем характеристики от уровня урона игрока
        _currentHealth = _baseHealth + (damageLevel * _healthPerLevel);
        _currentDamageToFire = _baseDamageToFire + (damageLevel * _damagePerLevel);
        _currentMoney = _baseMoney + (damageLevel * _moneyPerLevel);
    }

    private void Update()
    {
        if (_campFire == null || _isDead)
        {
            return;
        }

        float distanceToCampFire = Vector3.Distance(transform.position, _campFire.transform.position);

        if (distanceToCampFire <= _attackDistance)
        {
            _navMeshAgent.isStopped = true;
            if (Time.time >= _lastAttackTime + _attackCooldown)
            {
                AttackCamp();
                _lastAttackTime = Time.time;
            }
        }
        else
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(_campFire.transform.position);
        }
    }

    private void AttackCamp()
    {
        _campFire.RemoveTime(_currentDamageToFire);

        if (_isSuicide)
        {
            Die(false);
        }
    }

    public void TakeDamage(float damage, PlayerStats attacker)
    {
        if (_isDead) return;

        _playerStats = attacker;
        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {
            Die(true);
        }
    }

    private void Die(bool giveReward)
    {
        if (_isDead) return;
        _isDead = true;

        if (giveReward && _playerStats != null)
        {
            _playerStats.AddMoney(_currentMoney);
        }

        if (_enemySpawner != null)
        {
            _enemySpawner.RegisterEnemyDeath();
        }

        Destroy(gameObject);
    }
}