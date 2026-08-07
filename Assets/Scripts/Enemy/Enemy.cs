using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("HealthStats")]
    [SerializeField] private float _health = 30f;
    [SerializeField] private bool _isSuicide = true;
    [Header("AttackStats")]
    [SerializeField] private float _damageToFire = 10f;
    [SerializeField] private float _attackDistance = 2f;
    [SerializeField] private float _attackCooldown = 2f;
    
    
    private NavMeshAgent _navMeshAgent;
    private CampFire _campFire;
    private float _lastAttackTime;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    
    }
    private void Start()
    {
        _campFire = FindAnyObjectByType<CampFire>();
        if (_campFire != null)
        {
            _navMeshAgent.SetDestination(_campFire.transform.position);
        }
        else
        {
            Debug.LogWarning("CampFire not found in the scene.");
        }
    }


    private void Update()
    {
        if(_campFire == null)
        {
            return;
        }

        float distanceToCampFire = Vector3.Distance(transform.position, _campFire.transform.position);
        
        if (distanceToCampFire <= _attackDistance)
        {
            _navMeshAgent.isStopped = true;
            if(Time.time >= _lastAttackTime + _attackCooldown)
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
        _campFire.RemoveTime(_damageToFire);
        
        if(_isSuicide)
        {
            Die();
        }
    }
    public void TakeDamage(float damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            Die();

        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
