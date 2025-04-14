using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    PlayerController playerController => 
        GameManager.instance.blackboard.TryGetValue("PlayerController", out PlayerController _controller) ? _controller : null;
    [Header("Health")]
    public int healthAmount;

    [Header("PathFinder")]
    public PathFinder pathFinder;

    [Header("LOS")]
    [SerializeField] private int gridRadius;
    [SerializeField] private float checkAreaRadius;
    [SerializeField] private float attackRange;

    [Header("Patrol Points")]
    public List<Transform> patrolPoints;

    [Header("Player Mask")]
    [SerializeField] private LayerMask playerMask;
    [Header("Ray Mask")]
    [SerializeField] private LayerMask rayMask;

    [Header("Center")]
    public Transform centerPoint;

    bool pathInProgress;

    int patrolIndex;

    int pathIndex;

    List<PathGrid> path;
    BehaviourTree enemyBehaviourTree;

    [Header("Conditions")]
    public bool walk;
    public bool run;
    public bool idle;
    public bool isDead;
    public bool canAttack;
    public bool damageTaken;
    public bool attack;
    public bool stagger;
    public bool deathAfterDamage;

    [Header("Last Seen Position")]
    public Vector3 lastSeenPos;

    [Header("Animator")]
    public Animator enemyAnimator;

    [Header("Blood Particle")]
    [SerializeField] private ParticleSystem particleReference;
    public Vector3 damagePosition;

    [HideInInspector] public Rigidbody rb;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        enemyAnimator = GetComponent<Animator>();
        playerMask = LayerMask.GetMask("Player");
        pathFinder = GetComponent<PathFinder>();
        pathFinder.InitPathGridTable(gridRadius);

        enemyBehaviourTree = new BehaviourTree();

        SortedSelectorNode mainSelector = new SortedSelectorNode("MainSelector");
        enemyBehaviourTree.AddChild(mainSelector);

        var patrolStrategy = new Leaf("PatrolStrategy", new PatrolStrategy(this),30);
        

        SequenceNode chaseSequence = new SequenceNode("ChaseSequnce", 25);

        var chaseCondition = new Leaf("ChaseCondition", new Condition(() => CanChase()));
        var chaseStrategy = new Leaf("ChaseStrategy", new ChaseStrategy(this));
        var moveToLastSeenPosStrategy = new Leaf("MoveToLastSeenPos", new MoveToLastSeenPositionStrategy(this));

        chaseSequence.AddChild(chaseCondition);
        chaseSequence.AddChild(chaseStrategy);
        chaseSequence.AddChild(moveToLastSeenPosStrategy);

        SequenceNode attackSequence = new SequenceNode("AttackSequence", 20);
        var attackCondition = new Leaf("AttackCondition", new Condition(() => CanAttack()));
        var attackStrategy = new Leaf("AttackStrategy", new AttackStrategy(this));

        attackSequence.AddChild(attackCondition);
        attackSequence.AddChild(attackStrategy);

        SequenceNode damageTakenSequence = new SequenceNode("DamageTakenSequence",15);

        var damageTakenCondition = new Leaf("DamageTakenCondition", new Condition(() => damageTaken && !stagger && !attack));
        var damageTakenStrategy = new Leaf("DamageTakenStrategy", new DamageTakenStrategy(this));

        damageTakenSequence.AddChild(damageTakenCondition);
        damageTakenSequence.AddChild(damageTakenStrategy);


        SequenceNode deathSequence = new SequenceNode("DeathSequence", 10);
        var deathCondition = new Leaf("DeathCondition", new Condition(() => isDead || deathAfterDamage));
        var deathStrategy = new Leaf("DeathStrategy", new DeathStrategy(this));

        deathSequence.AddChild(deathCondition);
        deathSequence.AddChild(deathStrategy);


        mainSelector.AddChild(deathSequence);
        mainSelector.AddChild(damageTakenSequence);
        mainSelector.AddChild(attackSequence);
        mainSelector.AddChild(chaseSequence);
        mainSelector.AddChild(patrolStrategy);

    }

    /*private void Update()
    {
        if (pathInProgress)
        {
            transform.position = Vector3.MoveTowards(transform.position, path[pathIndex].transform.position, Time.deltaTime * speed);

            if (Vector3.Distance(transform.position, path[pathIndex].transform.position) <= 0.05f)
            {
                if (pathIndex < path.Count - 1)
                    pathIndex++;
                else
                {
                    if (patrolIndex < patrolPoints.Count - 1)
                        patrolIndex++;
                    else
                        patrolIndex = 0;
                    PathGrid startGrid = path[path.Count - 1];
                    path = pathFinder.DrawPath(transform.position, patrolPoints[patrolIndex].position);
                    pathIndex = 0;
                }
                List<PathGrid> newPath = pathFinder.DrawPath(transform.position, patrolPoints[patrolIndex].position);
                if (ShouldPathChange(newPath))
                {
                    path = newPath;
                    pathIndex = 0;
                }
              
            }

        }      
         
    }*/

    private void Update()
    {
        SetAnimations();
    }

    private void FixedUpdate()
    {
        enemyBehaviourTree.Evaluate();
        /*if (pathInProgress)
        {
            transform.position = Vector3.MoveTowards(transform.position, path[pathIndex].transform.position, Time.deltaTime * speed);

            if (Vector3.Distance(transform.position, path[0].transform.position) <= 0.05f)
            {
                if (pathIndex < path.Count - 1)
                {

                }     
                else
                {
                    if (patrolIndex < patrolPoints.Count - 1)
                        patrolIndex++;
                    else
                        patrolIndex = 0;
                }
                List<PathGrid> newPath = pathFinder.DrawPath(transform.position, patrolPoints[patrolIndex].position);
                if (ShouldPathChange(newPath))
                {
                    path = newPath;
                    Debug.Log("Path changed");
                }

            }

        }*/
    }

    protected void SetAnimations()
    {
        enemyAnimator.SetBool("idle",idle);
        enemyAnimator.SetBool("walk", walk);
        enemyAnimator.SetBool("run", run);
        enemyAnimator.SetBool("damageTaken", damageTaken);
        enemyAnimator.SetBool("dead", isDead);
        enemyAnimator.SetBool("attack", attack);
        enemyAnimator.SetBool("stagger", stagger);
        enemyAnimator.SetBool("deathAfterDamage", deathAfterDamage);
    }

    //for melee weapons
    public virtual void OnDamage(int damage)
    {
        if(!isDead || !deathAfterDamage)
        {
            damageTaken = true;
            //SplatBlood(damagePosition);
        }
           
        if (stagger)
            stagger = false;
        healthAmount -= damage;
        Debug.Log($"Amount of damage inflicted: {damage} - Remained health: {healthAmount}");  

    }

    //for guns
    public virtual void OnStagger(int damage)
    {

        Debug.Log("Splash Blood");
        healthAmount -= damage;
        //SplatBlood(damagePosition);

        if (healthAmount <= 0)
        {
            damageTaken = false;
            healthAmount = 0;
            isDead = true;
        }

        if (stagger)
            stagger = false;

        if (!isDead)
            stagger = true;
              
    }

    public bool CanChase()
    {
        bool playerDetected = Physics.CheckSphere(transform.position, checkAreaRadius, playerMask);
        if (playerDetected)
        {
            Debug.Log("Collided with player");
            if (IsInLineOfSight())
                return true;
        }
           
        return false;
            
    }
    private bool IsInLineOfSight()
    {
        Vector3 rayDirection = playerController.centerPoint.position - transform.position;
        Ray ray = new Ray(transform.position, rayDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, checkAreaRadius,rayMask,QueryTriggerInteraction.Ignore))
        {
            Debug.DrawRay(transform.position, rayDirection * checkAreaRadius,Color.red);
            if (hit.transform.GetComponent<PlayerController>() != null)
            {
                Debug.Log("Player is in los");
                return true;
            }
                
        }
           
        return false;

    }
    public void SetStagger()
    {
        stagger = false;
        damageTaken = false;
    }
    public bool CanAttack()
    {
        if(Vector3.Distance(playerController.transform.position,transform.position) <= attackRange)
            return true;
        return false;
    }
    public float CalculatePriority(PlayerController p_controller) =>
        Vector3.Distance(p_controller.transform.position, transform.position);

    private void SplatBlood(Vector3 position)
    {
        ParticleSystem bloodParticle = Instantiate(particleReference);
        bloodParticle.transform.position = position;
        bloodParticle.Play();
    }
}
