using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipe : Weapon
{
    public override int InflictDamage()
    {    
        return playerController.chargeAttack ? 5 : 2;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(playerController != null)
            if (other.TryGetComponent<Enemy>(out Enemy enemy) && playerController.IsAttacking())
                if (!enemy.isDead)
                {
                    enemy.damagePosition = other.ClosestPoint(transform.position);
                    Debug.Log($"Trigger point: {other.ClosestPoint(transform.position)} -- Center Point: {enemy.centerPoint.position}");

                    enemy.OnDamage(InflictDamage());

                }
                    
    }
}
