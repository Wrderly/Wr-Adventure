using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAttribute
{
    EnemyAttribute attribute { get; set; }
}

//敌人状态机基类
public abstract class EnemyFSM<ENUM> : MonoBehaviour, IEnemyAttribute where ENUM : System.Enum
{
    //每个敌人应当包含敌人的属性、当前状态与所应当具有的状态的枚举
    public EnemyAttribute _attribute;
    public EnemyAttribute attribute { get { return _attribute; } set { _attribute = value; } }
    protected IState curState;
    protected Dictionary<ENUM, IState> states = new();

    void Start()
    {
        //在这里 应当将敌人的状态枚举与对应的状态类的实例注册到状态字典中
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
