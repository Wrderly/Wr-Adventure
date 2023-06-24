using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyCharacterController : MonoBehaviour
{
    //public static MyCharacterController instance;

    public float speed;

    public @PlayerControls playerInput;
    //private CharacterController characterController;


    private Animator animator;
    private SpriteRenderer renderer;
    private Rigidbody2D rigidbody2D;
    private BoxCollider2D collider2D;

    //private AnimatorControllerParameter ACPdoAttack;
    private AnimatorControllerParameter ACPdoAttack1;
    private AnimatorControllerParameter ACPdoAttack2;
    private AnimatorControllerParameter ACPdoAttackTrans1Judge;
    private AnimatorControllerParameter ACPisJump;
    private AnimatorControllerParameter ACPisRun;
    private AnimatorControllerParameter ACPisSlide;
    private AnimatorControllerParameter ACPisCrouch;
    private AnimatorControllerParameter ACPisDash;
    private AnimatorControllerParameter ACPdoDashAttack;

    //private int direction = 0;
    private bool isRight = true;

    //private bool doAttack = false;
    private bool canAttack = true;
    private int numAttack = 0;

    private bool isJump = false;
    private bool isRun = false;
    private bool canRun = true;
    private bool isSlide = false;
    private bool isCrouch = false;
    private bool isDash = false;
    private bool isDashAttack = false;

    private float boxcolliderX;

    //����һ��������������ʾ�Ƿ�ʹ�ð��°�ť�ף��ٰ�һ�°�ť��ķ�ʽ
    private bool useCrouchToggle = true;
    //private bool useCrouchToggle = false;

    //private float attackDuration = 1f;
    private void Awake()
    {
        //instance = this;
        renderer = GetComponent<SpriteRenderer>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<BoxCollider2D>();
        boxcolliderX = collider2D.offset.x;
        //playerInput = GetComponent<@PlayerControls>();
        playerInput = new PlayerControls();
        playerInput.Enable();

        animator = GetComponent<Animator>();
        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Bool)
            {
                if (param.name == "doAttack1")
                {
                    ACPdoAttack1 = param;
                }
                else if (param.name == "doAttack2")
                {
                    ACPdoAttack2 = param;
                }
                else if (param.name == "doAttackTrans1Judge")
                {
                    ACPdoAttackTrans1Judge = param;
                }
                else if (param.name == "isJump")
                {
                    ACPisJump = param;
                }
                else if (param.name == "isRun")
                {
                    ACPisRun = param;
                }
                else if (param.name == "isSlide")
                {
                    ACPisSlide = param;
                }
                else if (param.name == "isCrouch")
                {
                    ACPisCrouch = param;
                }
                else if (param.name == "isDash")
                {
                    ACPisDash = param;
                }
                else if (param.name == "doDashAttack")
                {
                    ACPdoDashAttack = param;
                }
            }
        }
    }

    private void Start()
    {

    }

    private void OnEnable()
    {
        playerInput.Enable();
        playerInput.Player.Crouch.performed += OnCrouchPerformed;
        playerInput.Player.Crouch.canceled += OnCrouchCanceled;
    }

    private void OnDisable()
    {
        playerInput.Disable();
        playerInput.Player.Crouch.performed -= OnCrouchPerformed;
        playerInput.Player.Crouch.canceled -= OnCrouchCanceled;
    }

    private void Update()
    {
        /*=======================================�ƶ�=======================================*/
        Vector2 vector2d = playerInput.Player.Run.ReadValue<Vector2>();
        if (canRun && vector2d != Vector2.zero)
        {
            Debug.Log("Run");
            vector2d.Normalize();
            if (vector2d.x < 0 && isRight)
            {
                isRight = false;
                renderer.flipX = true;
                collider2D.offset = new Vector2(-boxcolliderX, collider2D.offset.y);
            }
            else if (vector2d.x > 0 && !isRight)
            {
                isRight = true;
                renderer.flipX = false;
                collider2D.offset = new Vector2(boxcolliderX, collider2D.offset.y);
            }
            isRun = true;
            animator.SetBool(ACPisRun.nameHash, true);
            if (!isSlide && !isDash)
            {
                rigidbody2D.velocity = speed * vector2d;
            }
            else
            {
                rigidbody2D.velocity = 2.0f * speed * vector2d;
            }
        }
        else
        {
            isRun = false;
            animator.SetBool(ACPisRun.nameHash, false);
            rigidbody2D.velocity = Vector2.zero;
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
                animator.SetBool(ACPisSlide.nameHash, true);
                StartCoroutine(EndSlide());
            }
            else if (!isRun && !isSlide)
            {
                //ִ�ж��»�վ���߼�
                canRun = !isCrouch;
                animator.SetBool(ACPisCrouch.nameHash, isCrouch);
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
                isCrouch = false;
                canRun = true;
                animator.SetBool(ACPisCrouch.nameHash, false);
            }
        }
    }

    private IEnumerator EndSlide()
    {
        yield return new WaitForSeconds(0.5f);
        isSlide = false;
        isCrouch = false;
        animator.SetBool(ACPisSlide.nameHash, false);
    }
    /*====================================���¡�����====================================*/

    /*=======================================��Ծ=======================================*/
    public void StartJump(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            Debug.Log("Jump");
            canAttack = false;
            isJump = true;
            animator.SetBool(ACPisJump.nameHash, true);
            StartCoroutine(EndJump());
        }
    }

    private IEnumerator EndJump()
    {
        yield return new WaitForSeconds(0.367f);
        canAttack = true;
        isJump = false;
        animator.SetBool(ACPisJump.nameHash, false);
    }
    /*=======================================��Ծ=======================================*/

    /*=======================================����=======================================*/
    public void StartAttack(InputAction.CallbackContext ctx)
    {
        if (canAttack && ctx.phase == InputActionPhase.Performed)
        {
            Debug.Log("Attack");
            canAttack = false;

            canRun = false;
            isRun = false;
            isJump = false;
            isCrouch = false;
            animator.SetBool(ACPisRun.nameHash, false);
            animator.SetBool(ACPisCrouch.nameHash, false);
            animator.SetBool(ACPisJump.nameHash, false);


            if (isDash)
            {
                canRun = true;
                isDash = false;
                animator.SetBool(ACPdoDashAttack.nameHash, true);
                
                StartCoroutine(EndDashAttack());
                return;
            }
            

            if (numAttack == 0)
            {
                numAttack = 1;
                animator.SetBool(ACPdoAttack1.nameHash, true);
                animator.SetBool(ACPdoAttackTrans1Judge.nameHash, false);
                StartCoroutine(EndAttack1());
            }
            else if (numAttack == 1)
            {
                numAttack = 0;
                animator.SetBool(ACPdoAttack2.nameHash, true);
                StartCoroutine(EndAttack2());
            }
        }
    }

    private IEnumerator EndAttack1()
    {
        yield return new WaitForSeconds(0.317f);
        canAttack = true;
        animator.SetBool(ACPdoAttack1.nameHash, false);
        StartCoroutine(TransAttack1());
    }

    private IEnumerator TransAttack1()
    {
        yield return new WaitForSeconds(0.417f);
        canRun = true;
        if (!animator.GetBool(ACPdoAttack2.nameHash))
        {
            numAttack = 0;
        }
        animator.SetBool(ACPdoAttackTrans1Judge.nameHash, true);
    }

    private IEnumerator EndAttack2()
    {
        while (!animator.GetBool(ACPdoAttackTrans1Judge.nameHash))
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.217f);
        canRun = true;
        canAttack = true;
        animator.SetBool(ACPdoAttack2.nameHash, false);
    }

    private IEnumerator EndDashAttack()
    {
        yield return new WaitForSeconds(0.5f);
        
        canAttack = true;
        animator.SetBool(ACPisDash.nameHash, false);
        animator.SetBool(ACPdoDashAttack.nameHash, false);
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
            animator.SetBool(ACPisDash.nameHash, true);
            animator.SetBool(ACPisCrouch.nameHash, false);
            animator.SetBool(ACPisJump.nameHash, false);
            animator.SetBool(ACPisSlide.nameHash, false);
            animator.SetBool(ACPdoAttack1.nameHash, false);
            animator.SetBool(ACPdoAttack2.nameHash, false);
            animator.SetBool(ACPdoAttackTrans1Judge.nameHash, true);
            StartCoroutine(EndDash());
        }
    }

    private IEnumerator EndDash()
    {
        yield return new WaitForSeconds(0.7f);
        isDash = false;
        animator.SetBool(ACPisDash.nameHash, false);
    }
    /*=======================================���=======================================*/
}
