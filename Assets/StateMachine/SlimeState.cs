using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Physics2D;

//վ��״̬
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
        //��ҽ���׷�ٷ�Χ���ҵ�ǰλ��û�г���׷������
        if (parameter.target != null && Vector2.Distance(manager.transform.position, parameter.originPoint) < parameter.chaseLength)
        {
            //׷��״̬
            manager.TransitionState(SlimeStateType.Chase);
        }
        if (timer >= parameter.idleTime)
        {
            //Ѳ��״̬
            manager.TransitionState(SlimeStateType.Patrol);
        }
    }
    public void OnExit()
    {
        timer = 0.0f;
    }
}


//Ѳ��״̬
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
        //�趨Ѳ��Ŀ���
        Vector2 dstPoint = new Vector2(parameter.originPoint.x + parameter.patrolLength * patrolDirection[patrolPosition].x, 
            parameter.originPoint.y + parameter.patrolLength * patrolDirection[patrolPosition].y);
        //ת��
        manager.FlipTo(dstPoint);
        //��Ѳ�ߵ��ƶ�
        manager.transform.position = Vector2.MoveTowards(manager.transform.position,
            dstPoint, parameter.moveSpeed * Time.deltaTime);
        //��ҽ���׷�ٷ�Χ���ҵ�ǰλ��û�г���׷������
        if (parameter.target != null && Vector2.Distance(manager.transform.position, parameter.originPoint) < parameter.chaseLength)
        {
            //׷��״̬
            manager.TransitionState(SlimeStateType.Chase);
        }
        //�ﵽѲ������
        if (Vector2.Distance(manager.transform.position, dstPoint) < 0.1f)
        {
            //վ��
            manager.TransitionState(SlimeStateType.Idle);
        }
    }
    public void OnExit()
    {
        patrolPosition++;
        patrolPosition %= patrolDirection.Length;
    }
}

//׷��״̬
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
        {//��ֹ������ɫʹ�����������ɫ�޷������˳���ײ�е��¹����޷��˳�����״̬
            parameter.target = null;
        }
        //����Ŀ��
        manager.FlipTo(parameter.target);
        //׷��������Ŀ��
        if (parameter.target != null)
        {//׷��
            manager.transform.position = Vector2.MoveTowards(manager.transform.position,
            parameter.target.position, parameter.chaseSpeed * Time.deltaTime);
        }
        if (parameter.target == null || Vector2.Distance(manager.transform.position, parameter.originPoint) > parameter.chaseLength)
        {//Ŀ�궪ʧ���߳���׷������
            //վ��
            manager.TransitionState(SlimeStateType.Idle);
        }
        //Ŀ����빥����Χ
        if (parameter.target != null && Vector2.Distance(parameter.attackPoint.position, parameter.targetPos) < parameter.attackArea)
        {
            manager.TransitionState(SlimeStateType.Attack);
        }
    }
    public void OnExit()
    {

    }
}

//����״̬
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
        {//���������ж�
            if (!didAttack && parameter.target != null && Vector2.Distance(parameter.attackPoint.position, parameter.targetPos) < parameter.attackArea)
            {
                didAttack = true;//һ�ι���ֻ����һ�������ж�
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

//����״̬
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

//����״̬
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