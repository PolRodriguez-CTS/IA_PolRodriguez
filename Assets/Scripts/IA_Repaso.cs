using UnityEngine;
using UnityEngine.AI;

public class IA_Repaso : MonoBehaviour
{
//---------------------------------------------------------------------------------------------------------------------------
    //2 puntos por montar el escenario de probuilder

    //Shift y mover con la herramienta de mover se extruye
    //Escaleras hacer los steps según la altura y no count, así la IA puede calcular los escalones que puede subir

    //Package de la IA (AI Navigation)

    //Bakear el nav mesh surface (generate links para los saltos y caídas, y heigh mesh para la altura de los escalones)
//---------------------------------------------------------------------------------------------------------------------------
    //2 puntos crear el enum y switch
    // //Capsula con el script y nav mesh agent

    //6 puntos crear la programación (patrullar, perseguir, atacar, etc), 3 estados

//---------------------------------------------------------------------------------------------------------------------------
[SerializeField] Transform player;
private NavMeshAgent navMeshAgent;
private State currentState;

//Cosas patrullar
[SerializeField] private Transform[] _patrolPoints;
[SerializeField] private int _patrolIndex;

//Cosas detección
[SerializeField] private float _detectionRange = 7f;
[SerializeField] private float _attackRange = 2f;

public enum State
{
    Patrolling,
    Chasing,
    Attacking
}

void Awake()
{
    navMeshAgent = GetComponent<NavMeshAgent>();
    player = GameObject.FindGameObjectWithTag("Player").transform;
}
    void Start()
    {
        SetPatrolPoint();
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrolling:
            Patrol();
            break;

            case State.Chasing:
            Chase();
            break;

            case State.Attacking:
            Attacking();
            break;

            default:
            Patrol();
            break;
        }
    }

    bool OnRange(float distance)
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if(distanceToPlayer < distance) return true;

        else
        {
            return false;
        }
    }

    void Patrol()
    {
        if(OnRange(_detectionRange))
        {
            currentState = State.Chasing;
        }

        if(navMeshAgent.remainingDistance < 0.1f)
        {
            SetRandomPatrolPoint();
        }
    }

    void SetRandomPatrolPoint()
    {
        navMeshAgent.SetDestination(_patrolPoints[Random.Range(0, _patrolPoints.Length)].position);
    }

    void SetPatrolPoint()
    {
        navMeshAgent.SetDestination(_patrolPoints[_patrolIndex].position);
        _patrolIndex = (_patrolIndex + 1) % _patrolPoints.Length;
    }

    void Chase()
    {
        if(!OnRange(_detectionRange))
        {
            currentState = State.Patrolling;
        }

        /*
        if(Vector3.Distance(transform.position, player.position) < _attackRange) {}
        */

        if(OnRange(_attackRange))
        {
            attackTimer = attackDelay;
            currentState = State.Attacking;
        }

        navMeshAgent.SetDestination(player.transform.position);
    }


    float attackTimer;
    float attackDelay = 2f;

    void Attacking()
    {
        if(!OnRange(_attackRange))
        {
            currentState = State.Chasing;
        }

        if(attackTimer < attackDelay)
        {
            attackTimer += Time.deltaTime;

            return;
        }

        Debug.Log("Ataque");
        attackTimer = 0;
    }
}
