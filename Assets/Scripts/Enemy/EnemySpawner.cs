using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    // Событие завершения волны
    public static event Action OnWaveCompleted;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] _enemyPrefabs;
    [SerializeField] private CampFire _campFire;

    [Header("Spawn Settings")]
    [SerializeField] private float _spawnInterval = 5f;
    [SerializeField] private float _minRadius = 10f;
    [SerializeField] private float _maxRadius = 20f;

    [Header("Wave Scaling")]
    [SerializeField] private int _baseEnemiesCount = 10; // Базовый размер волны
    [SerializeField] private int _additionalEnemiesPerLevel = 3; // Сколько врагов добавляется за уровень урона

    private int _maxEnemiesToEnd;
    private float _timer;
    private int _spawnedEnemiesCount = 0;
    private int _aliveEnemiesCount = 0;
    private bool _isSpawningFinished = false;

    private void Start()
    {
        CalculateWaveSize();

        if (_campFire == null)
        {
            _campFire = FindAnyObjectByType<CampFire>();
            if (_campFire == null)
            {
                Debug.LogError("CampFire not found in the scene.");
            }
        }
    }

    private void CalculateWaveSize()
    {
        int damageLevel = 0;
        if (GameData.Instance != null)
        {
            damageLevel = GameData.Instance.damageLevel;
        }

        // Вычисляем общее количество врагов в волне
        _maxEnemiesToEnd = _baseEnemiesCount + (damageLevel * _additionalEnemiesPerLevel);
    }

    private void Update()
    {
        if (_isSpawningFinished || _campFire == null || _enemyPrefabs == null || _enemyPrefabs.Length == 0)
        {
            return;
        }

        _timer += Time.deltaTime;

        if (_timer > _spawnInterval)
        {
            SpawnRandomEnemy();
            _timer = 0f;
        }
    }

    private void OnEnable()
    {
        CampFire.OnTimeEnd += StopSpawn;
    }

    private void OnDisable()
    {
        CampFire.OnTimeEnd -= StopSpawn;
    }

    private void StopSpawn()
    {
        _isSpawningFinished = true;
    }

    private void SpawnRandomEnemy()
    {
        if (_spawnedEnemiesCount >= _maxEnemiesToEnd)
        {
            _isSpawningFinished = true;
            return;
        }

        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(_minRadius, _maxRadius);
        Vector3 spawnPosition = _campFire.transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            int randomIndex = UnityEngine.Random.Range(0, _enemyPrefabs.Length);
            GameObject selectedPrefab = _enemyPrefabs[randomIndex];

            Instantiate(selectedPrefab, hit.position, Quaternion.identity);

            _spawnedEnemiesCount++;
            _aliveEnemiesCount++;

            if (_spawnedEnemiesCount >= _maxEnemiesToEnd)
            {
                _isSpawningFinished = true;
            }
        }
    }

    public void RegisterEnemyDeath()
    {
        _aliveEnemiesCount--;

        // Если заспавнили всех И убили последнего живого
        if (_isSpawningFinished && _aliveEnemiesCount <= 0)
        {
            Debug.Log("Волна полностью отбита!");
            OnWaveCompleted?.Invoke();
        }
    }

    public bool isEnd()
    {
        return _isSpawningFinished && _aliveEnemiesCount <= 0;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = _campFire != null ? _campFire.transform.position : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, _minRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, _maxRadius);
    }
}