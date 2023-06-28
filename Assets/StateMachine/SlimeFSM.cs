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
    public float maxHealth;
    public float health;
    public float atk;
    public float def;
    public float moveSpeed;
    public float chaseSpeed;
    public float idleTime;
    public float patrolLength;
    public float chaseLength;
    public Vector2 originPoint;
    public Transform target;
    public Vector3 targetPos;
    public LayerMask targetLayer;
    public Transform attackPoint;
    public float attackArea;
    //public bool isHit = false;
    public bool isDead = false;
    public Animator animator;
    public RuntimeAnimatorController idleController;
    public RuntimeAnimatorController hurtController;
    public RuntimeAnimatorController jumpController;
    public RuntimeAnimatorController deathController;
}

public class SlimeFSM : MonoBehaviour,IEnemy
{
    public Parameter parameter;
    private IState curState;
    private Dictionary<SlimeStateType, IState> states = new();
    public Image bloodBar;
    public GameObject bloodBarBackground;
    //private Quaternion initialBloodBarRotation;
    // private Quaternion flipBloodBarRotation;
    public Canvas canvas;
    public GameObject SlimeObject;
    private const float bloobBarLerpSpeed = 3f;

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

        bloodBar.fillAmount = parameter.health / parameter.maxHealth;
        //initialBloodBarRotation = bloodBarBackground.transform.rotation;
        //flipBloodBarRotation = initialBloodBarRotation;
        //flipBloodBarRotation.x = -initialBloodBarRotation.x;
        //bloodBarBackground = GetComponent<GameObject>();
    }

    void Update()
    {
        curState.OnUpdate();
        canvas.transform.position = transform.position;
    }

    public void GetHit(float damage)
    {
        //parameter.isHit = true;
        damage /= (1 + parameter.def / 100);
        parameter.health -= damage;
        StartCoroutine(UpDateBloodBar());
        TransitionState(SlimeStateType.Hurt);
        if (parameter.health <= 0.0f && !parameter.isDead)
        {
            parameter.isDead = true;
            TransitionState(SlimeStateType.Death);
        }
    }

    public void TransitionState(SlimeStateType type)
    {
        if (curState != null)
        {
            curState.OnExit();
        }
        curState = states[type];
        curState.OnEnter();
    }

    public void FlipTo(Transform target)
    {
        if (target != null)
        {
            if (transform.position.x > target.position.x)
            {
                transform.localScale = new Vector3(-1, 1, 1);
                //bloodBarBackground.transform.rotation.
                
            }
            else if(transform.position.x < target.position.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
                //bloodBarBackground.transform.rotation = initialBloodBarRotation;
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
                //bloodBarBackground.transform.rotation = flipBloodBarRotation;
            }
            else if (transform.position.x < target.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
                //bloodBarBackground.transform.rotation = initialBloodBarRotation;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !collision.GetComponent<MyCharacterController>().parameter.isDead)
        {
            parameter.target = collision.transform;
            Collider2D collider = parameter.target.GetComponent<Collider2D>();
            Bounds bounds = collider.bounds;
            parameter.targetPos = bounds.center;
        }
        else
        {
            parameter.target = null;
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

    }

    private IEnumerator UpDateBloodBar()
    {
        float timer = 1f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            bloodBar.fillAmount = Mathf.Lerp(bloodBar.fillAmount, parameter.health / parameter.maxHealth, bloobBarLerpSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
