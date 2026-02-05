using UnityEngine;

public class Loglin : MonoBehaviour
{
    private UnityEngine.AI.NavMeshAgent _enemyAgent;

    public enum EnemyState
    {
        Patrolling,
        Chasing,
        Attacking
    }

    public EnemyState currentState;
    private Transform _player;


    private float speed = 10f;
    private Vector3 loglineDirection = Vector3.forward;

    void Patrolling()
    {
        
    }

    void RandomDirection()
    {
        //Vector3 
    }
}
