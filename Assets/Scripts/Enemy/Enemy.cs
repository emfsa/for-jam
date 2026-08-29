using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
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

    [Header("Animations")]
    [SerializeField] private Animator _animator;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private List<AudioClip> _takeDamageSounds; // Звуки получения урона
    [SerializeField] private List<AudioClip> _attackSounds;     // Звуки атаки / взрыва суицидника
    [SerializeField] private List<AudioClip> _deathSounds;      // Звуки смерти
    [SerializeField] private List<AudioClip> _ambientSounds;    // Звуки рычания/ходьбы
    [SerializeField] private float _ambientSoundInterval = 5f; // Интервал воспроизведения фоновых звуков

    private float _currentHealth;
    private float _currentDamageToFire;
    private int _currentMoney;

    private NavMeshAgent _navMeshAgent;
    private CampFire _campFire;
    private EnemySpawner _enemySpawner;
    private float _lastAttackTime;
    private float _nextAmbientSoundTime;
    private bool _isDead = false;

    private PlayerStats _playerStats;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();

        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }

        // Настройка AudioSource для 3D-звука в пространстве
        if (_audioSource != null)
        {
            _audioSource.spatialBlend = 1.0f; // Полный 3D звук (тише на расстоянии)
            _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            _audioSource.minDistance = 2f;
            _audioSource.maxDistance = 20f;
        }
    }

    private void Start()
    {
        _campFire = FindAnyObjectByType<CampFire>();
        _enemySpawner = FindAnyObjectByType<EnemySpawner>();
        _animator = GetComponentInChildren<Animator>();
        ApplyStatsScaling();

        if (_campFire != null)
        {
            _navMeshAgent.SetDestination(_campFire.transform.position);
        }
        else
        {
            Debug.LogWarning("CampFire not found in the scene.");
        }

        _nextAmbientSoundTime = Time.time + Random.Range(1f, _ambientSoundInterval);
    }

    private void ApplyStatsScaling()
    {
        int damageLevel = 0;
        if (GameData.Instance != null)
        {
            damageLevel = GameData.Instance.damageLevel;
        }

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

        HandleAmbientSounds();

        float distanceToCampFire = Vector3.Distance(transform.position, _campFire.transform.position);

        if (distanceToCampFire <= _attackDistance)
        {
            _navMeshAgent.isStopped = true;
            if (Time.time >= _lastAttackTime + _attackCooldown)
            {
                switchAnim();
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

    private void HandleAmbientSounds()
    {
        if (_ambientSounds == null || _ambientSounds.Count == 0 || _isDead) return;

        if (Time.time >= _nextAmbientSoundTime)
        {
            PlayRandomClip(_ambientSounds);
            _nextAmbientSoundTime = Time.time + _ambientSoundInterval + Random.Range(-1f, 2f);
        }
    }

    private void switchAnim()
    {
        if (_animator == null || _isSuicide) return;

        _animator.SetBool("IsCampFire", true);
    }

    private void AttackCamp()
    {
        _campFire.RemoveTime(_currentDamageToFire);
        PlayRandomClip(_attackSounds);

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

        PlayRandomClip(_takeDamageSounds);

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

        // Играем звук смерти в точке уничтожения (AudioSource.PlayClipAtPoint),
        // чтобы звук не обрывался при вызве Destroy(gameObject)
        AudioClip deathClip = GetRandomClip(_deathSounds);
        if (deathClip != null)
        {
            AudioSource.PlayClipAtPoint(deathClip, transform.position, _audioSource != null ? _audioSource.volume : 1f);
        }

        Destroy(gameObject);
    }

    private void PlayRandomClip(List<AudioClip> clips)
    {
        AudioClip clip = GetRandomClip(clips);
        if (clip != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }

    private AudioClip GetRandomClip(List<AudioClip> clips)
    {
        if (clips == null || clips.Count == 0) return null;
        return clips[Random.Range(0, clips.Count)];
    }
}