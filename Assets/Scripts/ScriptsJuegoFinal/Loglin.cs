using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Loglin : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private Coroutine stateRoutine;

    public enum LoglinState
    {
        Wandering,
        Chase,
        Attacking
    }

    [Header("State")]
    public LoglinState currentState;

    [Header("Wandering")]
    [SerializeField] private float wanderingArea = 6f;
    [SerializeField] private float minimumWanderingDistance = 3f;

    [Header("Aggro / Chase")]
    private bool isAggro = false;
    [SerializeField] private float chaseRange = 8f;
    [SerializeField] private float chaseTime = 5f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float attackCooldown = 1.5f;

    private bool canAttack = true;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
    }

    private void Start()
    {
        SetState(LoglinState.Wandering);
    }

    private void Update()
    {
        UpdateState();
    }

    #region FSM

    void SetState(LoglinState newState)
    {
        if (currentState == newState) return;

        if (stateRoutine != null)
            StopCoroutine(stateRoutine);

        currentState = newState;

        switch (currentState)
        {
            case LoglinState.Wandering:
                agent.isStopped = false;
                RandomWandering();
                break;

            case LoglinState.Chase:
                agent.isStopped = false;
                stateRoutine = StartCoroutine(ChaseTimer());
                break;

            case LoglinState.Attacking:
                agent.isStopped = true;
                break;
        }
    }

    void UpdateState()
    {
        switch (currentState)
        {
            case LoglinState.Wandering:
                UpdateWandering();
                break;

            case LoglinState.Chase:
                UpdateChase();
                break;

            case LoglinState.Attacking:
                UpdateAttack();
                break;
        }
    }

    #endregion

    #region Wandering

    void UpdateWandering()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= chaseRange && isAggro)
        {
            SetState(LoglinState.Chase);
            return;
        }

        bool reached = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
        bool stuck = agent.velocity.sqrMagnitude < 0.01f;

        if (reached || stuck)
        {
            RandomWandering();
        }
    }

    void RandomWandering()
    {
        const int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * wanderingArea;
            randomDir += transform.position;

            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, wanderingArea, NavMesh.AllAreas))
            {
                float dist = Vector3.Distance(transform.position, hit.position);

                if (dist >= minimumWanderingDistance)
                {
                    agent.SetDestination(hit.position);
                    return;
                }
            }
        }
    }

    #endregion

    #region Chase

    void UpdateChase()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        agent.SetDestination(player.position);

        if (distance <= attackRange)
        {
            SetState(LoglinState.Attacking);
        }
    }

    IEnumerator ChaseTimer()
    {
        yield return new WaitForSeconds(chaseTime);
        isAggro = false;
        SetState(LoglinState.Wandering);
    }

    #endregion

    #region Attack

    void UpdateAttack()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange + 0.5f)
        {
            SetState(LoglinState.Chase);
            return;
        }

        if (canAttack)
        {
            StartCoroutine(AttackCooldown());
            PerformAttack();
        }
    }

    void PerformAttack()
    {
        Debug.Log("Loglin ataca");
        // Aquí iría animación + daño
    }

    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    #endregion
}
