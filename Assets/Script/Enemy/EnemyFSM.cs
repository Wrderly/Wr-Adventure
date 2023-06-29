using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAttribute
{
    EnemyAttribute attribute { get; set; }
}

//����״̬������
public abstract class EnemyFSM<ENUM> : MonoBehaviour, IEnemyAttribute where ENUM : System.Enum
{
    //ÿ������Ӧ���������˵����ԡ���ǰ״̬����Ӧ�����е�״̬��ö��
    public EnemyAttribute _attribute;
    public EnemyAttribute attribute { get { return _attribute; } set { _attribute = value; } }
    protected IState curState;
    protected Dictionary<ENUM, IState> states = new();

    void Start()
    {
        //������ Ӧ�������˵�״̬ö�����Ӧ��״̬���ʵ��ע�ᵽ״̬�ֵ���
    }

    void Update()
    {
        curState.OnUpdate();
    }

    public void TransitionState(ENUM type)
    {
        if (curState != null)
        {
            curState.OnExit();
        }
        curState = states[type];
        curState.OnEnter();
    }
}
