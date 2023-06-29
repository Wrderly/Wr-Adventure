using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SlimeStateType
{
    Idle, Patrol, Chase, Hurt, Attack, Death
}

[Serializable]
public class Parameter
{
    public float idleTime;
    //public float patrolLength;
    public float chaseLength;
    public Transform chasePoint;
    public Vector2 originPoint;
    public Transform target;
    public Vector3 targetPos;
    public LayerMask targetLayer;
    public Transform attackPoint;
    public float attackArea;

    public float patrolSpeed;
    public float chaseSpeed;
    
    public Animator animator;
    public RuntimeAnimatorController idleController;
    public RuntimeAnimatorController hurtController;
    public RuntimeAnimatorController jumpController;
    public RuntimeAnimatorController deathController;
}

public class SlimeFSM : EnemyFSM<SlimeStateType>, IEnemy
{
    public Parameter parameter;
    public GameObject SlimeObject;
    public GameObject bloodBar;

    public Transform[] patrolPoints;

    private void Awake()
    {
        parameter.originPoint = new Vector2(transform.position.x, transform.position.y);
    }

    void Start()
    {
        states.Add(SlimeStateType.Idle, new SlimeIdleState(this));
        states.Add(SlimeStateType.Patrol, new SlimePatrolState(this));
        states.Add(SlimeStateType.Chase, new SlimeChaseState(this));
        states.Add(SlimeStateType.Hurt, new SlimeHurtState(this));
        states.Add(SlimeStateType.Attack, new SlimeAttackState(this));
        states.Add(SlimeStateType.Death, new SlimeDeathState(this));

        TransitionState(SlimeStateType.Idle);
        parameter.animator = GetComponent<Animator>();
    }

    void Update()
    {
        curState.OnUpdate();
    }

    public void GetHit(float damage)
    {
        if (!attribute.isDead)
        {
            damage /= (1 + attribute.def / 100);
            attribute.health -= damage;
            if (attribute.health <= 0.0f)
            {
                attribute.isDead = true;
                TransitionState(SlimeStateType.Death);
            }
            else
            {
                TransitionState(SlimeStateType.Hurt);
            }
        }
    }

    public void FlipTo(Transform target)
    {
        if (target != null)
        {
            if (transform.position.x > target.position.x)
            {
                transform.localScale = new Vector3(-1, 1, 1); 
            }
            else if(transform.position.x < target.position.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    public void FlipTo(Vector2 target)
    {
        if (target != null)
        {
            if (transform.position.x > target.x)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (transform.position.x < target.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !collision.GetComponent<MyCharacterController>().parameter.isDead)
        {
            //Debug.Log("Player Enter");
            parameter.target = collision.transform;
            Collider2D collider = parameter.target.GetComponent<Collider2D>();
            Bounds bounds = collider.bounds;
            parameter.targetPos = bounds.center;
        }
        else
        {
            //Debug.Log("No Enter");
            //parameter.target = null;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            parameter.target = null;
            
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(parameter.attackPoint.position, parameter.attackArea);
        Gizmos.DrawWireSphere(parameter.chasePoint.position, parameter.chaseLength);
    }
}
