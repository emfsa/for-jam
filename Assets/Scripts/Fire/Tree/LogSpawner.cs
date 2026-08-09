using UnityEngine;

public class LogSpawner : MonoBehaviour
{
    [Header("Prefabs & Center Target")]
    [SerializeField] private GameObject _logPrefab;    
    [SerializeField] private Transform _centerPoint;   

    [Header("Spawn Settings")]
    [SerializeField] private float _spawnInterval = 5f; 
    [SerializeField] private float _minRadius = 3f;     
    [SerializeField] private float _maxRadius = 10f;     
    [SerializeField] private int _maxLogsOnScene = 10;   

    private float _timer;

    private void Start()
    {
        if (_centerPoint == null)
        {
            CampFire camp = FindAnyObjectByType<CampFire>();
            if (camp != null) _centerPoint = camp.transform;
        }
    }

    private void Update()
    {
        if (_centerPoint == null || _logPrefab == null) return;

        int currentLogsCount = GameObject.FindGameObjectsWithTag("Log").Length;
        if (currentLogsCount >= _maxLogsOnScene) return;

        _timer += Time.deltaTime;

        if (_timer >= _spawnInterval)
        {
            SpawnLog();
            _timer = 0f;
        }
    }

    private void SpawnLog()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(_minRadius, _maxRadius);

        Vector3 spawnPosition = _centerPoint.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        Instantiate(_logPrefab, spawnPosition, randomRotation);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = _centerPoint != null ? _centerPoint.position : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, _minRadius);
        Gizmos.DrawWireSphere(center, _maxRadius);
    }
}