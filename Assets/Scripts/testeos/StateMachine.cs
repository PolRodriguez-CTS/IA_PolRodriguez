using UnityEngine;
using UnityEngine.AI;

public class StateMachine : MonoBehaviour
{
    private NavMeshAgent _agent;
    public enum State 
    {
        Patrolling,
        Chasing,
        Searching,
        Waiting,
        Attacking
    }
    private State _currentState;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _currentState = State.Patrolling;
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.Patrolling:
                Patrol();
            break;

            case State.Chasing:
                Chase();
            break;

            case State.Searching:
                Search();
            break;

            case State.Waiting:
                Wait();
            break;

            case State.Attacking:
                Attack();
            break;
        }
    }

    private void Patrol()
    {
        
    }

    private void Chase()
    {
        
    }

    private void Search()
    {
        
    }

    private void Wait()
    {
        
    }

    private void Attack()
    {
        
    }
}
