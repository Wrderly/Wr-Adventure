using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
//���ű����ڽ�ɫ����

[Serializable]
public class MyCharacterParameter
{//��ɫ������
    public float maxHealth;
    public float health;
    public float atk;
    public float def;
    public bool isDead = false;
}

public class MyCharacterController : MonoBehaviour,IEnemy
{//��ɫ������
    //��ɫ����
    public MyCharacterParameter parameter;
    public float speed;
    //ͳһ����ϵͳʵ��
    public @PlayerInput playerInput;
    public GameObject virtualCamera;
    //��ɫ���� �ֱ�Ϊ�������ơ�������ƺ���ײ�еȵ�
    public Animator animator;
    //public SpriteRenderer characterRenderer;
    public Rigidbody2D characterRigidbody;
    //public BoxCollider2D characterCollider2D;
    //Ѫ�����
    public Image bloodBar;
    private const float bloobBarLerpSpeed = 3f;
    //��������״̬����������
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
    //��ɫ����
    private bool isRight = true;
    private Vector2 vector2d;
    //��ɫ�߼�״̬����
    private bool canAttack = true;
    private int numAttack = 0;
    private bool isJump = false;
    private bool isRun = false;
    private bool canRun = true;
    private bool isSlide = false;
    private bool isCrouch = false;
    private bool isDash = false;
    private bool isDashAttack = false;
    //��ɫ��֡�������
    private bool didHitPause = false;
    private const float hitPauseTime = 0.15f;
    private float originAnimatorSpeed;
    //��ɫ��ײ�м�¼����
    //private float boxcolliderX;
    //����һ��������������ʾ�Ƿ�ʹ�ð��°�ť�ף��ٰ�һ�°�ť��ķ�ʽ
    private bool useCrouchToggle = true;

    private void Awake()
    {
        //��ȡ��ɫ����
        //characterRenderer = GetComponent<SpriteRenderer>();
        characterRigidbody = GetComponent<Rigidbody2D>();
        //characterCollider2D = GetComponent<BoxCollider2D>();
        //boxcolliderX = characterCollider2D.offset.x;
        //��ʼ��ͳһ����ϵͳ������
        playerInput = new PlayerInput();
        playerInput.Enable();
        //��ȡ�������������� ����ʼ������״̬������
        animator = GetComponent<Animator>();
        InitACPParaHash();
    }

    private void Start()
    {
        //������������ٶ�
        originAnimatorSpeed = animator.speed;
        bloodBar.fillAmount = parameter.health / parameter.maxHealth;
    }

    private void OnEnable()
    {
        //ע���ɫͳһ�����¼�
        playerInput.Enable();
        playerInput.Player.Crouch.performed += OnCrouchPerformed;
        playerInput.Player.Crouch.canceled += OnCrouchCanceled;
        playerInput.Player.Attack.performed += StartAttack;
        playerInput.Player.Dash.performed += StartDash;
        playerInput.Player.Jump.performed += StartJump;
    }

    private void OnDisable()
    {
        //ж��ͳһ�����¼�
        playerInput.Disable();
        playerInput.Player.Crouch.performed -= OnCrouchPerformed;
        playerInput.Player.Crouch.canceled -= OnCrouchCanceled;
        playerInput.Player.Attack.performed -= StartAttack;
        playerInput.Player.Dash.performed -= StartDash;
        playerInput.Player.Jump.performed -= StartJump;
    }

    private void Update()
    {
        /*=======================================�ƶ�=======================================*/
        //��ȡͳһ����
        vector2d = playerInput.Player.Run.ReadValue<Vector2>();
        if (canRun && vector2d != Vector2.zero)
        {
            //��һ������
            vector2d.Normalize();
            //���ҷ�ת
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
            //���ܶ�״̬
            isRun = true;
            animator.SetBool(HashACPisRun, true);
            //�����Ƿ��̡������������ܲ����ٶ�
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
            //ȡ���ܲ�״̬
            isRun = false;
            animator.SetBool(HashACPisRun, false);
            characterRigidbody.velocity = Vector2.zero;
        }
        /*=======================================�ƶ�=======================================*/
    }

    /*====================================���¡�����====================================*/
    void OnCrouchPerformed(InputAction.CallbackContext context)
    {
        // ���������Ծ״̬��ִ�ж��»����߼�
        if (!isJump)
        {
            bool crouchPressed = playerInput.Player.Crouch.WasPressedThisFrame();

            if (useCrouchToggle && crouchPressed)//����� �ٵ���
            {
                //�л�isCrouch��ֵ
                isCrouch = !isCrouch;
            }
            else if (!useCrouchToggle)//��ס�� �ɿ���
            {
                isCrouch = true;
            }

            if (isRun && !isSlide && crouchPressed)
            {
                //ִ�л����߼�
                isSlide = true;
                animator.SetBool(HashACPisSlide, true);
                StartCoroutine(EndSlide());
            }
            else if (!isRun && !isSlide)
            {
                //ִ�ж��»�վ���߼�
                canRun = !isCrouch;
                animator.SetBool(HashACPisCrouch, isCrouch);
            }
        }
    }

    void OnCrouchCanceled(InputAction.CallbackContext context)
    {
        // ���������Ծ״̬��ִ��վ���߼�
        if (!isJump)
        {
            if (useCrouchToggle)
            {
                //�����κβ���
            }
            else
            {
                //�����
                isCrouch = false;
                canRun = true;
                animator.SetBool(HashACPisCrouch, false);
            }
        }
    }

    private IEnumerator EndSlide()
    {//����
        yield return new WaitForSeconds(0.5f);
        //�������
        isSlide = false;
        isCrouch = false;
        animator.SetBool(HashACPisSlide, false);
    }
    /*====================================���¡�����====================================*/

    /*=======================================��Ծ=======================================*/
    public void StartJump(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            Debug.Log("Jump");
            //���ù���
            canAttack = false;
            //������Ծ
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
    /*=======================================��Ծ=======================================*/

    /*=======================================����=======================================*/
    public void StartAttack(InputAction.CallbackContext ctx)
    {
        if (canAttack && ctx.phase == InputActionPhase.Performed)
        {
            Debug.Log("Attack");
            //���ù��� ��ֹ��ʱ��������������
            canAttack = false;
            //�����ƶ�
            canRun = false;
            //�����Ծ���¶ס����С��ƶ�״̬
            isRun = false;
            isJump = false;
            isCrouch = false;
            animator.SetBool(HashACPisRun, false);
            animator.SetBool(HashACPisCrouch, false);
            animator.SetBool(HashACPisJump, false);


            if (isDash)
            {//λ�ڳ��״̬�򴥷���̹���
                canRun = true;
                isDash = false;
                animator.SetBool(HashACPdoDashAttack, true);
                
                StartCoroutine(EndDashAttack());
                return;
            }
            
            //����ǰ��������������ͬ�Ĺ���
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

    //��ײ���
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

    //��֡����
    private IEnumerator EndHitPause()
    {
        yield return new WaitForSeconds(hitPauseTime);
        animator.speed = originAnimatorSpeed;
    }
    /*=======================================����=======================================*/

    /*=======================================���=======================================*/
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
    /*=======================================���=======================================*/

    /*=======================================�ܻ�=======================================*/
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
    /*=======================================�ܻ�=======================================*/

    //���ز���
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
