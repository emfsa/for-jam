using UnityEngine;
using UnityEngine.SceneManagement;

public class TreeLogic : MonoBehaviour
{
    [SerializeField] private int _logCount = 2;
    [SerializeField] private float _treeHealth = 30f;
    [SerializeField] private GameObject _logPrefab;

    [Header("Ground Placement Settings")]
    [SerializeField] private LayerMask _groundLayer; // Укажите слой земли/рельефа
    [SerializeField] private float _raycastDistance = 10f;
    [SerializeField] private float _spawnYOffset = 0.2f; // Небольшой отступ вверх, чтобы бревна не проваливались сквозь землю
    private SceneManager _sceneManager;
    public void TakeDamage(float damage)
    {
        _treeHealth -= damage;
        if (_treeHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (_logPrefab != null)
        {
            for (int i = 0; i < _logCount; i++)
            {
                // Смещение по X и Z
                Vector3 horizontalOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));

                // Стартовая точка для луча (чуть выше текущей позиции дерева)
                Vector3 rayOrigin = transform.position + horizontalOffset + Vector3.up * 2f;
                Vector3 spawnPosition = transform.position + horizontalOffset;

                // Пускаем луч вниз, чтобы найти поверхность земли
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, _raycastDistance, _groundLayer))
                {
                    spawnPosition = hit.point + Vector3.up * _spawnYOffset;
                }

                Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                Instantiate(_logPrefab, spawnPosition, randomRotation);
            }
        }

        Destroy(gameObject);
    }
}