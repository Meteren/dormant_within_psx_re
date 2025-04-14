
using System.Collections;
using UnityEngine;
public class BasePlayerState : IState
{
    protected PlayerController playerController;
    protected bool lockOn;
    float rotationSpeed = 7f;

    public BasePlayerState(PlayerController playerController)
    {
        this.playerController = playerController;
    }
    public virtual void OnStart()
    {
        playerController.rb.velocity = Vector3.zero;
    }

    public virtual void OnExit()
    {
        return;
    }

    public virtual void Update()
    {
        if (Input.GetMouseButton(1) && playerController.equippedItem != null && !UIManager.instance.inventory.activeSelf)
        {
            Weapon weapon = playerController.equippedItem as Weapon;
            if (weapon.isMelee)
            {
                if (!playerController.getStance)
                {
                    playerController.getStance = true;
                    playerController.isPressingM2 = true;
                    playerController.idle = true;
                }
               
            }
            else
            {
                playerController.isPressingM2 = true;
                playerController.aim = true;      
            }
            
        }

        if (playerController.getStance)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!playerController.primaryAttack && !playerController.secondaryAttack)
                    playerController.StartCoroutine(TryCharge());
                if (playerController.readyToCombo)
                {
                    playerController.secondaryAttack = true;
                    playerController.StartCoroutine(SetAttack(start: 0.05f, finish: 0.25f));
                }
                    
            }

            if(Input.GetMouseButtonUp(0) && playerController.charge)
            {
                playerController.chargeAttack = true;
                playerController.StartCoroutine(SetAttack(start: 0.15f, finish: 0.25f));
                playerController.charge = false;
            }
                

        }          

        if (Input.GetMouseButtonUp(1) || !playerController.isPressingM2)
        {
            playerController.getStance = false;
            playerController.StopCoroutine(TryCharge());
            if (playerController.charge)
            {
                playerController.chargeAttack = true;
                playerController.charge = false;
            }
                
        }
            

        if(Input.GetKeyDown(KeyCode.V) && !playerController.shoot && !playerController.primaryAttack && 
            !playerController.secondaryAttack && !playerController.charge && !playerController.chargeAttack)
            playerController.kick = true;

    }

    private IEnumerator TryCharge()
    {
        yield return new WaitForSeconds(0.12f);

        if (Input.GetMouseButton(0))
            playerController.charge = true;
        else
        {
            playerController.primaryAttack = true;
            playerController.StartCoroutine(SetAttack(start: 0.55f, finish: 0.2f));
        }
            

    }

    private IEnumerator SetAttack(float start,float finish)
    {
        Weapon meleeWeapon = playerController.equippedItem as Weapon;
        Collider weaponColl = meleeWeapon.GetComponent<Collider>();
        yield return new WaitForSeconds(start);
        weaponColl.enabled = true;
        weaponColl.isTrigger = true;
        yield return new WaitForSeconds(finish);
        weaponColl.isTrigger = false;
        weaponColl.enabled = false;
    }

    protected void AimAtClosest()
    {
        Debug.Log("Enemies in range count:" + playerController.enemiesInRange.Count);
        if(!playerController.enemiesInRange.Contains(playerController.lockedEnemy))
        {
            playerController.lockedEnemy = GetClosestEnemy();
            lockOn = false;
        }

        if (playerController.lockedEnemy == null)
            return;

        if (playerController.lockedEnemy.isDead)
        {
            playerController.lockedEnemy = GetClosestEnemy();
            lockOn = false;
        }

        if (playerController.lockedEnemy == null)
            return;
                 
        Weapon weapon = playerController.equippedItem as Weapon;
        Vector3 direction = playerController.lockedEnemy.transform.position - weapon.muzzlePoint.position;

        direction.y = 0f;

        if (direction.magnitude < 1f)
        {
            lockOn = false;
            return;
        }

        Quaternion lookAtDirection = Quaternion.LookRotation(direction);

        float angleDifference = Quaternion.Angle(playerController.transform.rotation, lookAtDirection);

        if (!lockOn)
        {
            Debug.Log("Lockin on");
            playerController.transform.rotation =
               Quaternion.Slerp(playerController.transform.rotation, lookAtDirection, Time.deltaTime * rotationSpeed);
            if(angleDifference <= 2f)
                lockOn = true;
        }
        else
        {
            playerController.transform.rotation =
               lookAtDirection;
            Debug.Log("Locked on");
        }
           
    }
    protected Enemy GetClosestEnemy()
    {
        playerController.enemiesInRange.Sort((x, y) =>
        x.CalculatePriority(playerController).CompareTo(y.CalculatePriority(playerController)));
        Enemy closestEnemy = null;
        foreach(var enemy in playerController.enemiesInRange)
        {
            Vector3 direction = enemy.centerPoint.position - playerController.centerPoint.position;
            Ray ray = new Ray(playerController.centerPoint.position, direction);

            if (Physics.Raycast(ray, out RaycastHit hit, playerController.aimRadius, playerController.aimMask, QueryTriggerInteraction.Ignore))
            {
                Debug.DrawRay(playerController.transform.position, direction * playerController.aimRadius);
                if (hit.transform.GetComponent<Enemy>() == enemy)
                    closestEnemy = enemy;
                else
                    continue;
            }

        }
        return closestEnemy;

    }

  

}
