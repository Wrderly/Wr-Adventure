using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
//本脚本用于角色控制

[Serializable]
public class MyCharacterParameter
{//角色属性类
    public float maxHealth;
    public float health;
    public float atk;
    public float def;
    public bool isDead = false;
}

public class MyCharacterController : MonoBehaviour,IEnemy
{//角色控制类
    //角色属性
    public MyCharacterParameter parameter;
    public float speed;
    //统一输入系统实例
    public @PlayerInput playerInput;
    public GameObject virtualCamera;
    //角色挂载 分别为动画控制、精灵控制和碰撞盒等等
    public Animator animator;
    //public SpriteRenderer characterRenderer;
    public Rigidbody2D characterRigidbody;
    //public BoxCollider2D characterCollider2D;
    //血条相关
    public Image bloodBar;
    private const float bloobBarLerpSpeed = 3f;
    //动画有限状态机参数引用
    /*
    private AnimatorControllerParameter ACPdoAttack1;
    private AnimatorControllerParameter ACPdoAttack2;
    private AnimatorControllerParameter ACPdoAttackTrans1Judge;
    private AnimatorControllerParameter ACPisJump;
    private AnimatorControllerParameter ACPisRun;
    private AnimatorControllerParameter ACPisSlide;
    private AnimatorControllerParameter ACPisCrouch;
    private AnimatorControllerParameter ACPisDash;
    private AnimatorControllerParameter ACPdoDashAttack;
    private AnimatorControllerParameter ACPisHurt;
    private AnimatorControllerParameter ACPisDead;
    */
    private int HashACPdoAttack1;
    private int HashACPdoAttack2;
    private int HashACPdoAttackTrans1Judge;
    private int HashACPisJump;
    private int HashACPisRun;
    private int HashACPisSlide;
    private int HashACPisCrouch;
    private int HashACPisDash;
    private int HashACPdoDashAttack;
    private int HashACPisHurt;
    private int HashACPisDead;
    //角色方向
    private bool isRight = true;
    private Vector2 vector2d;
    //角色逻辑状态控制
    private bool canAttack = true;
    private int numAttack = 0;
    private bool isJump = false;
    private bool isRun = false;
    private bool canRun = true;
    private bool isSlide = false;
    private bool isCrouch = false;
    private bool isDash = false;
    private bool isDashAttack = false;
    //角色顿帧卡肉控制
    private bool didHitPause = false;
    private const float hitPauseTime = 0.15f;
    private float originAnimatorSpeed;
    //角色碰撞盒记录参数
    //private float boxcolliderX;
    //声明一个布尔变量来表示是否使用按下按钮蹲，再按一下按钮起的方式
    private bool useCrouchToggle = true;

    private void Awake()
    {
        //获取角色挂载
        //characterRenderer = GetComponent<SpriteRenderer>();
        characterRigidbody = GetComponent<Rigidbody2D>();
        //characterCollider2D = GetComponent<BoxCollider2D>();
        //boxcolliderX = characterCollider2D.offset.x;
        //初始化统一输入系统并开启
        playerInput = new PlayerInput();
        playerInput.Enable();
        //获取动画控制器挂载 并初始化有限状态机参数
        animator = GetComponent<Animator>();
        InitACPParaHash();
    }

    private void Start()
    {
        //保存基本动画速度
        originAnimatorSpeed = animator.speed;
        bloodBar.fillAmount = parameter.health / parameter.maxHealth;
    }

    private void OnEnable()
    {
        //注册角色统一输入事件
        playerInput.Enable();
        playerInput.Player.Crouch.performed += OnCrouchPerformed;
        playerInput.Player.Crouch.canceled += OnCrouchCanceled;
        playerInput.Player.Attack.performed += StartAttack;
        playerInput.Player.Dash.performed += StartDash;
        playerInput.Player.Jump.performed += StartJump;
    }

    private void OnDisable()
    {
        //卸载统一输入事件
        playerInput.Disable();
        playerInput.Player.Crouch.performed -= OnCrouchPerformed;
        playerInput.Player.Crouch.canceled -= OnCrouchCanceled;
        playerInput.Player.Attack.performed -= StartAttack;
        playerInput.Player.Dash.performed -= StartDash;
        playerInput.Player.Jump.performed -= StartJump;
    }

    private void Update()
    {
        /*=======================================移动=======================================*/
        //读取统一输入
        vector2d = playerInput.Player.Run.ReadValue<Vector2>();
        if (canRun && vector2d != Vector2.zero)
        {
            //归一化输入
            vector2d.Normalize();
            //左右翻转
            if (vector2d.x < 0 && isRight)
            {
                isRight = false;
                transform.localScale = new Vector3(-1, 1, 0);
                //isDash = false;
                //animator.SetBool(HashACPisDash, false);
                //characterRenderer.flipX = true;
                //characterCollider2D.offset = new Vector2(-boxcolliderX, characterCollider2D.offset.y);
            }
            else if (vector2d.x > 0 && !isRight)
            {
                isRight = true;
                transform.localScale = new Vector3(1, 1, 0);
                //isDash = false;
                //animator.SetBool(HashACPisDash, false);
                //characterRenderer.flipX = false;
                //characterCollider2D.offset = new Vector2(boxcolliderX, characterCollider2D.offset.y);
            }
            //置跑动状态
            isRun = true;
            animator.SetBool(HashACPisRun, true);
            //根据是否冲刺、滑动或正常跑步置速度
            if (!isSlide && !isDash)
            {
                characterRigidbody.velocity = speed * vector2d;
            }
            else if(isSlide || isDash || isDashAttack)
            {
                characterRigidbody.velocity = 1.5f * speed * vector2d;
            }
        }
        else
        {
            //取消跑步状态
            isRun = false;
            animator.SetBool(HashACPisRun, false);
            characterRigidbody.velocity = Vector2.zero;
        }
        /*=======================================移动=======================================*/
    }

    /*====================================蹲下、滑行====================================*/
    void OnCrouchPerformed(InputAction.CallbackContext context)
    {
        // 如果不在跳跃状态，执行蹲下或滑行逻辑
        if (!isJump)
        {
            bool crouchPressed = playerInput.Player.Crouch.WasPressedThisFrame();

            if (useCrouchToggle && crouchPressed)//点击蹲 再点起
            {
                //切换isCrouch的值
                isCrouch = !isCrouch;
            }
            else if (!useCrouchToggle)//按住蹲 松开起
            {
                isCrouch = true;
            }

            if (isRun && !isSlide && crouchPressed)
            {
                //执行滑行逻辑
                isSlide = true;
                animator.SetBool(HashACPisSlide, true);
                StartCoroutine(EndSlide());
            }
            else if (!isRun && !isSlide)
            {
                //执行蹲下或站立逻辑
                canRun = !isCrouch;
                animator.SetBool(HashACPisCrouch, isCrouch);
            }
        }
    }

    void OnCrouchCanceled(InputAction.CallbackContext context)
    {
        // 如果不在跳跃状态，执行站立逻辑
        if (!isJump)
        {
            if (useCrouchToggle)
            {
                //不做任何操作
            }
            else
            {
                //解除蹲
                isCrouch = false;
                canRun = true;
                animator.SetBool(HashACPisCrouch, false);
            }
        }
    }

    private IEnumerator EndSlide()
    {//滑行
        yield return new WaitForSeconds(0.5f);
        //解除滑行
        isSlide = false;
        isCrouch = false;
        animator.SetBool(HashACPisSlide, false);
    }
    /*====================================蹲下、滑行====================================*/

    /*=======================================跳跃=======================================*/
    public void StartJump(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            Debug.Log("Jump");
            //禁用攻击
            canAttack = false;
            //启用跳跃
            isJump = true;
            animator.SetBool(HashACPisJump, true);
            StartCoroutine(EndJump());
        }
    }

    private IEnumerator EndJump()
    {
        yield return new WaitForSeconds(0.367f);
        canAttack = true;
        isJump = false;
        animator.SetBool(HashACPisJump, false);
    }
    /*=======================================跳跃=======================================*/

    /*=======================================攻击=======================================*/
    public void StartAttack(InputAction.CallbackContext ctx)
    {
        if (canAttack && ctx.phase == InputActionPhase.Performed)
        {
            Debug.Log("Attack");
            //禁用攻击 防止短时间连续触发攻击
            canAttack = false;
            //禁用移动
            canRun = false;
            //解除跳跃、下蹲、滑行、移动状态
            isRun = false;
            isJump = false;
            isCrouch = false;
            animator.SetBool(HashACPisRun, false);
            animator.SetBool(HashACPisCrouch, false);
            animator.SetBool(HashACPisJump, false);


            if (isDash)
            {//位于冲刺状态则触发冲刺攻击
                canRun = true;
                isDash = false;
                animator.SetBool(HashACPdoDashAttack, true);
                
                StartCoroutine(EndDashAttack());
                return;
            }
            
            //按当前连击段数触发不同的攻击
            if (numAttack == 0)
            {
                numAttack = 1;
                animator.SetBool(HashACPdoAttack1, true);
                animator.SetBool(HashACPdoAttackTrans1Judge, false);
                StartCoroutine(EndAttack1());
            }
            else if (numAttack == 1)
            {
                numAttack = 0;
                animator.SetBool(HashACPdoAttack2, true);
                StartCoroutine(EndAttack2());
            }
        }
    }

    private IEnumerator EndAttack1()
    {
        //rigidbody2D.velocity = 0.2f * speed * vector2d;
        yield return new WaitForSeconds(0.317f);
        canAttack = true;
        animator.SetBool(HashACPdoAttack1, false);
        StartCoroutine(TransAttack1());
    }

    private IEnumerator TransAttack1()
    {
        yield return new WaitForSeconds(0.417f);
        if (didHitPause)
        {
            didHitPause = false;
            yield return new WaitForSeconds(hitPauseTime);
        }
        canRun = true;
        if (!animator.GetBool(HashACPdoAttack2))
        {
            numAttack = 0;
        }
        animator.SetBool(HashACPdoAttackTrans1Judge, true);
        //rigidbody2D.velocity = Vector2.zero;
    }

    private IEnumerator EndAttack2()
    {
        //rigidbody2D.velocity = 0.2f * speed * vector2d;
        while (!animator.GetBool(HashACPdoAttackTrans1Judge))
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.217f);
        if (didHitPause)
        {
            didHitPause = false;
            yield return new WaitForSeconds(hitPauseTime);
        }
        canRun = true;
        canAttack = true;
        animator.SetBool(HashACPdoAttack2, false);
        //rigidbody2D.velocity = Vector2.zero;
    }

    private IEnumerator EndDashAttack()
    {
        yield return new WaitForSeconds(0.5f);
        if (didHitPause)
        {
            didHitPause = false;
            yield return new WaitForSeconds(hitPauseTime);
        }
        
        canAttack = true;
        animator.SetBool(HashACPisDash, false);
        animator.SetBool(HashACPdoDashAttack, false);
    }

    //碰撞检测
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<IEnemy>().GetHit(parameter.atk);
            didHitPause = true;
            animator.speed = 0f;
            StartCoroutine(EndHitPause());
            virtualCamera.GetComponent<CameraShake>().Shake();
        }
    }

    //顿帧卡肉
    private IEnumerator EndHitPause()
    {
        yield return new WaitForSeconds(hitPauseTime);
        animator.speed = originAnimatorSpeed;
    }
    /*=======================================攻击=======================================*/

    /*=======================================冲刺=======================================*/
    public void StartDash(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            Debug.Log("Dash");
            isDash = true;
            isCrouch = false;
            isJump = false;
            isSlide = false;
            canAttack = true;
            canRun = true;
            animator.SetBool(HashACPisDash, true);
            animator.SetBool(HashACPisCrouch, false);
            animator.SetBool(HashACPisJump, false);
            animator.SetBool(HashACPisSlide, false);
            animator.SetBool(HashACPdoAttack1, false);
            animator.SetBool(HashACPdoAttack2, false);
            animator.SetBool(HashACPdoAttackTrans1Judge, true);
            StartCoroutine(EndDash());
        }
    }

    private IEnumerator EndDash()
    {
        yield return new WaitForSeconds(0.7f);
        isDash = false;
        animator.SetBool(HashACPisDash, false);
    }
    /*=======================================冲刺=======================================*/

    /*=======================================受击=======================================*/
    public void GetHit(float damage)
    {
        if (!isDash && !parameter.isDead)
        {
            damage /= (1 + parameter.def / 100);
            parameter.health -= damage;
            virtualCamera.GetComponent<CameraShake>().Shake();
            StartCoroutine(UpDateBloodBar());
            animator.SetBool(HashACPisHurt, true);
            if (parameter.health <= 0.0f && !parameter.isDead)
            {
                parameter.isDead = true;
                animator.SetBool(HashACPisHurt, false);
                animator.SetBool(HashACPisDead, true);
                playerInput.Disable();
            }
            StartCoroutine(EndHit());
        }
    }

    private IEnumerator EndHit()
    {
        yield return new WaitForSeconds(0.35f);
        animator.SetBool(HashACPisHurt, false);
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
    /*=======================================受击=======================================*/

    //加载参数
    private void InitACPParaHash()
    {
        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Bool)
            {
                if (param.name == "doAttack1")
                {
                    HashACPdoAttack1 = param.nameHash;
                }
                else if (param.name == "doAttack2")
                {
                    HashACPdoAttack2 = param.nameHash;
                }
                else if (param.name == "doAttackTrans1Judge")
                {
                    HashACPdoAttackTrans1Judge = param.nameHash;
                }
                else if (param.name == "isJump")
                {
                    HashACPisJump = param.nameHash;
                }
                else if (param.name == "isRun")
                {
                    HashACPisRun = param.nameHash;
                }
                else if (param.name == "isSlide")
                {
                    HashACPisSlide = param.nameHash;
                }
                else if (param.name == "isCrouch")
                {
                    HashACPisCrouch = param.nameHash;
                }
                else if (param.name == "isDash")
                {
                    HashACPisDash = param.nameHash;
                }
                else if (param.name == "doDashAttack")
                {
                    HashACPdoDashAttack = param.nameHash;
                }
                else if (param.name == "isHurt")
                {
                    HashACPisHurt = param.nameHash;
                }
                else if (param.name == "isDead")
                {
                    HashACPisDead = param.nameHash;
                }
            }
        }
    }
}
