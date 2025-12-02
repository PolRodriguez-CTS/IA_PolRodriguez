using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent _enemyAgent;

    public enum EnemyState
    {
        Patrolling,

        Chasing,

        Searching
    }

    public EnemyState currentState;
    [SerializeField] private Transform[] _patrolPoints;

    void Awake()
    {
        _enemyAgent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        currentState = EnemyState.Patrolling;
        SetRandomPatrolPoint();
    }

    void Update()
    {
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
            default:
                Patrol();
            break;
        }
    }

    void Patrol()
    {
        if(_enemyAgent.remainingDistance < 0.5f)
        {
            SetRandomPatrolPoint();
        }
    }

    void Chase()
    {
        
    }

    void Search()
    {
        
    }


    void SetRandomPatrolPoint()
    {
        _enemyAgent.SetDestination(_patrolPoints[Random.Range(0, _patrolPoints.Length)].position);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach(Transform point in _patrolPoints)
        {
            Gizmos.DrawSphere(point.position, 0.3f);
        }
    }
}
