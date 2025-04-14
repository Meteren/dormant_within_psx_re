
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private enum RotationDirection
    {
        left, right,none
    }

    RotationDirection direction = RotationDirection.none;
    public Animator anim;
    public Rigidbody rb;
    private float turnSpeed = 100;

    public HashSet<string> inventory = new HashSet<string>();

    public PuzzleObject interactedPuzzleObject;

    IState baseState;

    [Header("Conditions")]
    public bool idle = true;
    public bool walk;
    public bool run;
    public bool aim;
    public bool walkBackwards;
    public bool shoot;
    public bool isPressingM2;
    public bool comeFromShooting;
    public bool turnBack;
    public bool isDead;
    public bool isInjured;
    public bool getStance;
    public bool primaryAttack;
    public bool secondaryAttack;
    public bool kick;
    public bool canRotate;
    public bool readyToCombo;
    public bool charge;
    public bool chargeAttack;
    public bool damageTaken;

    [Header("Reference Point")]
    [SerializeField] private Transform reference;

    [Header("Equipped Item")]
    public IEquippable equippedItem;

    [Header("Aim Radius")]
    public float aimRadius;

    [Header("Radius Check")]
    public List<Enemy> enemiesInRange;

    [Header("Locked Enemy")]
    public Enemy lockedEnemy;

    [Header("Center Point")]
    public Transform centerPoint;

    [Header("Aim Mask")]
    public LayerMask aimMask;

    [Header("Health Segment")]
    public HealthBar healthBar;
    public float maxHealth = 100;
    public float currentHealth;
    public Vector3 ForwardDirection {  get; private set; }

    StateMachine playerStateMachine = new StateMachine();

    public Quaternion targetRotation;

    float rotationSpeed = 500f;

    void Start()
    {

        GameManager.instance.blackboard.SetValue("PlayerController", this);

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        currentHealth = maxHealth;
        healthBar.SetHealth(currentHealth, ShouldFlicker);

        var idleState = new IdleState(this);
        var walkState = new WalkState(this);
        var runState = new RunState(this);
        var walkBackwardsState = new WalkBackwardsState(this);
        var aimState = new AimState(this);
        var shootState = new ShootState(this);
        var turnBackState = new TurnBackState(this);
        var kickState = new KickState(this);

        baseState = idleState;

        Add(idleState, walkState, new FuncPredicate(() => walk));
        Add(idleState, runState, new FuncPredicate(() => run));
        Add(idleState, walkBackwardsState, new FuncPredicate(() => walkBackwards));
        Add(walkState, idleState, new FuncPredicate(() => idle));
        Add(walkState, runState, new FuncPredicate(() => run));
        Add(runState, walkState, new FuncPredicate(() => walk));
        Add(runState, idleState, new FuncPredicate(() => idle));
        Add(walkBackwardsState, idleState, new FuncPredicate(() => idle));
        Add(walkBackwardsState, turnBackState, new FuncPredicate(() => turnBack));
        Add(turnBackState, idleState, new FuncPredicate(() => !turnBack));
        Add(idleState, turnBackState, new FuncPredicate(() => turnBack));

        Any(aimState, new FuncPredicate(() => aim && !kick));
        Any(kickState, new FuncPredicate(() => kick));

        Add(kickState, idleState, new FuncPredicate(() => idle));
        Add(aimState, shootState, new FuncPredicate(() => shoot));
        Add(aimState, idleState, new FuncPredicate(() => idle));
        Add(shootState, aimState, new FuncPredicate(() => aim));

        playerStateMachine.CurrentState = idleState;
        
    }
    
    private void Add(IState from, IState to, IPredicate condition)
    {
        playerStateMachine.Add(from, to, condition);
    }

    private void Any(IState to, IPredicate condition)
    {
        playerStateMachine.Any(to, condition);
    }

    private void FixedUpdate()
    {
        if (!turnBack)
        {
            Vector3 backDirection = -1 * ForwardDirection;
            backDirection.y = 0;
            targetRotation = Quaternion.LookRotation(backDirection);
        }   
        SetForwardDirection();
        if (turnBack)
        {
            TurnBack();
            if (CheckIfRotationHandled())
                turnBack = false;
        }
        if(canRotate)
            Rotate();  

    }
    void Update()
    {
        Debug.Log("Pressing m2:" + isPressingM2);
        if (Input.GetMouseButtonUp(1))
            isPressingM2 = false;

        if (!isPressingM2)
        {
            getStance = false;
        }
            
        SetRotationDirection();
        UpdateAnimations();
        playerStateMachine.Update();
        if (Input.GetKeyDown(KeyCode.B))
            OnTakeDamage(20f);
        if (Input.GetKeyDown(KeyCode.H))
            OnHeal(20);
    }

    private void SetRotationDirection()
    {
        if (Input.GetKey(KeyCode.D))
            direction = RotationDirection.right;
        else if (Input.GetKey(KeyCode.A))
            direction = RotationDirection.left;
        else
            direction = RotationDirection.none;
    }

    private void UpdateAnimations()
    {
        anim.SetBool("idle", idle);
        anim.SetBool("walk", walk);
        anim.SetBool("walkBackwards", walkBackwards);
        anim.SetBool("run", run);
        anim.SetBool("aim", aim);
        anim.SetBool("shoot", shoot);
        anim.SetBool("getStance", getStance);
        anim.SetBool("primaryAttack", primaryAttack);
        anim.SetBool("secondaryAttack", secondaryAttack);
        anim.SetBool("kick", kick);
        anim.SetBool("charge", charge);
        anim.SetBool("chargeAttack", chargeAttack);
        anim.SetBool("damageTaken", damageTaken);
    }

    public void SetForwardDirection()
    {
        ForwardDirection = reference.position - transform.position;
    }
    private void Rotate()
    {
        Turn(direction);
    }

    private void Turn(RotationDirection direction)
    {
        Vector3 rotation = transform.eulerAngles;
        if (direction == RotationDirection.left)
        {
            rotation.y -= turnSpeed * Time.deltaTime;
        }
        else if (direction == RotationDirection.right)
        {
            rotation.y += turnSpeed * Time.deltaTime;
        }
        else
        {
            return;
        }
        transform.rotation = Quaternion.Euler(rotation);

    }
    private void ResetState()
    {
        idle = true;
        walkBackwards = aim = run = walk = false;
        playerStateMachine.CurrentState = baseState;
    } 

    private void OnDisable()
    {
        ResetState();
        enemiesInRange.Clear();
    }

    private void TurnBack()
    {
        transform.rotation =
            Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
    private bool CheckIfRotationHandled()
    {
        float angle = Quaternion.Angle(transform.rotation, targetRotation);
        if (angle < 5f)
        {
            transform.rotation = targetRotation;
            return true;
        }
        else return false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, aimRadius);
    }

    public void OnTakeDamage(float damageAmount)
    {
        if (damageTaken)
            damageTaken = false;

        damageTaken = true;

        if (isDead)
            return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            //isDead = true; --will be used later
        }

        AdjustInjuredState();
        
        healthBar.SetHealth(currentHealth, ShouldFlicker);
    }

    public void OnHeal(float healAmount)
    {
        currentHealth += healAmount;
        healthBar.SetHealth(currentHealth,ShouldFlicker);
        if (currentHealth >= 100)
        {
            currentHealth = 100;
        }
        AdjustInjuredState();
    }


    public void AdjustInjuredState()
    {
        if (currentHealth <= 30f && !isInjured)
        {
            isInjured = true;
            StartCoroutine(SmoothWeightTransformation(0, 1));
        }    
        else if (currentHealth >= 30f && isInjured)
        {
            isInjured = false;
            StartCoroutine(SmoothWeightTransformation(1, 0));
        }
            
    }

    private IEnumerator SmoothWeightTransformation(float startPoint, float endPoint)
    {
        float weightValue = startPoint;
        float duration = 2f;
        float elapsedTime = 0;
        while(elapsedTime <= duration)
        {
            elapsedTime += Time.deltaTime;
            weightValue = Mathf.Lerp(startPoint, endPoint, elapsedTime / duration);
            anim.SetLayerWeight(4, weightValue);
            yield return null;
        }
        anim.SetLayerWeight(4, endPoint);
    }

    public bool IsAttacking() => primaryAttack || secondaryAttack || chargeAttack || kick;

    public bool ShouldFlicker() => currentHealth <= 30f;

    public void ReadyForSecondaryMeleeAttack() => StartCoroutine(ComboFrame());

    public void SetSecondaryMeleeAttack() => secondaryAttack = false;

    public void SetChargedAttack() => chargeAttack = false;

    public void SetDamageTakenState() => damageTaken = false;

    public void HandleChargeAtTheEndIfNeeded()
    {
        if (!getStance)
            chargeAttack = true;
    }
       
    private IEnumerator ComboFrame()
    {
        readyToCombo = true;
        yield return new WaitForSeconds(0.3f);
        readyToCombo = false;
        primaryAttack = false;
    }
    
}
