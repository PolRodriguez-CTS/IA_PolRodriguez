using UnityEngine;
using UnityEngine.AI;

public class EnemyGPT : MonoBehaviour
{
    private NavMeshAgent _enemyAgent;

    public enum EnemyState
    {
        Patrolling,
        Chasing,
        Searching,
        Waiting,
        Attacking
    }

    public EnemyState currentState;

    private Transform _player;
    private Vector3 _playerLastKnownPosition;

    // -------- PATRULLA --------
    [SerializeField] private Transform[] _patrolPoints;
    private int patrolIndex;

    // -------- DETECCIÓN --------
    [SerializeField] private float _detectionRange = 7f;
    [SerializeField] private float _detectionAngle = 90f;
    [SerializeField] private float _eyeHeight = 1.5f;

    // -------- BÚSQUEDA --------
    private float _searchTimer;
    [SerializeField] private float _searchWaitTime = 15f;
    [SerializeField] private float _searchRadius = 10f;

    // -------- ESPERA --------
    private float _waitTimer;
    [SerializeField] private float _waitTime = 2f;

    // -------- ATAQUE --------
    private float _attackCooldown;
    [SerializeField] private float _attackTime = 1.5f;
    [SerializeField] private float _attackRange = 1.5f;

    void Awake()
    {
        _enemyAgent = GetComponent<NavMeshAgent>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Start()
    {
        patrolIndex = 0;
        currentState = EnemyState.Patrolling;
        SetPatrolPoint();
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                Patrol();
                break;

            case EnemyState.Chasing:
                Chase();
                break;

            case EnemyState.Searching:
                Search();
                break;

            case EnemyState.Waiting:
                Wait();
                break;

            case EnemyState.Attacking:
                Attack();
                break;
        }
    }

    // =========================
    //          ESTADOS
    // =========================

    void Patrol()
    {
        if (CanSeePlayer())
        {
            currentState = EnemyState.Chasing;
            return;
        }

        if (!_enemyAgent.pathPending && _enemyAgent.remainingDistance <= 0.5f)
        {
            patrolIndex = (patrolIndex + 1) % _patrolPoints.Length;
            SetPatrolPoint();
            currentState = EnemyState.Waiting;
        }
    }

   void Chase()
{
    if (!CanSeePlayer())
    {
        currentState = EnemyState.Searching;
        return;
    }

    _enemyAgent.isStopped = false;
    _enemyAgent.SetDestination(_player.position);
    _playerLastKnownPosition = _player.position;

    // Medir la distancia real al jugador
    float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

    // Cambiar a ataque si está lo suficientemente cerca
    if (distanceToPlayer <= _attackRange)
    {
        _attackCooldown = 0;
        currentState = EnemyState.Attacking;
        Debug.Log("Entrando en ATAQUE");
    }
}



    void Search()
    {
        _searchTimer += Time.deltaTime;

        if (CanSeePlayer())
        {
            currentState = EnemyState.Chasing;
            return;
        }

        if (_searchTimer < _searchWaitTime)
        {
            if (!_enemyAgent.pathPending && _enemyAgent.remainingDistance <= 0.5f)
            {
                Vector3 randomPoint;
                if (RandomSearchPoint(_playerLastKnownPosition, _searchRadius, out randomPoint))
                {
                    _enemyAgent.SetDestination(randomPoint);
                }
            }
        }
        else
        {
            _searchTimer = 0;
            currentState = EnemyState.Patrolling;
            SetPatrolPoint();
        }
    }

    void Wait()
    {
        _enemyAgent.isStopped = true;
        _waitTimer += Time.deltaTime;

        if (_waitTimer >= _waitTime)
        {
            _waitTimer = 0;
            _enemyAgent.isStopped = false;
            currentState = EnemyState.Patrolling;
        }
    }

    void Attack()
{
    _enemyAgent.isStopped = true; // Detener al enemigo
    transform.LookAt(_player);     // Mirar al jugador

    _attackCooldown += Time.deltaTime;

    if (_attackCooldown <= 0.1f)
    {
        Debug.Log("ATAQUE ejecutado");
        // Aquí aplicas daño o empuje
    }

    if (_attackCooldown >= _attackTime)
    {
        _attackCooldown = 0;
        _enemyAgent.isStopped = false;
        currentState = EnemyState.Chasing;
    }
}


    // =========================
    //        DETECCIÓN
    // =========================

    bool CanSeePlayer()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * _eyeHeight;
        Vector3 directionToPlayer = _player.position - rayOrigin;

        float distance = directionToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (distance > _detectionRange)
            return false;

        if (angle > _detectionAngle * 0.5f)
            return false;

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, directionToPlayer.normalized, out hit, _detectionRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                _playerLastKnownPosition = _player.position;
                return true;
            }
        }

        return false;
    }

    // =========================
    //        UTILIDADES
    // =========================

    bool RandomSearchPoint(Vector3 center, float radius, out Vector3 point)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * radius;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, radius, NavMesh.AllAreas))
        {
            point = hit.position;
            return true;
        }

        point = Vector3.zero;
        return false;
    }

    void SetPatrolPoint()
    {
        _enemyAgent.SetDestination(_patrolPoints[patrolIndex].position);
    }

    // =========================
    //          GIZMOS
    // =========================

    void OnDrawGizmos()
    {
        if (_player == null) return;

        Gizmos.color = Color.red;
        foreach (Transform point in _patrolPoints)
        {
            Gizmos.DrawSphere(point.position, 0.3f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);

        Gizmos.color = Color.yellow;
        Vector3 left = Quaternion.AngleAxis(-_detectionAngle / 2, Vector3.up) * transform.forward * _detectionRange;
        Vector3 right = Quaternion.AngleAxis(_detectionAngle / 2, Vector3.up) * transform.forward * _detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.up * _eyeHeight, _player.position);
    }
}
