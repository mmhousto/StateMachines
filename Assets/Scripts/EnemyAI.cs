using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class EnemyAI : MonoBehaviour
{
    // CAMEL CASE - Naming convention for variables
    public Transform target;
    public Transform patrolPoint;
    private NavMeshAgent ai;
    public enum EnemyState { Idle, Patrol, Chase, Attack}
    public EnemyState enemyState;
    private Animator anim;
    private float distanceToTarget;
    Coroutine idleToPatrol;
    private bool isJumping;

    // Start is called before the first frame update
    void Start()
    {
        ai = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        enemyState = EnemyState.Idle;
        distanceToTarget = Mathf.Abs(Vector3.Distance(target.position, transform.position));
        isJumping = false;
    }

    // Update is called once per frame
    void Update()
    {
        distanceToTarget = Vector3.Distance(target.position, transform.position);

        switch (enemyState)
        {
            case EnemyState.Idle:
                SwitchState(0);

                ai.SetDestination(transform.position);

                if (idleToPatrol == null)
                {
                    idleToPatrol = StartCoroutine(SwitchToPatrol());
                }

                break;
            case EnemyState.Patrol:
                float distanceToPatrolPoint = Mathf.Abs(Vector3.Distance(patrolPoint.position, transform.position));

                if (distanceToPatrolPoint > 2)
                {
                    if (ai.isOnOffMeshLink)
                    {
                        ai.speed = 1f;

                        if(isJumping == false)
                        {
                            isJumping = true;
                            anim.SetTrigger("JumpDown");
                            StartCoroutine(WaitTilJumped());
                        }
                        
                    }
                    else if (isJumping == false)
                    {
                        ai.speed = 3.5f;
                        SwitchState(1);
                        ai.SetDestination(patrolPoint.position);
                    }
                }else
                {
                    SwitchState(0);
                }

                if (distanceToTarget <= 15)
                {
                    enemyState = EnemyState.Chase;
                }

                break;
            case EnemyState.Chase:
                SwitchState(2);

                ai.SetDestination(target.position);
                if (ai.isOnNavMesh) anim.SetTrigger("JumpDown");

                if (distanceToTarget <= 5)
                {
                    enemyState = EnemyState.Attack;
                }else if (distanceToTarget > 15)
                {
                    enemyState = EnemyState.Idle;
                }

                break;
            case EnemyState.Attack:
                SwitchState(3);

                if (distanceToTarget > 5 && distanceToTarget <= 15)
                {
                    enemyState = EnemyState.Chase;
                }else if(distanceToTarget > 15)
                    enemyState = EnemyState.Idle;

                break;
            default:

                break;
        }
    }

    IEnumerator WaitTilJumped()
    {
        yield return new WaitForSeconds(2.3f);
        isJumping = false;
    }

    IEnumerator SwitchToPatrol()
    {
        yield return new WaitForSeconds(5);
        enemyState = EnemyState.Patrol;
        idleToPatrol = null;
    }

    /// <summary>
    /// Switches animation to next state
    /// </summary>
    /// <param name="newState">Animation State to switch to</param>
    private void SwitchState(int newState)
    {
        if (anim.GetInteger("State") != newState)
            anim.SetInteger("State", newState);
    }
}
