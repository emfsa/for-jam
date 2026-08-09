using UnityEngine;

public class TreeLogic : MonoBehaviour
{
    [SerializeField] private int _logCount = 2;
    [SerializeField] private float _treeHealth = 30f;
    [SerializeField] private GameObject _logPrefab;
   
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

                Vector3 spawnOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
                Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                Instantiate(_logPrefab, transform.position + spawnOffset, randomRotation);
            }
        }
        Destroy(gameObject);
        
        

    }
}
