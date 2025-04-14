using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy
{
    //later will be changed with spesific enemy types
    protected float rotationSpeed = 300f;
    protected Enemy enemy;
    protected List<PathGrid> path;
    protected bool initPlayerPath;
    protected PlayerController playerController => 
        GameManager.instance.blackboard.TryGetValue("PlayerController", 
            out PlayerController _playerController) ? _playerController : null;

    public BaseEnemy(Enemy enemy)
    {
        this.enemy = enemy; 
    }

    public bool TryPointAt(Vector3 toPoint)
    {
        Vector3 direction = toPoint - enemy.transform.position;
        direction.y = 0;
        Quaternion lookAt = Quaternion.LookRotation(direction);
        if (Quaternion.Angle(enemy.transform.rotation, lookAt) <= 1f)
        {
            enemy.transform.rotation = lookAt;
            return true;
        }      
        else
        {
            enemy.transform.rotation = Quaternion.RotateTowards(enemy.transform.rotation, lookAt, Time.deltaTime * rotationSpeed);
            return true;
        }
    }
}

public class PatrolStrategy : BaseEnemy, IStrategy
{    
    bool initPathConstruct;
    float patrolSpeed = 1f;
    int patrolIndex;

    bool coroutineStarted;
    public PatrolStrategy(Enemy enemy) : base(enemy)
    {
    }
    public Node.NodeStatus Evaluate()
    {
        Debug.Log("PatrolStrategy");
        if (enemy.CanChase())
        {
            initPathConstruct = false;
            coroutineStarted = false;
            return Node.NodeStatus.SUCCESS;
        }

        if (enemy.damageTaken)
        {
            initPlayerPath = false;
            return Node.NodeStatus.SUCCESS;
        }

        if (!coroutineStarted)
        {
            coroutineStarted = true;
            enemy.StartCoroutine(InitConstruction());
        } 

        if(enemy.isDead)
            return Node.NodeStatus.SUCCESS;
            
        if (initPathConstruct)
        {
            enemy.walk = true;
            enemy.transform.position = 
                Vector3.MoveTowards(enemy.transform.position, path[0].transform.position, Time.deltaTime * patrolSpeed);

            TryPointAt(path[0].transform.position);

            if (Vector3.Distance(enemy.transform.position, path[0].transform.position) <= 0.05f)
            {
                if (0 >= path.Count - 1)
                {
                    if (patrolIndex < enemy.patrolPoints.Count - 1)
                        patrolIndex++;
                    else
                        patrolIndex = 0;
                }
                List<PathGrid> newPath = enemy.pathFinder.DrawPath(enemy.transform.position, enemy.patrolPoints[patrolIndex].position);
                path = newPath;
                Debug.Log("Path changed");

            }
           
        }
        return Node.NodeStatus.RUNNING;

    }
    private IEnumerator InitConstruction()
    {
        enemy.walk = false;
        enemy.run = false;
        enemy.idle = false;
        yield return new WaitForSeconds(1f);
        path = enemy.pathFinder.DrawPath(enemy.transform.position, enemy.patrolPoints[0].position);
        initPathConstruct = true;
    }
}

public class ChaseStrategy : BaseEnemy, IStrategy
{

    float chaseSpeedInCloseRange = 1f;
    float chaseSpeedInLongRange = 2.7f;
    float distance = 5f;

    public ChaseStrategy(Enemy enemy) : base(enemy)
    {
    }
    public Node.NodeStatus Evaluate()
    {
        Debug.Log("ChaseStrategy");

        if (enemy.CanAttack())
        {
            enemy.canAttack = true;
            initPlayerPath = false;
            return Node.NodeStatus.SUCCESS;
        }

        if (enemy.damageTaken)
        {
            initPlayerPath = false;
            return Node.NodeStatus.SUCCESS;
        }

        if (!initPlayerPath)
        {
            enemy.idle = false;
            path = enemy.pathFinder.DrawPath(enemy.transform.position, playerController.transform.position);
            
            initPlayerPath = true;
        }
        if (!enemy.CanChase())
        {
            initPlayerPath = false;
            enemy.lastSeenPos = path[path.Count - 1].transform.position;
            Debug.Log($"Last seen pos: Y:{path[path.Count - 1].Y} - X:{path[path.Count - 1].X}");
            return Node.NodeStatus.SUCCESS;    
        }

        if (enemy.isDead)
            return Node.NodeStatus.SUCCESS;

        if (path.Count > 0)
        {
            TryPointAt(path[0].transform.position);
            if (initPlayerPath)
            {
                if (Vector3.Distance(enemy.centerPoint.transform.position, playerController.centerPoint.transform.position) <= distance)
                {
                    enemy.walk = true;
                    enemy.run = false;
                }
                else
                {
                    enemy.walk = false;
                    enemy.run = true;
                }

                enemy.transform.position =
                  Vector3.MoveTowards(enemy.transform.position, path[0].transform.position, Time.deltaTime * (enemy.run ? chaseSpeedInLongRange : chaseSpeedInCloseRange));
                if (Vector3.Distance(enemy.transform.position, path[0].transform.position) <= 0.05f)
                {
                    List<PathGrid> newPath = enemy.pathFinder.DrawPath(enemy.transform.position, playerController.transform.position);
                    path = newPath;
                    Debug.Log("Path changed");
                }
            }
        }        
        return Node.NodeStatus.RUNNING;
    }

}

public class MoveToLastSeenPositionStrategy : BaseEnemy, IStrategy
{
    float moveSpeed = 1.7f;
    public MoveToLastSeenPositionStrategy(Enemy enemy) : base(enemy)
    {
    }

    public Node.NodeStatus Evaluate()
    {
        Debug.Log("MoveToLastSeen Strategy");
        if (enemy.CanChase())
        {
            initPlayerPath = false;
            return Node.NodeStatus.SUCCESS;
        }

        if (enemy.CanAttack())
        {
            enemy.canAttack = true;
            initPlayerPath = false;
            return Node.NodeStatus.SUCCESS;
        }

        if (!initPlayerPath)
        {
            enemy.idle = false;
            path = enemy.pathFinder.DrawPath(enemy.transform.position, enemy.lastSeenPos);
            foreach (var pathgrid in path)
            {
                //Debug.Log($"Y:{pathgrid.Y}--X:{pathgrid.X}");
            }
            initPlayerPath = true;
        }
        
        if (enemy.damageTaken)
        {
            initPlayerPath = false;
            return Node.NodeStatus.SUCCESS;
        }

        if (enemy.isDead)
            return Node.NodeStatus.SUCCESS;


        TryPointAt(path[0].transform.position);

        enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, path[0].transform.position, Time.deltaTime * moveSpeed);

        if (Vector3.Distance(enemy.transform.position, path[0].transform.position) <= 0.05f)
        {
            path = enemy.pathFinder.DrawPath(enemy.transform.position,enemy.lastSeenPos);
        }

        if (Vector3.Distance(enemy.transform.position, enemy.lastSeenPos) <= 0.05f)
        {
            initPlayerPath = false;
            return Node.NodeStatus.SUCCESS;
        }

        return Node.NodeStatus.RUNNING;
        
    }
}

public class AttackStrategy : BaseEnemy, IStrategy
{
    AnimatorStateInfo stateInfo;
    public AttackStrategy(Enemy enemy) : base(enemy)
    {
    }
    public Node.NodeStatus Evaluate()
    {
        Debug.Log("Attack Strategy");
        enemy.idle = false;
        enemy.attack = true;
        stateInfo = enemy.enemyAnimator.GetCurrentAnimatorStateInfo(0);

        if (enemy.isDead)
        {
            enemy.attack = false;
            return Node.NodeStatus.SUCCESS;
        }
            
        if (enemy.damageTaken)
        {
            enemy.damageTaken = false;
            enemy.OnStagger(0);
        }
            
        if (!TryPointAt(playerController.transform.position))
            return Node.NodeStatus.RUNNING;

        if (enemy.enemyAnimator.IsInTransition(0)) return Node.NodeStatus.RUNNING;
                  
        if (stateInfo.IsName("attack"))
        {
            if (stateInfo.normalizedTime >= 1)
            {
                if (enemy.stagger)
                {
                    enemy.stagger = false;
                }
                    
                enemy.attack = false;
                enemy.idle = true;
                return Node.NodeStatus.SUCCESS;
            }
        }
           
        return Node.NodeStatus.RUNNING;

    }
}

public class DamageTakenStrategy : BaseEnemy, IStrategy
{
    AnimatorStateInfo stateInfo;
    bool forceToThisState;
    public DamageTakenStrategy(Enemy enemy) : base(enemy)
    {

    }

    public Node.NodeStatus Evaluate()
    {
        Debug.Log("DamageTaken Strategy");

        enemy.damageTaken = false;
        enemy.idle = false;
        stateInfo = enemy.enemyAnimator.GetCurrentAnimatorStateInfo(0);

        /*if (playerController.idle)
            return Node.NodeStatus.SUCCESS;*/

        if (enemy.enemyAnimator.IsInTransition(0) || enemy.enemyAnimator.IsInTransition(1))
        {
            if (!forceToThisState)
            {
                forceToThisState = true;
                enemy.enemyAnimator.Play("damage_taken", 0, 0);

            }
            return Node.NodeStatus.RUNNING;
        }

        if (enemy.stagger)
        {
            forceToThisState = false;
            return Node.NodeStatus.SUCCESS;
        }

        if (stateInfo.IsName("damage_taken"))
        {
            if (stateInfo.normalizedTime >= 1)
            {
                Debug.Log("Exited");
                if (enemy.healthAmount <= 0)
                {
                    enemy.deathAfterDamage = true;
                    if (playerController.enemiesInRange.Contains(enemy))
                        playerController.enemiesInRange.Remove(enemy);

                }
                if (!enemy.deathAfterDamage)
                    enemy.idle = true;
                forceToThisState = false;
                return Node.NodeStatus.SUCCESS;
            }
        }

        return Node.NodeStatus.RUNNING;
    }

}
public class DeathStrategy : BaseEnemy, IStrategy
{
    bool handleCollider;
    public DeathStrategy(Enemy enemy) : base(enemy)
    {
    }
    public Node.NodeStatus Evaluate()
    {
        Debug.Log("Death Strategy");
        if (!handleCollider)
        {
            enemy.GetComponent<Collider>().enabled = false;
            handleCollider = false;
        }           
        enemy.isDead = true;
        return Node.NodeStatus.RUNNING; 
    }
}
