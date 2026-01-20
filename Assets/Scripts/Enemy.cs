using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
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
    Transform _player;
    Vector3 _playerLastKnownPosition;

    //cosas patrullar
    [SerializeField] private Transform[] _patrolPoints;
    int patrolIndex;

    //cosas detección
    [SerializeField] private float _detectionRange = 7;
    [SerializeField] private float _detectionAngle = 90;

    //cosas busqueda
    private float _searchTimer;
    [SerializeField] private float _searchWaitTIme = 15;
    [SerializeField] private float _searchRadius = 10;

    //cosas de esperar
    private float _waitTimer;
    [SerializeField] private float _waitTime = 3;

    //cosas de atacar
    private float _attackTimer;
    [SerializeField] private float _attackTime = 2;
    [SerializeField] private float _attackRange = 1.5f;
    
    // Cooldown de ataque
    private float _attackCooldownTimer;
    [SerializeField] private float _attackCooldownTime = 3f;
    private bool _canAttack = true;

    

    void Awake()
    {
        _enemyAgent = GetComponent<NavMeshAgent>();
        _player = GameObject.FindWithTag("Player").transform;
    }

    void Start()
    {
        currentState = EnemyState.Patrolling;
        //SetRandomPatrolPoint();
        patrolIndex = 0;
        SetPatrolPoint();

    }

    void Update()
    {
        if (!_canAttack)
        {
            _attackCooldownTimer += Time.deltaTime;
            if (_attackCooldownTimer >= _attackCooldownTime)
            {
                _canAttack = true;
                _attackCooldownTimer = 0;
                Debug.Log("cooldown terminado");
            }
        }

        switch(currentState)
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
            default:
                Patrol();
                break;
        }
    }

    void Patrol()
    {
        Debug.Log("Patrullando");
        if(OnRange())
        {
            currentState = EnemyState.Chasing;
            return;
        }

        if(_enemyAgent.remainingDistance <= 0.5f && !_enemyAgent.pathPending)
        {
            patrolIndex = (patrolIndex + 1) % _patrolPoints.Length;
            Debug.Log(patrolIndex);
            SetPatrolPoint();
            currentState = EnemyState.Waiting;
        }
    }

    void Chase()
    {
        Debug.Log("Persiguiendo - Puede atacar: " + _canAttack);

        if(!OnRange())
        {
            currentState = EnemyState.Searching;
            _searchTimer = 0;
            return;
        }

        _enemyAgent.SetDestination(_player.position);
        _playerLastKnownPosition = _player.position;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
        
        if(distanceToPlayer <= _attackRange && _canAttack)
        {
            _attackTimer = 0;
            currentState = EnemyState.Attacking;
        }
    }

    void Search()
    {
        Debug.Log("Buscando");
        if(OnRange())
        {
            currentState = EnemyState.Chasing;
        }

        _searchTimer += Time.deltaTime;

        if(_searchTimer < _searchWaitTIme)
        {
            if(_enemyAgent.remainingDistance < 0.5f)
            {
                Vector3 randomPoint;
                if(RandomSearchPoint(_playerLastKnownPosition, _searchRadius, out randomPoint))
                {
                    _enemyAgent.SetDestination(randomPoint);
                }
            }
        }
        else
        {
            currentState = EnemyState.Patrolling;
            _searchTimer = 0;
        }
    }

    void Wait()
    {
        if(OnRange())
        {
            _enemyAgent.isStopped = false;
            currentState = EnemyState.Chasing;
            return;
        }

        _enemyAgent.isStopped = true;
        _waitTimer += Time.deltaTime;

        if(_waitTimer < _waitTime)
        {
            Debug.Log("esperando");
        }
        else
        {
            _waitTimer = 0;
            _enemyAgent.isStopped = false;
            currentState = EnemyState.Patrolling;
        }
    }

    void Attack()
    {
        if(_attackTimer == 0)
        {
            _enemyAgent.ResetPath();
            Debug.Log("Ataque");
        }

        _attackTimer += Time.deltaTime;

        if(_attackTimer >= _attackTime)
        {
            Debug.Log("cooldown...");
            
            _canAttack = false;
            _attackCooldownTimer = 0;
            
            currentState = EnemyState.Chasing;
            
            _attackTimer = 0;
        }        
    }

    bool RandomSearchPoint(Vector3 center, float radius, out Vector3 point)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * radius;

        NavMeshHit hit;

        if(NavMesh.SamplePosition(randomPoint, out hit, 4, NavMesh.AllAreas))
        {
            point = hit.position;
            return true;
        }

        point = Vector3.zero;
        return false;
    }


    void SetRandomPatrolPoint()
    {
        _enemyAgent.SetDestination(_patrolPoints[Random.Range(0, _patrolPoints.Length)].position);
    }

    void SetPatrolPoint()
    {
        _enemyAgent.SetDestination(_patrolPoints[patrolIndex].position);
    }

    bool OnRange()
    {
        Vector3 directionToPlayer = (_player.position - transform.position).normalized;

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        if(distanceToPlayer > _detectionRange)
        {
            return false;
        }

        if(angleToPlayer > _detectionAngle * 0.5)
        {
            return false;
        }

        RaycastHit hit;
        if(Physics.Raycast(transform.position, directionToPlayer, out hit, distanceToPlayer))
        {
            if(hit.collider.CompareTag("Player"))
            {
                _playerLastKnownPosition = _player.position;
                return true;
            }
        }

        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach(Transform point in _patrolPoints)
        {
            Gizmos.DrawSphere(point.position, 0.3f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);

        Gizmos.color = Color.yellow;

        Vector3 fovLine1 = Quaternion.AngleAxis(_detectionAngle * 0.5f, transform.up) * transform.forward * _detectionRange;
        Vector3 fovLine2 = Quaternion.AngleAxis(-_detectionAngle * 0.5f, transform.up) * transform.forward * _detectionRange;

        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);
    }
}
