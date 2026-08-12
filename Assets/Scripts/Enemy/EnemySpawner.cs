using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] _enemyPrefabs;
    [SerializeField] private CampFire _campFire;

    [Header("Spawn Settings")]
    [SerializeField] private float _spawnInterval = 5f;
    [SerializeField] private float _minRadius = 10f;
    [SerializeField] private float _maxRadius = 20f;

    private float _timer;

    private void Start()
    {
        if(_campFire == null)
        {
            _campFire = FindAnyObjectByType<CampFire>();
            if (_campFire == null)
            {
                Debug.LogError("CampFire not found in the scene.");
            }
        }
    }

    private void Update()
    {
        if(_campFire == null || _enemyPrefabs == null || _enemyPrefabs.Length == 0)
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


    private void SpawnRandomEnemy()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(_minRadius, _maxRadius);
        Vector3 spawnPosition = _campFire.transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        if(NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            int randomIndex = Random.Range(0, _enemyPrefabs.Length);
            GameObject selectedPrefab = _enemyPrefabs[randomIndex];

            Instantiate(selectedPrefab, hit.position, Quaternion.identity);
        }
    }

    //удалить потом
    private void OnDrawGizmosSelected()
    {
        Vector3 center = _campFire != null ? _campFire.transform.position : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, _minRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, _maxRadius);
    }
}
