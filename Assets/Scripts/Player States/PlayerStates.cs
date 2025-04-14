
using System.Collections;
using UnityEngine;
public class IdleState : BasePlayerState
{

    public IdleState(PlayerController playerController) : base(playerController)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
    }

    public override void OnExit()
    {
        base.OnExit();
        playerController.idle = false;
    }

    public override void Update()
    {

        base.Update();
        Debug.Log("Idle");

        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift) && 
            !playerController.getStance && !playerController.primaryAttack && !playerController.secondaryAttack)
        {
            playerController.run = true;
        }          
        else if (Input.GetKey(KeyCode.W))
        {
            playerController.walk = true;
        }            
        else if (Input.GetKey(KeyCode.S))
        {
            playerController.walkBackwards = true;
        }
            
        else if (Input.GetKeyDown(KeyCode.C))
            playerController.turnBack = true;

    }
}

public class WalkState : BasePlayerState
{

    float walkSpeed = 3f;
    public WalkState(PlayerController playerController) : base(playerController)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
    }

    public override void OnExit()
    {
        base.OnExit();
        playerController.walk = false;
    }


    public override void Update()
    {
        base.Update();
        Debug.Log("Walk");
        if (!Input.GetKey(KeyCode.W))
            playerController.idle = true;
           
        if (Input.GetKeyDown(KeyCode.LeftShift) && !playerController.getStance && !playerController.primaryAttack && !playerController.secondaryAttack)
            playerController.run = true;

        playerController.rb.velocity = new Vector3(playerController.ForwardDirection.x * walkSpeed,
               playerController.rb.velocity.y, playerController.ForwardDirection.z * walkSpeed);
    }
}

public class WalkBackwardsState : BasePlayerState
{
    float backwardsSpeed = 1.5f;
    public WalkBackwardsState(PlayerController playerController) : base(playerController)
    {
    }
    public override void OnStart()
    {
        base.OnStart();
    }
    public override void OnExit()
    {
        base.OnExit();
        playerController.walkBackwards = false;
    }

    public override void Update()
    {
        base.Update();
        Debug.Log("WalkBackwards");

        if (!Input.GetKey(KeyCode.S))
            playerController.idle = true;
        
        if (Input.GetKeyDown(KeyCode.LeftShift))
            playerController.turnBack = true;
        playerController.rb.velocity = new Vector3(playerController.ForwardDirection.x * backwardsSpeed * -1,
            playerController.rb.velocity.y, playerController.ForwardDirection.z * backwardsSpeed * -1);
    }
}

public class RunState : BasePlayerState
{
    float runSpeed = 6f;
    public RunState(PlayerController playerController) : base(playerController)
    {
    }
    public override void OnStart()
    {
        base.OnStart();
    }

    public override void OnExit()
    {
        base.OnExit();
        playerController.run = false;   
    }

    public override void Update()
    {
        base.Update();
        Debug.Log("Run");
        if (Input.GetKeyUp(KeyCode.W))
            playerController.idle = true;
        else if(Input.GetKeyUp(KeyCode.LeftShift))
            playerController.walk = true;
        playerController.rb.velocity = new Vector3(playerController.ForwardDirection.x * runSpeed,
              playerController.rb.velocity.y, playerController.ForwardDirection.z * runSpeed);
    }
}

public class TurnBackState : BasePlayerState
{
    public TurnBackState(PlayerController playerController) : base(playerController)
    {
    }
    public override void OnStart()
    {
        base.OnStart();
        Debug.Log("TurnBackState start initted");
        playerController.idle = true;
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void Update()
    {

        Debug.Log("Turn Back");
       
    }

}

public class AimState : BasePlayerState
{
    float transitionDuration = 0.2f;
    float waitStance = 0.2f;
    bool readyToShoot;
    bool enemySpotted;

    public AimState(PlayerController playerController) : base(playerController)
    {
    }
    public override void OnStart()
    {
        base.OnStart();
        if(playerController.comeFromShooting)
            readyToShoot = true;
       
        Debug.Log("Closest enemy spotted");
        if(playerController.enemiesInRange.Count > 0)
            playerController.lockedEnemy = GetClosestEnemy();
       
        foreach (var enemy in playerController.enemiesInRange)
            Debug.Log(enemy.name);
        
    }

    public override void OnExit()
    {
        base.OnExit();
        readyToShoot = false;
        waitStance = 0.2f;
        playerController.aim = false;
        playerController.comeFromShooting = false;
        lockOn = false;
    }

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            playerController.kick = true;

        if (!enemySpotted)
        {
            if(playerController.enemiesInRange.Count > 0)
                playerController.lockedEnemy = GetClosestEnemy();
                
            if (playerController.lockedEnemy != null)
                enemySpotted = true;
        }           
        if(playerController.enemiesInRange.Count > 0)
            AimAtClosest();
        Debug.Log("Ready to shoot: " + readyToShoot);
        if (!readyToShoot)
        {
            if (waitStance <= 0)
                readyToShoot = true;
            else
                waitStance -= Time.deltaTime;
        }
        Debug.Log("Aim state");
        if (Input.GetMouseButtonDown(0) && readyToShoot)
        {
            if (playerController.equippedItem is Weapon weapon && !weapon.GetClip().isEmpty)
            {
                playerController.shoot = true;
            }
            else
                UIManager.instance.HandleIndicator("Out of ammo.",1f);
        }
 
        if (Input.GetMouseButtonUp(1) || !playerController.isPressingM2)
            GameManager.instance.StartCoroutine(WaitTransition());
    }

    private IEnumerator WaitTransition()
    {
        yield return new WaitForSeconds(transitionDuration);
        if(!playerController.kick)
            playerController.idle = true;
    }
}

public class ShootState : BasePlayerState
{
    AnimatorStateInfo stateInfo;
    public ShootState(PlayerController playerController) : base(playerController)
    {
    }
    public override void OnStart()
    {
        base.OnStart();
        Debug.Log("On shooting start");
        Weapon weapon = playerController.equippedItem as Weapon;
        RaycastHit hit = weapon.Shoot();
        HandleShooting(hit, weapon);
    }

    public override void OnExit()
    {
        base.OnExit();
        playerController.shoot = false;
        playerController.comeFromShooting = true;
    }

    public override void Update()
    {
        base.Update();
        if (playerController.enemiesInRange.Count > 0)
            AimAtClosest();
        Debug.Log("Shoot State");

        if (playerController.anim.IsInTransition(0)) return;

        stateInfo = playerController.anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("shooting"))
            if (stateInfo.normalizedTime >= 1)
                playerController.aim = true;
    }
    private void HandleShooting(RaycastHit hit,Weapon weapon)
    {
        if (hit.collider != null)
        {
            Debug.Log($"{hit.transform.name} hitted.");
            if (hit.transform.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.damagePosition = hit.point;
                enemy.OnStagger(weapon.InflictDamage());
            }
                
        }
        else
            Debug.Log("Missed");
       
    }
}

public class KickState : BasePlayerState
{
    AnimatorStateInfo stateInfo;
    public KickState(PlayerController playerController) : base(playerController)
    {
    }
    public override void OnStart()
    {
        base.OnStart();
        playerController.canRotate = false;
    }
    public override void OnExit()
    {
        base.OnExit();
        playerController.kick = false;
        playerController.canRotate = true;
    }

    public override void Update()
    {
        // base.Update();
        Debug.Log("Kick state");
        stateInfo = playerController.anim.GetCurrentAnimatorStateInfo(0);

        if (playerController.anim.IsInTransition(0)) return;

        if (stateInfo.IsName("kick"))
            if (stateInfo.normalizedTime >= 1)
            {
                Debug.Log("Exited");
                playerController.idle = true;
            }
                

    }
}


