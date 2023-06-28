using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Physics2D;

//站立状态
public class SlimeIdleState : IState
{
    private SlimeFSM manager;
    private Parameter parameter;
    private float timer;

    public SlimeIdleState(SlimeFSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        Debug.Log("Slime Idle");
        parameter.animator.runtimeAnimatorController = parameter.idleController;
    }
    public void OnUpdate()
    {
        timer += Time.deltaTime;
        //玩家进入追踪范围并且当前位置没有超出追踪上限
        if (parameter.target != null && Vector2.Distance(manager.transform.position, parameter.originPoint) < parameter.chaseLength)
        {
            //追踪状态
            manager.TransitionState(SlimeStateType.Chase);
        }
        if (timer >= parameter.idleTime)
        {
            //巡逻状态
            manager.TransitionState(SlimeStateType.Patrol);
        }
    }
    public void OnExit()
    {
        timer = 0.0f;
    }
}


//巡逻状态
public class SlimePatrolState : IState
{
    private SlimeFSM manager;
    private Parameter parameter;
    private int patrolPosition = 0;
    private Vector2[] patrolDirection = { new Vector2(1, 1), new Vector2(-1, -1) };

    public SlimePatrolState(SlimeFSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        Debug.Log("Slime Patrol");
        parameter.animator.runtimeAnimatorController = parameter.idleController;
    }
    public void OnUpdate()
    {
        //设定巡逻目标点
        Vector2 dstPoint = new Vector2(parameter.originPoint.x + parameter.patrolLength * patrolDirection[patrolPosition].x, 
            parameter.originPoint.y + parameter.patrolLength * patrolDirection[patrolPosition].y);
        //转向
        manager.FlipTo(dstPoint);
        //向巡逻点移动
        manager.transform.position = Vector2.MoveTowards(manager.transform.position,
            dstPoint, parameter.moveSpeed * Time.deltaTime);
        //玩家进入追踪范围并且当前位置没有超出追踪上限
        if (parameter.target != null && Vector2.Distance(manager.transform.position, parameter.originPoint) < parameter.chaseLength)
        {
            //追踪状态
            manager.TransitionState(SlimeStateType.Chase);
        }
        //达到巡逻上限
        if (Vector2.Distance(manager.transform.position, dstPoint) < 0.1f)
        {
            //站立
            manager.TransitionState(SlimeStateType.Idle);
        }
    }
    public void OnExit()
    {
        patrolPosition++;
        patrolPosition %= patrolDirection.Length;
    }
}

//追踪状态
public class SlimeChaseState : IState
{
    private SlimeFSM manager;
    private Parameter parameter;
    ContactFilter2D filter;

    public SlimeChaseState(SlimeFSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
        filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Player"));
    }

    public void OnEnter()
    {
        Debug.Log("Slime Chase");
        parameter.animator.runtimeAnimatorController = parameter.idleController;
    }
    public void OnUpdate()
    {
        if (parameter.target != null && parameter.target.GetComponent<MyCharacterController>().parameter.isDead)
        {//防止攻击角色使其阵亡后，因角色无法自主退出碰撞盒导致怪物无法退出攻击状态
            parameter.target = null;
        }
        //朝向目标
        manager.FlipTo(parameter.target);
        //追踪区内有目标
        if (parameter.target != null)
        {//追踪
            manager.transform.position = Vector2.MoveTowards(manager.transform.position,
            parameter.target.position, parameter.chaseSpeed * Time.deltaTime);
        }
        if (parameter.target == null || Vector2.Distance(manager.transform.position, parameter.originPoint) > parameter.chaseLength)
        {//目标丢失或者超出追踪上限
            //站立
            manager.TransitionState(SlimeStateType.Idle);
        }
        //目标进入攻击范围
        if (parameter.target != null && Vector2.Distance(parameter.attackPoint.position, parameter.targetPos) < parameter.attackArea)
        {
            manager.TransitionState(SlimeStateType.Attack);
        }
    }
    public void OnExit()
    {

    }
}

//攻击状态
public class SlimeAttackState : IState
{
    private SlimeFSM manager;
    private Parameter parameter;
    private Vector2 dstPoint;
    private float timer = 0.0f;
    private bool didAttack = false;

    public SlimeAttackState(SlimeFSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        Debug.Log("Slime Attack");
        parameter.animator.runtimeAnimatorController = parameter.jumpController;
        if (parameter.target != null)
        {
            dstPoint = parameter.target.position;
        }
    }
    public void OnUpdate()
    {
        timer += Time.deltaTime;
        if(timer < 1.25)
        {
            manager.transform.position = Vector2.MoveTowards(manager.transform.position,
            dstPoint, 0.1f * Time.deltaTime);
        }
        else if(1.25 < timer && timer < 2.2)
        {
            manager.transform.position = Vector2.MoveTowards(manager.transform.position,
            dstPoint, -0.1f * Time.deltaTime);
        }
        else if(timer > 2.2)
        {
            manager.TransitionState(SlimeStateType.Chase);
        }

        if(0.9f < timer && timer < 1.6f)
        {//攻击命中判定
            if (!didAttack && parameter.target != null && Vector2.Distance(parameter.attackPoint.position, parameter.targetPos) < parameter.attackArea)
            {
                didAttack = true;//一次攻击只进行一次命中判定
                parameter.target.GetComponent<IEnemy>().GetHit(parameter.atk);
            }
        }
    }
    public void OnExit()
    {
        timer = 0.0f;
        didAttack = false;
    }
}

//受伤状态
public class SlimeHurtState : IState
{
    private SlimeFSM manager;
    private Parameter parameter;
    private Vector2 dstPoint;
    private float timer = 0.0f;

    public SlimeHurtState(SlimeFSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        Debug.Log("Slime Hurt");
        parameter.animator.runtimeAnimatorController = parameter.hurtController;
        if (parameter.target != null)
        {
            dstPoint = parameter.target.position;
        }
        else
        {
            dstPoint = manager.transform.position;
        }
    }
    public void OnUpdate()
    {
        timer += Time.deltaTime;
        if (timer < 0.10f)
        {
            manager.transform.position = Vector2.MoveTowards(manager.transform.position,
                dstPoint, -2f * Time.deltaTime);
        }
        if (timer > 1.0f)
        {
            manager.TransitionState(SlimeStateType.Chase);
        }
    }
    public void OnExit()
    {
        timer = 0.0f;
    }
}

//死亡状态
public class SlimeDeathState : IState
{
    private SlimeFSM manager;
    private Parameter parameter;
    private float timer = 0.0f;
    private bool didDeathAnimator = false;

    public SlimeDeathState(SlimeFSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        Debug.Log("Slime Death");
        parameter.animator.runtimeAnimatorController = parameter.deathController;
        manager.bloodBarBackground.SetActive(false);
    }
    public void OnUpdate()
    {
        if (!didDeathAnimator)
        {
            timer += Time.deltaTime;
            if (timer > 1.4f)
            {
                didDeathAnimator = true;
                manager.SlimeObject.SetActive(false);
                timer = 0.0f;
            }
        }
    }
    public void OnExit()
    {
        timer = 0.0f;
    }
}