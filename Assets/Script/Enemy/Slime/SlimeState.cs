using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Physics2D;

//站立状态
public class SlimeIdleState : IState
{
    private SlimeFSM fsm;
    //private Parameter parameter;
    private float timer;

    public SlimeIdleState(SlimeFSM fsm)
    {
        this.fsm = fsm;
        //this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        Debug.Log("Slime Idle");
        fsm.parameter.animator.runtimeAnimatorController = fsm.parameter.idleController;
    }
    public void OnUpdate()
    {
        timer += Time.deltaTime;
        //玩家进入追踪范围并且当前位置没有超出追踪上限
        if (fsm.parameter.target != null && Vector2.Distance(fsm.transform.position, fsm.parameter.originPoint) < fsm.parameter.chaseLength)
        {
            //追踪状态
            fsm.TransitionState(SlimeStateType.Chase);
        }
        if (timer >= fsm.parameter.idleTime)
        {
            //巡逻状态
            fsm.TransitionState(SlimeStateType.Patrol);
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
    private SlimeFSM fsm;
    //private Parameter parameter;
    private int patrolPosition = 0;
    private Vector2[] patrolDirection = { new Vector2(1, 1), new Vector2(-1, -1) };

    public SlimePatrolState(SlimeFSM fsm)
    {
        this.fsm = fsm;
        //this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        Debug.Log("Slime Patrol");
        fsm.parameter.animator.runtimeAnimatorController = fsm.parameter.idleController;
    }
    public void OnUpdate()
    {
        //设定巡逻目标点
        Vector2 dstPoint = new Vector2(fsm.parameter.originPoint.x + fsm.parameter.patrolLength * patrolDirection[patrolPosition].x,
            fsm.parameter.originPoint.y + fsm.parameter.patrolLength * patrolDirection[patrolPosition].y);
        //转向
        fsm.FlipTo(dstPoint);
        //向巡逻点移动
        fsm.transform.position = Vector2.MoveTowards(fsm.transform.position,
            dstPoint, fsm.parameter.patrolSpeed * Time.deltaTime);
        //玩家进入追踪范围并且当前位置没有超出追踪上限
        if (fsm.parameter.target != null && Vector2.Distance(fsm.transform.position, fsm.parameter.originPoint) < fsm.parameter.chaseLength)
        {
            //追踪状态
            fsm.TransitionState(SlimeStateType.Chase);
        }
        //达到巡逻上限
        if (Vector2.Distance(fsm.transform.position, dstPoint) < 0.1f)
        {
            //站立
            fsm.TransitionState(SlimeStateType.Idle);
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
    private SlimeFSM fsm;
    //private Parameter parameter;
    ContactFilter2D filter;

    public SlimeChaseState(SlimeFSM fsm)
    {
        this.fsm = fsm;
        //this.parameter = manager.parameter;
        filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Player"));
    }

    public void OnEnter()
    {
        Debug.Log("Slime Chase");
        fsm.parameter.animator.runtimeAnimatorController = fsm.parameter.idleController;
    }
    public void OnUpdate()
    {
        if (fsm.parameter.target != null && fsm.parameter.target.GetComponent<MyCharacterController>().parameter.isDead)
        {//防止攻击角色使其阵亡后，因角色无法自主退出碰撞盒导致怪物无法退出攻击状态
            fsm.parameter.target = null;
        }
        //朝向目标
        fsm.FlipTo(fsm.parameter.target);
        //追踪区内有目标
        if (fsm.parameter.target != null)
        {//追踪
            fsm.transform.position = Vector2.MoveTowards(fsm.transform.position,
            fsm.parameter.target.position, fsm.parameter.chaseSpeed * Time.deltaTime);
        }
        if (fsm.parameter.target == null || Vector2.Distance(fsm.transform.position, fsm.parameter.originPoint) > fsm.parameter.chaseLength)
        {//目标丢失或者超出追踪上限
            //站立
            fsm.TransitionState(SlimeStateType.Idle);
        }
        //目标进入攻击范围
        if (fsm.parameter.target != null && Vector2.Distance(fsm.parameter.attackPoint.position, fsm.parameter.targetPos) < fsm.parameter.attackArea)
        {
            fsm.TransitionState(SlimeStateType.Attack);
        }
    }
    public void OnExit()
    {

    }
}

//攻击状态
public class SlimeAttackState : IState
{
    private SlimeFSM fsm;
    //private Parameter parameter;
    private Vector2 dstPoint;
    private float timer = 0.0f;
    private bool didAttack = false;

    public SlimeAttackState(SlimeFSM fsm)
    {
        this.fsm = fsm;
        //this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        Debug.Log("Slime Attack");
        fsm.parameter.animator.runtimeAnimatorController = fsm.parameter.jumpController;
        if (fsm.parameter.target != null)
        {
            dstPoint = fsm.parameter.target.position;
        }
    }
    public void OnUpdate()
    {
        timer += Time.deltaTime;
        if(timer < 1.25)
        {
            fsm.transform.position = Vector2.MoveTowards(fsm.transform.position,
            dstPoint, 0.1f * Time.deltaTime);
        }
        else if(1.25 < timer && timer < 2.2)
        {
            fsm.transform.position = Vector2.MoveTowards(fsm.transform.position,
            dstPoint, -0.1f * Time.deltaTime);
        }
        else if(timer > 2.2)
        {
            fsm.TransitionState(SlimeStateType.Chase);
        }

        if(0.9f < timer && timer < 1.6f)
        {//攻击命中判定
            if (!didAttack && fsm.parameter.target != null && Vector2.Distance(fsm.parameter.attackPoint.position, fsm.parameter.targetPos) < fsm.parameter.attackArea)
            {
                didAttack = true;//一次攻击只进行一次命中判定
                fsm.parameter.target.GetComponent<IEnemy>().GetHit(fsm.attribute.atk);
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
    private SlimeFSM fsm;
    //private Parameter parameter;
    private Vector2 dstPoint;
    private float timer = 0.0f;

    public SlimeHurtState(SlimeFSM fsm)
    {
        this.fsm = fsm;
        //this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        Debug.Log("Slime Hurt");
        fsm.parameter.animator.runtimeAnimatorController = fsm.parameter.hurtController;
        if (fsm.parameter.target != null)
        {
            dstPoint = fsm.parameter.target.position;
        }
        else
        {
            dstPoint = fsm.transform.position;
        }
    }
    public void OnUpdate()
    {
        timer += Time.deltaTime;
        if (timer < 0.10f)
        {
            fsm.transform.position = Vector2.MoveTowards(fsm.transform.position,
                dstPoint, -2f * Time.deltaTime);
        }
        if (timer > 1.0f)
        {
            fsm.TransitionState(SlimeStateType.Chase);
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
    private SlimeFSM fsm;
    //private Parameter parameter;
    private float timer = 0.0f;
    private bool didDeathAnimator = false;

    public SlimeDeathState(SlimeFSM fsm)
    {
        this.fsm = fsm;
        //this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        Debug.Log("Slime Death");
        fsm.parameter.animator.runtimeAnimatorController = fsm.parameter.deathController;
        //manager.bloodBarBackground.SetActive(false);
        if (fsm.bloodBar != null)
        {
            fsm.bloodBar.SetActive(false);
        }
    }
    public void OnUpdate()
    {
        if (!didDeathAnimator)
        {
            timer += Time.deltaTime;
            if (timer > 1.4f)
            {
                didDeathAnimator = true;
                fsm.SlimeObject.SetActive(false);
                timer = 0.0f;
            }
        }
    }
    public void OnExit()
    {
        timer = 0.0f;
    }
}